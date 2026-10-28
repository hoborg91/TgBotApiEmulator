# TSK-4 Rename session (Done)

Add the ability to rename a session. Provide a backend endpoint that persists a new `NonUniqueName` for a given session ID, plus UI in the session list so a user can edit a session's name and have it persist across reloads.

**This addresses DBT-1 complaint:**
- "provides no way to rename sessions"

## Acceptance criteria

- Add endpoint `PATCH /test/sessions/{sessionId}/name` (or equivalent) that updates `NonUniqueName` for the session and returns the updated name.
- `SessionBrowser` renders an inline rename control (input + save) next to each session row.
- After saving a new name, the list reflects the new name without a full page reload; the name persists across reloads.

## Affected files

- `src/TgBotApiEmulator/Controllers/TestController.cs` (rename endpoint)
- `src/TgBotApiEmulator/Storage/ITgBotSessionStorage.cs` and `TgBotSessionStorage.cs` (update `NonUniqueName`)
- `src/tgbotapiemulator-webgui/app/components/Sessions/SessionBrowser.tsx` (rename UI per row)
- `src/tgbotapiemulator-webgui/app/contracts/emulator-api.g.d.ts` (regenerate if new endpoint added to Swagger)

## References

- docs/debt/DBT-1 Difficult to operate session list and player.md
