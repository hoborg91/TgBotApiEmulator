# AGENTS.md

## Commands

- Run the emulator: `dotnet run --project src/TgBotApiEmulator` (http://localhost:5134, Swagger at `/swagger`).
- Build/test everything in the .NET solution: `dotnet build src/TgBotApiEmulator.sln`, `dotnet test src/TgBotApiEmulator.sln`.
- Run one test class: `dotnet test src/TgBotApiEmulator.sln --filter FullyQualifiedName~BotControllerGetUpdatesTests`.
- Run the sample bot: `$env:TOKEN='<token>'; dotnet run --project src/TgBotSample -- http://localhost:5134`.
- GUI: from `src/tgbotapiemulator-webgui`, use `npm run dev` (port 3000), `npm run lint`, and `npm run build`.

## Layout and runtime

- `src/TgBotApiEmulator` is the ASP.NET Core app; `Program.cs` is the composition root.
- `RecordingProxyMiddleware` intercepts `/bot<token>/<method>` traffic before MVC. In `RecordingProxy` mode it forwards to `api.telegram.org`; controllers handle requests only in `Emulator` mode. Operating mode is stored per bot-token hash.
- `src/TgBotApiEmulator.Tests` is NUnit + Moq, uses in-memory SQLite, and can access internals via `InternalsVisibleTo`.
- `src/TgBotSample` is not part of `TgBotApiEmulator.sln`; build/run it explicitly when changing it.
- `src/tgbotapiemulator-webgui` is a separate Next.js app, not part of the .NET solution.
- Persistence is EF Core SQLite with `EnsureCreated`, not migrations. Schema changes require deleting the local `*.db` file; do not add migrations unless asked.
- Behavior specs live in `docs/Req.md`, user stories in `docs/user-stories`, and bug analyses in `docs/bugs`.

## Gotchas

- Both main and test projects use `TreatWarningsAsErrors` and `EnforceCodeStyleInBuild`. `src/.editorconfig` enforces `var` everywhere (IDE0007 is an error), `_camelCase` instance fields, and `s_` static fields.
- Strongly typed ID comparison operators (`TgSessionRecordId`, `UpdateOffset`) intentionally throw. Compare `.Value` in normal code; if EF cannot translate an expression, materialize with `AsEnumerable()` before filtering/ordering.
- Update queue identity is the update record primary key. `TgSessionUpdateRecord.UpdateId` is the real Telegram ID and is null for locally produced updates; `IsProducedByBot` keeps bot-generated updates out of `getUpdates` while leaving them visible to `TestController` and `AssertController`.
- `UpdateReader` with a zero timeout polls forever. Tests use the `IClock` mock and timeouts greater than 100 ms because smaller nonzero timeouts are rejected.
- `src/tgbotapiemulator-webgui/app/contracts/emulator-api.g.d.ts` is generated from live Swagger. With the backend running, regenerate it using the `openapi-typescript` command in that folder's `README.md`; do not edit it by hand.
