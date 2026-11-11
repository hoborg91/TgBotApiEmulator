"use client"

import { useCallback, useEffect, useState } from "react"
import { createApiClient, createSessionByTokenAndSetActive } from "@/app/utils/api"
import { SessionList } from "@/app/components/SessionList"
import { Player } from "@/app/components/Player/Player"

export type TgSession = {
  id: { value: number }
  non_unique_name: string | null
  dtm_created: string
  dtm_last_updated: string
  bot_token_hash: string | null
  bot_name?: string | null
  is_active?: boolean
}

export type SelectedSession =
  | { kind: "none" }
  | {
      kind: "real"
      sessionId: number
      sessionName: string
      dtm_created: string
      dtm_last_updated: string
      botTokenHash: string | null
      isActive?: boolean
    }
  | { kind: "mocked" }

export default function MainPage() {
  const [sessions, setSessions] = useState<
    TgSession[] | null | undefined
  >(undefined)
  const [sessionsError, setSessionsError] = useState<string | null>(null)
  const [selected, setSelected] = useState<SelectedSession>({
    kind: "none",
  })
  const [sessionListCollapsed, setSessionListCollapsed] = useState(false)

  useEffect(() => {
    if (typeof window === "undefined") {
      return
    }
    try {
      const raw = localStorage.getItem("tgbot-session-list-collapsed")
      setSessionListCollapsed(raw === "true")
    } catch {
      // Ignore storage quota/private-mode errors.
    }
  }, [])

  useEffect(() => {
    try {
      localStorage.setItem(
        "tgbot-session-list-collapsed",
        String(sessionListCollapsed),
      )
    } catch {
      // Ignore storage quota/private-mode errors.
    }
  }, [sessionListCollapsed])

  const loadSessions = useCallback(async () => {
    setSessionsError(null)
    setSessions(undefined)
    const client = createApiClient()
    try {
      const { data, error } = await client.GET("/test/sessions")
      if (error) {
        setSessions(null)
        setSessionsError("Loading sessions failed")
        return
      }
      setSessions((data ?? []) as TgSession[])
    } catch {
      setSessions(null)
      setSessionsError("Loading sessions failed")
    }
  }, [])

  useEffect(() => {
    loadSessions()
  }, [loadSessions])

  const handleSelect = useCallback(
    async (sessionId: number) => {
      const session = sessions?.find((s) => s.id.value === sessionId)
      if (!session) return

      setSelected({
        kind: "real",
        sessionId,
        sessionName:
          session.non_unique_name ?? `Session #${session.id.value}`,
        dtm_created: session.dtm_created,
        dtm_last_updated: session.dtm_last_updated,
        botTokenHash: session.bot_token_hash,
        isActive: session.is_active,
      })
      setSessionsError(null)
    },
    [sessions],
  )

  const handleSelectMocked = useCallback(() => {
    setSelected({ kind: "mocked" })
    setSessionsError(null)
  }, [])

  const handleCreateSession = useCallback(
    async (botTokenHash: string, name: string) => {
      const client = createApiClient()
      const { data, error } = await createSessionByTokenAndSetActive(client, botTokenHash, name)
      if (error) {
        setSessionsError("Failed to create session")
        return
      }
      const created = data as TgSession
      setSessions((prev) => {
        if (!prev) return [{ ...created, is_active: true }]
        return [...prev.map((s) => ({ ...s, is_active: false })), { ...created, is_active: true }]
      })
    },
    [],
  )

  const handleSetActive = useCallback(
    (sessionId: number, botTokenHash: string | null) => {
      setSessions((prev) => {
        if (!prev) return prev
        return prev.map((s) => ({
          ...s,
          is_active: s.id.value === sessionId && s.bot_token_hash === botTokenHash,
        }))
      })
    },
    [],
  )

  const handleRename = useCallback(
    (sessionId: number, newName: string) => {
      setSessions((prev) => {
        if (!prev) return prev
        return prev.map((s) =>
          s.id.value === sessionId
            ? { ...s, non_unique_name: newName }
            : s
        )
      })
      setSelected((prev) => {
        if (prev.kind === "real" && prev.sessionId === sessionId) {
          return { ...prev, sessionName: newName }
        }
        return prev
      })
    },
    []
  )

  const handleDeleteSession = useCallback(
    async (sessionId: number) => {
      const client = createApiClient()
      const { error } = await client.DELETE("/test/sessions/{sessionId}", {
        params: { path: { sessionId } },
      })
      if (error) {
        setSessionsError("Failed to delete session")
        return
      }

      setSessions((prev) => {
        if (!prev) return prev
        return prev.filter((s) => s.id.value !== sessionId)
      })

      setSelected((prev) => {
        if (prev.kind === "real" && prev.sessionId === sessionId) {
          return { kind: "none" }
        }
        return prev
      })

      setSessionsError(null)
    },
    [],
  )

  const isLoading = sessions === undefined

  return (
    <div className="flex flex-1 overflow-hidden">
      <SessionList
        sessions={sessions}
        sessionsError={sessionsError}
        isLoading={isLoading}
        onSelect={handleSelect}
        onSelectMocked={handleSelectMocked}
        onCreateSession={handleCreateSession}
        onReload={loadSessions}
        collapsed={sessionListCollapsed}
        onToggleCollapse={setSessionListCollapsed}
      />
      <Player
        session={selected.kind === "none" ? null : selected}
        onRename={handleRename}
        onDeleteSession={handleDeleteSession}
        onSetActive={handleSetActive}
      />
    </div>
  )
}
