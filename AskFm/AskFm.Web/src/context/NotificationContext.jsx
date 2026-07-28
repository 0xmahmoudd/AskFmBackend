import React, { createContext, useContext, useState, useEffect, useCallback } from 'react';
import { getUnreadNotificationCount } from '../api/notification';
import { initSignalR, stopSignalR } from '../services/signalr';
import { useAuth } from './AuthContext';

const NotificationContext = createContext(null);

export const NotificationProvider = ({ children }) => {
  const { isAuthenticated } = useAuth();
  const [unreadCount, setUnreadCount] = useState(0);
  const [toasts, setToasts] = useState([]);

  const fetchUnreadCount = useCallback(async () => {
    if (!isAuthenticated) return;
    try {
      const res = await getUnreadNotificationCount();
      const count = res.unreadCount !== undefined ? res.unreadCount : (res.data || res.Data || 0);
      setUnreadCount(count);
    } catch (err) {
      console.error('Failed to fetch unread notification count:', err);
    }
  }, [isAuthenticated]);

  const addToast = (message, type = 'info') => {
    const id = Date.now();
    setToasts((prev) => [...prev, { id, message, type }]);
    setTimeout(() => {
      removeToast(id);
    }, 4000);
  };

  const removeToast = (id) => {
    setToasts((prev) => prev.filter((t) => t.id !== id));
  };

  useEffect(() => {
    if (isAuthenticated) {
      fetchUnreadCount();

      const connection = initSignalR(
        (newNotif) => {
          setUnreadCount((prev) => prev + 1);
          addToast(newNotif.message || 'New notification received!', 'info');
        },
        (count) => {
          setUnreadCount(count);
        }
      );

      return () => {
        stopSignalR();
      };
    } else {
      setUnreadCount(0);
    }
  }, [isAuthenticated, fetchUnreadCount]);

  return (
    <NotificationContext.Provider
      value={{
        unreadCount,
        setUnreadCount,
        fetchUnreadCount,
        toasts,
        addToast,
        removeToast,
      }}
    >
      {children}
    </NotificationContext.Provider>
  );
};

export const useNotification = () => {
  const context = useContext(NotificationContext);
  if (!context) {
    throw new Error('useNotification must be used within a NotificationProvider');
  }
  return context;
};
