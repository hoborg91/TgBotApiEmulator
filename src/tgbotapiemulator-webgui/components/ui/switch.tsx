"use client"

import * as React from "react"
import * as CheckboxPrimitive from "@radix-ui/react-checkbox"

import { cn } from "@/lib/utils"

const Switch = React.forwardRef<
  React.ElementRef<typeof CheckboxPrimitive.Root>,
  React.ComponentPropsWithoutRef<typeof CheckboxPrimitive.Root>
>(({ className, ...props }, ref) => (
  <CheckboxPrimitive.Root
    ref={ref}
    className={cn(
      "peer h-[22px] w-[38px] shrink-0 rounded-full border border-transparent focus-visible:outline-none disabled:cursor-not-allowed disabled:opacity-50 data-[state=checked]:bg-primary data-[state=unchecked]:bg-input",
      className
    )}
    {...props}
  >
    <CheckboxPrimitive.Indicator
      className={cn(
        "translate-x-[-1px] h-[16px] w-[16px] rounded-full bg-background shadow ring-0 transition-transform data-[state=checked]:translate-x-full"
      )}
    />
  </CheckboxPrimitive.Root>
))
Switch.displayName = "Switch"

export { Switch }
