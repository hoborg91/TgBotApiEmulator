import React, { useState, useRef, useEffect } from 'react';
import { ChatHeader } from './ChatHeader';
import { MessageBubble } from './MessageBubble';
import { MessageInput } from './MessageInput';
import { Chat as ChatType, Message } from './ChatTypes';
import { Avatar } from './Avatar';

interface ChatProps {
  chat: ChatType;
  onSendMessage: (message: string) => void;
  onReactionAdd: (messageId: string, emoji: string) => void;
  onReactionRemove: (messageId: string, emoji: string) => void;
  currentUserId: string;
  onBack?: () => void;
}

const shouldShowAvatar = (messages: Message[], currentIndex: number): boolean => {
  if (currentIndex >= messages.length || currentIndex < 0) {
    throw new Error("currentIndex >= messages.length || currentIndex < 0");
  }
  const currentMessage = messages[currentIndex]!;
  
  // Never show avatar for user messages
  if (currentMessage.sender === 'user') return false;
  
  // If it's the last message, show avatar
  if (currentIndex === messages.length - 1) return true;
  
  const nextMessage = messages[currentIndex + 1]!;
  
  // Show avatar if:
  // 1. Next message is from a different sender, OR
  // 2. Next message is from user, OR
  // 3. There's a significant time gap to next message (more than 5 minutes)
  const timeDiff = nextMessage.timestamp.getTime() - currentMessage.timestamp.getTime();
  const isLongTimeGap = timeDiff > 5 * 60 * 1000; // 5 minutes in milliseconds
  
  return nextMessage.from.id !== currentMessage.from.id || isLongTimeGap;
};

export const ChatComponent: React.FC<ChatProps> = ({ 
  chat, 
  onSendMessage, 
  onReactionAdd,
  onReactionRemove,
  currentUserId,
  onBack 
}) => {
  const messagesEndRef = useRef<HTMLDivElement>(null);
  const messagesContainerRef = useRef<HTMLDivElement>(null);
  const messageRefs = useRef<Map<string, HTMLDivElement>>(new Map());
  const [isTyping, setIsTyping] = useState(false);
  const [stickyAvatarMessage, setStickyAvatarMessage] = useState<Message | null>(null);

  // Find the message that should show sticky avatar based on visibility
  const findStickyAvatarMessage = () => {
    if (!messagesContainerRef.current) return null;

    const container = messagesContainerRef.current;
    const containerRect = container.getBoundingClientRect();
    const containerBottom = containerRect.bottom;
    
    // Find all contact messages that have avatars
    const avatarMessages = chat.messages
      .map((message, index) => ({ message, index }))
      .filter(({ message, index }) => 
        message.sender === 'contact'// && shouldShowAvatar(chat.messages, index)
      );

    // Find the first avatar message that is partially visible at the top
    for (const { message } of avatarMessages) {
      const messageElement = messageRefs.current.get(message.id);
      if (messageElement) {
        const messageRectangle = messageElement.getBoundingClientRect();
        const isPartiallyVisible = messageRectangle.top < containerBottom && messageRectangle.bottom > containerRect.top;
        const isCutOffAtTop = messageRectangle.top < containerBottom && messageRectangle.bottom > containerBottom;
        
        // If message is partially visible but its avatar area is cut off
        if (isPartiallyVisible && isCutOffAtTop) {
          return message;
        }
      }
    }

    return null;
  };

  const checkStickyAvatar = () => {
    const message = findStickyAvatarMessage();
    setStickyAvatarMessage(message);
  };

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  };

  useEffect(() => {
    scrollToBottom();
  }, [chat.messages]);

  useEffect(() => {
    const timer = setTimeout(() => {
      scrollToBottom();
    }, 100);
    return () => clearTimeout(timer);
  }, []);

  // Add scroll event listener and initial check
  useEffect(() => {
    const container = messagesContainerRef.current;
    if (container) {
      container.addEventListener('scroll', checkStickyAvatar);
      // Initial check
      checkStickyAvatar();
      return () => container.removeEventListener('scroll', checkStickyAvatar);
    }
  }, [chat.messages]);

  // Re-check when messages change
  useEffect(() => {
    checkStickyAvatar();
  }, [chat.messages]);

  const handleSendMessage = (message: string) => {
    onSendMessage(message);
    setIsTyping(true);
    setTimeout(() => setIsTyping(false), 2000);
  };

  // Register message element refs
  const registerMessageRef = (messageId: string, element: HTMLDivElement | null) => {
    if (element) {
      messageRefs.current.set(messageId, element);
    } else {
      messageRefs.current.delete(messageId);
    }
  };

  return (
    <div className="relative flex flex-col bg-green-100 dark:bg-gray-900" style={{ height: "50vh" }}>
      <div className="flex-shrink-0">
        <ChatHeader contact={chat.contact} onBack={onBack} />
      </div>
      
      <div 
        ref={messagesContainerRef}
        className="flex-1 overflow-y-auto"
        style={{ 
          height: '0px',
          minHeight: '0px'
        }}
      >
        <div className="min-h-full px-4 py-2">
          <div className="max-w-3xl mx-auto">
            {chat.messages.map((message, index) => {
              const showAvatar = shouldShowAvatar(chat.messages, index);
              const isStickyCandidate = message.sender === 'contact';// && showAvatar;
              
              return (
                <div 
                  key={message.id} 
                  ref={isStickyCandidate ? (el) => registerMessageRef(message.id, el) : undefined}
                >
                  <MessageBubble 
                    message={message}
                    currentUserId={currentUserId}
                    showAvatar={showAvatar}
                    onReactionAdd={onReactionAdd}
                    onReactionRemove={onReactionRemove}
                  />
                </div>
              );
            })}
            
            {isTyping && (
              <div className="flex justify-start mb-4">
                <div className="flex items-end">
                  <div className="flex-shrink-0 mr-2">
                    <Avatar
                      src={chat.contact.avatar}
                      name={chat.contact.name}
                      size="sm"
                    />
                  </div>
                  <div className="bg-gray-200 dark:bg-gray-700 text-gray-800 dark:text-gray-100 px-4 py-2 rounded-2xl rounded-bl-none">
                    <div className="flex space-x-1">
                      <div className="w-2 h-2 bg-gray-500 rounded-full animate-bounce"></div>
                      <div className="w-2 h-2 bg-gray-500 rounded-full animate-bounce" style={{ animationDelay: '0.1s' }}></div>
                      <div className="w-2 h-2 bg-gray-500 rounded-full animate-bounce" style={{ animationDelay: '0.2s' }}></div>
                    </div>
                  </div>
                </div>
              </div>
            )}
            
            <div ref={messagesEndRef} />
          </div>
        </div>
      </div>
      
      {/* Sticky Avatar - shows when a message's avatar is partially cut off at the top */}
      {stickyAvatarMessage && (
        <div className="absolute left-4 bottom-20 z-10">
          <Avatar
            src={stickyAvatarMessage.avatar}
            name={stickyAvatarMessage.from.name}
            size="sm"
            className="border-2 border-white shadow-lg"
          />
        </div>
      )}
      
      <div className="flex-shrink-0">
        <MessageInput onSendMessage={handleSendMessage} />
      </div>
    </div>
  );
};
