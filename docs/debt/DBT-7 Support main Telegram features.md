# DBT-7

**As a** developer using the Emulator for regression testing, **I find that it is increasingly**
- frustrating **to** test bots with rich behaviour **because** the Emulator support messages and message reactions only.

## Preliminary analysis

This is the list of features that the Emulator lacks support for and that should be implemented first:
- keyboards;
- inline keyboards;
- inline requests;
- polls and quizes;
- checklists.

## Affected use cases

- UST-1 Record interaction with bot and replay it
    - USC-1.1 Replay via Postman collection
    - USC-1.3 Replay via web GUI
