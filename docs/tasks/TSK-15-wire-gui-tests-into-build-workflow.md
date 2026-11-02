# TSK-15 Wire GUI tests into WebGUI build/lint workflow

Document how to run GUI tests and ensure they pass alongside `npm run lint` and `npm run build`. No .NET backend is required.

**This addresses DBT-2 Excess manual work in GUI testing:** makes automated screenshot testing part of the standard WebGUI verification workflow.

## Acceptance criteria

- `src/tgbotapiemulator-webgui/README.md` (and/or root readme) documents: run `npm run dev` on port 3000, then `npm run test:gui`; no backend needed.
- `npm run lint` passes after adding Playwright/MSW deps and tests.
- `npm run build` passes after adding Playwright/MSW deps and tests.
- `npm run test:gui` passes after `npm run dev` is running on port 3000.

## Affected files

- `src/tgbotapiemulator-webgui/README.md` (workflow documentation)
- `src/tgbotapiemulator-webgui/package.json` (scripts, if not already added)

## References

- docs/debt/DBT-2 Excess manual work in GUI testing.md
- docs/tasks/TSK-10-playwright-gui-smoke-test-scaffold.md
