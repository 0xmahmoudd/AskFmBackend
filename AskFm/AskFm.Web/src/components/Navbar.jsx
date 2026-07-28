import React, { useState } from 'react';
import { Link, useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { useNotification } from '../context/NotificationContext';
import { Avatar } from './Avatar';
import { MessageSquare, Bell, Search, LogOut, User, Settings, ShieldAlert, Home, Compass, Bookmark, Inbox } from 'lucide-react';

export const Navbar = () => {
  const { user, isAuthenticated, logout } = useAuth();
  const { unreadCount } = useNotification();
  const [searchQuery, setSearchQuery] = useState('');
  const [menuOpen, setMenuOpen] = useState(false);
  const navigate = useNavigate();
  const location = useLocation();

  const handleSearchSubmit = (e) => {
    e.preventDefault();
    if (searchQuery.trim()) {
      navigate(`/search?q=${encodeURIComponent(searchQuery.trim())}`);
      setSearchQuery('');
    }
  };

  return (
    <nav className="navbar">
      <div className="navbar-inner">
        <Link to="/" className="logo">
          <MessageSquare style={{ width: '28px', height: '28px', color: 'var(--primary)' }} />
          <span>AskFm</span>
        </Link>

        {isAuthenticated && (
          <form onSubmit={handleSearchSubmit} style={{ flex: 1, maxWidth: '400px', display: 'flex', gap: '8px' }}>
            <div style={{ position: 'relative', width: '100%' }}>
              <Search
                style={{
                  position: 'absolute',
                  left: '12px',
                  top: '50%',
                  transform: 'translateY(-50%)',
                  width: '16px',
                  height: '16px',
                  color: 'var(--text-muted)',
                }}
              />
              <input
                type="text"
                className="form-input"
                placeholder="Search users..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                style={{ paddingLeft: '36px', height: '38px' }}
              />
            </div>
          </form>
        )}

        <div className="navbar-nav">
          {isAuthenticated ? (
            <>
              <Link to="/notifications" className={`nav-link ${location.pathname === '/notifications' ? 'active' : ''}`} style={{ position: 'relative' }}>
                <Bell style={{ width: '20px', height: '20px' }} />
                {unreadCount > 0 && (
                  <span
                    className="badge"
                    style={{
                      position: 'absolute',
                      top: '2px',
                      right: '2px',
                      padding: '2px 6px',
                      fontSize: '10px',
                      borderRadius: '10px',
                      backgroundColor: '#ef4444',
                      color: '#fff',
                    }}
                  >
                    {unreadCount}
                  </span>
                )}
              </Link>

              <div style={{ position: 'relative' }}>
                <button
                  onClick={() => setMenuOpen(!menuOpen)}
                  style={{ display: 'flex', alignItems: 'center', gap: '8px', padding: '4px' }}
                >
                  <Avatar src={user?.avatarPath} name={user?.name || user?.email} size="sm" />
                </button>

                {menuOpen && (
                  <div
                    style={{
                      position: 'absolute',
                      right: 0,
                      top: '48px',
                      width: '200px',
                      background: 'var(--bg-card)',
                      border: '1px solid var(--border-color)',
                      borderRadius: 'var(--radius-md)',
                      boxShadow: 'var(--shadow-md)',
                      display: 'flex',
                      flexDirection: 'column',
                      zIndex: 100,
                    }}
                    onClick={() => setMenuOpen(false)}
                  >
                    <Link to="/profile/me" className="nav-link" style={{ padding: '10px 14px' }}>
                      <User style={{ width: '16px', height: '16px' }} /> Profile
                    </Link>
                    <Link to="/settings/profile" className="nav-link" style={{ padding: '10px 14px' }}>
                      <Settings style={{ width: '16px', height: '16px' }} /> Settings
                    </Link>
                    <Link to="/settings/blocked" className="nav-link" style={{ padding: '10px 14px' }}>
                      <ShieldAlert style={{ width: '16px', height: '16px' }} /> Moderation
                    </Link>
                    <hr style={{ border: 'none', borderTop: '1px solid var(--border-color)', margin: '4px 0' }} />
                    <button
                      onClick={logout}
                      className="nav-link"
                      style={{ padding: '10px 14px', width: '100%', textAlign: 'left', color: '#ef4444' }}
                    >
                      <LogOut style={{ width: '16px', height: '16px' }} /> Logout
                    </button>
                  </div>
                )}
              </div>
            </>
          ) : (
            <div style={{ display: 'flex', gap: '12px' }}>
              <Link to="/login" className="btn btn-secondary">
                Login
              </Link>
              <Link to="/register" className="btn btn-primary">
                Register
              </Link>
            </div>
          )}
        </div>
      </div>
    </nav>
  );
};
