import React, { useState } from 'react';
import { Message } from "./ChatTypes";
import { ReactionBar } from './ReactionBar';
import { ReactionPicker } from './ReactionPicker';
import { Avatar } from './Avatar';

interface MessageBubbleProps {
  message: Message;
  currentUserId: string;
  showAvatar: boolean;
  onReactionAdd?: (messageId: string, emoji: string) => void;
  onReactionRemove?: (messageId: string, emoji: string) => void;
}

export const MessageBubble: React.FC<MessageBubbleProps> = ({ 
  message, 
  currentUserId,
  showAvatar,
  onReactionAdd,
  onReactionRemove 
}) => {
  const [showReactionPicker, setShowReactionPicker] = useState(false);
  const isUser = message.sender === 'user';
  
  const formatTime = (date: Date) => {
    return date.toLocaleTimeString('en-US', { 
      hour: '2-digit', 
      minute: '2-digit' 
    });
  };

  const getStatusIcon = (status: Message['status']) => {
    switch (status) {
      case 'sent':
        return '✓';
      case 'delivered':
        return '✓✓';
      case 'read':
        return '✓✓🔵';
      default:
        return '✓';
    }
  };

  const handleReactionClick = (emoji: string) => {
    // Toggle reaction - if user already reacted, remove it; otherwise add it
    const existingReaction = message.reactions?.find(r => 
      r.users.includes(currentUserId) && r.emoji === emoji
    );

    if (existingReaction) {
      onReactionRemove?.(message.id, emoji);
    } else {
      onReactionAdd?.(message.id, emoji);
    }
  };

  const handleReactionSelect = (emoji: string) => {
    onReactionAdd?.(message.id, emoji);
  };

  return (
    <div className={`group flex ${isUser ? 'justify-end' : 'justify-start'} mb-4`}>
      <div className={`flex ${isUser ? 'flex-row-reverse' : 'flex-row'} items-end max-w-xs lg:max-w-md`}>
        {showAvatar && (
          <div className={`flex-shrink-0 ${isUser ? 'ml-2' : 'mr-2'}`}>
            <Avatar
              src={message.avatar}
              name={message.from.name}
              size="sm"
            />
          </div>
        )}
        
        {/* Spacer when avatar is hidden */}
        {!showAvatar && (
          <div className={`w-8 ${isUser ? 'ml-2' : 'mr-2'}`} />
        )}
        
        <div className={`px-4 py-2 rounded-2xl ${
          isUser 
            ? 'bg-green-50 text-gray-800 rounded-br-none' 
            : 'bg-gray-50 text-gray-800 rounded-bl-none'
        }`}>
          <p className="text-sm whitespace-pre-wrap"><b>{message.from.name}</b></p>
          {message.replyTo && (
            <div className={`text-xs p-2 mb-1 rounded ${
              isUser ? 'bg-blue-400' : 'bg-gray-300'
            }`}>
              Replying to: {message.replyTo}
            </div>
          )}
          
          <p className="text-sm whitespace-pre-wrap">{message.text}</p>
          
          <div className={`text-xs mt-1 flex items-center space-x-1 ${
            isUser ? 'text-green-500 justify-end' : 'text-gray-500 justify-start'
          }`}>
            {message.reactions && message.reactions.length > 0 && (
              <div className={isUser ? 'text-right' : 'text-left'}>
                <ReactionBar
                  reactions={message.reactions}
                  onReactionClick={handleReactionClick}
                  messageId={message.id}
                  currentUserId={currentUserId}
                />
              </div>
            )}

            {message.isEdited && (
              <span className="italic">edited</span>
            )}
            <span>{formatTime(message.timestamp)}</span>
            {isUser && (
              <span className="text-xs">{getStatusIcon(message.status)}</span>
            )}
          </div>
        </div>

        {/* <div className={`absolute ${isUser ? 'left-0' : 'right-0'} ${
          message.reactions && message.reactions.length > 0 ? '-bottom-6' : '-bottom-2'
        } opacity-0 group-hover:opacity-100 transition-opacity`}>
          <ReactionPicker
            onReactionSelect={handleReactionSelect}
            position={isUser ? 'top' : 'bottom'}
          />
        </div> */}
      </div>
    </div>
  );
};
