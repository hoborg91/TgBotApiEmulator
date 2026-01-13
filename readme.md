# Telegram Bot API Emulator

This is a framework for Telegram bot testing. It allows to decouple the part of development and testing cycle from Telegram infrastructure.

> ⚠ Fow now only `getUpdates` technique of bot interaction is supported.

## Example: regression testing

You can use the Emulator to record some proper interaction of users with the bot. Once you add new feature to the bot you can run that previously recroded flow to check if the flow is broken. This test can be saved as a Postman collection. Also it can be played in draft version of GUI.

1. Run the Emulator in the recording proxy mode:

```sh
dotnet run -- Settings:ServerOperatingMode="RecordingProxy"
```

2. Run your bot and aim it to the Emulator (instead of Telegram servers). The default is http://localhost:5134. The Emulator will translate the bot calls to Telegram servers, record them and Telegram responses.

3. Interact with the bot as usual and utilize the bot functionality which should be covered by regression tests.

4. Make Postman collection representing the above interaction: `GET /test/makePostmanCollection/{sessionId}` (`sessionId` denotes the interaction).

5. Inegrate the Postman collection to your development and testing cycle, if necessary.

6. Play the recorded workflow in the GUI to visualize it.

![Emulator GUI](docs\screenshots\gui-general-261014.png "Title")
