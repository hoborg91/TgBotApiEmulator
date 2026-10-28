# DBT-1 Difficult to operate session list and player (Resolved)

**As a** developer using the Emulator for regression testing, **I find that it is increasingly**
- frustrating **to** navigate between recorded sessions **because** the session list
    - lacks visual grouping and metadata (e. g., time of start and end),
    - provides non-readable artificial session names,
    - provides no way to rename sessions,
- uncomfortable **to** operate the player **because**
    - its main window is too small (renders only two-three small messages at once),
    - link to the session list is unreachable unless I scroll up (and it would be even more better to have session list right here, e.g. on the left-hand side of the page),
    - the player does not show the data of the current session.

## Preliminary analysis

- Combining the list and player in one page may reduce navigation cost.
- The default session names should be more user friendly, e.g. "Session N", where N - is a natural-number index.
- The session list should be improved according to the debt description.
- The player should show the name of the current session as a header. Session metadata should be also visible or reachable.
- The player should provide opportunity to rename session.

## Affected use cases

- UST-1 Record interaction with bot and replay it
    - USC-1.2 Replay via web GUI
