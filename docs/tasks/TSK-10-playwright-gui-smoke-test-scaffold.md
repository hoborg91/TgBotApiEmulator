# TSK-10 Add Playwright and GUI smoke test scaffold

Add Playwright as a devDependency to the WebGUI with a smoke test that loads `http://localhost:3000`, asserts SessionList + Player render, and takes a screenshot. All backend requests are mocked so no running .NET backend is required. Add `test:gui` / `test:gui:headed` npm scripts and gitignore the Playwright report/results/screenshot dirs.

**This addresses DBT-2 Excess manual work in GUI testing:**
> "There should be some automated utility (test), which runs web GUI, performs some actions (e.g., clicks) and take screenshots."

## Acceptance criteria

- `@playwright/test` added to `src/tgbotapiemulator-webgui/package.json` devDependencies.
- `playwright.config.ts` created with `baseURL: http://localhost:3000`.
- A smoke test navigates to the root page, asserts SessionList + Player are visible, and captures a screenshot.
- `npm run test:gui` and `npm run test:gui:headed` scripts exist and run against `npm run dev` (no .NET backend running).
- `src/tgbotapiemulator-webgui/.gitignore` ignores `playwright-report/`, `test-results/`, `screenshots/`.

## Affected files

- `src/tgbotapiemulator-webgui/package.json`
- `src/tgbotapiemulator-webgui/playwright.config.ts` (new)
- `src/tgbotapiemulator-webgui/tests/` (new smoke test)
- `src/tgbotapiemulator-webgui/.gitignore`

## References

- docs/debt/DBT-2 Excess manual work in GUI testing.md
- skill: tgbotapiemulator — reference/webgui.md (main page structure)
