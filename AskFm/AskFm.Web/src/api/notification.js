import apiClient from './client';

export const getUserNotifications = async (pageNumber = 1, pageSize = 10) => {
  const response = await apiClient.get('/Notification', {
    params: { pageNumber, pageSize },
  });
  return response.data;
};

export const getNotificationsByType = async (category, pageNumber = 1, pageSize = 10) => {
  const response = await apiClient.get(`/Notification/type/${category}`, {
    params: { pageNumber, pageSize },
  });
  return response.data;
};

export const markNotificationAsRead = async (notificationId) => {
  const response = await apiClient.put(`/Notification/${notificationId}/read`);
  return response.data;
};

export const markAllNotificationsAsRead = async () => {
  const response = await apiClient.put('/Notification/read-all');
  return response.data;
};

export const getUnreadNotificationCount = async () => {
  const response = await apiClient.get('/Notification/unread-count');
  return response.data;
};

export const createNotification = async (data) => {
  // data: { userId, type, resourceId, message }
  const response = await apiClient.post('/Notification', data);
  return response.data;
};
