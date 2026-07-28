import React, { useState, useEffect, useCallback } from 'react';
import { useSearchParams, Link } from 'react-router-dom';
import { searchUsers } from '../api/user';
import { Avatar } from '../components/Avatar';
import { Spinner } from '../components/Spinner';
import { EmptyState } from '../components/EmptyState';
import { Pagination } from '../components/Pagination';
import { Search, User } from 'lucide-react';

export const SearchUsersPage = () => {
  const [searchParams, setSearchParams] = useSearchParams();
  const query = searchParams.get('q') || '';
  const [users, setUsers] = useState([]);
  const [page, setPage] = useState(1);
  const [totalItems, setTotalItems] = useState(0);
  const [loading, setLoading] = useState(false);
  const [inputText, setInputText] = useState(query);

  const handleSearch = useCallback(async () => {
    if (!query.trim()) {
      setUsers([]);
      setTotalItems(0);
      return;
    }
    setLoading(true);
    try {
      const res = await searchUsers(query, page, 10);
      const data = res.data || res.Data || res;
      if (data.items || data.Items) {
        setUsers(data.items || data.Items);
        setTotalItems(data.totalCount || data.TotalCount || 0);
      } else if (Array.isArray(data)) {
        setUsers(data);
        setTotalItems(data.length);
      } else {
        setUsers([]);
      }
    } catch (err) {
      console.error('User search failed:', err);
      setUsers([]);
    } finally {
      setLoading(false);
    }
  }, [query, page]);

  useEffect(() => {
    handleSearch();
  }, [handleSearch]);

  const handleSubmit = (e) => {
    e.preventDefault();
    if (inputText.trim()) {
      setSearchParams({ q: inputText.trim() });
      setPage(1);
    }
  };

  return (
    <div>
      <div className="card" style={{ marginBottom: '20px' }}>
        <form onSubmit={handleSubmit} style={{ display: 'flex', gap: '10px' }}>
          <div style={{ position: 'relative', flex: 1 }}>
            <Search
              style={{
                position: 'absolute',
                left: '12px',
                top: '50%',
                transform: 'translateY(-50%)',
                width: '18px',
                height: '18px',
                color: 'var(--text-muted)',
              }}
            />
            <input
              type="text"
              className="form-input"
              placeholder="Search users by name or email..."
              value={inputText}
              onChange={(e) => setInputText(e.target.value)}
              style={{ paddingLeft: '38px' }}
            />
          </div>
          <button type="submit" className="btn btn-primary">
            Search
          </button>
        </form>
      </div>

      <h3 style={{ fontSize: '16px', fontWeight: '700', marginBottom: '12px' }}>
        {query ? `Search Results for "${query}"` : 'Search Users'}
      </h3>

      {loading ? (
        <Spinner />
      ) : users.length === 0 ? (
        <EmptyState
          title={query ? 'No users found' : 'Type a name to search'}
          message={query ? 'Try searching for another name or keyword.' : 'Find friends and interesting people to ask questions.'}
          icon={User}
        />
      ) : (
        <div className="card">
          <div style={{ display: 'flex', flexDirection: 'column', gap: '14px' }}>
            {users.map((u) => (
              <div
                key={u.id || u.Id}
                style={{
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'space-between',
                  paddingBottom: '12px',
                  borderBottom: '1px solid var(--border-color)',
                }}
              >
                <Link to={`/profile/${u.id || u.Id}`} style={{ display: 'flex', alignItems: 'center', gap: '12px' }}>
                  <Avatar src={u.avatarPath || u.AvatarPath} name={u.name || u.Name} size="md" />
                  <div>
                    <h4 style={{ fontWeight: '700', fontSize: '15px' }}>{u.name || u.Name}</h4>
                    <p style={{ fontSize: '13px', color: 'var(--text-muted)' }}>{u.email || u.Email}</p>
                    {u.bio || u.Bio ? (
                      <p style={{ fontSize: '12px', color: 'var(--text-main)', marginTop: '2px' }}>{u.bio || u.Bio}</p>
                    ) : null}
                  </div>
                </Link>

                <Link to={`/profile/${u.id || u.Id}`} className="btn btn-secondary">
                  View Profile
                </Link>
              </div>
            ))}
          </div>
          <Pagination page={page} pageSize={10} totalItems={totalItems} onPageChange={setPage} />
        </div>
      )}
    </div>
  );
};
