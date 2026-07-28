import React from 'react';
import { NavLink } from 'react-router-dom';
import { Home, Compass, Inbox, Bookmark, Bell, Settings, ShieldAlert, User } from 'lucide-react';
import { useAuth } from '../context/AuthContext';
import { useNotification } from '../context/NotificationContext';

export const Sidebar = () => {
  const { isAuthenticated } = useAuth();
  const { unreadCount } = useNotification();

  if (!isAuthenticated) return null;

  return (
    <aside className="sidebar">
      <div className="card" style={{ padding: '12px' }}>
        <nav style={{ display: 'flex', flexDirection: 'column', gap: '4px' }}>
          <NavLink
            to="/"
            end
            className={({ isActive }) => `nav-link ${isActive ? 'active' : ''}`}
          >
            <Home style={{ width: '18px', height: '18px' }} /> Feed
          </NavLink>

          <NavLink
            to="/explore"
            className={({ isActive }) => `nav-link ${isActive ? 'active' : ''}`}
          >
            <Compass style={{ width: '18px', height: '18px' }} /> Explore
          </NavLink>

          <NavLink
            to="/inbox"
            className={({ isActive }) => `nav-link ${isActive ? 'active' : ''}`}
          >
            <Inbox style={{ width: '18px', height: '18px' }} /> Inbox (Questions)
          </NavLink>

          <NavLink
            to="/saved"
            className={({ isActive }) => `nav-link ${isActive ? 'active' : ''}`}
          >
            <Bookmark style={{ width: '18px', height: '18px' }} /> Saved Threads
          </NavLink>

          <NavLink
            to="/notifications"
            className={({ isActive }) => `nav-link ${isActive ? 'active' : ''}`}
            style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}
          >
            <div style={{ display: 'flex', alignItems: 'center', gap: '6px' }}>
              <Bell style={{ width: '18px', height: '18px' }} /> Notifications
            </div>
            {unreadCount > 0 && <span className="badge">{unreadCount}</span>}
          </NavLink>

          <NavLink
            to="/profile/me"
            className={({ isActive }) => `nav-link ${isActive ? 'active' : ''}`}
          >
            <User style={{ width: '18px', height: '18px' }} /> Profile
          </NavLink>

          <NavLink
            to="/settings/profile"
            className={({ isActive }) => `nav-link ${isActive ? 'active' : ''}`}
          >
            <Settings style={{ width: '18px', height: '18px' }} /> Settings
          </NavLink>

          <NavLink
            to="/settings/blocked"
            className={({ isActive }) => `nav-link ${isActive ? 'active' : ''}`}
          >
            <ShieldAlert style={{ width: '18px', height: '18px' }} /> Moderation
          </NavLink>
        </nav>
      </div>
    </aside>
  );
};
