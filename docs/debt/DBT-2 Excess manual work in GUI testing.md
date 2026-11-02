# DBT-2 Excess manual work in GUI testing

**As a** developer of TgBotApiEmulator **I find that it is increasingly** time consuming **to** check web GUI changes **because** of excess manual work (restart web develeopment server, mouse clicks).

## Preliminary analysis

- Screenshotting looks suitable. There should be some automated utility (test), which runs web GUI, performs some actions (e.g., clicks) and take screenshots. After that the developer can look at these screenshots to verify them.

## Affected use cases

- UST-1 Record interaction with bot and replay it
    - USC-1.2 Replay via web GUI
