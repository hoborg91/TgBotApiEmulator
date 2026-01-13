import React from 'react';

interface ChatContainerProps {
  children: React.ReactNode;
}

export const ChatContainer: React.FC<ChatContainerProps> = ({ children }) => {
  return (
    <div className="flex flex-col [&>*]:flex-shrink-0 [&>.flex-1]:min-h-0">
      {children}
    </div>
  );
};