import { components } from "@/app/contracts/emulator-api.g";

export function isEmoji(reaction: components["schemas"]["ReactionType"]): reaction is components["schemas"]["ReactionTypeEmoji"] {
  return (reaction as components["schemas"]["ReactionTypeEmoji"]).emoji !== undefined;
}

export function tgRenderActorName(messageReactionUpdated: components["schemas"]["MessageReactionUpdated"]): string {
    const views = [] as string[];

    if (messageReactionUpdated.user) {
        views.push(tgRenderUserName(messageReactionUpdated.user));
    }

    if (messageReactionUpdated.actor_chat) {
        views.push(tgRenderChatName(messageReactionUpdated.actor_chat)) 
    }

    return views.length === 0
        ? "Unknown sender"
        : views.reduce((a, b) => `${a}, ${b}`)
}

export function tgRenderSenderName(message: components["schemas"]["Message"]): string {
    const views = [] as string[];

    if (message.from) {
        views.push(tgRenderUserName(message.from));
    }

    if (message.sender_chat) {
        views.push(tgRenderChatName(message.sender_chat)) 
    }

    if (message.sender_business_bot) {
        views.push(tgRenderUserName(message.sender_business_bot)) 
    }

    return views.length === 0
        ? "Unknown sender"
        : views.reduce((a, b) => `${a}, ${b}`)
}

export function tgRenderUserName(user: components["schemas"]["User"]): string {
    if (user.username) {
        return user.username;
    }
    
    if (user.first_name && user.last_name) {
        return `${user.first_name} ${user.last_name}`;
    }

    if (user.first_name) {
        return user.first_name;
    }

    if (user.last_name) {
        return user.last_name;
    }

    if (user.id) {
        return user.id + "";
    }

    return "Anonymous user";
}

export function tgRenderChatName(chat: components["schemas"]["Chat"]): string {
    if (chat.title) {
        return chat.title;
    }

    if (chat.username) {
        return chat.username;
    }
    
    if (chat.first_name && chat.last_name) {
        return `${chat.first_name} ${chat.last_name}`;
    }

    if (chat.first_name) {
        return chat.first_name;
    }

    if (chat.last_name) {
        return chat.last_name;
    }

    if (chat.id) {
        return chat.id + "";
    }

    return "Anonymous user";
}
