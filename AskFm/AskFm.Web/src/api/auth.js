import apiClient from './client';

export const registerUser = async (data) => {
  const payload = {
    name: data.name,
    username: data.username || data.name.replace(/\s+/g, '_').toLowerCase() || data.email.split('@')[0],
    email: data.email,
    bio: data.bio || '',
    avatarPath: data.avatarPath || 'default.jpg',
    passwrod: data.password || data.passwrod,
    lastSeen: new Date().toISOString(),
  };
  const response = await apiClient.post('/Auth/register', payload);
  return response.data;
};

export const loginUser = async (data) => {
  const payload = {
    email: data.email,
    password: data.password,
  };
  const response = await apiClient.post('/Auth/login', payload);
  return response.data;
};

export const refreshToken = async (userId) => {
  const response = await apiClient.post(`/Auth/refresh-token/${userId}`);
  return response.data;
};

export const logoutUser = async (userId) => {
  const response = await apiClient.post(`/Auth/logout/${userId}`);
  return response.data;
};

export const forgotPassword = async (email) => {
  const response = await apiClient.post('/Auth/forgot-password', { email });
  return response.data;
};

export const resetPassword = async (data) => {
  const payload = {
    email: data.email,
    token: data.token,
    newPassword: data.newPassword,
    confirmPassword: data.confirmPassword || data.newPassword,
  };
  const response = await apiClient.post('/Auth/reset-password', payload);
  return response.data;
};

export const sendEmailConfirmation = async (email) => {
  const response = await apiClient.post('/Auth/send-email-confirmation', { email });
  return response.data;
};

export const confirmEmail = async (data) => {
  const response = await apiClient.post('/Auth/confirm-email', data);
  return response.data;
};
