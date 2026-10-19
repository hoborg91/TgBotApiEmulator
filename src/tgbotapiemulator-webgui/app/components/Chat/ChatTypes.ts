export interface Contact {
  id: string;
  name: string;
  avatar?: string;
  status: 'online' | 'offline' | 'away';
  lastSeen?: Date;
}

export interface Chat {
  id: string;
  contact: Contact;
  messages: Message[];
  unreadCount: number;
  lastMessage?: Message;
}

export interface Reaction {
  emoji: string;
  count: number;
  users: string[]; // user IDs who added this reaction
}

export interface Message {
  id: string;
  text: string;
  sender: 'user' | 'contact';
  from: Contact,
  timestamp: Date;
  status: 'sent' | 'delivered' | 'read';
  isEdited?: boolean;
  replyTo?: string;
  reactions?: Reaction[];
  avatar?: string;
  isBot?: boolean;
}
