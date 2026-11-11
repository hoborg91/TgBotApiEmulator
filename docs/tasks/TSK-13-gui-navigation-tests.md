# TSK-13 Add GUI navigation tests for UpdateBrowser controls (Done)

Add tests covering the UpdateBrowser navigation controls (`Start | Backward | Forward | End`) and assert the resulting offset and visible message set after each transition.

**This addresses DBT-2 Excess manual work in GUI testing:** automates the mouse-click verification of update navigation instead of doing it manually.

## Acceptance criteria

- `Start` → offset `null` (nothing rendered).
- `Backward` → `null` when `offset === null || offset <= 1`, else `offset - 1`.
- `Forward` → `1` when `null`, else `min(offset + 1, updates.length)`.
- `End` → `updates.length` (or `null` when empty).
- After each transition, assert the expected offset and the `msg-<index>` scroll target / visible message set.
- Backward/Forward disabled only when there are no updates (covered by test).

## Affected files

- `src/tgbotapiemulator-webgui/tests/gui/` (new navigation tests)

## References

- docs/debt/DBT-2 Excess manual work in GUI testing.md
- docs/tasks/TSK-10-playwright-gui-smoke-test-scaffold.md
- docs/tasks/TSK-11-mock-webgui-backend-requests.md
- skill: tgbotapiemulator — reference/webgui.md (UpdateBrowser control semantics)
