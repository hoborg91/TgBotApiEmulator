# How to...

This document contains console commands intended to be run from the repository root (unless stated otherwise). All commands are provided for PowerShell.

The most commands suggest that ["just"](https://github.com/casey/just) is installed. If you do not want to install "just", then refer to *justfile* in the repository root to view "raw" commands. To install "just", run the following command (provided NPM is installed):

```pwsh
npm install -g rust-just
```

## Launch and stop the Emulator

There are two alternative ways to launch the Emulator.

### Option 1. Docker Compose

```pwsh
just run-as-docker
```

This command blocks the console until you press Ctrl+C, which stops the Emulator. Open another console to run other commands.

### Option 2. Raw

```pwsh
just run-server
```

This command runs the server (the back-end part of the Emulator) only. It blocks the console until you press Ctrl+C, which stops the server. Open another console to run other commands.

```pwsh
just run-webgui
```

This command runs the web GUI (the front-end part of the Emulator) only. It blocks the console until you press Ctrl+C, which stops the web GUI. Open another console to run other commands.

## Clean the Emulator database

Usually it is easier to delete the database completely:

```pwsh
just remove-database
```

Alternatively one can delete necessary session data using raw SQL queries (e. g., via database administration tool). Path to database: *./src/TgBotApiEmulator/TgBotApiEmulator.TgBotSession.db*.

## Set the bot token environment variable

> Here 1234567891:abcdefghijklmnopqrstuvwxyz123456789 denotes the bot token used in this story; substitute a real token.

```pwsh
$Env:TOKEN = "1234567891:abcdefghijklmnopqrstuvwxyz123456789"
```

Run this command once per console window where the token is required. Running the same command again in the same console window has no effect.

## Switch request retranslation on and off

The Emulator starts in recording proxy mode (request retranslation on) for all bots. The mode can be changed individually for each bot.

> ⚠ Set the bot token environment variable first.

### Switch request retranslation off

```pwsh
just turn-retranslation-off $Env:TOKEN
```

### Switch request retranslation on

```pwsh
just turn-retranslation-on $Env:TOKEN
```

## Run the sample bot

> ⚠ Set the bot token environment variable first.

```pwsh
just run-sample-bot $Env:TOKEN
```

## Save the Postman collection of the recorded conversation

> ⚠ Set the bot token environment variable first.

```pwsh
just download-postman-collection $Env:TOKEN
```

This command saves the file *regressionTest.json* in the repository root (provided it is run from the repository root and completes successfully).

## Run the Postman collection

> ⚠ Set the bot token environment variable first.

> ⚠ Precondition: the Postman CLI must be installed. Given that npm is already installed, one can run `npm install -g postman-cli`.

```pwsh
just run-postman-collection $Env:TOKEN
```
