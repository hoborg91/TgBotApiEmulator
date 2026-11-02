# DBT-4 Multiple sessions for one bot token

**As a** developer using the Emulator for regression testing **I find that it is increasingly** uncomfortable **to** use same bot token for multiple tests **because** I need to clear previous sessions to carry out new clean test.

## Preliminary analysis

One needs to clear previous sessions to carry out new clean test because for now there is 1-to-1 mapping between bot token and session. There is not mechanism to perform multiple isolated sessions with one bot token: all new updates from all interactions are being mixed in one session.

## Affected use cases

- UST-1 Record interaction with bot and replay it
    - USC-1.1. Replay via Postman collection
