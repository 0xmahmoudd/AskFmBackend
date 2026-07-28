import React, { useState, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { getUserNotifications, getNotificationsByType, markNotificationAsRead, markAllNotificationsAsRead } from '../api/notification';
import { useNotification } from '../context/NotificationContext';
import { parseApiError } from '../api/client';
import { Spinner } from '../components/Spinner';
import { EmptyState } from '../components/EmptyState';
import { Pagination } from '../components/Pagination';
import { Bell, CheckCheck, ChevronRight } from 'lucide-react';

export const NotificationsPage = () => {
  const navigate = useNavigate();
  const { fetchUnreadCount, addToast } = useNotification();
  const [notifications, setNotifications] = useState([]);
  const [category, setCategory] = useState('ALL'); // 'ALL' | 'FOLLOW' | 'LIKE' | 'COMMENT' | 'ANSWER' | 'QUESTION'
  const [page, setPage] = useState(1);
  const [totalItems, setTotalItems] = useState(0);
  const [loading, setLoading] = useState(true);

  const fetchNotifications = useCallback(async () => {
    setLoading(true);
    try {
      let res;
      if (category === 'ALL') {
        res = await getUserNotifications(page, 10);
      } else {
        res = await getNotificationsByType(category, page, 10);
      }
      const data = res.data || res.Data || res;
      if (data.items || data.Items) {
        setNotifications(data.items || data.Items);
        setTotalItems(data.totalCount || data.TotalCount || 0);
      } else if (Array.isArray(data)) {
        setNotifications(data);
        setTotalItems(data.length);
      } else {
        setNotifications([]);
      }
    } catch (err) {
      console.error('Failed to load notifications:', err);
      setNotifications([]);
    } finally {
      setLoading(false);
    }
  }, [category, page]);

  useEffect(() => {
    fetchNotifications();

    const handleNotification = () => {
      fetchNotifications();
    };

    window.addEventListener('askfm_notification', handleNotification);
    return () => window.removeEventListener('askfm_notification', handleNotification);
  }, [fetchNotifications]);

  const handleMarkRead = async (e, id) => {
    e.stopPropagation();
    try {
      await markNotificationAsRead(id);
      fetchNotifications();
      fetchUnreadCount();
    } catch (err) {
      addToast(parseApiError(err), 'error');
    }
  };

  const handleMarkAllRead = async () => {
    try {
      await markAllNotificationsAsRead();
      addToast('All notifications marked as read', 'success');
      fetchNotifications();
      fetchUnreadCount();
    } catch (err) {
      addToast(parseApiError(err), 'error');
    }
  };

  const handleNotificationClick = async (notif) => {
    const notifId = notif.id || notif.Id;
    const isRead = notif.isRead || notif.IsRead;
    const type = (notif.type || notif.Type || '').toUpperCase();
    const resourceId = notif.resourceId || notif.ResourceId;

    if (!isRead) {
      try {
        await markNotificationAsRead(notifId);
        fetchUnreadCount();
      } catch (err) {
        // ignore error
      }
    }

    if (type === 'FOLLOW') {
      if (resourceId) navigate(`/profile/${resourceId}`);
    } else if (type === 'QUESTION') {
      navigate('/inbox');
    } else if (resourceId) {
      navigate(`/thread/${resourceId}`);
    }
  };

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '16px' }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
          <Bell style={{ width: '24px', height: '24px', color: 'var(--primary)' }} />
          <h2 style={{ fontSize: '20px', fontWeight: '800' }}>Notifications</h2>
        </div>

        <button className="btn btn-secondary" onClick={handleMarkAllRead}>
          <CheckCheck style={{ width: '16px', height: '16px' }} /> Mark All Read
        </button>
      </div>

      {/* Category filter tabs */}
      <div style={{ display: 'flex', gap: '8px', overflowX: 'auto', marginBottom: '16px', paddingBottom: '4px' }}>
        {['ALL', 'FOLLOW', 'LIKE', 'COMMENT', 'ANSWER', 'QUESTION'].map((cat) => (
          <button
            key={cat}
            className={`btn ${category === cat ? 'btn-primary' : 'btn-secondary'}`}
            onClick={() => {
              setCategory(cat);
              setPage(1);
            }}
            style={{ fontSize: '13px', padding: '6px 14px' }}
          >
            {cat}
          </button>
        ))}
      </div>

      {loading ? (
        <Spinner />
      ) : notifications.length === 0 ? (
        <EmptyState title="No notifications" message="You have no notifications in this category." icon={Bell} />
      ) : (
        <div className="card">
          <div style={{ display: 'flex', flexDirection: 'column', gap: '10px' }}>
            {notifications.map((notif) => {
              const notifId = notif.id || notif.Id;
              const isRead = notif.isRead || notif.IsRead;

              return (
                <div
                  key={notifId}
                  onClick={() => handleNotificationClick(notif)}
                  style={{
                    display: 'flex',
                    justifyContent: 'space-between',
                    alignItems: 'center',
                    padding: '12px 14px',
                    borderRadius: 'var(--radius-sm)',
                    background: isRead ? '#ffffff' : 'var(--primary-light)',
                    borderLeft: isRead ? '4px solid transparent' : '4px solid var(--primary)',
                    borderBottom: '1px solid var(--border-color)',
                    cursor: 'pointer',
                    transition: 'background 0.2s',
                  }}
                >
                  <div style={{ flex: 1, paddingRight: '12px' }}>
                    <p style={{ fontWeight: isRead ? '400' : '700', fontSize: '14px', color: 'var(--text-main)', marginBottom: '4px' }}>
                      {notif.message || notif.Message}
                    </p>
                    <span style={{ fontSize: '12px', color: 'var(--text-muted)' }}>
                      {new Date(notif.createdAt || notif.CreatedAt).toLocaleString()}
                    </span>
                  </div>

                  <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                    {!isRead && (
                      <button
                        className="btn btn-secondary"
                        onClick={(e) => handleMarkRead(e, notifId)}
                        style={{ fontSize: '12px', padding: '4px 10px' }}
                      >
                        Read
                      </button>
                    )}
                    <ChevronRight style={{ width: '18px', height: '18px', color: 'var(--text-muted)' }} />
                  </div>
                </div>
              );
            })}
          </div>
          <Pagination page={page} pageSize={10} totalItems={totalItems} onPageChange={setPage} />
        </div>
      )}
    </div>
  );
};
