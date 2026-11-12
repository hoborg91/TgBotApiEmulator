# TSK-17 Facade: add CreateSessionAsync + resolve current session per bot token (Done)

Change session resolution so a bot token can have multiple sessions. `GetOrCreateSessionAsync` returns the current session for the token (creating a default one on first use), while a new `CreateSessionAsync` always creates a fresh isolated session.

**This addresses DBT-4 complaints:**
- "there is not mechanism to perform multiple isolated sessions with one bot token"

## Acceptance criteria

- `ISessionFacadeRead` / `IRecordingProxyFacadeWrite` expose `CreateSessionAsync(string botTokenHash, CancellationToken)` that always creates and returns a new `TgSession` for the given token hash.
- `GetOrCreateSessionAsync(botTokenHash)` returns the current session for the token (per-token current session, see TSK-18), creating a default session named "Session N" on first use if none is current.
- Creating a new session does not affect other sessions sharing the same token hash.
- Existing `GetSessionAsync(sessionId)`, `GetSessionsAsync(botTokenHash: ...)`, and update-recording methods continue to work per `TgSessionId`.

## Affected files

- `src/TgBotApiEmulator/Application/RecordingProxyFacade.cs` — add `CreateSessionAsync`; rework `GetOrCreateSessionAsync` to use per-token current session.
- `src/TgBotApiEmulator/Application/ISessionFacadeRead.cs` — if a separate read interface exists, add `CreateSessionAsync` to `IRecordingProxyFacadeWrite`.
- `src/TgBotApiEmulator/Storage/ITgBotSessionStorage.cs` — keep `CreateTgSession` (already exists); facade wraps it.

## References

- docs/debt/DBT-4 Multiple sessions for one bot token.md
- src/TgBotApiEmulator/Application/RecordingProxyFacade.cs:94 (`GetOrCreateSessionAsync`)
- skill: tgbotapiemulator — Facades & application services
