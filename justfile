run-as-docker:
    docker compose -f ./src/docker-compose.yml up

run-server:
    dotnet run --project ./src/TgBotApiEmulator/TgBotApiEmulator.csproj

run-webgui:
    npm --prefix ./src/tgbotapiemulator-webgui run dev

remove-database:
    rm ./src/TgBotApiEmulator/TgBotApiEmulator.TgBotSession.db

turn-retranslation-off $TOKEN:
    curl -X 'PUT' 'http://localhost:5134/admin/operatingMode/{{TOKEN}}/Emulator'

turn-retranslation-on $TOKEN:
    curl -X 'PUT' 'http://localhost:5134/admin/operatingMode/{{TOKEN}}/RecordingProxy'

run-sample-bot $TOKEN:
    dotnet run --project ./src/TgBotSample/TgBotSample.csproj -- http://localhost:5134

download-postman-collection $TOKEN:
    curl -X 'GET' 'http://localhost:5134/test/bot/{{TOKEN}}/postman-collection' -o regressionTest.json

run-postman-collection $TOKEN:
    postman collection run regressionTest.json --env-var 'emulatorUrl=http://localhost:5134' --env-var 'botToken={{TOKEN}}'

screenshot:
    npm --prefix ./src/tgbotapiemulator-webgui run test:gui

update-contracts:
    npx openapi-typescript http://localhost:5134/swagger/v1/swagger.json -o ./src/tgbotapiemulator-webgui/app/contracts/emulator-api.g.d.ts

test:
    dotnet test ./src/TgBotApiEmulator.Tests/TgBotApiEmulator.Tests.csproj
    npm --prefix ./src/tgbotapiemulator-webgui run test:gui
