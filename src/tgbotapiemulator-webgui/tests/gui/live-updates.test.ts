import { test, expect, type Page } from "@playwright/test"
import type { MockUpdate } from "../fixtures/updates"
import { MOCK_SESSIONS } from "../fixtures/sessions"
import { MOCK_UPDATES } from "../fixtures/updates"
import { mockSignalR } from "../mockBackend"

declare global {
  interface Window {
    __tgTestInvokeNewUpdate?: () => void
  }
}

const BACKEND_URL = "http://localhost:5134"

// 4th update used to simulate a new live update arriving from the bot.
const LIVE_UPDATE: MockUpdate = {
  update_id: 1004,
  message: { message_id: 3, from: { id: 30, is_bot: true }, text: "New live update!" },
}

// mockAllEndpoints installs route mocks. updateCount is read via a getter so
// that changes to the variable in the test are visible to the route handler.
async function mockAllEndpoints(page: Page, getUpdateCount: () => number) {
  await page.route(`${BACKEND_URL}/test/sessions`, async (route) => {
    await route.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify(MOCK_SESSIONS),
    })
  })

  await page.route(`${BACKEND_URL}/test/sessions/1/name`, async (route) => {
    await route.fulfill({ status: 200, body: "" })
  })

  await page.route(`${BACKEND_URL}/test/sessions/1/updates`, async (route) => {
    const count = getUpdateCount()
    await route.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify([...MOCK_UPDATES.slice(0, count - 1), LIVE_UPDATE].slice(0, count)),
    })
  })

  await page.route(`${BACKEND_URL}/test/sessions/1/postman-collection`, async (route) => {
    await route.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify({ item: [] }),
    })
  })

  await mockSignalR(page)
}

async function selectSession(page: Page) {
  await page.getByText("Session #1").click()
  await expect(page.getByRole("heading", { name: "Session #1" })).toBeVisible()
}

async function invokeNewUpdate(page: Page) {
  await page.evaluate(() => window.__tgTestInvokeNewUpdate?.())
}

test("USC-2.1: new update appended but not checked when offset not at end", async ({
  page,
}) => {
  let updateCount = 3
  await mockAllEndpoints(page, () => updateCount)

  await page.goto("/", { waitUntil: "networkidle" })

  await selectSession(page)

  await page.getByRole("button", { name: "Forward" }).click()
  await expect(page.getByText("Offset 1 of 3")).toBeVisible()

  updateCount = 4
  const updatesResponse = page.waitForResponse(`${BACKEND_URL}/test/sessions/1/updates`)
  await invokeNewUpdate(page)
  await updatesResponse

  await expect(page.getByText("Offset 1 of 4")).toBeVisible()
  await expect(page.getByRole("checkbox", { name: "Update 4" })).toBeVisible()
  await expect(page.getByRole("checkbox", { name: "Update 4" })).not.toBeChecked()
  await expect(page.getByRole("checkbox", { name: "Update 1" })).toBeChecked()
})

test("USC-2.2: new update checked and visible when all updates were checked", async ({
  page,
}) => {
  let updateCount = 3
  await mockAllEndpoints(page, () => updateCount)

  await page.goto("/", { waitUntil: "networkidle" })

  await selectSession(page)

  await page.getByRole("button", { name: "End" }).click()
  await expect(page.getByText("Offset 3 of 3")).toBeVisible()

  updateCount = 4
  const updatesResponse = page.waitForResponse(`${BACKEND_URL}/test/sessions/1/updates`)
  await invokeNewUpdate(page)
  await updatesResponse

  await expect(page.getByText("Offset 4 of 4")).toBeVisible()
  await expect(page.getByRole("checkbox", { name: "Update 4" })).toBeChecked()
  await expect(page.getByText("New live update!", { exact: true })).toBeVisible()
})

test("scroll preserved when not at bottom before live update", async ({ page }) => {
  let updateCount = 3
  await mockAllEndpoints(page, () => updateCount)

  await page.goto("/", { waitUntil: "networkidle" })

  await selectSession(page)

  await page.getByRole("button", { name: "Forward" }).click()
  await expect(page.getByText("Offset 1 of 3")).toBeVisible()

  updateCount = 4
  const updatesResponse = page.waitForResponse(`${BACKEND_URL}/test/sessions/1/updates`)
  await invokeNewUpdate(page)
  await updatesResponse

  await expect(page.getByText("Offset 1 of 4")).toBeVisible()
})
