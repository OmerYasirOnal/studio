# Unity license activation runbook

> Owner: developer (one-time interactive step).
> Cross-ref: `runbooks/first-build.md` step 4.
>
> Unity Editor requires a valid license to import the project or run any
> Editor operation. The license is machine-bound (Personal) or seat-bound (Pro).
> The autonomous run could not perform this step because it requires a Unity
> ID sign-in. The activation flow takes ~3 minutes total.

## Quick path (Unity Hub UI — recommended)

```bash
open -a "Unity Hub"
```

1. Sign In (top-right) with your Unity ID
2. Preferences → Licenses → **Add** → "Get a free personal license"
3. Accept the EULA — Hub downloads the `.ulf` file silently to
   `~/Library/Application Support/Unity/Unity_v6.x.ulf`
4. Close Hub. Verify activation worked:

```bash
/Applications/Unity/Hub/Editor/6000.0.74f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -logFile - -projectPath games/brave-bunny/unity -quit
```

The licensing client should report a valid license and Unity should perform
its initial project import. Expect 5-10 minutes the first time (Library/ generation).

## Manual path (already prepped)

The autonomous run generated:

```
Unity_v6000.0.74f1.alf       (at repo root — gitignored)
```

If you can't use Hub UI for some reason:

1. Visit <https://license.unity3d.com/manual>
2. Upload `Unity_v6000.0.74f1.alf`
3. Sign in with your Unity ID, pick Personal license
4. Download the resulting `Unity_v6000.0.74f1.ulf`
5. Activate:

```bash
/Applications/Unity/Hub/Editor/6000.0.74f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -logFile - \
  -manualLicenseFile ~/Downloads/Unity_v6000.0.74f1.ulf -quit
```

## CI license setup (after local activation)

CI needs the same `.ulf` content as a GitHub Actions secret:

```bash
gh secret set UNITY_LICENSE --repo OmerYasirOnal/studio \
  < ~/Library/Application\ Support/Unity/Unity_v6.x.ulf
```

The game-ci/unity-builder action in `.github/workflows/bb-ios-build.yml` reads
this secret and feeds it to the Editor inside the macOS runner.

## Returning the license (when retiring a machine)

```bash
/Applications/Unity/Hub/Editor/6000.0.74f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -quit -returnlicense
```

Personal licenses are seat-counted; return when you're done.

## Troubleshooting

- **"No serial number"** — license never activated; re-run the quick path
- **"License is invalid"** — `.ulf` corrupted or for a different machine; regenerate `.alf` and re-upload
- **"Maximum number of activations reached"** — return the license from another machine, or contact Unity support
- **Hub UI says "license is broken"** — File → Sign Out, then sign back in
