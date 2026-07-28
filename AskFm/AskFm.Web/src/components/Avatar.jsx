import React from 'react';

export const Avatar = ({ src, name = '', size = 'md', className = '' }) => {
  const getInitials = (n) => {
    if (!n) return '?';
    const parts = n.trim().split(' ');
    if (parts.length >= 2) {
      return (parts[0][0] + parts[1][0]).toUpperCase();
    }
    return n.slice(0, 2).toUpperCase();
  };

  const sizeClass = size === 'lg' ? 'avatar-lg' : size === 'sm' ? 'avatar-sm' : '';

  if (src) {
    const fullSrc = src.startsWith('http') ? src : src.startsWith('/') ? src : `/${src}`;
    return (
      <img
        src={fullSrc}
        alt={name || 'User Avatar'}
        className={`avatar ${sizeClass} ${className}`}
        onError={(e) => {
          e.target.style.display = 'none';
          if (e.target.nextSibling) {
            e.target.nextSibling.style.display = 'flex';
          }
        }}
      />
    );
  }

  return (
    <div className={`avatar ${sizeClass} ${className}`}>
      {getInitials(name)}
    </div>
  );
};
