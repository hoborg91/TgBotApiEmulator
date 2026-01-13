import React from 'react';
import { Reaction } from "./ChatTypes";

interface ReactionBarProps {
  reactions: Reaction[];
  onReactionClick?: (emoji: string) => void;
  messageId: string;
  currentUserId: string;
}

export const ReactionBar: React.FC<ReactionBarProps> = ({ 
  reactions, 
  onReactionClick,
  messageId,
  currentUserId 
}) => {
  if (!reactions || reactions.length === 0) return null;

  const hasUserReacted = (reaction: Reaction) => {
    return reaction.users.includes(currentUserId);
  };

  return (
    <div className="flex flex-wrap gap-1 mt-2 justify-end">
      {reactions.map((reaction, index) => {
        const userReacted = hasUserReacted(reaction);
        return (
          <button
            key={`${reaction.emoji}-${index}`}
            onClick={() => onReactionClick?.(reaction.emoji)}
            className={`
              flex items-center space-x-1 px-2 py-1 rounded-full text-xs transition-colors
              ${userReacted 
                ? 'bg-blue-500 text-white' 
                : 'bg-green-200 text-gray-800'
              }
            `}
          >
            <span className="text-xs">{reaction.emoji}</span>
            <span className="font-medium">{reaction.count}</span>
          </button>
        );
      })}
    </div>
  );
};
