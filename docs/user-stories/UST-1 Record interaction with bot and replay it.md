# UST-1 Record interaction with bot and replay it

## Terms

**The bot** - sample bot from this repository. It counts symbols in the received messages. The user specifies token for this bot when he runs it.

**Postman collection** - collection representing conversation (interaction) of the user and the bot during the use case.

## Notes

1. The following instructions contain several CLI commands. All of them are to be run from the root of the repository.

1. This command sets the bot token:

```pwsh
$Env:TOKEN = 1234567891:abcdefghijklmnopqrstuvwxyz123456789
```

Here `1234567891:abcdefghijklmnopqrstuvwxyz123456789` denotes bot token used in the use case (a real token must be substituted). It is enough to run this command only once in each console window.

## Pre-conditions and common preliminary steps

1. Pre-conditions. Database does not contain any records for the bot token used in the use case.
1. User runs the Emulator at http://localhost:5134.

```pwsh
dotnet run --project .\src\TgBotApiEmulator\TgBotApiEmulator.csproj
```

3. User runs the bot and aims it at http://localhost:5134 (instead of Telegram Bot API servers).

```pwsh
$Env:TOKEN = 1234567891:abcdefghijklmnopqrstuvwxyz123456789
dotnet run --project .\src\TgBotSample\TgBotSample.csproj -- http://localhost:5134
```

4. User turns recording proxy mode on for the bot.

```pwsh
$Env:TOKEN = 1234567891:abcdefghijklmnopqrstuvwxyz123456789
curl -X 'PUT' "http://localhost:5134/admin/operatingMode/$Env:TOKEN/RecordingProxy"
```

5. User opens Telegram client and starts chat with the bot.
    1. User says: "qwerty". After that the bot says: "Text length: 6."
    1. User says: "asdf". After that the bot says: "Text length: 4."
6. User closes his Telegram client.
1. User turns recording proxy mode off for the bot.

```pwsh
$Env:TOKEN = 1234567891:abcdefghijklmnopqrstuvwxyz123456789
curl -X 'PUT' "http://localhost:5134/admin/operatingMode/$Env:TOKEN/Emulator"
```

## USC-1.1 Record conversation with bot and replay it as Postman collection

1. User saves the Postman collection of the above conversation.

```pwsh
$Env:TOKEN = 1234567891:abcdefghijklmnopqrstuvwxyz123456789
curl -X 'GET' 'http://localhost:5134/test/makePostmanCollection/$Env:TOKEN' -o counter.json
```

2. User runs the Postman collection.

```pwsh
postman collection run counter.json --env-var "emulatorUrl=http://localhost:5134" --env-var "botToken=$Env:TOKEN"
```

3. The bot receives message: "qwerty". The bot says: "Text length: 6."
1. The bot receives message: "asdf". The bot says: "Text length: 4."
1. All tests pass.
1. User stops the bot.
1. User stops the Emulator.

## USC-1.2 Record conversation with bot and view it in web GUI

1. User runs web GUI.

```pwsh
npm --prefix .\src\tgbotapiemulator-webgui run dev
```

2. User opens http://localhost:3000/sessions. The user clicks on the session corresponding to the use case and sees http://localhost:3000/player?sessionId={sessionId} with appropriate `sessionId`.
1. The update navigation panel contains 4 updates (except artificial "Start" update). All of them (except artifical "Start" update) are not checked. The messenger emulator screen shows no messages.
1. In the update navigation panel user clicks on the first update after "Start" containing message "qwerty". The corresponding message appears on the messenger emulator screen. The update in the navigation panel is being checked.
1. In the update navigation panel user clicks "Forward". The corresponding message appears. The next update in the navigation panel is being checked.
1. In the update navigation panel user clicks "Forward" many times until the offset counter freezes. The messenger emulator screen shows all 4 messages corresponding to the recorded conversation. All updates in the navigation panel are checked by now.
