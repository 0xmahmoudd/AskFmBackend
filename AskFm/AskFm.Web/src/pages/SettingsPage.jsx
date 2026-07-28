import React, { useState } from 'react';
import { useAuth } from '../context/AuthContext';
import { useNotification } from '../context/NotificationContext';
import { updateUserProfile, uploadAvatar, updatePassword, requestEmailChange, confirmEmailChange, deleteUserAccount } from '../api/user';
import { parseApiError } from '../api/client';
import { Avatar } from '../components/Avatar';
import { User, Lock, Mail, Upload, Trash2, CheckCircle } from 'lucide-react';

export const SettingsPage = () => {
  const { user, refreshProfile, logout } = useAuth();
  const { addToast } = useNotification();

  const userId = user?.id || user?.Id;

  // Profile info
  const [name, setName] = useState(user?.name || user?.Name || '');
  const [bio, setBio] = useState(user?.bio || user?.Bio || '');
  const [updatingProfile, setUpdatingProfile] = useState(false);

  // Avatar upload
  const [avatarFile, setAvatarFile] = useState(null);
  const [uploadingAvatar, setUploadingAvatar] = useState(false);

  // Password update
  const [oldPassword, setOldPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [updatingPass, setUpdatingPass] = useState(false);

  // Email update
  const [newEmail, setNewEmail] = useState('');
  const [emailToken, setEmailToken] = useState('');
  const [emailStep, setEmailStep] = useState(1); // 1 = request, 2 = confirm
  const [updatingEmail, setUpdatingEmail] = useState(false);

  const handleUpdateProfile = async (e) => {
    e.preventDefault();
    setUpdatingProfile(true);
    try {
      await updateUserProfile(userId, { name, bio });
      await refreshProfile();
      addToast('Profile updated successfully!', 'success');
    } catch (err) {
      addToast(parseApiError(err), 'error');
    } finally {
      setUpdatingProfile(false);
    }
  };

  const handleUploadAvatar = async (e) => {
    e.preventDefault();
    if (!avatarFile) return;
    setUploadingAvatar(true);
    try {
      await uploadAvatar(userId, avatarFile);
      await refreshProfile();
      addToast('Avatar uploaded successfully!', 'success');
      setAvatarFile(null);
    } catch (err) {
      addToast(parseApiError(err), 'error');
    } finally {
      setUploadingAvatar(false);
    }
  };

  const handleUpdatePassword = async (e) => {
    e.preventDefault();
    setUpdatingPass(true);
    try {
      await updatePassword(userId, { oldPassword, newPassword });
      addToast('Password changed successfully!', 'success');
      setOldPassword('');
      setNewPassword('');
    } catch (err) {
      addToast(parseApiError(err), 'error');
    } finally {
      setUpdatingPass(false);
    }
  };

  const handleRequestEmail = async (e) => {
    e.preventDefault();
    setUpdatingEmail(true);
    try {
      const res = await requestEmailChange(userId, newEmail);
      addToast(typeof res === 'string' ? res : 'Confirmation token sent to your new email.', 'info');
      setEmailStep(2);
    } catch (err) {
      addToast(parseApiError(err), 'error');
    } finally {
      setUpdatingEmail(false);
    }
  };

  const handleConfirmEmail = async (e) => {
    e.preventDefault();
    setUpdatingEmail(true);
    try {
      await confirmEmailChange(userId, newEmail, emailToken);
      await refreshProfile();
      addToast('Email updated successfully!', 'success');
      setNewEmail('');
      setEmailToken('');
      setEmailStep(1);
    } catch (err) {
      addToast(parseApiError(err), 'error');
    } finally {
      setUpdatingEmail(false);
    }
  };

  const handleDeleteAccount = async () => {
    if (!window.confirm('WARNING: Are you sure you want to permanently delete your account? This action cannot be undone.')) return;
    try {
      await deleteUserAccount(userId);
      addToast('Account deleted', 'info');
      logout();
    } catch (err) {
      addToast(parseApiError(err), 'error');
    }
  };

  return (
    <div style={{ maxWidth: '640px', margin: '0 auto' }}>
      <h2 style={{ fontSize: '22px', fontWeight: '800', marginBottom: '20px' }}>Account Settings</h2>

      {/* Profile & Avatar */}
      <div className="card">
        <h3 style={{ fontSize: '16px', fontWeight: '700', marginBottom: '16px', display: 'flex', alignItems: 'center', gap: '8px' }}>
          <User style={{ width: '18px', height: '18px', color: 'var(--primary)' }} /> Edit Profile
        </h3>

        <form onSubmit={handleUploadAvatar} style={{ display: 'flex', alignItems: 'center', gap: '16px', marginBottom: '20px', paddingBottom: '16px', borderBottom: '1px solid var(--border-color)' }}>
          <Avatar src={user?.avatarPath || user?.AvatarPath} name={user?.name || user?.Name} size="lg" />
          <div style={{ flex: 1 }}>
            <label className="form-label">Upload New Avatar</label>
            <input
              type="file"
              accept="image/*"
              className="form-input"
              onChange={(e) => setAvatarFile(e.target.files[0])}
            />
          </div>
          <button type="submit" className="btn btn-secondary" disabled={uploadingAvatar || !avatarFile} style={{ marginTop: '20px' }}>
            <Upload style={{ width: '14px', height: '14px' }} />
            {uploadingAvatar ? 'Uploading...' : 'Upload'}
          </button>
        </form>

        <form onSubmit={handleUpdateProfile}>
          <div className="form-group">
            <label className="form-label">Full Name</label>
            <input
              type="text"
              className="form-input"
              value={name}
              onChange={(e) => setName(e.target.value)}
              required
            />
          </div>

          <div className="form-group">
            <label className="form-label">Bio</label>
            <textarea
              className="form-textarea"
              rows={3}
              value={bio}
              onChange={(e) => setBio(e.target.value)}
            />
          </div>

          <button type="submit" className="btn btn-primary" disabled={updatingProfile}>
            {updatingProfile ? 'Saving...' : 'Save Profile Changes'}
          </button>
        </form>
      </div>

      {/* Security & Password */}
      <div className="card">
        <h3 style={{ fontSize: '16px', fontWeight: '700', marginBottom: '16px', display: 'flex', alignItems: 'center', gap: '8px' }}>
          <Lock style={{ width: '18px', height: '18px', color: 'var(--primary)' }} /> Security & Password
        </h3>

        <form onSubmit={handleUpdatePassword}>
          <div className="form-group">
            <label className="form-label">Current Password</label>
            <input
              type="password"
              className="form-input"
              value={oldPassword}
              onChange={(e) => setOldPassword(e.target.value)}
              required
            />
          </div>

          <div className="form-group">
            <label className="form-label">New Password</label>
            <input
              type="password"
              className="form-input"
              value={newPassword}
              onChange={(e) => setNewPassword(e.target.value)}
              required
            />
          </div>

          <button type="submit" className="btn btn-primary" disabled={updatingPass || !oldPassword || !newPassword}>
            {updatingPass ? 'Updating...' : 'Update Password'}
          </button>
        </form>
      </div>

      {/* Email Change */}
      <div className="card">
        <h3 style={{ fontSize: '16px', fontWeight: '700', marginBottom: '16px', display: 'flex', alignItems: 'center', gap: '8px' }}>
          <Mail style={{ width: '18px', height: '18px', color: 'var(--primary)' }} /> Update Email Address
        </h3>

        {emailStep === 1 ? (
          <form onSubmit={handleRequestEmail}>
            <div className="form-group">
              <label className="form-label">New Email Address</label>
              <input
                type="email"
                className="form-input"
                placeholder="newname@example.com"
                value={newEmail}
                onChange={(e) => setNewEmail(e.target.value)}
                required
              />
            </div>
            <button type="submit" className="btn btn-primary" disabled={updatingEmail || !newEmail}>
              {updatingEmail ? 'Sending...' : 'Request Email Change'}
            </button>
          </form>
        ) : (
          <form onSubmit={handleConfirmEmail}>
            <div className="form-group">
              <label className="form-label">Confirmation Token</label>
              <input
                type="text"
                className="form-input"
                placeholder="Paste token sent to new email"
                value={emailToken}
                onChange={(e) => setEmailToken(e.target.value)}
                required
              />
            </div>
            <div style={{ display: 'flex', gap: '10px' }}>
              <button type="button" className="btn btn-secondary" onClick={() => setEmailStep(1)}>
                Cancel
              </button>
              <button type="submit" className="btn btn-primary" disabled={updatingEmail || !emailToken}>
                <CheckCircle style={{ width: '16px', height: '16px' }} />
                {updatingEmail ? 'Confirming...' : 'Confirm Email Change'}
              </button>
            </div>
          </form>
        )}
      </div>

      {/* Danger Zone */}
      <div className="card" style={{ border: '1px solid #fecaca', background: '#fff5f5' }}>
        <h3 style={{ fontSize: '16px', fontWeight: '700', color: '#dc2626', marginBottom: '8px' }}>Danger Zone</h3>
        <p style={{ fontSize: '13px', color: 'var(--text-muted)', marginBottom: '14px' }}>
          Deleting your account will permanently wipe your profile, threads, comments, and notifications.
        </p>
        <button className="btn btn-danger" onClick={handleDeleteAccount}>
          <Trash2 style={{ width: '16px', height: '16px' }} /> Delete My Account
        </button>
      </div>
    </div>
  );
};
