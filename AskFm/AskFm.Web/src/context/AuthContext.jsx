import React, { createContext, useContext, useState, useEffect, useCallback } from 'react';
import { loginUser, registerUser, logoutUser } from '../api/auth';
import { getCurrentUserProfile } from '../api/user';
import { parseApiError } from '../api/client';

const AuthContext = createContext(null);

export const AuthProvider = ({ children }) => {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);
  const [authError, setAuthError] = useState(null);

  const fetchCurrentUser = useCallback(async () => {
    const token = localStorage.getItem('accessToken');
    if (!token) {
      setUser(null);
      setLoading(false);
      return;
    }
    try {
      const userData = await getCurrentUserProfile();
      setUser(userData);
    } catch (err) {
      console.error('Failed to load profile:', err);
      // Clear token if invalid
      localStorage.removeItem('accessToken');
      localStorage.removeItem('userId');
      setUser(null);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchCurrentUser();
  }, [fetchCurrentUser]);

  const handleLogin = async (credentials) => {
    setAuthError(null);
    try {
      const res = await loginUser(credentials);
      // Backend returns ServiceResult<AuthResponseDTO>
      const authData = res.data || res.Data || res;
      if (authData.token) {
        localStorage.setItem('accessToken', authData.token);
      }
      const userId = authData.user?.id || authData.user?.Id || authData.id;
      if (userId) {
        localStorage.setItem('userId', userId.toString());
      }
      await fetchCurrentUser();
      return { success: true };
    } catch (err) {
      const msg = parseApiError(err);
      setAuthError(msg);
      return { success: false, error: msg };
    }
  };

  const handleRegister = async (data) => {
    setAuthError(null);
    try {
      const res = await registerUser(data);
      const authData = res.data || res.Data || res;
      if (authData.token) {
        localStorage.setItem('accessToken', authData.token);
      }
      const userId = authData.user?.id || authData.user?.Id || authData.id;
      if (userId) {
        localStorage.setItem('userId', userId.toString());
      }
      await fetchCurrentUser();
      return { success: true };
    } catch (err) {
      const msg = parseApiError(err);
      setAuthError(msg);
      return { success: false, error: msg };
    }
  };

  const handleLogout = async () => {
    const userId = localStorage.getItem('userId');
    if (userId) {
      try {
        await logoutUser(userId);
      } catch (err) {
        console.error('Logout error:', err);
      }
    }
    localStorage.removeItem('accessToken');
    localStorage.removeItem('userId');
    setUser(null);
  };

  const refreshProfile = async () => {
    await fetchCurrentUser();
  };

  return (
    <AuthContext.Provider
      value={{
        user,
        loading,
        authError,
        login: handleLogin,
        register: handleRegister,
        logout: handleLogout,
        refreshProfile,
        isAuthenticated: !!user,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};
