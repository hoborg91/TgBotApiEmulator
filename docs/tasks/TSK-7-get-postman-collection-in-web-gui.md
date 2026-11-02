# TSK-7 Get Postman collection in web GUI

There should be a button for downloading the Postman collection representing the selected conversation. The button should be visible only when a conversation is selected. The button is located near the "rename session" button.

The back-end counterpart should be already implemented, however there is possibility that it accepts bot tokens only, while the web GUI does not have it. In such case a new back-end method should be introduced.

**This addresses UST-1 Record interaction with bot and replay it, USC-1.1. Replay via Postman collection:**
> the user... requests the Postman collection (either via "raw" HTTP request, or via web GUI)

## Acceptance criteria

The player contains a button for downloading the Postman collection representing the selected conversation.
