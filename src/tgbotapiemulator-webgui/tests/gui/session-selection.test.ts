import { test, expect } from "@playwright/test"
import { mockBackendEndpoints } from "../mockBackend"

// Session selection tests for the none | mocked | real selection union.
// Verifies the Player renders correctly for each selection and that offset
// resets to null whenever the selected session changes.
//
// Note: the current UI exposes no explicit "deselect back to none" control,
// so "none" is only the initial state. Selection changes (mocked <-> real)
// are covered instead, including offset reset on session switch.

test("Session selection: none (initial) -> mocked -> real", async ({ page }) => {
  await mockBackendEndpoints(page)
  await page.goto("/", { waitUntil: "networkidle" })

  // none: no session selected
  await expect(page.getByText("Select a session to play it back.")).toBeVisible()

  // mocked: click "Open mocked session"
  await page.getByRole("button", { name: "Open mocked session" }).click()
  await expect(page.getByText("Mocked session", { exact: true })).toBeVisible()

  // real: click Session #1
  await page.getByText("Session #1").click()
  await expect(page.getByRole("heading", { name: "Session #1" })).toBeVisible()
})

test("Session selection: offset resets to null when session changes", async ({
  page,
}) => {
  await mockBackendEndpoints(page)
  await page.goto("/", { waitUntil: "networkidle" })

  // Select real session, navigate to offset 2.
  await page.getByText("Session #1").click()
  await expect(page.getByRole("heading", { name: "Session #1" })).toBeVisible()
  await page.getByRole("button", { name: "Forward" }).click()
  await page.getByRole("button", { name: "Forward" }).click()
  await expect(page.getByText("Offset 2 of 3")).toBeVisible()

  // Switch to mocked session -> offset must reset to null (Start).
  await page.getByRole("button", { name: "Open mocked session" }).click()
  await expect(page.getByText("Mocked session", { exact: true })).toBeVisible()
  await expect(
    page.getByRole("checkbox", { name: "Update 1", exact: true })
  ).not.toBeChecked()
  await expect(
    page.getByRole("checkbox", { name: "Update 2", exact: true })
  ).not.toBeChecked()
})
