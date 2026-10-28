"use client"

import { useCallback } from "react"
import type { components } from "@/app/contracts/emulator-api.g"
import { tgRenderUserName } from "@/app/utils/tg/all"
import { Checkbox } from "@/components/ui/checkbox"
import { Separator } from "@/components/ui/separator"
import { Button } from "@/components/ui/button"
import { ScrollArea } from "@/components/ui/scroll-area"

type Update = components["schemas"]["Update"]

interface UpdateBrowserProps {
  updates: Update[]
  offset: number | null
  onChangeOffset: (next: number | null) => void
}

function updateSender(update: Update): string {
  if (update.message?.from) {
    return tgRenderUserName(update.message.from)
  }
  if (update.message_reaction?.user) {
    return tgRenderUserName(update.message_reaction.user)
  }
  return "Unknown sender"
}

function updateBrief(update: Update, index: number): string {
  const number = index + 1
  if (update.message) {
    const text = update.message.text ?? ""
    return `${number}. ${updateSender(update)}: ${text}`.slice(0, 60)
  }
  if (update.message_reaction) {
    const emoji =
      (update.message_reaction.new_reaction?.[0] as { emoji?: string })
        ?.emoji ?? ""
    return `${number}. ${updateSender(update)} reacted ${emoji} to message ${update.message_reaction.message_id ?? "?"}`
  }
  return `${number}. update`
}

export function UpdateBrowser({
  updates,
  offset,
  onChangeOffset,
}: UpdateBrowserProps) {
  const backward = useCallback(() => {
    // Erase everything (acts as "Start") when already at the first update or at Start.
    onChangeOffset(offset === null || offset <= 1 ? null : offset - 1)
  }, [offset, onChangeOffset])

  const forward = useCallback(() => {
    onChangeOffset(
      offset === null ? 1 : Math.min(offset + 1, updates.length)
    )
  }, [offset, updates.length, onChangeOffset])

  const start = useCallback(() => {
    onChangeOffset(null)
  }, [onChangeOffset])

  const end = useCallback(() => {
    onChangeOffset(updates.length > 0 ? updates.length : null)
  }, [updates.length, onChangeOffset])

  return (
    <div className="w-96 border-l bg-muted/50 flex flex-col shrink-0">
      <div className="p-4 border-b shrink-0">
        <div className="text-sm font-semibold mb-1">
          {offset === null ? "Start" : `Offset ${offset} of ${updates.length}`}
        </div>
        <div className="flex gap-2 flex-wrap">
          <Button variant="outline" size="sm" onClick={start}>
            Start
          </Button>
          <Button
            variant="outline"
            size="sm"
            onClick={backward}
            disabled={updates.length === 0}
          >
            Backward
          </Button>
          <Button
            variant="outline"
            size="sm"
            onClick={forward}
            disabled={updates.length === 0}
          >
            Forward
          </Button>
          <Button variant="outline" size="sm" onClick={end}>
            End
          </Button>
        </div>
      </div>
      <ScrollArea className="flex-1">
        <div className="p-2 space-y-2">
          <Separator className="my-2" />
          {updates.map((update, index) => {
            const number = index + 1
            const checked = offset !== null && offset >= number
            return (
              <div
                key={number}
                className="flex items-center gap-2 shrink-0"
              >
                <Checkbox
                  checked={checked}
                  onCheckedChange={() => onChangeOffset(number)}
                  aria-label={`Update ${number}`}
                />
                <span
                  className="text-xs cursor-pointer select-none truncate"
                  title={updateBrief(update, index)}
                  onClick={() => onChangeOffset(number)}
                >
                  {updateBrief(update, index)}
                </span>
              </div>
            )
          })}
        </div>
      </ScrollArea>
    </div>
  )
}
