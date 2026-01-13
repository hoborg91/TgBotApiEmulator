import React from 'react';
import { Contact } from './ChatTypes';

interface ChatHeaderProps {
  contact: Contact;
  onBack?: () => void;
}

export const ChatHeader: React.FC<ChatHeaderProps> = ({ contact, onBack }) => {
  const getStatusText = () => {
    if (contact.status === 'online') return 'Online';
    if (contact.status === 'away') return 'Away';
    return `Last seen ${contact.lastSeen?.toLocaleTimeString()}`;
  };

  return (
    <div className="bg-white border-b border-gray-200 px-4 py-3 flex items-center space-x-3">
      {onBack && (
        <button 
          onClick={onBack}
          className="lg:hidden text-gray-600 hover:text-gray-800"
        >
          <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
          </svg>
        </button>
      )}
      
      <div className="relative">
        {/* <img 
          src={contact.avatar} 
          alt={contact.name}
          className="w-10 h-10 rounded-full"
        /> */}
        <div className={`absolute bottom-0 right-0 w-3 h-3 rounded-full border-2 border-white ${
          contact.status === 'online' ? 'bg-green-500' : 
          contact.status === 'away' ? 'bg-yellow-500' : 'bg-gray-400'
        }`} />
      </div>
      
      <div className="flex-1">
        <h2 className="font-semibold text-gray-800">{contact.name}</h2>
        <p className="text-xs text-gray-500">{getStatusText()}</p>
      </div>
      
      <div className="flex space-x-2">
        <button className="text-gray-600 hover:text-gray-800 p-2">
          <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
          </svg>
        </button>

        <button className="text-gray-600 hover:text-gray-800 p-2">
          <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 5a2 2 0 012-2h3.28a1 1 0 01.948.684l1.498 4.493a1 1 0 01-.502 1.21l-2.257 1.13a11.042 11.042 0 005.516 5.516l1.13-2.257a1 1 0 011.21-.502l4.493 1.498a1 1 0 01.684.949V19a2 2 0 01-2 2h-1C9.716 21 3 14.284 3 6V5z" />
          </svg>
        </button>
        
        <button className="text-gray-600 hover:text-gray-800 p-2">
          <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 5v.01M12 12v.01M12 19v.01M12 6a1 1 0 110-2 1 1 0 010 2zm0 7a1 1 0 110-2 1 1 0 010 2zm0 7a1 1 0 110-2 1 1 0 010 2z" />
          </svg>
        </button>
      </div>
    </div>
  );
};
