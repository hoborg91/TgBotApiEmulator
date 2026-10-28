# TSK-1 Combined session list and player page (Done)

Merge the sessions list (`/sessions`) and the player (`/player`) into a single page so navigation cost between them is reduced. Render the session list on the left-hand side and the player on the right-hand side. Clicking a session loads its updates into the right panel without a full-page navigation (no route change, just swapping the updates shown). Keep the "Open mocked session" link working when the session list is empty.

**This addresses DBT-1 complaints:**
- "link to the session list is unreachable unless I scroll up (and it would be even more better to have session list right here, e.g. on the left-hand side of the page)"
- "Combining the list and player in one page may reduce navigation cost."

## Acceptance criteria

- Opening `http://localhost:3000` (which redirects to `/sessions`) shows a single page with the session list on the left and the player on the right.
- Clicking a session row replaces the updates shown in the right-hand player panel without a full page reload.
- When there are no sessions, the page shows the empty state plus "Open mocked session" link that opens `/player` with no `sessionId` (mocked data).
- The player component accepts an optional `sessionId` and loads updates for that session via `GET /test/sessions/{sessionId}/updates`.

## Affected files

- `src/tgbotapiemulator-webgui/app/sessions/page.tsx`
- `src/tgbotapiemulator-webgui/app/components/Sessions/SessionBrowser.tsx`
- `src/tgbotapiemulator-webgui/app/components/Player/Player.tsx`
- `src/tgbotapiemulator-webgui/app/page.tsx` (root redirect — verify it still targets the combined page)

## References

- docs/debt/DBT-1 Difficult to operate session list and player.md
- skill: tgbotapiemulator — WebGUI sessions page default routing & load states
