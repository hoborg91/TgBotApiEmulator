# BUG-3 Web GUI player does not visibly differentiate messages from different parties (Resolved)

When user opens session in web GUI and reveals all the messages then all messages are located on the one side of the messenger emulator screen (this is wrong).

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
1. The update navigation panel contains 4 updates (except artifical "Start" update).
1. In the update navigation panel user clicks "Forward" many times until the offset counter freezes.

**Expected.** The (two) user messages are located on the right hand side, and the (two) bot messages are located on the left hand side.

**Actual.** All messages are located on the left hand side.

## Preliminary analysis

Player.tsx respects `username` to decide what side of the screen the message belongs to. This is outdated technique.

## Solution draft

Use `is_bot` field. When `true`, show message on the left hand side. When `false`, show message on the right hand side.

Adjust mocks in UpdateMock.ts and Hamlet-Act-1-Scene-2.ts. Hamlet must not be a bot; other participants must be bots.

## Affected use cases

- UST-1 Record interaction with bot and replay it
    - USC-1.2 Replay via web GUI
