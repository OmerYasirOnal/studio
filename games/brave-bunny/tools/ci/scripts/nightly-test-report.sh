#!/usr/bin/env bash
# nightly-test-report.sh — format Unity NUnit XML test results into a markdown
# summary suitable for a GitHub issue body or Actions step summary.
#
# Owner: build-engineer. Cross-ref: .github/workflows/bb-nightly-tests.yml,
# tech-spec 10-build-and-ci.md (Wave 11 — scheduled CI).
#
# USAGE
#   nightly-test-report.sh <artifacts-dir> <output-md> [run-id] [run-number] [server-repo-url]
#
# INPUTS
#   <artifacts-dir>     Directory containing game-ci NUnit XML output (recursively scanned).
#   <output-md>         Path to write the markdown report to.
#   [run-id]            Optional GitHub Actions run id (for run link).
#   [run-number]        Optional GitHub Actions run number (for header).
#   [server-repo-url]   Optional `${server_url}/${repo}` (for run link).
#
# OUTPUT
#   Writes a markdown report with totals + failure detail to <output-md>.
#   Exits 0 even if tests failed — the caller (workflow) decides what to do
#   with the data. Exits non-zero only on usage / fatal parse errors.

set -uo pipefail

if [[ $# -lt 2 ]]; then
  echo "usage: $0 <artifacts-dir> <output-md> [run-id] [run-number] [server-repo-url]" >&2
  exit 2
fi

ARTIFACTS_DIR="$1"
OUT_MD="$2"
RUN_ID="${3:-}"
RUN_NUMBER="${4:-}"
REPO_URL="${5:-}"

mkdir -p "$(dirname "$OUT_MD")"

if [[ ! -d "$ARTIFACTS_DIR" ]]; then
  {
    echo "### Nightly test report"
    echo
    echo "_No artifacts directory found at \`$ARTIFACTS_DIR\` — the test runner likely failed before producing results._"
  } > "$OUT_MD"
  exit 0
fi

# Collect every NUnit XML the test runner emitted (filename varies by mode).
# Use a portable loop instead of `mapfile` — macOS ships bash 3.2 without it.
XML_FILES=()
while IFS= read -r line; do
  XML_FILES+=("$line")
done < <(find "$ARTIFACTS_DIR" -type f -name "*.xml" 2>/dev/null | sort)

if [[ ${#XML_FILES[@]} -eq 0 ]]; then
  {
    echo "### Nightly test report"
    echo
    echo "_No NUnit XML files found under \`$ARTIFACTS_DIR\`._"
  } > "$OUT_MD"
  exit 0
fi

# Parse with python — bash + sed against XML is a footgun.
python3 - "$OUT_MD" "$RUN_ID" "$RUN_NUMBER" "$REPO_URL" "${XML_FILES[@]}" <<'PY'
import os, sys, xml.etree.ElementTree as ET

out_md, run_id, run_number, repo_url, *xml_files = sys.argv[1:]

total = passed = failed = skipped = inconclusive = 0
failure_blocks = []
parse_errors = []

for path in xml_files:
    try:
        tree = ET.parse(path)
    except ET.ParseError as e:
        parse_errors.append((path, str(e)))
        continue
    root = tree.getroot()

    # NUnit3 schema: <test-run total=".." passed=".." failed=".." skipped=".." inconclusive="..">
    if root.tag == "test-run":
        total += int(root.get("total", 0) or 0)
        passed += int(root.get("passed", 0) or 0)
        failed += int(root.get("failed", 0) or 0)
        skipped += int(root.get("skipped", 0) or 0)
        inconclusive += int(root.get("inconclusive", 0) or 0)

    for case in root.iter("test-case"):
        result = case.get("result", "")
        if result not in ("Failed", "Error"):
            continue
        name = case.get("fullname") or case.get("name") or "<unknown>"
        message_el = case.find("./failure/message")
        stack_el = case.find("./failure/stack-trace")
        msg = (message_el.text or "").strip() if message_el is not None else ""
        stack = (stack_el.text or "").strip() if stack_el is not None else ""
        block = [f"#### `{name}`"]
        if msg:
            block.append("")
            block.append("```")
            block.append(msg[:2000])
            block.append("```")
        if stack:
            block.append("<details><summary>Stack trace</summary>")
            block.append("")
            block.append("```")
            block.append(stack[:4000])
            block.append("```")
            block.append("</details>")
        failure_blocks.append("\n".join(block))

lines = []
lines.append("### Nightly EditMode test report")
lines.append("")
if run_number:
    lines.append(f"- Run number: `#{run_number}`")
if run_id and repo_url:
    lines.append(f"- Run: {repo_url}/actions/runs/{run_id}")
lines.append(f"- XML files parsed: {len(xml_files) - len(parse_errors)} / {len(xml_files)}")
lines.append("")
lines.append("| Total | Passed | Failed | Skipped | Inconclusive |")
lines.append("|---:|---:|---:|---:|---:|")
lines.append(f"| {total} | {passed} | **{failed}** | {skipped} | {inconclusive} |")
lines.append("")

if failure_blocks:
    cap = 20
    lines.append(f"### Failures ({len(failure_blocks)} — showing first {min(cap, len(failure_blocks))})")
    lines.append("")
    lines.extend(failure_blocks[:cap])
    if len(failure_blocks) > cap:
        lines.append("")
        lines.append(f"_{len(failure_blocks) - cap} additional failure(s) omitted — see artifact for full XML._")
else:
    lines.append("All tests passed.")

if parse_errors:
    lines.append("")
    lines.append("### Parse errors")
    for path, err in parse_errors:
        lines.append(f"- `{path}` — {err}")

with open(out_md, "w") as f:
    f.write("\n".join(lines) + "\n")
PY

exit 0
