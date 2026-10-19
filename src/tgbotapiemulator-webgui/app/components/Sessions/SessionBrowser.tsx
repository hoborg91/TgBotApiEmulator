"use client"

import React from "react";
import Link from "next/link";
import type { components } from "../../contracts/emulator-api.g";
import { Req } from "../../utils/types/Req";
import { Item } from "@/components/ui/item";
import { ScrollArea } from "@/components/ui/scroll-area";
import { Separator } from "@/components/ui/separator";

interface SessionBrowserProps {
  sessions: Req<components["schemas"]["TgSession"]>[];
}

export default function SessionBrowser({ sessions }: SessionBrowserProps) {
  return (
    <div>
      <Item>Sessions</Item>
      {sessions.length === 0
        ? (
          <div>
            <Item variant="muted">No sessions recorded yet.</Item>
            <Item asChild size="sm">
              <Link href="/player">Open mocked session</Link>
            </Item>
          </div>
        )
        : (
          <ScrollArea className="h-96 w-96 rounded-md border">
            <div className="p-4">
              {sessions.map(session => (
                <React.Fragment key={session.id.value}>
                  <Item asChild size="sm">
                    <Link href={`/player?sessionId=${session.id.value}`}>
                      {session.non_unique_name} (#{session.id.value})
                    </Link>
                  </Item>
                  <Separator className="my-2" />
                </React.Fragment>
              ))}
            </div>
          </ScrollArea>
        )}
    </div>
  );
}
