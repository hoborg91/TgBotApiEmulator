# DBT-6

**As a** developer using the Emulator for regression testing, **I find that it is increasingly**
- frustrating **to** operate the player (messenger emulator screen) **because** message bubbles are very narrow (no matter how wide the emulator screen is) and contain very few symbols per line (thus message bubbles are too big in vertical dimension).

## Preliminary analysis

- Message bubbles should be wider and contain more symbols per line.
- Also message bubbles should stick to the corresponding sides of the emulator screen (while now they stick to the center of the screen).

## Affected use cases

- UST-1 Record interaction with bot and replay it
    - USC-1.2 Replay via web GUI
