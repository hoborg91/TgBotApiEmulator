import type { TgSession } from "@/app/page"

// Fixed, deterministic sessions used across GUI tests.
// Values are chosen so UpdateBrowser navigation (offset 1..length) is easy to assert.
export const MOCK_SESSIONS: TgSession[] = [
  {
    id: { value: 1 },
    non_unique_name: "Session #1",
    dtm_created: "2026-01-01T10:00:00Z",
    dtm_last_updated: "2026-01-01T10:05:00Z",
  },
  {
    id: { value: 2 },
    non_unique_name: null,
    dtm_created: "2026-01-02T11:00:00Z",
    dtm_last_updated: "2026-01-02T11:00:00Z",
  },
]
