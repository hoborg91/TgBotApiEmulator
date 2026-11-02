// Mock updates for the /test/sessions/{sessionId}/updates endpoint.
// Real Telegram Update shape is large; we only include fields the WebGUI reads:
// update_id, message (from.is_bot, text), message_reaction.
export type MockUpdate = {
  update_id: number
  message?: {
    message_id: number
    from: { id: number; is_bot: boolean }
    text?: string
  }
  message_reaction?: {
    message_id: number
    reaction: Array<{ type: string; emoji: string }>
  }
}

// Three updates so UpdateBrowser Forward/Backward/End navigation is meaningful:
//  1. user message
//  2. bot message
//  3. reaction to message 1
export const MOCK_UPDATES: MockUpdate[] = [
  {
    update_id: 1001,
    message: { message_id: 1, from: { id: 10, is_bot: false }, text: "Hello" },
  },
  {
    update_id: 1002,
    message: { message_id: 2, from: { id: 20, is_bot: true }, text: "Hi there" },
  },
  {
    update_id: 1003,
    message_reaction: { message_id: 1, reaction: [{ type: "emoji", emoji: "👍" }] },
  },
]
