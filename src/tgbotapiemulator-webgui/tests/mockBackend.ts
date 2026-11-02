import { type Page } from "@playwright/test"
import { MOCK_SESSIONS } from "./fixtures/sessions"
import { MOCK_UPDATES } from "./fixtures/updates"

// Base URL of the .NET backend the WebGUI calls (http://localhost:5134).
// Every request to it is mocked via page.route, so the WebGUI runs without
// a live .NET backend and no real backend request ever reaches the network.
const BACKEND_URL = "http://localhost:5134"

/**
 * Installs Playwright route mocks for all backend endpoints used by the WebGUI.
 * After calling this, the WebGUI can run without a live .NET backend.
 *
 * Mocked endpoints:
 *   GET    /test/sessions
 *   PATCH  /test/sessions/{sessionId}/name
 *   GET    /test/sessions/{sessionId}/updates
 *   GET    /test/sessions/{sessionId}/postman-collection
 *
 * Because every backend endpoint is intercepted by `page.route`, real backend
 * calls cannot reach the network. Tests that use this helper are backend-free.
 *
 * @param page - Playwright page
 * @param sessionId - session id used in path-mocked endpoints (default 1)
 */
export async function mockBackendEndpoints(page: Page, sessionId: number = 1) {
  // Sessions list
  await page.route(`${BACKEND_URL}/test/sessions`, async (route) => {
    await route.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify(MOCK_SESSIONS),
    })
  })

  // Rename session (returns empty 200 OK per contract)
  await page.route(
    `${BACKEND_URL}/test/sessions/${sessionId}/name`,
    async (route) => {
      await route.fulfill({ status: 200, body: "" })
    },
  )

  // Updates for a session
  await page.route(
    `${BACKEND_URL}/test/sessions/${sessionId}/updates`,
    async (route) => {
      await route.fulfill({
        status: 200,
        contentType: "application/json",
        body: JSON.stringify(MOCK_UPDATES),
      })
    },
  )

  // Postman collection download (returns a small JSON blob per contract)
  await page.route(
    `${BACKEND_URL}/test/sessions/${sessionId}/postman-collection`,
    async (route) => {
      await route.fulfill({
        status: 200,
        contentType: "application/json",
        body: JSON.stringify({ item: [] }),
      })
    },
  )
}
