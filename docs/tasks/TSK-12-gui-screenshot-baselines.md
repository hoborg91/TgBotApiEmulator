# TSK-12 Add GUI screenshot baselines for main page states (Done)

Commit deterministic screenshot baselines for the main page states so developers can visually verify GUI changes by comparing against baselines.

**This addresses DBT-2 Excess manual work in GUI testing:**
> "After that the developer can look at these screenshots to verify them."

## Acceptance criteria

- Baseline screenshot: empty page (no session selected, `kind: "none"`).
- Baseline screenshot: mocked session loaded, offset = null ("Start" state).
- Baseline screenshot: offset 1 after clicking `Forward`.
- Baseline screenshot: max offset after clicking `End`.
- Baselines committed under `src/tgbotapiemulator-webgui/tests/gui/snapshots/`.
- Screenshots are deterministic (static fixtures; no timestamps/random data).

## Affected files

- `src/tgbotapiemulator-webgui/tests/gui/snapshots/` (new)

## References

- docs/debt/DBT-2 Excess manual work in GUI testing.md
- docs/tasks/TSK-10-playwright-gui-smoke-test-scaffold.md
- docs/tasks/TSK-11-mock-webgui-backend-requests.md
- skill: tgbotapiemulator — reference/webgui.md (offset semantics, Player states)
