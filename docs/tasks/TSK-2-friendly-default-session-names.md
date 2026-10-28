# TSK-2 Friendly default session names ("Session N") (Done)

Replace the non-readable artificial session names (currently the bot token SHA256 hash used as `NonUniqueName`) with user-friendly default names of the form "Session N", where N is a natural-number index equal to `count of existing sessions + 1`. Note: the current state allows only one session per bot; this is not desirable and will be refactored later, but for now N should be computed as the total count of existing sessions plus one (globally, not per bot).

**This addresses DBT-1 complaints:**
- "provides non-readable artificial session names"
- "The default session names should be more user friendly, e.g. 'Session N', where N - is a natural-number index."

## Acceptance criteria

- Newly created sessions are named "Session 1", "Session 2", … in order of creation (N = total existing session count + 1 at creation time).
- The default name is shown in the session list (`SessionBrowser`) and in the player header (see TSK-6).
- Existing sessions keep their current name (only new sessions get the "Session N" default).

## Affected files

- `src/TgBotApiEmulator/Application/RecordingProxyFacade.cs` (`GetOrCreateSessionAsync`, `CreateTgSession` call)
- `src/TgBotApiEmulator/Storage/TgBotSessionStorage.cs` (`CreateTgSession`)
- `src/TgBotApiEmulator/Controllers/TestController.cs` (`GetSessionsAsync` returns name)
- `src/tgbotapiemulator-webgui/app/components/Sessions/SessionBrowser.tsx` (renders session name)

## References

- docs/debt/DBT-1 Difficult to operate session list and player.md
- skill: tgbotapiemulator — Storage layer (`CreateTgSession`), `RecordingProxyFacade` (`GetOrCreateSessionAsync`)
