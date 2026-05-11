---
description: Open the observer dashboard in the default browser. Starts it if not already running.
---

# /observer

Check whether the observer is running:

```bash
curl -s -o /dev/null -w "%{http_code}" http://localhost:7777/health || echo down
```

If not running, start it:

```bash
./core/scripts/observer-start.sh &
```

Wait briefly (≤ 3 s), then open the dashboard:

```bash
open http://localhost:7777/        # macOS
# or: xdg-open http://localhost:7777/   # Linux
```

Report: dashboard URL + active game from `.active-game` + count of active tmux agent sessions.
