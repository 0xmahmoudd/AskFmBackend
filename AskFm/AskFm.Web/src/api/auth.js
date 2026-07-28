import apiClient from './client';

export const registerUser = async (data) => {
  const response = await apiClient.post('/Auth/register', data);
  return response.data;
};

export const loginUser = async (data) => {
  const response = await apiClient.post('/Auth/login', data);
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
  const response = await apiClient.post('/Auth/reset-password', data);
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
