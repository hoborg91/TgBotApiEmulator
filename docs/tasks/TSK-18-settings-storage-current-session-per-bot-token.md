# TSK-18 Settings storage: track current session per bot token (Done)

Extend the settings storage so the "current session" is tracked per bot token hash instead of a single global active session. This lets `GetOrCreateSessionAsync` resume the right session for each token.

**This addresses DBT-4 complaints:**
- "all new updates from all interactions are being mixed in one session"

## Acceptance criteria

- `ISettingsStorage.FindCurrentTgSessionAsync(botTokenHash)` returns the current `TgSessionId` for the given bot token hash (or null if none).
- `ISettingsStorage.SetCurrentTgSessionAsync(botTokenHash, tgSessionId)` sets the current session for the token, deactivating only previous current-session rows for that same token hash.
- The `Settings` entity gains a `BotTokenHash` column so current-session is scoped per token.
- `GetCurrentSessionId()` (currently unused) is updated to accept a `botTokenHash` parameter.
- No global single-active-session remains; each bot token has its own current session.

## Affected files

- `src/TgBotApiEmulator/Storage/Contracts/Entities.cs` — add `BotTokenHash` to `Settings`.
- `src/TgBotApiEmulator/Storage/Contracts/ISettingsStorage.cs` — add `botTokenHash` param to `FindCurrentTgSessionAsync` / `SetCurrentTgSessionAsync`.
- `src/TgBotApiEmulator/Storage/SettingsStorage.cs` — scope activate/deactivate by `BotTokenHash`.
- `src/TgBotApiEmulator/Storage/TgBotSessonContext.cs` — update `Settings` mapping if needed.
- `src/TgBotApiEmulator/Application/RecordingProxyFacade.cs` — update `GetCurrentSessionId` caller.

## References

- docs/debt/DBT-4 Multiple sessions for one bot token.md
- src/TgBotApiEmulator/Storage/SettingsStorage.cs
- src/TgBotApiEmulator/Application/RecordingProxyFacade.cs:288 (`GetCurrentSessionId`)
