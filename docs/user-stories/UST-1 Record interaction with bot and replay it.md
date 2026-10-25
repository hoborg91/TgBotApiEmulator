# UST-1 Record interaction with bot and replay it

## User story

**As a** developer testing the Telegram bot,
**I want to** record a conversation with the bot and replay it outside the live chat,
**so that** I can reproduce the interaction deterministically in automated tests and in a visual player.

### Acceptance criteria

**Scenario A — replay via Postman collection (CLI)**

- **Given** the Emulator is running and the bot is aimed at it,
- **and given** the database contains no records for the bot token used in this story,
- **when** the user records a conversation in recording proxy mode, turns recording off, and requests the Postman collection,
- **then** the Emulator returns a Postman collection file,
- **and when** the user runs that collection with Postman,
- **then** the bot receives "qwerty" and replies "Text length: 6.",
- **and** the bot receives "asdf" and replies "Text length: 4.",
- **and** all tests in the collection pass.

**Scenario B1 — open a recorded session in the web GUI**

- **Given** the recorded session exists in the Emulator database,
- **and given** the web GUI is running,
- **when** the user opens the sessions page at http://localhost:3000/sessions and selects the session corresponding to this story,
- **then** the player opens at http://localhost:3000/player?sessionId={sessionId} with the correct `sessionId`,
- **and** the update navigation panel contains 4 updates in addition to the artificial "Start" update, all unchecked,
- **and** the messenger emulator screen shows no messages.

**Scenario B2 — step through the recorded session in the web GUI**

- **Given** the player is open on the session from Scenario B1,
- **when** the user selects the first update after "Start" (the one containing "qwerty"),
- **then** the corresponding message appears on the messenger emulator screen and that update becomes checked,
- **and when** the user activates "Forward",
- **then** the corresponding message appears and the next update becomes checked,
- **and when** the user keeps activating "Forward" until the offset counter reaches its final value,
- **then** the screen shows all 4 messages of the recorded conversation and every update is checked.

> **Consistency requirement.** The conversation replayed by Scenario A and the conversation shown by Scenarios B1–B2 must be identical (same 4 messages, same order, same text).

## Terms

**The bot** — the sample bot from this repository. It counts the symbols in each received message. The user supplies the bot token when starting the bot.

**Postman collection** — a file that represents the recorded conversation between the user and the bot and can be replayed by Postman.

**Recording proxy mode (request retranslation on)** — an Emulator operating mode in which requests from the bot are forwarded to Telegram servers. Both those requests and the responses to them are saved to the Emulator database. The user can interact with the bot in the usual way, via a common Telegram client.

**Emulator mode (request retranslation off)** — an Emulator operating mode in which the Emulator acts as a terminal agent answering the bot's requests. The bot receives updates from the Emulator database. The user interacts with the bot using the Emulator's `/emulate` methods.

**The server** — the back-end part of the Emulator. It provides all the functionality, but can be accessed only via HTTP (e.g., cURL, Postman).

**The web GUI (graphical user interface)** — the front-end part of the Emulator. It provides a visual player for the recorded conversation.

## Toolbox

This section contains console commands intended to be run from the repository root (unless stated otherwise).

### How to launch and stop the Emulator

There are two alternative ways to launch the Emulator.

#### Option 1. Docker Compose

```pwsh
docker compose -f ./src/docker-compose.yml up
```

This command blocks the console until you press Ctrl+C, which stops the Emulator. Open another console to run other commands.

#### Option 2. Raw

```pwsh
dotnet run --project .\src\TgBotApiEmulator\TgBotApiEmulator.csproj
```

This command runs the server (the back-end part of the Emulator) only. It blocks the console until you press Ctrl+C, which stops the server. Open another console to run other commands.

```pwsh
npm --prefix .\src\tgbotapiemulator-webgui run dev
```

This command runs the web GUI (the front-end part of the Emulator) only. It blocks the console until you press Ctrl+C, which stops the web GUI. Open another console to run other commands.

### How to clean the Emulator database

Usually it is easier to delete the database completely:

```pwsh
rm ./src/TgBotApiEmulator/TgBotApiEmulator.TgBotSession.db
```

Alternatively one can delete necessary session data using raw SQL queries (e. g., via database administration tool). Path to database: *./src/TgBotApiEmulator/TgBotApiEmulator.TgBotSession.db*.

### How to set the bot token environment variable

Here 1234567891:abcdefghijklmnopqrstuvwxyz123456789 denotes the bot token used in this story; substitute a real token.

```pwsh
$Env:TOKEN = 1234567891:abcdefghijklmnopqrstuvwxyz123456789
```

Run this command once per console window where the token is required. Running the same command again in the same console window has no effect.

### How to switch request retranslation on and off

The Emulator starts in recording proxy mode (request retranslation on) for all bots. The mode can be changed individually for each bot.

> ⚠ Set the bot token environment variable first.

#### Switch request retranslation off

```pwsh
curl -X 'PUT' "http://localhost:5134/admin/operatingMode/$Env:TOKEN/Emulator"
```

#### Switch request retranslation on

```pwsh
curl -X 'PUT' "http://localhost:5134/admin/operatingMode/$Env:TOKEN/RecordingProxy"
```

### How to run the sample bot

```pwsh
dotnet run --project .\src\TgBotSample\TgBotSample.csproj -- http://localhost:5134
```

### How to save the Postman collection of the recorded conversation

```pwsh
curl -X 'GET' 'http://localhost:5134/test/makePostmanCollection/$Env:TOKEN' -o regressionTest.json
```

This command saves the file *regressionTest.json* in the repository root (provided it is run from the repository root and completes successfully).

### How to run the Postman collection

> ⚠ Precondition: the Postman CLI must be installed. Given that npm is already installed, one can run npm install -g postman-cli.

```pwsh
postman collection run regressionTest.json --env-var "emulatorUrl=http://localhost:5134" --env-var "botToken=$Env:TOKEN"
```

## Pre-conditions and common preliminary steps

> Commands referenced in the use case below are listed in the Toolbox.

**Pre-conditions.** The database contains no records for the bot token used in this story.

1. The user runs the Emulator at http://localhost:5134.
1. The user runs the bot and aims it at http://localhost:5134 instead of the Telegram Bot API servers.
1. The user turns recording proxy mode on for the bot.
1. The user opens the Telegram client and starts a chat with the bot.
    1. The user sends "qwerty". The bot replies "Text length: 6."
    1. The user sends "asdf". The bot replies "Text length: 4."
1. The user closes the Telegram client.
1. The user turns recording proxy mode off for the bot.

## Use case: Record a conversation with the bot and replay it

### Main success scenario

1. The user records a conversation with the bot (common preliminary steps above).
1. The user obtains the recorded conversation from the Emulator in a form suitable for replay.
1. The user replays the conversation outside the live chat.

### Alternative flow A. Replay via Postman collection (USC-1.1)

1. The user requests the Postman collection of the recorded conversation from the Emulator.
1. The user replays the conversation by running the collection with Postman.
1. The bot receives "qwerty" and replies "Text length: 6."
1. The bot receives "asdf" and replies "Text length: 4."
1. All tests pass.
1. The user stops the bot.
1. The user stops the Emulator.

### Alternative flow B. Replay via web GUI (USC-1.2)

1. The user starts the web GUI (Toolbox, Option 2).
1. The user selects the recorded session in the sessions list.
1. The player displays the session. The update navigation panel shows 4 updates in addition to the artificial "Start" update; none of the 4 updates is checked, and the messenger emulator screen shows no messages.
1. The user selects the first update after "Start" (the one containing "qwerty"). The player displays the corresponding message and marks that update as checked.
1. The user steps to the next update. The player displays the corresponding message and marks the next update as checked.
1. The user keeps stepping forward until the offset counter reaches its final value. The messenger emulator screen shows all 4 messages of the recorded conversation, and every update is checked.
1. The user stops the web GUI and the Emulator.
