import axios from 'axios';

const API_BASE_URL = '/api';

const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  withCredentials: true,
});

// Request interceptor to attach JWT token
apiClient.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('accessToken');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Response interceptor to handle token expiration & standardized errors
apiClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    // Handle 401 Unauthorized token refresh
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;
      const userId = localStorage.getItem('userId');
      if (userId) {
        try {
          const res = await axios.post(
            `/api/Auth/refresh-token/${userId}`,
            {},
            { withCredentials: true }
          );
          if (res.data?.data?.token) {
            localStorage.setItem('accessToken', res.data.data.token);
            originalRequest.headers.Authorization = `Bearer ${res.data.data.token}`;
            return apiClient(originalRequest);
          }
        } catch (refreshError) {
          localStorage.removeItem('accessToken');
          localStorage.removeItem('userId');
          window.location.href = '/login';
          return Promise.reject(refreshError);
        }
      }
    }

    return Promise.reject(error);
  }
);

export const parseApiError = (error) => {
  if (!error.response) {
    return 'Network error. Please check backend connection.';
  }
  const data = error.response.data;
  if (typeof data === 'string') return data;
  if (Array.isArray(data)) return data.join(', ');
  if (data?.message) return data.message;
  if (data?.Errors && Array.isArray(data.Errors)) return data.Errors.join(', ');
  return 'An unexpected error occurred.';
};

export default apiClient;
