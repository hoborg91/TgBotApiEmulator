# BUG-1 Bot receives its own messages (Resolved)

When user turns recording proxy mode off for the bot, the bot starts receive messages.

**Steps to reproduce**

1. User runs the Emulator at http://localhost:5134.
1. User runs the bot and aims it at http://localhost:5134 (instead of Telegram Bot API servers).
1. User turns recording proxy mode on for the bot.
1. User opens his Telegram client and starts chat with the bot.
    1. User says: "qwerty". After that the bot says: "Text length: 6."
    1. User says: "asdf". After that the bot says: "Text length: 4."
1. User closes his Telegram client.
1. User turns recording proxy mode off for the bot.

**Expected.** Nothing happens to the bot.

**Actual.** The bot receives message: "Text length: 4."

## Preliminary analysis

All updates receive IDs. "Natural" updates coming from Telegram servers have IDs assigned by Telegram. "Artifical" update records produced by the Emulator and they are based on "natural" IDs (method: `RecordingProxyFacade.RecordUpdateAsync`). This produces chaos. Example follows.

Given the last "natural" ID was 12345. The bot replied to this with message which will be assigned "artifical" ID 12346 (next number after 12345). The bot does not aware of that and requests all updates starting from ID 12346 (the next ID expected by it). Since there is no such update in real Telegram server, nothing happens (that is OK).

When the user turns recording proxy mode off and thus requests updates from the Emulator (still starting from ID 12346), the Emulator discovers that "artifical" update and returns it to the bot (that is the bug).

## Solution draft

"Artifical" update IDs are not required; they should be abandoned. If the bot wants to receive updates from the Emulator, the Emulator should find last update primary key received by the bot. After that the Emulator can use primary key to discover updates for the bot.

There should be a new property of update which can tell wether this update was produced by the bot. When the bot requests updates (`BotController.GetUpdatesAsync`), the Emulator should not send own bot's updates. But these updates must be visible via `TestController` and `AssertController` actions.

## Affected use cases

* USC-1.1 Record conversation with bot and replay it as Postman collection.
