import createClient from "openapi-fetch";
import type { paths } from "../contracts/emulator-api.g";

const backendUrl = process.env.BACKEND_INTERNAL_URL || "http://localhost:5134";

export function createApiClient() {
  return createClient<paths>({ baseUrl: backendUrl });
}

export async function createSessionByTokenAndSetActive(
  client: ReturnType<typeof createApiClient>,
  botTokenHash: string,
  name: string,
) {
  return client.POST("/test/bot/{botTokenHash}/sessions", {
    params: { path: { botTokenHash } },
    body: { name },
  });
}

export async function setActiveSessionByToken(
  client: ReturnType<typeof createApiClient>,
  botTokenHash: string,
  sessionId: number,
) {
  return client.POST("/test/bot/{botTokenHash}/sessions/{sessionId}/set-active", {
    params: { path: { botTokenHash, sessionId } },
  });
}
