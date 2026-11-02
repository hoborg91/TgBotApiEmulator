import { test, expect, type Page } from "@playwright/test"
import { mockBackendEndpoints } from "../mockBackend"

// Navigation tests for the UpdateBrowser controls (Start | Backward | Forward | End).
// Uses a real session whose /updates returns 3 deterministic updates (MOCK_UPDATES),
// so offsets 1..3 are stable and assertions are not fragile.
//
// Control semantics (per UpdateBrowser):
//   Start   -> null
//   Backward -> null when offset === null || offset <= 1, else offset - 1
//   Forward  -> 1 when null, else min(offset + 1, updates.length)
//   End      -> updates.length (3) or null when empty

async function selectRealSession(page: Page) {
  await page.getByText("Session #1").click()
  await expect(page.getByRole("heading", { name: "Session #1" })).toBeVisible()
}

test("UpdateBrowser: Start sets offset to null", async ({ page }) => {
  await mockBackendEndpoints(page)
  await page.goto("/", { waitUntil: "networkidle" })
  await selectRealSession(page)
  await page.getByRole("button", { name: "Forward" }).click()
  await expect(page.getByText("Offset 1 of 3")).toBeVisible()
  await page.getByRole("button", { name: "Start" }).click()
  // offset null: no checkbox checked
  await expect(page.getByRole("checkbox", { name: "Update 1" })).not.toBeChecked()
  await expect(page.getByRole("checkbox", { name: "Update 2" })).not.toBeChecked()
  await expect(page.getByRole("checkbox", { name: "Update 3" })).not.toBeChecked()
})

test("UpdateBrowser: Forward from null goes to 1, then increments, clamps at 3", async ({
  page,
}) => {
  await mockBackendEndpoints(page)
  await page.goto("/", { waitUntil: "networkidle" })
  await selectRealSession(page)

  await page.getByRole("button", { name: "Forward" }).click()
  await expect(page.getByText("Offset 1 of 3")).toBeVisible()

  await page.getByRole("button", { name: "Forward" }).click()
  await expect(page.getByText("Offset 2 of 3")).toBeVisible()

  await page.getByRole("button", { name: "Forward" }).click()
  await expect(page.getByText("Offset 3 of 3")).toBeVisible()

  // Clamped: further Forward stays at 3.
  await page.getByRole("button", { name: "Forward" }).click()
  await expect(page.getByText("Offset 3 of 3")).toBeVisible()
})

test("UpdateBrowser: End goes to 3", async ({ page }) => {
  await mockBackendEndpoints(page)
  await page.goto("/", { waitUntil: "networkidle" })
  await selectRealSession(page)

  await page.getByRole("button", { name: "End" }).click()
  await expect(page.getByText("Offset 3 of 3")).toBeVisible()
})

test("UpdateBrowser: Backward decrements and erases at/below 1", async ({
  page,
}) => {
  await mockBackendEndpoints(page)
  await page.goto("/", { waitUntil: "networkidle" })
  await selectRealSession(page)

  await page.getByRole("button", { name: "End" }).click()
  await expect(page.getByText("Offset 3 of 3")).toBeVisible()

  await page.getByRole("button", { name: "Backward" }).click()
  await expect(page.getByText("Offset 2 of 3")).toBeVisible()

  await page.getByRole("button", { name: "Backward" }).click()
  await expect(page.getByText("Offset 1 of 3")).toBeVisible()

  // Backward from 1 erases everything (Start behavior).
  await page.getByRole("button", { name: "Backward" }).click()
  await expect(page.getByRole("checkbox", { name: "Update 1" })).not.toBeChecked()
  await expect(page.getByRole("checkbox", { name: "Update 2" })).not.toBeChecked()
  await expect(page.getByRole("checkbox", { name: "Update 3" })).not.toBeChecked()

  // Backward at null keeps null.
  await page.getByRole("button", { name: "Backward" }).click()
  await expect(page.getByRole("checkbox", { name: "Update 1" })).not.toBeChecked()
})

test("UpdateBrowser: checkboxes track offset", async ({ page }) => {
  await mockBackendEndpoints(page)
  await page.goto("/", { waitUntil: "networkidle" })
  await selectRealSession(page)

  // offset null: nothing checked
  await expect(page.getByRole("checkbox", { name: "Update 1" })).not.toBeChecked()

  await page.getByRole("button", { name: "Forward" }).click()
  await expect(page.getByRole("checkbox", { name: "Update 1" })).toBeChecked()
  await expect(page.getByRole("checkbox", { name: "Update 2" })).not.toBeChecked()

  await page.getByRole("button", { name: "End" }).click()
  await expect(page.getByRole("checkbox", { name: "Update 1" })).toBeChecked()
  await expect(page.getByRole("checkbox", { name: "Update 2" })).toBeChecked()
  await expect(page.getByRole("checkbox", { name: "Update 3" })).toBeChecked()
})
