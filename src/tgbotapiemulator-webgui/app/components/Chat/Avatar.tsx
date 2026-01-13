import React from 'react';

interface AvatarProps {
  src?: string;
  name: string;
  size?: 'sm' | 'md' | 'lg';
  className?: string;
}

export const Avatar: React.FC<AvatarProps> = ({ 
  src, 
  name, 
  size = 'md', 
  className = '' 
}) => {
  const sizeClasses = {
    sm: 'w-6 h-6 text-xs',
    md: 'w-8 h-8 text-sm',
    lg: 'w-10 h-10 text-base'
  };

  const getInitials = (name: string): string => {
    const names = name.trim().split(' ');
    if (names.length === 1) {
      return names[0]!.charAt(0).toUpperCase();
    }
    if (names.length > 1){
      return (names[0]!.charAt(0) + names[names.length - 1]!.charAt(0)).toUpperCase();
    }
    return name.substring(0, 2);
  };

  const getBackgroundColor = (name: string): string => {
    // Generate consistent background color based on name
    const colors = [
      'bg-blue-500', 'bg-green-500', 'bg-purple-500', 'bg-pink-500', 
      'bg-orange-500', 'bg-teal-500', 'bg-cyan-500', 'bg-amber-500'
    ];
    const index = name.charCodeAt(0) % colors.length;
    return colors[index]!;
  };

  if (src) {
    return (
      <img
        src={src}
        alt={name}
        className={`rounded-full object-cover ${sizeClasses[size]} ${className}`}
      />
    );
  }

  return (
    <div
      className={`
        rounded-full flex items-center justify-center text-white font-medium
        ${sizeClasses[size]} ${getBackgroundColor(name)} ${className}
      `}
    >
      {getInitials(name)}
    </div>
  );
};
