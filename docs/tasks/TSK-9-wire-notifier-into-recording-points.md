# TSK-9 Wire update notifier into update recording points

Call the new update notifier from every code path that records an update in emulator mode, so waiters in `UpdateReader.WaitForUpdateAsync` are woken instead of timing out after repeated empty polls.

**This addresses DBT-3 complaints:**
- "this strategy spawns plenty of queries, and the most of them return nothing (which is known a priori)"

## Acceptance criteria

- `BotController.SendMessageAsync` notifies after recording a bot-produced message update.
- `BotController.SetMessageReaction` notifies after recording a bot-produced reaction update.
- `EmulatorController.MessageAsync` notifies after recording an emulated user-message update.
- Notifications are non-blocking and do not alter the recorded update or its offset.
- `UpdateReader` consumers (`getUpdates`, `botShouldSendMessage`, `botShouldSetMessageReaction`) return promptly once a matching update is recorded.
- End-to-end flow works: record an update → a concurrently waiting `getUpdates`/assert returns the update without polling to timeout.

## Affected files

- `src/TgBotApiEmulator/Controllers/BotController.cs`
- `src/TgBotApiEmulator/Controllers/EmulatorController.cs`
- `src/TgBotApiEmulator/Application/RecordingProxyFacade.cs` (optional: `RecordUpdateAsync` / `RecordTgServerResponseAsync`)
- `src/TgBotApiEmulator.Tests/` (add integration test for notify → wait flow)

## References

- docs/tasks/TSK-8-update-notifier-service.md
- docs/debt/DBT-3 DB polling should be replaced by synchronization.md
- skill: tgbotapiemulator — `RecordingProxyFacade.RecordUpdateAsync`, `UpdateReader`, `BotController`, `EmulatorController`
