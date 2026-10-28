"use client"

import { useCallback, useEffect, useState } from "react"
import { createApiClient } from "@/app/utils/api"
import type { components } from "@/app/contracts/emulator-api.g"
import { GetUpdateMocks } from "@/app/player-mocks/UpdateMock"
import { MessengerScreen } from "@/app/components/Player/MessengerScreen"
import { UpdateBrowser } from "@/app/components/Player/UpdateBrowser"
import { Input } from "@/components/ui/input"
import { Button } from "@/components/ui/button"
import { Edit2, Save, X } from "lucide-react"

type Update = components["schemas"]["Update"]

type Session =
  | {
      kind: "real"
      sessionId: number
      sessionName: string
      dtm_created: string
      dtm_last_updated: string
    }
  | { kind: "mocked" }

interface PlayerProps {
  session: Session | null
  onRename?: (sessionId: number, newName: string) => void
}

function formatDateTime(value: string): string {
  return new Date(value).toLocaleString(undefined, {
    year: "numeric",
    month: "short",
    day: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  })
}

interface RenameSessionRequest {
  name: string
}

export function Player({ session, onRename }: PlayerProps) {
  const [updates, setUpdates] = useState<Update[] | null | undefined>(
    undefined
  )
  const [offset, setOffset] = useState<number | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [isRenaming, setIsRenaming] = useState(false)
  const [newName, setNewName] = useState("")

  const loadUpdates = useCallback(async () => {
    setError(null)
    setOffset(null)

    if (session?.kind === "mocked") {
      setUpdates(GetUpdateMocks())
      return
    }

    if (!session) {
      setUpdates([])
      return
    }

    const client = createApiClient()
    try {
      const { data, error: err } = await client.GET(
        "/test/sessions/{sessionId}/updates",
        { params: { path: { sessionId: session.sessionId } } }
      )
      if (err) throw err
      setUpdates((data ?? []) as Update[])
    } catch {
      setUpdates(null)
      setError("Failed to load updates.")
    }
  }, [session])

  useEffect(() => {
    loadUpdates()
  }, [session, loadUpdates])

  const sessionName =
    session?.kind === "mocked"
      ? "Mocked session"
      : session?.kind === "real"
        ? session.sessionName
        : null

  const startRename = useCallback(() => {
    setNewName(sessionName ?? "")
    setIsRenaming(true)
  }, [sessionName])

  const cancelRename = useCallback(() => {
    setIsRenaming(false)
    setNewName("")
  }, [])

  const saveRename = useCallback(async () => {
    if (!newName.trim() || session?.kind !== "real") return

    const client = createApiClient()
    const { error: err } = await client.PATCH(
      "/test/sessions/{sessionId}/name",
      {
        params: { path: { sessionId: session.sessionId } },
        body: { name: newName.trim() } as RenameSessionRequest,
      },
    )

    if (!err) {
      onRename?.(session.sessionId, newName.trim())
      setIsRenaming(false)
      setNewName("")
    }
  }, [newName, session, onRename])

  const headerContent = session?.kind === "real" ? (
    <>
      <h1 className="text-sm font-semibold truncate">
        {session.sessionName}
      </h1>
      <div className="text-xs text-muted-foreground mt-0.5">
        Started {formatDateTime(session.dtm_created)}
        {session.dtm_last_updated && (
          <>
            &nbsp;·&nbsp; Ended {formatDateTime(session.dtm_last_updated)}
          </>
        )}
      </div>
      <div className="mt-2 flex items-center gap-2">
        <Button
          variant="ghost"
          size="sm"
          onClick={startRename}
          className="h-8 w-8 p-0 opacity-0 group-hover:opacity-100 transition-opacity"
          title="Rename session"
        >
          <Edit2 className="h-4 w-4" />
        </Button>
        {isRenaming ? (
          <>
            <Input
              value={newName}
              onChange={(e) => setNewName(e.target.value)}
              onKeyDown={(e) => {
                if (e.key === "Enter") saveRename()
                if (e.key === "Escape") cancelRename()
              }}
              className="flex-1 text-sm border border-accent"
              autoFocus
            />
            <Button
              variant="default"
              size="sm"
              onClick={saveRename}
              disabled={!newName.trim()}
              className="h-8 w-8 p-0"
            >
              <Save className="h-4 w-4" />
            </Button>
            <Button
              variant="outline"
              size="sm"
              onClick={cancelRename}
              className="h-8 w-8 p-0"
            >
              <X className="h-4 w-4" />
            </Button>
          </>
        ) : null}
      </div>
    </>
  ) : (
    <p className="text-sm text-muted-foreground">Mocked session</p>
  )

  return (
    <div className="flex-1 flex flex-col min-w-0">
      <header
        className={`border-b px-4 py-3 bg-secondary/50 shrink-0 group ${
          session ? "cursor-default" : ""
        }`}
      >
        {session ? headerContent : (
          <p className="text-sm text-muted-foreground">Select a session</p>
        )}
      </header>
      {session ? (
          <div className="flex-1 flex overflow-hidden min-w-0">
            {updates === null ? (
              <div className="flex-1 flex flex-col items-center justify-center text-destructive text-sm p-4">
                {error ?? "Failed to load updates."}
              </div>
            ) : (
              <MessengerScreen updates={updates ?? []} offset={offset} />
            )}
            <UpdateBrowser
              updates={updates ?? []}
              offset={offset}
              onChangeOffset={setOffset}
            />
          </div>
        ) : (
        <div className="flex-1 flex items-center justify-center text-muted-foreground">
          Select a session to play it back.
        </div>
      )}
    </div>
  )
}
