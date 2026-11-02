# DBT-3 DB polling should be replaced by synchronization (Paied off)

**As a** developer of TgBotApiEmulator **I find that it is increasingly** expensive (meaning computational resources) **to** frequently query the database during `UpdateReader.WaitForUpdateAsync` method call **because** this strategy spawns plenty of queries, and the most of them return nothing (which is known a priori).

## Preliminary analysis

> **⚠** This debt covers only emulator mode (request retranslation off).

Looks like `TaskCompletionSource` (wrapped in a singleton service) suits well. Once the Emulator detects new update (since all updates in emulator mode are coming through the Emulator), the corresponding process notifies our `TaskCompletionSource`. This in turn "unlocks" `WaitForUpdateAsync` method.

## Affected use cases

- UST-1 Record interaction with bot and replay it
    - USC-1.1. Replay via Postman collection
