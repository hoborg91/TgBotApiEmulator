# UST-1 Record interaction with bot and replay it (Implemented)

## User story

- **As a** developer testing the Telegram bot,
- **I want to** record a conversation with the bot and replay it outside the live chat,
- **so that** I can reproduce the interaction deterministically in automated tests and in a visual player.

## Use cases

> **Tip.** File *how-to.md* contains instructions on how to operate the Emulator.

### USC-1.1. Replay via Postman collection

- **Given** the Emulator is running and the sample bot is aimed at it,
- **and given** the database contains no records for the sample bot token used in this story,
- **when** the user records a conversation (messages only) in recording proxy mode, turns recording off, and requests the Postman collection (either via "raw" HTTP request, or via web GUI),
- **then** the Emulator returns a Postman collection file,
- **and when** the user runs that collection with Postman,
- **then** the sample bot receives user messages from the recorded conversation and send same replies to them,
- **and** all tests in the collection pass.

Example conversation follows.

- **User**. qwerty.
- **Bot**. Text length: 6.
- **User**. asdf.
- **Bot**. Text length: 4.

### USC-1.2. Open session in web GUI

- **Given** the recorded session exists in the Emulator database,
- **and given** the web GUI is running,
- **when** the user opens http://localhost:3000 and selects the session corresponding to this story,
- **then** the player opens,
- **and** the update navigation panel contains updates corresponding to the recorded session, all unchecked,
- **and** the messenger emulator screen shows no messages.

### USC-1.3. Replay via web GUI

- **Given** the player is open on the session from USC-1.2,
- **when** the user selects the first update,
- **then** the corresponding message appears on the messenger emulator screen and that update becomes checked,
- **and when** the user activates "Forward",
- **then** the corresponding message appears and the next update becomes checked,
- **and when** the user keeps activating "Forward" until the offset counter reaches its final value,
- **then** the screen shows all messages of the recorded conversation and every update is checked.

## Terms

**Sample bot** — the sample bot from this repository. It counts the symbols in each received message. The user supplies the bot token when starting the bot.

**Postman collection** — a file that represents the recorded conversation between the user and the bot and can be replayed by Postman.

**Recording proxy mode (request retranslation on)** — an Emulator operating mode in which requests from the bot are forwarded to Telegram servers. Both those requests and the responses to them are saved to the Emulator database. The user can interact with the bot in the usual way, via a common Telegram client.

**Emulator mode (request retranslation off)** — an Emulator operating mode in which the Emulator acts as a terminal agent answering the bot's requests. The bot receives updates from the Emulator database. The user interacts with the bot using the Emulator's `/emulate` methods.

**The server** — the back-end part of the Emulator. It provides all the functionality, but can be accessed only via HTTP (e.g., cURL, Postman).

**The web GUI (graphical user interface)** — the front-end part of the Emulator. It provides a visual player for the recorded conversation.
