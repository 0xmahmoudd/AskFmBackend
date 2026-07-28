import React, { useState, useEffect, useCallback } from 'react';
import { getBlockedUsers, unblockUser } from '../api/moderation';
import { useNotification } from '../context/NotificationContext';
import { parseApiError } from '../api/client';
import { Avatar } from '../components/Avatar';
import { Spinner } from '../components/Spinner';
import { EmptyState } from '../components/EmptyState';
import { Pagination } from '../components/Pagination';
import { ShieldAlert, Unlock } from 'lucide-react';

export const BlockedUsersPage = () => {
  const { addToast } = useNotification();
  const [blockedUsers, setBlockedUsers] = useState([]);
  const [page, setPage] = useState(1);
  const [totalItems, setTotalItems] = useState(0);
  const [loading, setLoading] = useState(true);

  const fetchBlocked = useCallback(async () => {
    setLoading(true);
    try {
      const res = await getBlockedUsers(page, 10);
      const data = res.data || res.Data || res;
      if (data.items || data.Items) {
        setBlockedUsers(data.items || data.Items);
        setTotalItems(data.totalCount || data.TotalCount || 0);
      } else if (Array.isArray(data)) {
        setBlockedUsers(data);
        setTotalItems(data.length);
      } else {
        setBlockedUsers([]);
      }
    } catch (err) {
      console.error('Failed to fetch blocked users:', err);
      setBlockedUsers([]);
    } finally {
      setLoading(false);
    }
  }, [page]);

  useEffect(() => {
    fetchBlocked();
  }, [fetchBlocked]);

  const handleUnblock = async (userId, name) => {
    try {
      await unblockUser(userId);
      addToast(`Unblocked ${name || 'user'}`, 'success');
      fetchBlocked();
    } catch (err) {
      addToast(parseApiError(err), 'error');
    }
  };

  return (
    <div>
      <div style={{ display: 'flex', alignItems: 'center', gap: '8px', marginBottom: '16px' }}>
        <ShieldAlert style={{ width: '24px', height: '24px', color: '#ef4444' }} />
        <h2 style={{ fontSize: '20px', fontWeight: '800' }}>Blocked & Moderated Users</h2>
      </div>

      {loading ? (
        <Spinner />
      ) : blockedUsers.length === 0 ? (
        <EmptyState title="No blocked users" message="You have not blocked any users." icon={ShieldAlert} />
      ) : (
        <div className="card">
          <div style={{ display: 'flex', flexDirection: 'column', gap: '12px' }}>
            {blockedUsers.map((u) => {
              const uId = u.id || u.Id;
              const uName = u.name || u.Name || 'User';

              return (
                <div
                  key={uId}
                  style={{
                    display: 'flex',
                    alignItems: 'center',
                    justify: 'space-between',
                    padding: '10px 0',
                    borderBottom: '1px solid var(--border-color)',
                  }}
                >
                  <div style={{ display: 'flex', alignItems: 'center', gap: '12px' }}>
                    <Avatar src={u.avatarPath || u.AvatarPath} name={uName} size="md" />
                    <div>
                      <h4 style={{ fontWeight: '700', fontSize: '15px' }}>{uName}</h4>
                      <p style={{ fontSize: '13px', color: 'var(--text-muted)' }}>{u.email || u.Email}</p>
                    </div>
                  </div>

                  <button className="btn btn-secondary" onClick={() => handleUnblock(uId, uName)}>
                    <Unlock style={{ width: '16px', height: '16px' }} /> Unblock
                  </button>
                </div>
              );
            })}
          </div>
          <Pagination page={page} pageSize={10} totalItems={totalItems} onPageChange={setPage} />
        </div>
      )}
    </div>
  );
};
