## How to update types

Given the server is running at http://localhost:5134 and thus OpenAPI specification is available at http://localhost:5134/swagger/v1/swagger.json, one should run the following script from the repository root.

```bash
npx openapi-typescript http://localhost:5134/swagger/v1/swagger.json -o ./src/tgbotapiemulator-webgui/app/contracts/emulator-api.g.d.ts
```

# README generated with the application

This is a [Next.js](https://nextjs.org) project bootstrapped with [`create-next-app`](https://nextjs.org/docs/app/api-reference/cli/create-next-app).

## Getting Started

First, run the development server:

```bash
npm run dev
# or
yarn dev
# or
pnpm dev
# or
bun dev
```

Open [http://localhost:3000](http://localhost:3000) with your browser to see the result.

You can start editing the page by modifying `app/page.tsx`. The page auto-updates as you edit the file.

This project uses [`next/font`](https://nextjs.org/docs/app/building-your-application/optimizing/fonts) to automatically optimize and load [Geist](https://vercel.com/font), a new font family for Vercel.

## Learn More

To learn more about Next.js, take a look at the following resources:

- [Next.js Documentation](https://nextjs.org/docs) - learn about Next.js features and API.
- [Learn Next.js](https://nextjs.org/learn) - an interactive Next.js tutorial.

You can check out [the Next.js GitHub repository](https://github.com/vercel/next.js) - your feedback and contributions are welcome!

## Deploy on Vercel

The easiest way to deploy your Next.js app is to use the [Vercel Platform](https://vercel.com/new?utm_medium=default-template&filter=next.js&utm_source=create-next-app&utm_campaign=create-next-app-readme) from the creators of Next.js.

Check out our [Next.js deployment documentation](https://nextjs.org/docs/app/building-your-application/deploying) for more details.

## Automated GUI testing (DBT-2)

Screenshot-based GUI tests run with Playwright and mock every backend request, so no running .NET backend is required.

1. Start the WebGUI dev server: `npm run dev` (http://localhost:3000).
2. Run the GUI tests: `npm run test:gui` (headless) or `npm run test:gui:headed` (headed browser).

Screenshots are captured on each run and committed baselines live under `tests/gui/snapshots/`. Add new tests under `tests/gui/` and mock backend endpoints via `tests/mockBackend.ts`.
