# BUG-2 Web GUI player and update browser mess offsets (Resolved)

When user opens session in web GUI and clicks the first (real) update the user sees all updates immediately. Offset counter visible to the user cannot synchronize update offset when they are chosen by clicking the exact update and clicking buttons "Backward"/"Forward".

**Steps to reproduce**

*(Excerpt from UST-1, USC-1.2)*

1. Pre-conditions. Database does not contain any records for the bot token used in this use case.
1. User runs the Emulator at http://localhost:5134.
1. User runs the bot and aims it at http://localhost:5134 (instead of Telegram Bot API servers).
1. User turns recording proxy mode on for the bot.
1. User opens his Telegram client and starts chat with the bot.
    1. User says: "qwerty". After that the bot says: "Text length: 6."
    1. User says: "asdf". After that the bot says: "Text length: 4."
1. User closes his Telegram client.
1. User turns recording proxy mode off for the bot.
1. User runs web GUI and opens http://localhost:3000/sessions. The user clicks on the session corresponding to the use case and sees http://localhost:3000/player?sessionId={sessionId} whith appropriate `sessionId`.
1. The update navigation panel contains 4 updates (except artifical "Start" update). All of them (except artifical "Start" update) are not checked. The messenger emulator screen shows no messages.
1. In the update navigation panel user clicks on the first update after "Start" containing message "qwerty".

**Expected.** The corresponding message appears on the messenger emulator screen. 

**Actual.** The messenger emulator screen shows all 4 messages corresponding to the recorded conversation.

## Preliminary analysis

In Player.tsx the offset corresponds to indices of the `updates` array:

```ts
// Lines 21-22:
for (let i = 0; i <= currentOffset; i++) {
      const u = updates[i];
```

In the UpdateBrowser.tsx the offset corresponds to update ID:

```ts
// Line 36:
<HistoryItem itemOffset={update.update_id}>{ToLabel(update)}</HistoryItem>
```

This mismatch brings chaos to the user experience.

## Solution draft

Use "artifical" array indices (like in Player.tsx) to indicate update offset everywhere in code.

## Affected use cases

* USC-1.2. Record conversation with bot and view it in web GUI.
