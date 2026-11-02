# TSK-11 Mock all WebGUI backend requests

Add MSW (or Playwright route mocking) to stub every endpoint the WebGUI calls so GUI tests run without a live .NET backend. Centralize mock data fixtures shared across tests. Verify tests make zero real backend calls.

**This addresses DBT-2 Excess manual work in GUI testing:** removes dependence on a running backend for GUI verification, making tests faster and less fragile.

## Acceptance criteria

- Mock handlers exist for all WebGUI-called endpoints: `GET /test/sessions`, `GET /test/sessions/{sessionId}/updates`, `GET /test/sessions/{sessionId}/postman-collection`, and any other endpoint used.
- Mock data fixtures live in `src/tgbotapiemulator-webgui/tests/fixtures/` and are shared across GUI tests.
- Each GUI test asserts (via network spy) that no real backend request is made.
- Tests run with only `npm run dev` on port 3000; no .NET backend on 5134.

## Affected files

- `src/tgbotapiemulator-webgui/tests/fixtures/` (new)
- `src/tgbotapiemulator-webgui/tests/` mock setup (new or co-located with smoke test)

## References

- docs/debt/DBT-2 Excess manual work in GUI testing.md
- docs/tasks/TSK-10-playwright-gui-smoke-test-scaffold.md
- skill: tgbotapiemulator — reference/webgui.md (API client baseUrl, endpoints used)
