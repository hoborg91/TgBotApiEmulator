import type { components } from "../contracts/emulator-api.g";
import Player from "../components/Player/Player";
import { check, Req } from "../utils/types/Req";
import { GetUpdateMocks } from "./UpdateMock";
import { createApiClient } from "../utils/api";
import Link from "next/link";

interface PageProps {
  searchParams: Promise<{ sessionId?: string }>;
}

export default async function Page({ searchParams }: PageProps) {
  const { sessionId: sessionIdRaw } = await searchParams;
  const client = createApiClient();

  let updates: Req<components["schemas"]["Update"]>[];

  if (sessionIdRaw === undefined) {
    updates = GetUpdateMocks();
  } else {
    const sessionId = Number(sessionIdRaw);
    if (!Number.isInteger(sessionId)) {
      return <div>Invalid session id: {sessionIdRaw}</div>;
    }

    const { data, error } = await client.GET("/test/sessions/{sessionId}/updates", {
      params: { path: { sessionId } },
    });
    if (error) {
      return <div>Failed to load updates for session {sessionIdRaw}.</div>;
    }

    updates = data.map(update => check(update));
  }

  return (
    <div>
      {sessionIdRaw !== undefined && (
        <p><Link href="/sessions">← Sessions</Link></p>
      )}
      <Player updates={updates} />
    </div>
  );
}
