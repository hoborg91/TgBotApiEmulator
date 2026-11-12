# TSK-20 API endpoints to list/create/select sessions per bot token (Done)

Add test-only admin endpoints so a user/test can enumerate, create, and switch sessions for a given bot token. These are needed because multiple sessions now share one token and the client must be able to pick which one to use.

**This addresses DBT-4 complaints:**
- "I need to clear previous sessions to carry out new clean test"
- "there is not mechanism to perform multiple isolated sessions with one bot token"

## Acceptance criteria

- `GET /test/bot/{botToken}/sessions` lists all sessions for the given bot token (session id, name, created, last updated).
- `POST /test/bot/{botToken}/sessions` creates a new session for the given bot token and returns it (optionally makes it current).
- `POST /test/bot/{botToken}/sessions/{sessionId}/select` sets the given session as the current session for the bot token.
- Existing `GET /test/sessions` and `GET /test/sessions/{sessionId}` continue to work (may be filtered by bot token via query param).
- Endpoints live under `TestController` (test-only) and are backed by the facade/storage.
- Validation: 404 when the token/session does not exist; 409 when selecting a session that belongs to a different token.

## Affected files

- `src/TgBotApiEmulator/Controllers/TestController.cs` — add session-per-token endpoints.
- `src/TgBotApiEmulator/Application/RecordingProxyFacade.cs` — add methods to list sessions by token, create session, set current session per token.
- `src/TgBotApiEmulator/Storage/ITgBotSessionStorage.cs` — already supports filtering by `botTokenHash`; ensure `CreateTgSession` and settings setters are usable.
- `src/TgBotApiEmulator/Storage/Contracts/ISettingsStorage.cs` — per-token current session (TSK-18).

## References

- docs/debt/DBT-4 Multiple sessions for one bot token.md
- src/TgBotApiEmulator/Controllers/TestController.cs:18 (`GetSessionsAsync`)
- src/TgBotApiEmulator/Controllers/TestController.cs:99 (`GetSessionAsync` by token)
