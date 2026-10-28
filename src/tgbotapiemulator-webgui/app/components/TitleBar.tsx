"use client"

import { ThemeToggle } from "@/app/components/ThemeToggle"

export function TitleBar() {
  return (
    <div className="flex items-center justify-between px-4 py-2 border-b bg-muted/50 shrink-0">
      <div className="text-sm font-semibold tracking-tight">
        Telegram Bot API Emulator
      </div>
      <div className="flex items-center gap-2">
        <ThemeToggle />
        {/* Placeholder for future settings / account button */}
      </div>
    </div>
  )
}
