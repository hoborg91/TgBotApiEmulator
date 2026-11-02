import { test, expect } from "@playwright/test"
import { mockBackendEndpoints } from "./mockBackend"
import { SCREENSHOTS_DIR } from "./screenshotDir"
import { setTheme, screenshotPathForTheme } from "./setTheme"

const THEMES: ("light" | "dark")[] = ["light", "dark"]

test("smoke test: main page renders SessionList and Player", async ({ page }) => {
  await mockBackendEndpoints(page)

  await page.goto("/", { waitUntil: "networkidle" })

  // Two mocked sessions are returned, so the list shows them (not the empty state).
  await expect(page.getByText("Session #1")).toBeVisible()
  await expect(page.getByText("Session #2")).toBeVisible()
  await expect(page.getByText("Open mocked session")).toBeVisible()
  await expect(page.getByText("Select a session to play it back.")).toBeVisible()

  for (const theme of THEMES) {
    await setTheme(page, theme)
    await page.screenshot({
      path: screenshotPathForTheme(
        SCREENSHOTS_DIR,
        "smoke-main-page",
        theme
      ),
      fullPage: true,
    })
  }
})
