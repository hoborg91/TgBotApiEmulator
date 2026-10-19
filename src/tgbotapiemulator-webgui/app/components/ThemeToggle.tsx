"use client"

import { useEffect, useState } from "react"

export function ThemeToggle() {
  const [theme, setTheme] = useState<"light" | "dark" | "system">("system")

  const applyTheme = (t: "light" | "dark" | "system") => {
    const html = document.documentElement
    const resolved =
      t === "system"
        ? window.matchMedia("(prefers-color-scheme: dark)").matches
          ? "dark"
          : "light"
        : t
    if (resolved === "dark") {
      html.classList.add("dark")
    } else {
      html.classList.remove("dark")
    }
  }

  useEffect(() => {
    const stored = localStorage.getItem("theme") as
      | "light"
      | "dark"
      | "system"
      | null
    const initial = stored ?? "system"
    setTheme(initial)
    applyTheme(initial)
  }, [])

  const toggle = () => {
    const next =
      theme === "system"
        ? "light"
        : theme === "light"
          ? "dark"
          : "system"
    setTheme(next)
    localStorage.setItem("theme", next)
    applyTheme(next)
  }

  return (
    <button
      onClick={toggle}
      className="px-3 py-1 text-xs border rounded"
      title="Toggle theme"
    >
      {theme === "system" ? "System" : theme === "dark" ? "Dark" : "Light"}
    </button>
  )
}
