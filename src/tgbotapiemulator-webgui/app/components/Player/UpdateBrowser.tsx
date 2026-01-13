"use client"

import { Dispatch, SetStateAction } from "react";
import type { components } from "../../contracts/emulator-api.g";
import { Req } from "../../utils/types/Req";
import { Button } from "@/components/ui/button";
import { Item } from "@/components/ui/item";
import { ScrollArea } from "@/components/ui/scroll-area";
import React from "react";
import { Checkbox } from "@/components/ui/checkbox";
import { Label } from "@/components/ui/label";
import { createPositiveNumber, PositiveNumber } from "../../utils/types/opaqueTypes";
import { Separator } from "@/components/ui/separator";
import { isEmoji, tgRenderActorName, tgRenderSenderName } from "@/app/utils/tg/all";

interface ChatProps {
  updates: Req<components["schemas"]["Update"]>[];
  currentOffset: number | null;
  setCurrentOffset: Dispatch<SetStateAction<number | null>>,
}

export default function UpdateBrowser({ updates, currentOffset: offset, setCurrentOffset: setOffset }: ChatProps) {
  return (
    <div>
        <div>
            <Item>{offset === null ? "No offset selected" : `Offset ${offset}`}</Item>
            <Button variant="outline" onClick={() => setOffset(prev => prev === null || prev <= 0 ? null : prev - 1)}>Backward</Button>
            <Button variant="outline" onClick={() => setOffset(prev => prev === null ? 0 : (prev < updates.length ? prev + 1 : prev))}>Forward</Button>
        </div>
        <ScrollArea className="h-72 w-48 rounded-md border">
        <div className="p-4">
            <HistoryItem itemOffset={null}>Start</HistoryItem>
            <Separator className="my-2" />
            {updates.map(update => (
            <React.Fragment key={update.update_id}>
                <HistoryItem itemOffset={update.update_id}>{ToLabel(update)}</HistoryItem>
                <Separator className="my-2" />
            </React.Fragment>
            ))}
        </div>
        </ScrollArea>
    </div>
  );

  function HistoryItem({ itemOffset, children } : { itemOffset: number | null, children: React.ReactNode }) {
    return <div className="flex items-center gap-3">
      <Checkbox
        id={`update-checkbox-${itemOffset}`}
        checked={itemOffset == null ? true : offset !== null && offset >= itemOffset}
        onClick={() => setOffset(itemOffset)} />
      <Label
        htmlFor={`update-checkbox-${itemOffset}`}
        className="font-normal">{children}</Label>
    </div>;
  }
}

function Trim(text: string, symbolCount: PositiveNumber) {
    if (text.length <= symbolCount) {
        return text;
    }

    let length = symbolCount;
    while (length > 0 && text[length - 1] === " ") {
        length--;
    }

    return text.substring(0, length) + "…";
}

function ToLabel(update: components["schemas"]["Update"]) {
    if (update.message) {
        const message = update.message;
        return <><b>{tgRenderSenderName(message)}</b>: {Trim(message.text || "", createPositiveNumber(50))}</>;
    }

    if (update.message_reaction) {
        const reaction = update.message_reaction;

        if (reaction.new_reaction) {
            return <><b>{tgRenderActorName(reaction)}</b> sets {ReactionText(reaction.new_reaction)}</>
        }

        if (reaction.old_reaction) {
            return <><b>{tgRenderActorName(reaction)}</b> removes {ReactionText(reaction.old_reaction)}</>
        }

        return <><b>{tgRenderActorName(reaction)}</b> updates reactions</>
    }

    return <>Update #{update.update_id}</>;
}

function ReactionText(reactionCollection: components["schemas"]["ReactionType"][]): string {
    const rawEmojis = [] as string[];

    for (const reaction of reactionCollection) {
        if (isEmoji(reaction) && reaction.emoji) {
            rawEmojis.push(reaction.emoji);
        } else {
            return `${reactionCollection.length} reaction(s)`;
        }
    }

    return rawEmojis.length === 0
        ? `nothing`
        : rawEmojis.reduce((a, b) => `${a}${b}`);
}
