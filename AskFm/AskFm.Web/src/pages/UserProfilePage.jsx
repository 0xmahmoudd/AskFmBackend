import React, { useState, useEffect, useCallback } from 'react';
import { useParams, Link } from 'react-router-dom';
import { getUserById, getFollowers, getFollowing, followUser, unfollowUser, getFollowStatus } from '../api/user';
import { getThreadsForUser } from '../api/thread';
import { blockUser, muteUser } from '../api/moderation';
import { useAuth } from '../context/AuthContext';
import { useNotification } from '../context/NotificationContext';
import { parseApiError } from '../api/client';
import { Avatar } from '../components/Avatar';
import { AskBox } from '../components/AskBox';
import { ThreadCard } from '../components/ThreadCard';
import { Spinner } from '../components/Spinner';
import { EmptyState } from '../components/EmptyState';
import { Pagination } from '../components/Pagination';
import { UserPlus, UserCheck, ShieldAlert, VolumeX, Users, MessageSquare } from 'lucide-react';

export const UserProfilePage = () => {
  const { id } = useParams();
  const { user: currentUser } = useAuth();
  const { addToast } = useNotification();

  const isMe = !id || id === 'me' || id === (currentUser?.id || currentUser?.Id)?.toString();
  const targetUserId = isMe ? (currentUser?.id || currentUser?.Id) : parseInt(id, 10);

  const [profile, setProfile] = useState(null);
  const [activeTab, setActiveTab] = useState('threads'); // 'threads' | 'followers' | 'following'
  const [threads, setThreads] = useState([]);
  const [followers, setFollowers] = useState([]);
  const [following, setFollowing] = useState([]);
  const [isFollowing, setIsFollowing] = useState(false);
  const [page, setPage] = useState(1);
  const [totalItems, setTotalItems] = useState(0);
  const [loading, setLoading] = useState(true);

  const fetchProfileData = useCallback(async () => {
    if (!targetUserId) return;
    setLoading(true);
    try {
      if (isMe && currentUser) {
        setProfile(currentUser);
      } else {
        const res = await getUserById(targetUserId);
        setProfile(res.data || res.Data || res);

        try {
          const statusRes = await getFollowStatus(targetUserId);
          setIsFollowing(statusRes.isFollowing || false);
        } catch (e) {
          // ignore error if guest
        }
      }
    } catch (err) {
      console.error('Failed to load user profile:', err);
    } finally {
      setLoading(false);
    }
  }, [targetUserId, isMe, currentUser]);

  const fetchTabData = useCallback(async () => {
    if (!targetUserId) return;
    try {
      if (activeTab === 'threads') {
        const res = await getThreadsForUser(targetUserId, page, 10);
        const data = res.data || res.Data || res;
        if (data.items || data.Items) {
          setThreads(data.items || data.Items);
          setTotalItems(data.totalCount || data.TotalCount || 0);
        } else if (Array.isArray(data)) {
          setThreads(data);
          setTotalItems(data.length);
        }
      } else if (activeTab === 'followers') {
        const res = await getFollowers(targetUserId, page, 10);
        const data = res.data || res.Data || res;
        setFollowers(data.items || data.Items || (Array.isArray(data) ? data : []));
        setTotalItems(data.totalCount || data.TotalCount || 0);
      } else if (activeTab === 'following') {
        const res = await getFollowing(targetUserId, page, 10);
        const data = res.data || res.Data || res;
        setFollowing(data.items || data.Items || (Array.isArray(data) ? data : []));
        setTotalItems(data.totalCount || data.TotalCount || 0);
      }
    } catch (err) {
      console.error('Failed to load tab data:', err);
    }
  }, [targetUserId, activeTab, page]);

  useEffect(() => {
    fetchProfileData();
  }, [fetchProfileData]);

  useEffect(() => {
    fetchTabData();
  }, [fetchTabData]);

  const handleToggleFollow = async () => {
    const followerId = currentUser?.id || currentUser?.Id;
    try {
      if (isFollowing) {
        await unfollowUser(followerId, targetUserId);
        setIsFollowing(false);
        addToast(`Unfollowed ${profile?.name || 'user'}`, 'info');
      } else {
        await followUser(followerId, targetUserId);
        setIsFollowing(true);
        addToast(`Following ${profile?.name || 'user'}!`, 'success');
      }
      fetchProfileData();
    } catch (err) {
      addToast(parseApiError(err), 'error');
    }
  };

  const handleBlockUser = async () => {
    if (!window.confirm(`Block ${profile?.name}? You won't see their activity.`)) return;
    try {
      await blockUser(targetUserId);
      addToast(`${profile?.name} has been blocked.`, 'info');
    } catch (err) {
      addToast(parseApiError(err), 'error');
    }
  };

  const handleMuteUser = async () => {
    try {
      await muteUser(targetUserId);
      addToast(`${profile?.name} has been muted.`, 'info');
    } catch (err) {
      addToast(parseApiError(err), 'error');
    }
  };

  if (loading && !profile) return <Spinner />;

  return (
    <div>
      {/* Profile Header */}
      <div className="card" style={{ textAlign: 'center', padding: '30px 20px' }}>
        <div style={{ display: 'flex', justifyContent: 'center', marginBottom: '16px' }}>
          <Avatar src={profile?.avatarPath || profile?.AvatarPath} name={profile?.name || profile?.Name} size="lg" />
        </div>
        <h2 style={{ fontSize: '22px', fontWeight: '800' }}>{profile?.name || profile?.Name || 'User'}</h2>
        <p style={{ color: 'var(--text-muted)', fontSize: '14px', margin: '4px 0 12px' }}>{profile?.email || profile?.Email}</p>

        {profile?.bio || profile?.Bio ? (
          <p style={{ maxWidth: '500px', margin: '0 auto 16px', fontSize: '14px', color: 'var(--text-main)' }}>
            {profile?.bio || profile?.Bio}
          </p>
        ) : null}

        <div style={{ display: 'flex', justifyContent: 'center', gap: '24px', margin: '16px 0', fontSize: '14px' }}>
          <div>
            <span style={{ fontWeight: '800', display: 'block', fontSize: '18px' }}>
              {profile?.followerCount || profile?.FollowersCount || 0}
            </span>
            <span style={{ color: 'var(--text-muted)' }}>Followers</span>
          </div>
          <div>
            <span style={{ fontWeight: '800', display: 'block', fontSize: '18px' }}>
              {profile?.followingCount || profile?.FollowingCount || 0}
            </span>
            <span style={{ color: 'var(--text-muted)' }}>Following</span>
          </div>
        </div>

        {!isMe && (
          <div style={{ display: 'flex', justifyContent: 'center', gap: '10px', marginTop: '16px' }}>
            <button
              className={`btn ${isFollowing ? 'btn-secondary' : 'btn-primary'}`}
              onClick={handleToggleFollow}
            >
              {isFollowing ? (
                <>
                  <UserCheck style={{ width: '16px', height: '16px' }} /> Following
                </>
              ) : (
                <>
                  <UserPlus style={{ width: '16px', height: '16px' }} /> Follow
                </>
              )}
            </button>
            <button className="btn btn-secondary" onClick={handleMuteUser} title="Mute user">
              <VolumeX style={{ width: '16px', height: '16px' }} />
            </button>
            <button className="btn btn-danger" onClick={handleBlockUser} title="Block user">
              <ShieldAlert style={{ width: '16px', height: '16px' }} />
            </button>
          </div>
        )}
      </div>

      {/* Ask widget on user profile */}
      {!isMe && profile && <AskBox targetUser={profile} onQuestionAsked={fetchTabData} />}

      {/* Tabs */}
      <div style={{ display: 'flex', borderBottom: '1px solid var(--border-color)', marginBottom: '16px' }}>
        <button
          className={`nav-link ${activeTab === 'threads' ? 'active' : ''}`}
          onClick={() => {
            setActiveTab('threads');
            setPage(1);
          }}
          style={{ padding: '12px 20px', borderRadius: 0, borderBottom: activeTab === 'threads' ? '2px solid var(--primary)' : 'none' }}
        >
          <MessageSquare style={{ width: '16px', height: '16px' }} /> Threads
        </button>
        <button
          className={`nav-link ${activeTab === 'followers' ? 'active' : ''}`}
          onClick={() => {
            setActiveTab('followers');
            setPage(1);
          }}
          style={{ padding: '12px 20px', borderRadius: 0, borderBottom: activeTab === 'followers' ? '2px solid var(--primary)' : 'none' }}
        >
          <Users style={{ width: '16px', height: '16px' }} /> Followers
        </button>
        <button
          className={`nav-link ${activeTab === 'following' ? 'active' : ''}`}
          onClick={() => {
            setActiveTab('following');
            setPage(1);
          }}
          style={{ padding: '12px 20px', borderRadius: 0, borderBottom: activeTab === 'following' ? '2px solid var(--primary)' : 'none' }}
        >
          <Users style={{ width: '16px', height: '16px' }} /> Following
        </button>
      </div>

      {/* Tab Content */}
      {activeTab === 'threads' && (
        <div>
          {threads.length === 0 ? (
            <EmptyState title="No threads yet" message="There are no questions or answers on this profile." />
          ) : (
            threads.map((thread) => (
              <ThreadCard key={thread.id || thread.Id} thread={thread} onUpdate={fetchTabData} />
            ))
          )}
          <Pagination page={page} pageSize={10} totalItems={totalItems} onPageChange={setPage} />
        </div>
      )}

      {(activeTab === 'followers' || activeTab === 'following') && (
        <div className="card">
          {((activeTab === 'followers' ? followers : following) || []).length === 0 ? (
            <EmptyState title={`No ${activeTab}`} message={`This profile has no ${activeTab} yet.`} />
          ) : (
            <div style={{ display: 'flex', flexDirection: 'column', gap: '12px' }}>
              {(activeTab === 'followers' ? followers : following).map((u) => (
                <div key={u.id || u.Id} style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', padding: '8px 0', borderBottom: '1px solid var(--border-color)' }}>
                  <Link to={`/profile/${u.id || u.Id}`} style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
                    <Avatar src={u.avatarPath || u.AvatarPath} name={u.name || u.Name} size="sm" />
                    <div>
                      <p style={{ fontWeight: '600', fontSize: '14px' }}>{u.name || u.Name}</p>
                      <p style={{ fontSize: '12px', color: 'var(--text-muted)' }}>{u.email || u.Email}</p>
                    </div>
                  </Link>
                </div>
              ))}
              <Pagination page={page} pageSize={10} totalItems={totalItems} onPageChange={setPage} />
            </div>
          )}
        </div>
      )}
    </div>
  );
};
