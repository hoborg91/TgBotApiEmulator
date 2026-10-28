"use client"

import { useCallback, useEffect, useState } from "react"
import { createApiClient } from "@/app/utils/api"
import { SessionList } from "@/app/components/SessionList"
import { Player } from "@/app/components/Player/Player"

export type TgSession = {
  id: { value: number }
  non_unique_name: string | null
  dtm_created: string
  dtm_last_updated: string
}

export type SelectedSession =
  | { kind: "none" }
  | {
      kind: "real"
      sessionId: number
      sessionName: string
      dtm_created: string
      dtm_last_updated: string
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

  const isLoading = sessions === undefined

  return (
    <div className="flex flex-1 overflow-hidden">
      <SessionList
        sessions={sessions}
        sessionsError={sessionsError}
        isLoading={isLoading}
        onSelect={setSelected}
        onRename={handleRename}
        collapsed={sessionListCollapsed}
        onToggleCollapse={setSessionListCollapsed}
      />
      <Player
        session={selected.kind === "none" ? null : selected}
        onRename={handleRename}
      />
    </div>
  )
}
