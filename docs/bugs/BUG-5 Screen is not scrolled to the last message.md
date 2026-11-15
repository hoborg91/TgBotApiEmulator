# BUG-5 Screen is not scrolled to the last message

When a sequence of new messages arrive then the player screen is scrolled to bottom only on first message. While next messages arrive, they are visualized on the screen (that's good), but the screen is not scrolled to bottom to the new messages (that's bad).

**Steps to reproduce**

1. User opens a session full of messages and goes to the last update. All updates must be checked in the navigation panel, and the screen must be scrolled to bottom.
1. User issues at least two message updates. E. g. one can run sample bot and send this bot a message (update #1). The bot will reply (update #2).

**Expected.** Screen is scrolled to bottom to the last message (update #2 in the above example).

**Actual.** Screen is scrolled to the first new message (update #1 in the above example) only.

## Preliminary analysis

Root cause is in `Player.refreshUpdates` (`src/tgbotapiemulator-webgui/app/components/Player/Player.tsx:169`).
When a SignalR `NewUpdate` arrives, `refreshUpdates` captures `scrollToBottom = isAtBottom()` *before* fetching the new updates, then sets `shouldScroll = scrollToBottom`. `MessengerScreen` (`src/tgbotapiemulator-webgui/app/components/Player/MessengerScreen.tsx:94`) scrolls the last visible message into view with `scrollIntoView({ behavior: "smooth", block: "center" })` when `shouldScroll` is true.

Problem: `block: "center"` centers the target message in the viewport, which moves `scrollTop` away from the bottom edge. After the first live update, `scrollTop + clientHeight` is no longer `>= scrollHeight - 10`, so the next call to `isAtBottom()` returns `false`. Consequently `shouldScroll` is set to `false` for every subsequent live update, and the screen stops auto-scrolling.

Trace for consecutive updates:
1. Update #1 arrives: user is at bottom → `isAtBottom()` = true → `shouldScroll` = true → `scrollIntoView(block: "center")` centers last message → viewport is no longer at bottom edge.
2. Update #2 arrives: `isAtBottom()` = false (viewport centered after step 1) → `shouldScroll` = false → no scroll. Bug manifests.

Fix candidate: in `Player.refreshUpdates`, also treat "all updates were checked" (`previousOffset !== null && previousOffset === previousUpdatesLength`) as a reason to scroll, in addition to the pixel-level `isAtBottom()` check. This keeps auto-follow working across consecutive updates because after each update the offset is at the end again.

Alternative fix candidate: change `block: "center"` to `block: "end"` in `MessengerScreen` so the viewport bottom stays aligned with the content bottom after scrolling, keeping `isAtBottom()` true. This changes UX (messages align to bottom edge instead of center).

## Solution draft

Fix #1 (reverted): `setShouldScroll(scrollToBottom || allChecked)` in `Player.refreshUpdates` was too aggressive — `allChecked` is `true` even when the user manually scrolled away from the bottom while at end offset, causing unwanted scrolling. This violates the alternative flows of UST-2 Live updates (screen must not scroll when it was not scrolled to bottom before).

Implemented fix #2 instead: changed `block: "center"` → `block: "end"` in `MessengerScreen.tsx:109`. After scrolling, the viewport bottom stays aligned with the content bottom, so `isAtBottom()` stays `true` across consecutive live updates — auto-scroll keeps working (BUG-5 fixed) while manual scroll-away is still respected (UST-2 alternative flows preserved).

## Affected use cases

* UST-2 Live updates
    * USC-2.2 New update becomes visible immediately
