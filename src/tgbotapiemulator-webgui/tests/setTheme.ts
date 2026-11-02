import { type Page, type Locator } from "@playwright/test"

// Theme toggle button text reflects current theme state: "System" | "Light" | "Dark".
// Clicking cycles System -> Light -> Dark -> System and sets the "dark" class on <html>.
export const THEME_TOGGLE_SELECTOR = 'button[title="Toggle theme"]'

/**
 * Sets the page theme to "light" or "dark" by clicking the ThemeToggle button
 * until <html> matches the desired state, then returns the theme actually applied.
 *
 * @param page - Playwright page
 * @param theme - "light" or "dark"
 */
export async function setTheme(page: Page, theme: "light" | "dark") {
  const html = page.locator("html")
  const toggle = page.locator(THEME_TOGGLE_SELECTOR)

  const hasDark = async () =>
    (await html.getAttribute("class"))?.split(" ").includes("dark") ?? false

  const wantDark = theme === "dark"
  if ((await hasDark()) !== wantDark) {
    await toggle.click()
  }
  // In the worst case (started at "system" resolving to the unwanted theme),
  // one more click lands on the opposite explicit theme.
  if ((await hasDark()) !== wantDark) {
    await toggle.click()
  }

  if ((await hasDark()) !== wantDark) {
    throw new Error(`Unable to set theme to ${theme}`)
  }

  // The following code (waitForTimeout) resolves bug in screenshot tests.
  // Bug: some areas (namely session list) of screenshots taken in light theme
  // are colored as if they were in dark theme. Thus different themes mixed.
  // Current solution (waitForTimeout) makes tests fragile.
  // It was added because other methods did not work properly.
  // TODO. It should be replaced if some robust solution is found.
  await page.waitForTimeout(100);
}

export function screenshotPathForTheme(
  dir: string,
  baseName: string,
  theme: "light" | "dark"
): string {
  return `${dir}/${baseName}-${theme}.png`
}
