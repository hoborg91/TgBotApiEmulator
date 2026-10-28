# TSK-3 Session metadata (started/ended) in session list (Done)

Expose session metadata — time of start (`DtmCreated`) and time of last activity / end (`DtmLastUpdated`) — through the sessions API and render it per row in the session list with visual grouping. The `TgSession` entity already stores `DtmCreated` and `DtmLastUpdated`, but `TestController.GetSessionsAsync` currently returns only `Id` and `NonUniqueName`.

**This addresses DBT-1 complaint:**
- "lacks visual grouping and metadata (e. g., time of start and end)"

## Acceptance criteria

- `GET /test/sessions` returns each session with `id`, `non_unique_name`, `dtm_created` (started), and `dtm_last_updated` (ended / last activity).
- `SessionBrowser` renders each session row showing the session name plus the started/ended timestamps.
- Rows are visually grouped/separated so metadata is scannable.

## Affected files

- `src/TgBotApiEmulator/Controllers/TestController.cs` (`TgSession` record and `GetSessionsAsync` projection)
- `src/TgBotApiEmulator/Application/ISessionFacadeRead` and `RecordingProxyFacade.cs` (`GetSessionsAsync` returns `DtmCreated`/`DtmLastUpdated`)
- `src/tgbotapiemulator-webgui/app/contracts/emulator-api.g.d.ts` (regenerate from Swagger after backend change)
- `src/tgbotapiemulator-webgui/app/components/Sessions/SessionBrowser.tsx` (render timestamps per row)

## References

- docs/debt/DBT-1 Difficult to operate session list and player.md
- skill: tgbotapiemulator — `TgSession` entity fields `DtmCreated`/`DtmLastUpdated`
