# `shared/` — cross-game resources

Things that belong here:

- **`ui-kit/`** — UI components (UXML + USS + C#) reusable across games. Button styles, common screens, accessibility helpers.
- **`audio-library/`** — CC0 audio that multiple games will use (UI clicks, generic positive/negative feedback).
- **`shader-library/`** — Reusable Shader Graph assets (outline shader, dissolve, hit-flash).

If something is genre-specific (e.g., a roguelite-only HUD widget), it belongs in `core/templates/<genre>/` instead so it ships with the template.

If something is one-game specific, it belongs in `games/<name>/` and not here.

This directory is empty during framework v0.1.0; Brave Bunny's vertical slice will surface the first cross-game candidates and they'll be promoted into `shared/` as they emerge.
