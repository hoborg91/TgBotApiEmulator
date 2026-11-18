# DBT-7 Session management during testing

**As a** developer using the Emulator for regression testing, **I find that it is increasingly**
- frustrating **to** to see test updates right after real updates in the same session **because** it is hard to distinguish (1) test and real updates and (2) test updates from different Postman collection runs.

## Preliminary analysis

It would be far more comfortable if each new Postman collection run spawn new session.

### Alternative flows (should be analyzed)

- May be after the end of the run (test) the initial session (which was active before the test) should become active again.
- Or after the end of the run (test) the new session shoud be created (and automatically become active).

> **⚠** While testing should be all other sessions be blocked somehow? Looks like yes, the should be blocked (and the ability to create and make other sessions active should be bloked too).

## Affected use cases

- UST-1 Record interaction with bot and replay it
    - USC-1.1 Replay via Postman collection
    - USC-1.3 Replay via web GUI
