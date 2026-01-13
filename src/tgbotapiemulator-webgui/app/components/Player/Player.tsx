"use client"

import styles from "../../page.module.css";
import { useState } from "react";
import type { components } from "../../contracts/emulator-api.g";
import { check, Req } from "../../utils/types/Req";
import { ChatComponent } from "../Chat/ChatComponent";
import { Chat, Contact, Message } from "../Chat/ChatTypes";
import { ChatContainer } from "../Chat/ChatContainer";
import React from "react";
import UpdateBrowser from "./UpdateBrowser";
import { isEmoji, tgRenderSenderName } from "@/app/utils/tg/all";

export default function Player({ updates }: { updates: Req<components["schemas"]["Update"]>[] }) {
  const [currentOffset, setCurrentOffset] = useState(null as number | null);
  
  const messageModels = [] as Message[];
  const dt = new Date(2025, 1, 15, 8, 0, 0);

  if (currentOffset !== null) {
    for (let i = 0; i <= currentOffset; i++) {
      let u = updates[i];

      if (u === undefined || u === null) {
        continue;
      }

      let updateWrapper = check(u);
      if (updateWrapper.message) {
        const msg = updateWrapper.message;
        messageModels.push({
          id: msg.message_id + "",
          text: msg.text || "",
          sender: msg.from.username === "Hamlet" ? 'user' : 'contact',
          from: {
            id: msg.from.id + "",
            name: tgRenderSenderName(msg),
            // avatar: string;
            status: "online",
            //lastSeen?: Date;
          },
          timestamp: new Date(dt.getTime() + updateWrapper.update_id * 60000),
          status: "delivered",
          //isEdited?: boolean;
          //replyTo?: string;
          //reactions?: Reaction[];
        });
      } else if (updateWrapper.message_reaction) {
        const reactionArray = updateWrapper.message_reaction.new_reaction;
        if (reactionArray && reactionArray[0]) {
          const reaction = reactionArray[0];
          if (isEmoji(reaction)) {
            for (const message of messageModels) {
              if (message.id === updateWrapper.message_reaction.message_id + "") {
                const em = (reaction as components["schemas"]["ReactionTypeEmoji"]).emoji || "";
                message.reactions = [
                  {
                    emoji: em,
                    count: 1,
                    users: [updateWrapper.message_reaction.user.id + ""]
                  },
                ];
              }
            }
          }
        }
      }
    }
  }
  
  const chat = {
    id: "hello",
    contact: {
      id: "user1",
      name: "User",
      avatar: "",
      status: "online",
      lastSeen: new Date(),
    } as Contact,
    messages: messageModels,
    unreadCount: 0,
    lastMessage: messageModels.length > 0 ? messageModels[messageModels.length - 1] : null,
  } as Chat;

  return (
    <div className={styles.page}>
      <main className={styles.main} style={{ flexDirection: "row" }}>
        <ChatContainer>
          <ChatComponent
            chat={chat}
            onSendMessage={() => {}}
            onReactionAdd={() => {}}
            onReactionRemove={() => {}}
            currentUserId="user1"
            onBack={() => {}} />
        </ChatContainer>
        <UpdateBrowser
            updates={updates}
            currentOffset={currentOffset}
            setCurrentOffset={setCurrentOffset} />
      </main>
    </div>
  );
}
