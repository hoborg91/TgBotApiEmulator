# TSK-14 Add GUI session selection tests (none / mocked / real) (Done)

Add tests covering session selection transitions across the `none | mocked | real` selection union, asserting the Player renders correctly and offset resets to `null` on deselect.

**This addresses DBT-2 Excess manual work in GUI testing:** automates mouse-click verification of session selection instead of doing it manually.

## Acceptance criteria

- "Open mocked session" → Player shows mocked updates.
- Selecting a real session (from mocked `/test/sessions`) → Player shows `Session #<id>`.
- Deselecting (back to `none`) → Player receives `null`, offset resets to `null`.
- Header text matches: "Mocked session" / "Session #<id>" / "Select a session".

## Affected files

- `src/tgbotapiemulator-webgui/tests/gui/` (new session-selection tests)

## References

- docs/debt/DBT-2 Excess manual work in GUI testing.md
- docs/tasks/TSK-10-playwright-gui-smoke-test-scaffold.md
- docs/tasks/TSK-11-mock-webgui-backend-requests.md
- skill: tgbotapiemulator — reference/webgui.md (SelectedSession union, Player header)
