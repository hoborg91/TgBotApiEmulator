import type { paths, components } from "../contracts/emulator-api.g";
import createClient from "openapi-fetch";
import Player from "../components/Player/Player";
import { check, Req } from "../utils/types/Req";
import { GetUpdateMocks } from "./UpdateMock";

export default async function Page() {
  
  const client = createClient<paths>({ baseUrl: "http://localhost:5134" });

  // const { data, error } = await client.GET("/test/sessions/{sessionId}/updates", { params: { path: { sessionId: 1 }}});
  // if (error) {
  //  return <div>Error</div>;
  // }

  // const updates = check(data);

  const updates = GetUpdateMocks();

  return (
    <Player updates={updates} />
  );
}
