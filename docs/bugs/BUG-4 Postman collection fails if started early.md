# BUG-4 Postman collection fails if started early

There is a delay between the request retranslation is being turned off and bot starts to receive updates from the Emulator (not the Telegram servers). If one runs the corresponding Postman collection during that period it is likely fail because the bot does not receive updates produced by that Postman collection.

**Steps to reproduce**

*(Excerpt from UST-1, USC-1.1)*

> **Warning** The bug reproduction is flaky since it depends on timing. The key is to run the Postman collection as soon as possible after switching recording proxy mode off.

1. Pre-conditions. Database does not contain any records for the bot token used in the use case.
1. User runs the Emulator at http://localhost:5134.
1. User runs the bot and aims it at http://localhost:5134 (instead of Telegram Bot API servers).
1. User turns recording proxy mode on for the bot.
1. User opens Telegram client and starts chat with the bot.
    1. User says: "qwerty". After that the bot says: "Text length: 6."
    1. User says: "asdf". After that the bot says: "Text length: 4."
1. User turns recording proxy mode off for the bot.
1. User saves the Postman collection of the above conversation.
1. User runs the Postman collection.

**Expected.** All tests pass.

**Actual.** At least one test fail.

## Preliminary analysis

The bot calls `getUpdates` and waits its results for some time, e. g. 100 seconds. This call is retranslated to Telegram servers which (for example) do not have any updates. During that 100 seconds one runs the Postman collection which effectively creates updates in the Emulator database. But the bot does not see them, because it still waits for updates from Telegram servers. If the test waits for the reaction from the bot for, say, 10 seconds, and does not see that reaction, it fails.

## Solution draft

Switching recording proxy mode off should somehow break the awaiting of the answer from the Telegram servers for `getUpdates` retranslated request. This in turn should make repeat its request which now should go to the `BotController.GetUpdatesAsync` (not Telegram servers).

## Affected use cases

* USC-1.1 Record conversation with bot and replay it as Postman collection
