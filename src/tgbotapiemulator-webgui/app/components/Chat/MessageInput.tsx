import React, { useState, useRef } from 'react';

interface MessageInputProps {
  onSendMessage: (message: string) => void;
  placeholder?: string;
}

export const MessageInput: React.FC<MessageInputProps> = ({ 
  onSendMessage, 
  placeholder = "Write a message..." 
}) => {
  const [message, setMessage] = useState('');
  const textareaRef = useRef<HTMLTextAreaElement>(null);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (message.trim()) {
      onSendMessage(message.trim());
      setMessage('');
      if (textareaRef.current) {
        textareaRef.current.style.height = 'auto';
      }
    }
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      handleSubmit(e);
    }
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLTextAreaElement>) => {
    setMessage(e.target.value);
    
    // Auto-resize textarea
    if (textareaRef.current) {
      textareaRef.current.style.height = 'auto';
      textareaRef.current.style.height = Math.min(textareaRef.current.scrollHeight, 120) + 'px';
    }
  };

  return (
    <div className="bg-white dark:bg-gray-900 border-t dark:border-gray-700 p-4">
      <form onSubmit={handleSubmit} className="flex items-end space-x-2">
        <div className="flex space-x-2">
          <button
            type="button"
            className="text-gray-600 dark:text-gray-300 hover:text-gray-800 dark:hover:text-white p-2"
          >
            <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15.172 7l-6.586 6.586a2 2 0 102.828 2.828l6.414-6.586a4 4 0 00-5.656-5.656l-6.415 6.585a6 6 0 108.486 8.486L20.5 13" />
            </svg>
          </button>
        </div>

        <div className="flex-1 bg-gray-100 dark:bg-gray-800 rounded-2xl px-4 py-2">
          <textarea
            ref={textareaRef}
            value={message}
            onChange={handleInputChange}
            onKeyDown={handleKeyDown}
            placeholder={placeholder}
            className="w-full bg-transparent resize-none outline-none text-sm max-h-32"
            rows={1}
          />
        </div>
        
        <div className="flex space-x-2">
          <button
            type="button"
            className="text-gray-600 dark:text-gray-300 hover:text-gray-800 dark:hover:text-white p-2"
          >
            <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M14.828 14.828a4 4 0 01-5.656 0M9 10h.01M15 10h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
          </button>

          <button
            type="button"
            className="text-gray-600 dark:text-gray-300 hover:text-gray-800 dark:hover:text-white p-2"
          >
            <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path
                fill='none'
                d="m 8.8888888,6.8352491 v 5.5785439 c 0,0 -0.061303,1.83908 2.6360152,1.83908 2.697318,0 2.636015,-1.808429 2.636015,-1.808429 V 6.7432951 c 0,0 0.306514,-1.7777778 -2.482758,-1.7777778 -2.7892722,0 -2.7892722,1.8697318 -2.7892722,1.8697318 z"
                id="path3" />
              <path
                fill='none'
                d="m 6.6206897,12.536398 c 0,0 0.1226053,3.923372 4.8122603,3.923372 4.689655,0 4.904215,-3.89272 4.904215,-3.89272"
                id="path4" />
              <path
                fill='none'
                d="m 11.494253,16.521073 v 3.862069"
                id="path5" />
            </svg>
          </button>

          <button
            type="button"
            className="text-gray-600 dark:text-gray-300 hover:text-gray-800 dark:hover:text-white p-2"
          >
            <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path
                fill='none'
                d="M 14.130268,6.0996169 10.114943,15.877395"
                id="path4" />
              <path
                fill='none'
                d="m 7.4482759,3.4022989 h 8.7662831 c 0,0 2.176246,0.030651 2.206897,1.9003831 0.03065,1.8697318 0.03065,1.8697318 0.03065,1.8697318 L 18.29885,16.704981 c 0,0 -0.0613,1.685824 -2.206897,1.685824 -2.145594,0 -8.8275862,0 -8.8275862,0 0,0 -2.1455939,-0.122606 -2.1455939,-2.482759 0,-2.360153 0,-10.2681993 0,-10.2681993 0,0 0.061303,-2.2375478 2.329503,-2.2375478 z"
                id="path5" />
            </svg>
          </button>
        </div>
      </form>
    </div>
  );
};
