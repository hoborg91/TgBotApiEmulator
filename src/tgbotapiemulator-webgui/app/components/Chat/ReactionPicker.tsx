import React, { useState } from 'react';

interface ReactionPickerProps {
  onReactionSelect: (emoji: string) => void;
  position?: 'top' | 'bottom';
}

const COMMON_REACTIONS = ['👍', '❤️', '😂', '😮', '😢', '🙏'];

export const ReactionPicker: React.FC<ReactionPickerProps> = ({ 
  onReactionSelect,
  position = 'top'
}) => {
  const [isOpen, setIsOpen] = useState(false);

  const handleReactionSelect = (emoji: string) => {
    onReactionSelect(emoji);
    setIsOpen(false);
  };

  return (
    <div className="relative">
      <button
        onClick={() => setIsOpen(!isOpen)}
        className="opacity-0 group-hover:opacity-100 transition-opacity p-1 text-gray-400 hover:text-gray-600"
      >
        <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M14.828 14.828a4 4 0 01-5.656 0M9 10h.01M15 10h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
        </svg>
      </button>

      {isOpen && (
        <div className={`
          absolute flex space-x-1 bg-white border border-gray-200 rounded-full px-2 py-1 shadow-lg z-10
          ${position === 'top' ? 'bottom-full mb-2' : 'top-full mt-2'}
        `}>
          {COMMON_REACTIONS.map((emoji) => (
            <button
              key={emoji}
              onClick={() => handleReactionSelect(emoji)}
              className="hover:scale-125 transition-transform text-lg"
            >
              {emoji}
            </button>
          ))}
        </div>
      )}
    </div>
  );
};
