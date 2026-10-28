"use client"

import { useCallback, useState } from "react"
import { createApiClient } from "@/app/utils/api"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Separator } from "@/components/ui/separator"
import { Edit2, Save, X } from "lucide-react"
import { ChevronLeft, ChevronRight } from "lucide-react"
import type { TgSession } from "@/app/page"

type SelectableSession =
  | {
      kind: "real"
      sessionId: number
      sessionName: string
      dtm_created: string
      dtm_last_updated: string
    }
  | { kind: "mocked" }

interface SessionListProps {
  sessions: TgSession[] | null | undefined
  sessionsError: string | null
  isLoading: boolean
  onSelect: (session: SelectableSession) => void
  onRename: (sessionId: number, newName: string) => void
  collapsed: boolean
  onToggleCollapse: (collapsed: boolean) => void
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

export function SessionList({
  sessions,
  sessionsError,
  isLoading,
  onSelect,
  onRename,
  collapsed,
  onToggleCollapse,
}: SessionListProps) {
  const list = sessions ?? []
  const [editingId, setEditingId] = useState<number | null>(null)
  const [editName, setEditName] = useState("")

  const startEdit = useCallback((session: TgSession) => {
    setEditingId(session.id.value)
    setEditName(session.non_unique_name ?? "")
  }, [])

  const cancelEdit = useCallback(() => {
    setEditingId(null)
    setEditName("")
  }, [])

  const saveEdit = useCallback(async (session: TgSession) => {
    if (!editName.trim()) return

    const client = createApiClient()
    const { error } = await client.PATCH(
      "/test/sessions/{sessionId}/name",
      {
        params: { path: { sessionId: session.id.value } },
        body: { name: editName.trim() } as RenameSessionRequest,
      },
    )

    if (!error) {
      onRename(session.id.value, editName.trim())
      setEditingId(null)
      setEditName("")
    }
  }, [editName, onRename])

  return (
    <div
      className={`border-r bg-muted/50 overflow-y-auto flex flex-col gap-2 shrink-0 ${
        collapsed ? "w-24 lg:w-24" : "w-80 lg:w-96"
      } transition-width duration-200`}
    >
      <div className="flex items-center justify-between p-2">
        <h2 className={`text-sm font-semibold ${collapsed ? "hidden" : ""}`}>
          Sessions
        </h2>
        <Button
          variant="ghost"
          size="sm"
          onClick={() => onToggleCollapse(!collapsed)}
          className="h-8 w-8 p-0"
          title={collapsed ? "Expand session list" : "Collapse session list"}
        >
          {collapsed ? (
            <ChevronRight className="h-4 w-4" />
          ) : (
            <ChevronLeft className="h-4 w-4" />
          )}
        </Button>
      </div>

      {sessionsError ? (
        <p className="text-sm text-destructive">{sessionsError}</p>
      ) : isLoading ? (
        <p className="text-sm text-muted-foreground">Loading sessions…</p>
      ) : sessions === null ? (
        <p className="text-sm text-destructive">Failed to load sessions.</p>
      ) : list.length === 0 ? (
        <p className="text-sm text-muted-foreground">
          No sessions recorded yet.
        </p>
      ) : (
        <>
          {list.map((session) => {
            const isEditing = editingId === session.id.value
            const displayName =
              session.non_unique_name ?? `Session #${session.id.value}`

            if (isEditing) {
              return (
                <div
                  key={session.id.value}
                  className="flex items-center gap-2 p-2 rounded-lg border border-accent bg-accent/50"
                >
                  <Input
                    value={editName}
                    onChange={(e) => setEditName(e.target.value)}
                    onKeyDown={(e) => {
                      if (e.key === "Enter") saveEdit(session)
                      if (e.key === "Escape") cancelEdit()
                    }}
                    className="flex-1 text-sm"
                    autoFocus
                  />
                  <Button
                    variant="default"
                    size="sm"
                    onClick={() => saveEdit(session)}
                    disabled={!editName.trim()}
                    className="h-8 w-8 p-0"
                  >
                    <Save className="h-4 w-4" />
                  </Button>
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={cancelEdit}
                    className="h-8 w-8 p-0"
                  >
                    <X className="h-4 w-4" />
                  </Button>
                </div>
              )
            }

            return (
              <div
                key={session.id.value}
                className="group flex items-center justify-between gap-2 rounded-lg border border-transparent hover:border-accent hover:bg-accent/50 transition-colors p-2"
              onClick={() =>
                onSelect({
                  kind: "real",
                  sessionId: session.id.value,
                  sessionName: displayName,
                  dtm_created: session.dtm_created,
                  dtm_last_updated: session.dtm_last_updated,
                })
              }
              >
                <div className="flex-1 min-w-0">
                  <div className="text-sm font-medium truncate">
                    {displayName}
                  </div>
                  <div className="text-xs text-muted-foreground mt-0.5">
                    Started {formatDateTime(session.dtm_created)}
                    {session.dtm_last_updated && (
                      <>
                        &nbsp;·&nbsp; Ended {formatDateTime(
                          session.dtm_last_updated,
                        )}
                      </>
                    )}
                  </div>
                </div>
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={(e) => {
                    e.stopPropagation()
                    startEdit(session)
                  }}
                  className="h-8 w-8 p-0 opacity-0 group-hover:opacity-100 transition-opacity"
                >
                  <Edit2 className="h-4 w-4" />
                </Button>
              </div>
            )
          })}
          <Separator className="my-2" />
        </>
      )}

      <Button
        variant="link"
        className="w-full justify-start text-sm mt-2"
        onClick={() => onSelect({ kind: "mocked" })}
      >
        Open mocked session
      </Button>
    </div>
  )
}
