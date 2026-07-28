import apiClient from './client';

export const blockUser = async (userId) => {
  const response = await apiClient.post(`/Moderation/block/${userId}`);
  return response.data;
};

export const unblockUser = async (userId) => {
  const response = await apiClient.delete(`/Moderation/block/${userId}`);
  return response.data;
};

export const getBlockedUsers = async (page = 1, pageSize = 10) => {
  const response = await apiClient.get('/Moderation/blocked', {
    params: { page, pageSize },
  });
  return response.data;
};

export const muteUser = async (userId) => {
  const response = await apiClient.post(`/Moderation/mute/${userId}`);
  return response.data;
};

export const unmuteUser = async (userId) => {
  const response = await apiClient.delete(`/Moderation/mute/${userId}`);
  return response.data;
};
