import createClient from "openapi-fetch";
import type { paths } from "../contracts/emulator-api.g";

const backendUrl = process.env.BACKEND_INTERNAL_URL || "http://localhost:5134";

export function createApiClient() {
  return createClient<paths>({ baseUrl: backendUrl });
}
