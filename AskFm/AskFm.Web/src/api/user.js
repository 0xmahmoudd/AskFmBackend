import apiClient from './client';

export const getCurrentUserProfile = async () => {
  const response = await apiClient.get('/User/profile');
  return response.data;
};

export const getUserById = async (userId) => {
  const response = await apiClient.get(`/User/profile/${userId}`);
  return response.data;
};

export const updateUserProfile = async (userId, data) => {
  const response = await apiClient.post(`/User/profile/update/${userId}`, data);
  return response.data;
};

export const deleteUserAccount = async (userId) => {
  const response = await apiClient.delete(`/User/profile/${userId}`);
  return response.data;
};

export const uploadAvatar = async (userId, file) => {
  const formData = new FormData();
  formData.append('file', file);
  const response = await apiClient.post(`/User/profile/${userId}/avatar`, formData, {
    headers: {
      'Content-Type': 'multipart/form-data',
    },
  });
  return response.data;
};

export const followUser = async (followerId, targetUserId) => {
  const response = await apiClient.post(`/User/profile/${followerId}/follow/${targetUserId}`);
  return response.data;
};

export const unfollowUser = async (followerId, targetUserId) => {
  const response = await apiClient.post(`/User/profile/${followerId}/unfollow/${targetUserId}`);
  return response.data;
};

export const searchUsers = async (query, page = 1, pageSize = 10) => {
  const response = await apiClient.get('/User/search', {
    params: { q: query, page, pageSize },
  });
  return response.data;
};

export const getFollowers = async (userId, page = 1, pageSize = 10) => {
  const response = await apiClient.get(`/User/profile/${userId}/followers`, {
    params: { page, pageSize },
  });
  return response.data;
};

export const getFollowing = async (userId, page = 1, pageSize = 10) => {
  const response = await apiClient.get(`/User/profile/${userId}/following`, {
    params: { page, pageSize },
  });
  return response.data;
};

export const getFollowStatus = async (targetUserId) => {
  const response = await apiClient.get(`/User/profile/${targetUserId}/follow-status`);
  return response.data;
};

export const updatePassword = async (userId, data) => {
  const payload = {
    currentPassword: data.oldPassword || data.currentPassword,
    updatedPassword: data.newPassword || data.updatedPassword,
  };
  const response = await apiClient.post(`/User/profile/update/pass/${userId}`, payload);
  return response.data;
};

export const requestEmailChange = async (userId, newEmail) => {
  const response = await apiClient.post(`/User/profile/update/email/${userId}`, { newEmail });
  return response.data;
};

export const confirmEmailChange = async (userId, newEmail, token) => {
  const response = await apiClient.post(`/User/profile/update/email/${userId}/confirm`, {
    newEmail,
    token,
  });
  return response.data;
};
