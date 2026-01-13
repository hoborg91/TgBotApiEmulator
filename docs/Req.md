# BRQ-0101 [Bot]

The emulator provides following endpoints.

- `POST /{botTokenString}/getMe`,
- `POST /{botTokenString}/deleteWebhook`,
- `POST /{botTokenString}/getUpdates`,
- `POST /{botTokenString}/sendMessage`,
- `POST /{botTokenString}/setMessageReaction`.

`{botTokenString}` is a string of form `bot<token>`, where `<token>` must be replaced by the bot token.

# BRQ-0201 [Emulator]

The emulator provides following endpoints.

- `POST /emulator/sendMessage`.

# BRQ-0301 [Assert] botShould-endpoints

The following API endpoint handlers await update from the bot with the specified properties for the specified period of time. Caller also specifies the initial offset. The handler monitors the updates after the initial offset.

- `POST /assert/botShouldSendMessage`,
- `POST /assert/botShouldSetMessageReaction`.

Once the handler notices the update satisfying the given properties, it returns "Pass" object. This object contains 
- the elapsed time,
- the offset of the discovered update.

If the handler waits more than specified period of time, then it returns "Fail" object. This object contains
- the elapsed time,
- the reason of failure (timeout).

# TEC-0301 UpdateReader

Implements "BRQ-0301 [Assert] botShould-endpoints".

The following API endloint handlers utilize the same application service `UpdateReader` to regularly poll the database for updates after the given offset.

TODO Consider replacing the polling by application level cache with notifications (e. g., the other handler listening to incoming updates can publish event about new update).

- `POST /assert/botShouldSendMessage`,
- `POST /assert/botShouldSetMessageReaction`.

`UpdateReader` exposes the API with following input parameters.

- Offset value. `UpdateReader` searches for updates _after_ this value (the border is exclusive).
- Update filter. `UpdateReader` applies this filter to discovered updates. Once the update passes the filter `UpdateReader` returns it and stops polling.
- Timeout. If the update being searched for is not found within the given timeout, `UpdateReader` stops polling.
- Other technical parameters.

In any case `UpdateReader` returns following information.

- Time spent for waiting. This value includes all computational time but it changes only at each next database query. If the update being searched for is found in the very first database response then this value must be zero. TODO Consider removing this output parameter. Possibly it gives very little information and is not used. Be careful since this output parameter is logically connected with timeout input parameter.
- The most recent update offset observed by `UpdateReader`.

If the update being searched for is found `UpdateReader` also returns that update.

## Unit tests

1. Happy path.
    1. UNT-0111. Option 1. SUT finds the update immediately.
    1. UNT-0121. Option 2. SUT finds the update with needed features after a while.
1. Timeout. Time runs out, and SUT does not find the update with needed features.
    1. UNT-0211. Option 1. The update does not exist at all.
    1. UNT-0221. Option 2. The update exists but its offset is less than the offset given to SUT.
    1. UNT-0231. Option 3. The update appears after timeout.
1. Trivia. SUT checks input parameters.
    1. UNT-0311. Option 1. Offset value must not be negative. TODO Introduce support for negative offset. Quote from Telegram Bot API reference: "The negative offset can be specified to retrieve updates starting from -offset update from the end of the updates queue".
    1. UNT-0321. Option 2. If the timout is not zero it must be greater than the delay between queries.
