# TSK-5 Enlarge player main window (Done)

Make the player's main chat area larger so more than two or three small messages are visible at once without scrolling. Increase the chat container height (and/or make it resizable) so the player is comfortable to operate for regression testing.

**This addresses DBT-1 complaint:**
- "its main window is too small (renders only two-three small messages at once)"

## Acceptance criteria

- The player chat area displays substantially more messages at once (visibly more than 2–3).
- The layout still accommodates the `UpdateBrowser` navigation panel on the right side of the player.
- No horizontal overflow or layout breakage from the larger chat container.

## Affected files

- `src/tgbotapiemulator-webgui/app/components/Player/Player.tsx` (chat container sizing)
- `src/tgbotapiemulator-webgui/app/components/Chat/ChatContainer.tsx` (height constraints)
- `src/tgbotapiemulator-webgui/app/components/page.module.css` (or equivalent stylesheet) if used by Player/ChatContainer

## References

- docs/debt/DBT-1 Difficult to operate session list and player.md
- skill: tgbotapiemulator — `Player.tsx` renders `ChatComponent` inside `ChatContainer`
