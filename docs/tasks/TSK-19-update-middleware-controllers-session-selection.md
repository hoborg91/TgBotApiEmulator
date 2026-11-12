# TSK-19 Update middleware + controllers to select sessions per bot token (Done)

Update all request handlers that currently call `GetOrCreateSessionAsync(botTokenHash)` so they support explicit session selection (via an optional session id) and otherwise fall back to the current session for the bot token.

**This addresses DBT-4 complaints:**
- "all new updates from all interactions are being mixed in one session"

## Acceptance criteria

- `RecordingProxyMiddleware` resolves a session per request: uses an explicit session id if provided (header/query), otherwise the current session for the bot token (creating one if none).
- `BotController.GetSessionIdAsync` (used by getUpdates/sendMessage/setMessageReaction/getMe/deleteWebhook) selects the session per token, honoring an explicit session override.
- `AssertController` (botShouldSendMessage / botShouldSetMessageReaction) selects the session per token.
- `EmulatorController.MessageAsync` selects the session per token.
- An explicit session selector is available on bot API requests (e.g. optional `?sessionId=` query parameter or `X-Session-Id` header), so a test can target a specific session of a shared token.
- Sessions are no longer implicitly reused as "the one session" for a token unless that is the intended current session.

## Affected files

- `src/TgBotApiEmulator/Middleware/RecordingProxyMiddleware.cs` — session resolution per request.
- `src/TgBotApiEmulator/Controllers/BotController.cs` — `GetSessionIdAsync`.
- `src/TgBotApiEmulator/Controllers/AssertController.cs` — both assert endpoints.
- `src/TgBotApiEmulator/Controllers/EmulatorController.cs` — `MessageAsync`.
- `src/TgBotApiEmulator/Application/RecordingProxyFacade.cs` — session resolution helpers.
- `src/TgBotApiEmulator/Utilities/RecordingMiddlewareUtilities.cs` — if used to extract an optional session selector from the request.

## References

- docs/debt/DBT-4 Multiple sessions for one bot token.md
- src/TgBotApiEmulator/Middleware/RecordingProxyMiddleware.cs:42
- src/TgBotApiEmulator/Controllers/BotController.cs:56
