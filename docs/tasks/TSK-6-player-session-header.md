# TSK-6 Player session header (name + metadata + rename) (Done)

Show the current session's name as a header inside the player, with session metadata (started/ended timestamps) visible, and provide an opportunity to rename the session directly from the player. The player currently receives only `updates` and has no concept of the session it belongs to.

**This addresses DBT-1 complaints:**
- "the player does not show the data of the current session"
- "The Player should show the name of the current session as a header. Session metadata should be also visible or reachable."
- "The player should provide opportunity to rename session."

## Acceptance criteria

- The player displays the current session name as a header at the top of the player area.
- Session metadata (started / last activity timestamps) is visible or reachable from the player header.
- A rename control in the player header lets the user change the session name; the change persists and is reflected in the session list.
- When viewing a mocked session (no `sessionId`), the header shows a placeholder such as "Mocked session".

## Affected files

- `src/tgbotapiemulator-webgui/app/components/Player/Player.tsx` (header component, accepts session name + metadata)
- `src/tgbotapiemulator-webgui/app/sessions/page.tsx` or the combined page (passes session metadata to Player)
- `src/tgbotapiemulator-webgui/app/components/Sessions/SessionBrowser.tsx` (session metadata available to pass along)

## References

- docs/debt/DBT-1 Difficult to operate session list and player.md
- skill: tgbotapiemulator — `Player.tsx` currently takes only `updates` prop
