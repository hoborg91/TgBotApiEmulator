import createClient from "openapi-fetch";
import type { paths } from "../contracts/emulator-api.g";

export function createApiClient() {
  return createClient<paths>({ baseUrl: "http://localhost:5134" });
}
