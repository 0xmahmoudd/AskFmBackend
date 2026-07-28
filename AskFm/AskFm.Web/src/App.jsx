import React from 'react';
import { BrowserRouter, Routes, Route, Navigate, Outlet } from 'react-router-dom';
import { AuthProvider, useAuth } from './context/AuthContext';
import { NotificationProvider } from './context/NotificationContext';
import { Navbar } from './components/Navbar';
import { Sidebar } from './components/Sidebar';
import { ToastContainer } from './components/ToastContainer';
import { Spinner } from './components/Spinner';

// Pages
import { LoginPage } from './pages/LoginPage';
import { RegisterPage } from './pages/RegisterPage';
import { ForgotPasswordPage } from './pages/ForgotPasswordPage';
import { ResetPasswordPage } from './pages/ResetPasswordPage';
import { ConfirmEmailPage } from './pages/ConfirmEmailPage';
import { HomePage } from './pages/HomePage';
import { ExplorePage } from './pages/ExplorePage';
import { InboxPage } from './pages/InboxPage';
import { SavedThreadsPage } from './pages/SavedThreadsPage';
import { ThreadDetailPage } from './pages/ThreadDetailPage';
import { UserProfilePage } from './pages/UserProfilePage';
import { SearchUsersPage } from './pages/SearchUsersPage';
import { SettingsPage } from './pages/SettingsPage';
import { NotificationsPage } from './pages/NotificationsPage';
import { BlockedUsersPage } from './pages/BlockedUsersPage';

const ProtectedLayout = () => {
  const { isAuthenticated, loading } = useAuth();

  if (loading) {
    return <Spinner />;
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  return (
    <div className="app-container">
      <Navbar />
      <div className="main-layout">
        <Sidebar />
        <main className="content-area">
          <Outlet />
        </main>
      </div>
      <ToastContainer />
    </div>
  );
};

const PublicLayout = () => {
  const { loading } = useAuth();

  if (loading) {
    return <Spinner />;
  }

  return (
    <div className="app-container">
      <Navbar />
      <main style={{ flex: 1, padding: '20px' }}>
        <Outlet />
      </main>
      <ToastContainer />
    </div>
  );
};

export function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <NotificationProvider>
          <Routes>
            {/* Public Routes */}
            <Route element={<PublicLayout />}>
              <Route path="/login" element={<LoginPage />} />
              <Route path="/register" element={<RegisterPage />} />
              <Route path="/forgot-password" element={<ForgotPasswordPage />} />
              <Route path="/reset-password" element={<ResetPasswordPage />} />
              <Route path="/confirm-email" element={<ConfirmEmailPage />} />
            </Route>

            {/* Protected Routes */}
            <Route element={<ProtectedLayout />}>
              <Route path="/" element={<HomePage />} />
              <Route path="/explore" element={<ExplorePage />} />
              <Route path="/inbox" element={<InboxPage />} />
              <Route path="/saved" element={<SavedThreadsPage />} />
              <Route path="/thread/:id" element={<ThreadDetailPage />} />
              <Route path="/profile/:id" element={<UserProfilePage />} />
              <Route path="/search" element={<SearchUsersPage />} />
              <Route path="/settings/profile" element={<SettingsPage />} />
              <Route path="/notifications" element={<NotificationsPage />} />
              <Route path="/settings/blocked" element={<BlockedUsersPage />} />
            </Route>

            <Route path="*" element={<Navigate to="/" replace />} />
          </Routes>
        </NotificationProvider>
      </AuthProvider>
    </BrowserRouter>
  );
}

export default App;
