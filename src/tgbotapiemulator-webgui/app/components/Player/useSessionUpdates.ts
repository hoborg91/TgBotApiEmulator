"use client"

import { useEffect, useRef } from "react"
import { HubConnectionBuilder, HubConnection } from "@microsoft/signalr"

/**
 * Subscribes to SignalR live-update notifications for a session.
 *
 * When a `NewUpdate` message arrives from the server, `onNewUpdate` is invoked.
 * The connection joins the session's SignalR group on connect and leaves it on
 * cleanup. Automatic reconnection is enabled.
 */
declare global {
  interface Window {
    __tgTestInvokeNewUpdate?: () => void
  }
}

export function useSessionUpdates(
  sessionId: number | null,
  onNewUpdate: () => void
) {
  const callbackRef = useRef(onNewUpdate)
  callbackRef.current = onNewUpdate

  if (typeof window !== "undefined") {
    window.__tgTestInvokeNewUpdate = () => {
      callbackRef.current()
    }
  }

  useEffect(() => {
    if (sessionId === null) return

    const backendUrl =
      process.env.BACKEND_INTERNAL_URL || "http://localhost:5134"
    const hubUrl = `${backendUrl}/hubs/session-updates`

    const connection: HubConnection = new HubConnectionBuilder()
      .withUrl(hubUrl)
      .withAutomaticReconnect()
      .build()

    connection.on("NewUpdate", () => {
      callbackRef.current()
    })

    connection
      .start()
      .then(() => connection.invoke("JoinSession", sessionId))
      .catch((err: unknown) => {
        console.error("SignalR connection failed:", err)
      })

    return () => {
      connection.invoke("LeaveSession", sessionId).catch(() => {
        /* Ignore leave errors during cleanup */
      })
      connection.stop()
    }
  }, [sessionId])
}
