# TSK-8 Update notifier service (Done)

Introduce an application-level, singleton update notifier service so that `UpdateReader.WaitForUpdateAsync` can wait for new updates via notification instead of repeatedly polling the database. This addresses DBT-3 (emulator mode only), where all updates flow through the Emulator and most polls return nothing.

**This addresses DBT-3 complaints:**
- "it is increasingly expensive (meaning computational resources) to frequently query the database during `UpdateReader.WaitForUpdateAsync` method call because this strategy spawns plenty of queries, and the most of them return nothing"

## Acceptance criteria

- A singleton service exposes `Subscribe(sessionId)` returning a task that completes when a new update for that session is recorded, and `Notify(sessionId)` called after an update is persisted.
- Notification scope is per session: a waiter for one session is not woken by updates recorded for another session.
- `UpdateReader.WaitForUpdateAsync` checks the database first; if no matching update exists, it waits on the notifier (bounded by the remaining timeout) and re-polls the database on wakeup.
- Timeout, elapsed time, and most-recent-observed-offset outputs of `WaitForUpdateAsync` remain unchanged.
- All existing `UpdateReaderTests` pass without modification (mocked `IClock` still drives timing).

## Affected files

- `src/TgBotApiEmulator/Application/IUpdateNotifier.cs` (new)
- `src/TgBotApiEmulator/Application/UpdateNotifier.cs` (new)
- `src/TgBotApiEmulator/Application/UpdateReader.cs` (use notifier instead of pure polling loop)
- `src/TgBotApiEmulator/Controllers/BotController.cs` (call `Notify` after `RecordUpdateAsync` in `sendMessage`/`setMessageReaction`)
- `src/TgBotApiEmulator/Controllers/EmulatorController.cs` (call `Notify` after `RecordUpdateAsync`)
- `src/TgBotApiEmulator/Program.cs` (register the notifier as a singleton)
- `src/TgBotApiEmulator.Tests/UpdateReaderTests.cs` (extend/adjust as needed for the notifier)

## References

- docs/debt/DBT-3 DB polling should be replaced by synchronization.md
- docs/Req.md (TEC-0301 UpdateReader — contains the same polling→notification TODO)
- skill: tgbotapiemulator — UpdateReader (TEC-0301), strongly typed IDs, `var` everywhere / code style
