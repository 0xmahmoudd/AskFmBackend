import React from 'react';
import { MessageSquareOff } from 'lucide-react';

export const EmptyState = ({ title = 'No content found', message = 'Check back later or try exploring new users!', icon: Icon = MessageSquareOff }) => {
  return (
    <div className="card" style={{ textAlign: 'center', padding: '40px 20px' }}>
      <Icon style={{ width: '48px', height: '48px', color: 'var(--text-muted)', marginBottom: '12px', margin: '0 auto' }} />
      <h3 style={{ fontSize: '18px', fontWeight: '700', marginBottom: '6px' }}>{title}</h3>
      <p style={{ color: 'var(--text-muted)', fontSize: '14px' }}>{message}</p>
    </div>
  );
};
