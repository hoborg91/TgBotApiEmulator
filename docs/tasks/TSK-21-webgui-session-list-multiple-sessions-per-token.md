# TSK-21 WebGUI session list supports multiple sessions per bot token (Done)

Update the WebGUI session list so it can display and manage multiple sessions that share a single bot token, and allow selecting/switching the active session per token.

**This addresses DBT-4 complaints:**
- "all new updates from all interactions are being mixed in one session"

## Acceptance criteria

- The session list shows sessions grouped by / labeled with their bot token (or token hash prefix) so multiple sessions per token are distinguishable.
- Each session row has an action to select it as the current session for its bot token (so the player loads that session's updates).
- An action to create a new session for a given bot token (calls `POST /test/bot/{botToken}/sessions`).
- Selecting a session loads its updates into the player via `GET /test/sessions/{sessionId}/updates`.
- The player reflects updates only for the selected session, not all sessions of the token mixed together.

## Affected files

- `src/tgbotapiemulator-webgui/app/page.tsx` — session list + player composition.
- `src/tgbotapiemulator-webgui/app/components/SessionList.tsx` — render sessions with bot token label and select/create actions.
- `src/tgbotapiemulator-webgui/app/components/Player/Player.tsx` — load updates for selected session.
- `src/tgbotapiemulator-webgui/app/utils/api.ts` — add API calls for sessions-per-token (list/create/select) if not present.
- `src/tgbotapiemulator-webgui/app/contracts/emulator-api.g.d.ts` — regenerate/extend types for new endpoints.

## References

- docs/debt/DBT-4 Multiple sessions for one bot token.md
- skill: tgbotapiemulator — WebGUI main page (sessions list + player)
- src/tgbotapiemulator-webgui/app/components/SessionList.tsx
