import type { components } from "../contracts/emulator-api.g";
import SessionBrowser from "../components/Sessions/SessionBrowser";
import { createApiClient } from "../utils/api";
import { check, Req } from "../utils/types/Req";
import Link from "next/link";

export const dynamic = "force-dynamic";

export default async function Page() {
  const client = createApiClient();

  let sessions: Req<components["schemas"]["TgSession"]>[] = [];
  let loadError: string | null = null;

  try {
    const { data, error } = await client.GET("/test/sessions");
    if (error) {
      loadError = "Failed to load sessions.";
    } else if (data) {
      sessions = data.map(session => check(session));
    }
  } catch (err) {
    loadError = err instanceof Error ? err.message : "Failed to load sessions.";
  }

  if (loadError) {
    return (
      <div>
        <p>{loadError}</p>
        <p>
          <Link href="/player">Open mocked session</Link>
        </p>
      </div>
    );
  }

  return <SessionBrowser sessions={sessions} />;
}
