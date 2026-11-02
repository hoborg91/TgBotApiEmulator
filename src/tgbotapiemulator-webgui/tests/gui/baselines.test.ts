import { test, expect } from "@playwright/test"
import { mockBackendEndpoints } from "../mockBackend"
import { SCREENSHOTS_DIR } from "../screenshotDir"
import { setTheme, screenshotPathForTheme } from "../setTheme"

const THEMES: ("light" | "dark")[] = ["light", "dark"]

// Deterministic screenshot baselines for DBT-2: automated GUI verification.
// All backend requests are mocked; no .NET backend is required.
//
// Baselines captured (each in light AND dark theme):
//   - empty-page: no session selected
//   - mocked-session-start: mocked session opened, offset = null ("Start")
//   - real-session-offset-1: real session, Forward clicked (offset 1)
//   - real-session-end: real session, End clicked (offset = updates.length = 3)

async function captureInThemes(
  page: Parameters<typeof test>[0]["page"],
  baseName: string,
  setup: () => Promise<void>
) {
  await setup()
  for (const theme of THEMES) {
    await setTheme(page, theme)
    await page.screenshot({
      path: screenshotPathForTheme(SCREENSHOTS_DIR, baseName, theme),
      fullPage: true,
    })
  }
}

test("screenshot baselines: main page states", async ({ page }) => {
  await mockBackendEndpoints(page)
  await page.goto("/", { waitUntil: "networkidle" })

  // --- empty page (no session) ---
  await captureInThemes(page, "baseline-empty-page", async () => {
    await expect(page.getByText("Select a session to play it back.")).toBeVisible()
  })

  // --- mocked session at Start (offset = null) ---
  await captureInThemes(page, "baseline-mocked-session-start", async () => {
    await page.getByRole("button", { name: "Open mocked session" }).click()
    await expect(page.getByText("Mocked session", { exact: true })).toBeVisible()
  })

  // --- real session, then Forward → offset 1 ---
  await captureInThemes(page, "baseline-real-session-offset-1", async () => {
    await page.getByRole("button", { name: "Start" }).click()
    await page.getByText("Session #1").click()
    await expect(page.getByRole("heading", { name: "Session #1" })).toBeVisible()
    await page.getByRole("button", { name: "Forward" }).click()
    await expect(page.getByText("Offset 1 of 3")).toBeVisible()
    await expect(page.getByRole("checkbox", { name: "Update 1" })).toBeChecked()
  })

  // --- real session, End → offset 3 ---
  await captureInThemes(page, "baseline-real-session-end", async () => {
    await page.getByRole("button", { name: "End" }).click()
    await expect(page.getByText("Offset 3 of 3")).toBeVisible()
    await expect(page.getByRole("checkbox", { name: "Update 3" })).toBeChecked()
  })
})
