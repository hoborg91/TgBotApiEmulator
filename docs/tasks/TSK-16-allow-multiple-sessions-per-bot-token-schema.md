# TSK-16 Allow multiple TgSession rows per bot token hash (schema) (Done)

Remove the database-level uniqueness that enforces a 1-to-1 mapping between a bot token and a session, so multiple isolated sessions can share one bot token.

**This addresses DBT-4 complaints:**
- "there is 1-to-1 mapping between bot token and session"
- "all new updates from all interactions are being mixed in one session"

## Acceptance criteria

- `TgSession.BotTokenHash` is no longer unique — multiple `TgSession` rows may share the same `BotTokenHash`.
- `BotOperatingMode` remains keyed by `BotTokenHash` (operating mode is a per-bot-token concern, not per-session).
- Schema change is applied via `modelBuilder` in `TgBotSessonContext.cs` (no migrations; DB recreated).

## Affected files

- `src/TgBotApiEmulator/Storage/TgBotSessonContext.cs` — remove `.IsUnique()` from the `TgSession.BotTokenHash` index.

## References

- docs/debt/DBT-4 Multiple sessions for one bot token.md
- skill: tgbotapiemulator — Storage layer (TPH, strongly typed IDs)
