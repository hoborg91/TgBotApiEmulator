"use client"

import { useEffect, useMemo, useRef } from "react"
import type { components } from "@/app/contracts/emulator-api.g"
import { tgRenderUserName } from "@/app/utils/tg/all"

type Update = components["schemas"]["Update"]

interface ChatMessage {
  id: number
  sender: string
  text: string
  isBot: boolean
  reactions: string[]
  updateIndex: number
}

function buildMessages(updates: Update[]): {
  messages: ChatMessage[]
  scrollTargets: (number | null)[]
} {
  const messages: ChatMessage[] = []
  const byMessageId = new Map<
    number,
    { message: ChatMessage; updateIndex: number }
  >()
  const scrollTargets: (number | null)[] = []

  for (let i = 0; i < updates.length; i++) {
    const update = updates[i]!
    const message = update.message
    if (message) {
      const sender = message.from
        ? tgRenderUserName(message.from)
        : "Unknown sender"
      const entry: ChatMessage = {
        id: message.message_id ?? 0,
        sender,
        text: message.text ?? "",
        isBot: message.from?.is_bot === true,
        reactions: [],
        updateIndex: i,
      }
      messages.push(entry)
      scrollTargets.push(i)
      if (message.message_id !== undefined) {
        byMessageId.set(message.message_id, { message: entry, updateIndex: i })
      }
      continue
    }

    const reaction = update.message_reaction
    if (reaction) {
      const target = byMessageId.get(reaction.message_id ?? 0)
      if (target) {
        for (const r of reaction.new_reaction ?? []) {
          const emoji = (r as { emoji?: string }).emoji
          if (emoji) {
            target.message.reactions.push(emoji)
          }
        }
      }
      scrollTargets.push(target ? target.updateIndex : null)
      continue
    }

    scrollTargets.push(null)
  }

  return { messages, scrollTargets }
}

interface MessengerScreenProps {
  updates: Update[]
  offset: number | null
}

export function MessengerScreen({ updates, offset }: MessengerScreenProps) {
  const containerRef = useRef<HTMLDivElement>(null)
  const visibleUpdates = updates.slice(0, offset ?? 0)
  const { messages, scrollTargets } = useMemo(
    () => buildMessages(visibleUpdates),
    [visibleUpdates]
  )

  useEffect(() => {
    if (offset === null || offset <= 0 || visibleUpdates.length === 0) {
      return
    }
    const target = scrollTargets[visibleUpdates.length - 1]
    if (target == null) {
      return
    }
    const el = document.getElementById(`msg-${target}`)
    if (el && containerRef.current) {
      el.scrollIntoView({ behavior: "smooth", block: "center" })
    }
  }, [offset, visibleUpdates.length, scrollTargets])

  return (
    <div
      ref={containerRef}
      className="flex-1 overflow-y-auto bg-background shrink-0"
    >
      <div className="max-w-xs mx-auto py-6">
        {messages.map((message) => (
          <div
            key={message.id}
            id={`msg-${message.updateIndex}`}
            className={`mb-4 flex ${message.isBot ? "justify-start" : "justify-end"}`}
          >
            <div
              className={`max-w-[75%] rounded-lg p-2 text-sm leading-relaxed ${
                message.isBot
                  ? "bg-secondary dark:bg-[#111111] text-secondary-foreground"
                  : "bg-primary dark:bg-[#1a1a1a] text-primary-foreground dark:text-white"
              }`}
            >
              <div className="text-xs mb-1 opacity-70">{message.sender}</div>
              <div className="whitespace-pre-wrap">{message.text}</div>
              {message.reactions.length > 0 && (
                <div className="mt-1.5 text-xs opacity-90">
                  {message.reactions.join(" ")}
                </div>
              )}
            </div>
          </div>
        ))}
      </div>
    </div>
  )
}
