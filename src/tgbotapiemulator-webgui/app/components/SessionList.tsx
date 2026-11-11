"use client"

import { useCallback, useEffect, useState } from "react"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Separator } from "@/components/ui/separator"
import { Plus, X, Save, RefreshCw } from "lucide-react"
import { ChevronLeft, ChevronRight, ChevronDown } from "lucide-react"
import type { TgSession } from "@/app/page"

interface SessionListProps {
  sessions: TgSession[] | null | undefined
  sessionsError: string | null
  isLoading: boolean
  onSelect: (sessionId: number, botTokenHash: string | null) => void
  onSelectMocked: () => void
  onCreateSession: (botTokenHash: string, name: string) => void
  onReload: () => void
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

function tokenLabel(botTokenHash: string | null, botName: string | null): string {
  if (botName) return botName
  if (!botTokenHash) return "Unknown token"
  return `…${botTokenHash.slice(-6)}`
}

export function SessionList({
  sessions,
  sessionsError,
  isLoading,
  onSelect,
  onCreateSession,
  onReload,
  collapsed,
  onToggleCollapse,
  onSelectMocked,
}: SessionListProps) {
  const list = sessions ?? []
  const [creatingToken, setCreatingToken] = useState<string | null>(null)
  const [createName, setCreateName] = useState("")
  const [collapsedGroups, setCollapsedGroups] = useState<Set<string | null>>(
    new Set(),
  )

  useEffect(() => {
    if (typeof window === "undefined") {
      return
    }
    try {
      const raw = localStorage.getItem("tgbot-session-groups-collapsed")
      if (!raw) return
      const arr = JSON.parse(raw)
      if (!Array.isArray(arr)) return
      setCollapsedGroups(
        new Set(arr.map((v) => (v === "null" ? null : (v as string | null)))),
      )
    } catch {
      // Ignore storage quota/private-mode errors.
    }
  }, [])

  useEffect(() => {
    try {
      const arr = [...collapsedGroups].map((v) =>
        v === null ? "null" : v,
      )
      localStorage.setItem("tgbot-session-groups-collapsed", JSON.stringify(arr))
    } catch {
      // Ignore storage quota/private-mode errors.
    }
  }, [collapsedGroups])

  const groups = new Map<string | null, TgSession[]>()
  for (const session of list) {
    const key = session.bot_token_hash ?? null
    const bucket = groups.get(key) ?? []
    bucket.push(session)
    groups.set(key, bucket)
  }

  const toggleGroup = useCallback((botTokenHash: string | null) => {
    setCollapsedGroups((prev) => {
      const next = new Set(prev)
      if (next.has(botTokenHash)) {
        next.delete(botTokenHash)
      } else {
        next.add(botTokenHash)
      }
      return next
    })
  }, [])

  const isGroupCollapsed = useCallback(
    (botTokenHash: string | null) => collapsedGroups.has(botTokenHash),
    [collapsedGroups],
  )

  const startCreate = useCallback((botTokenHash: string) => {
    setCreatingToken(botTokenHash)
    setCreateName("")
  }, [])

  const cancelCreate = useCallback(() => {
    setCreatingToken(null)
    setCreateName("")
  }, [])

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
        <div className="flex items-center gap-1">
          <Button
            variant="ghost"
            size="sm"
            onClick={onReload}
            className="h-8 w-8 p-0"
            title="Reload sessions"
          >
            <RefreshCw className="h-4 w-4" />
          </Button>
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
          {[...groups.entries()].map(([botTokenHash, group]) => {
            const isCreating = creatingToken === botTokenHash
            const groupCollapsed = isGroupCollapsed(botTokenHash)
            const botName = group.find((s) => s.bot_name)?.bot_name ?? null

            return (
              <div
                key={botTokenHash ?? "null"}
                className="mb-2"
              >
                <div className="flex items-center gap-2 p-1.5 rounded-lg border border-transparent hover:border-accent hover:bg-accent/50 transition-colors">
                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={() => toggleGroup(botTokenHash)}
                    className="h-8 w-8 p-0 shrink-0"
                    title={groupCollapsed ? "Expand group" : "Collapse group"}
                  >
                    {groupCollapsed ? (
                      <ChevronRight className="h-4 w-4" />
                    ) : (
                      <ChevronDown className="h-4 w-4" />
                    )}
                  </Button>
                  <div className="flex-1 min-w-0">
                    <div className="text-xs font-medium truncate text-muted-foreground">
                      {tokenLabel(botTokenHash, botName)}
                    </div>
                    <div className="text-xs text-muted-foreground">
                      {group.length} session{group.length !== 1 ? "s" : ""}
                    </div>
                  </div>
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => startCreate(String(botTokenHash))}
                    disabled={isCreating || groupCollapsed}
                    className="h-8 w-8 p-0"
                    title="New session for this bot"
                  >
                    <Plus className="h-4 w-4" />
                  </Button>
                </div>

                {!groupCollapsed && (
                  <>
                    {isCreating ? (
                      <div className="flex items-center gap-2 p-2 rounded-lg border border-accent bg-accent/50">
                        <Input
                          value={createName}
                          onChange={(e) => setCreateName(e.target.value)}
                          placeholder="Session name"
                          className="flex-1 text-sm"
                          autoFocus
                          onKeyDown={(e) => {
                            if (e.key === "Enter") {
                              if (!createName.trim()) return
                              onCreateSession(String(botTokenHash), createName)
                              cancelCreate()
                            }
                            if (e.key === "Escape") cancelCreate()
                          }}
                        />
                        <Button
                          variant="default"
                          size="sm"
                          onClick={() => {
                            if (!createName.trim()) return
                            onCreateSession(String(botTokenHash), createName)
                            cancelCreate()
                          }}
                          disabled={!createName.trim()}
                          className="h-8 w-8 p-0"
                        >
                          <Save className="h-4 w-4" />
                        </Button>
                        <Button
                          variant="outline"
                          size="sm"
                          onClick={cancelCreate}
                          className="h-8 w-8 p-0"
                        >
                          <X className="h-4 w-4" />
                        </Button>
                      </div>
                    ) : null}

                    <ul className="divide-y divide-muted/30">
                      {group.map((session) => {
                        const displayName =
                          session.non_unique_name ?? `Session #${session.id.value}`
                        const isActive = Boolean(session.is_active)

                        return (
                          <li
                            key={session.id.value}
                            className="group flex items-center justify-between gap-2 rounded-lg border border-transparent hover:border-accent hover:bg-accent/50 transition-colors p-2"
                          >
                            <div
                              className="flex-1 min-w-0 cursor-pointer"
                              onClick={() =>
                                onSelect(
                                  session.id.value,
                                  session.bot_token_hash,
                                )
                              }
                            >
                              <div className="flex items-center gap-1">
                                <span className="text-sm font-medium truncate">
                                  {displayName}
                                </span>
                                {isActive && (
                                  <span className="inline-flex items-center rounded-md border border-accent px-1.5 py-0.5 text-xs font-semibold text-accent-foreground">
                                    active
                                  </span>
                                )}
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
                          </li>
                        )
                      })}
                    </ul>
                  </>
                )}
              </div>
            )
          })}
          <Separator className="my-2" />
        </>
      )}

      <Button
        variant="link"
        className="w-full justify-start text-sm mt-2"
        onClick={onSelectMocked}
      >
        Open mocked session
      </Button>
    </div>
  )
}
