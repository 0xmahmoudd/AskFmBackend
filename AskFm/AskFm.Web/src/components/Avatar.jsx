import React from 'react';

export const Avatar = ({ name = '', size = 'md', className = '' }) => {
  const getInitials = (n) => {
    if (!n) return '?';
    const parts = n.trim().split(' ');
    if (parts.length >= 2) {
      return (parts[0][0] + parts[1][0]).toUpperCase();
    }
    return n.slice(0, 2).toUpperCase();
  };

  const sizeClass = size === 'lg' ? 'avatar-lg' : size === 'sm' ? 'avatar-sm' : '';

  return (
    <div className={`avatar ${sizeClass} ${className}`}>
      {getInitials(name)}
    </div>
  );
};
