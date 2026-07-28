import React, { useState, useEffect, useCallback } from 'react';
import { getPersonalizedFeed } from '../api/thread';
import { useAuth } from '../context/AuthContext';
import { ThreadCard } from '../components/ThreadCard';
import { Spinner } from '../components/Spinner';
import { EmptyState } from '../components/EmptyState';
import { Pagination } from '../components/Pagination';

export const HomePage = () => {
  const { user } = useAuth();
  const [threads, setThreads] = useState([]);
  const [page, setPage] = useState(1);
  const [totalItems, setTotalItems] = useState(0);
  const [loading, setLoading] = useState(true);

  const fetchFeed = useCallback(async () => {
    setLoading(true);
    try {
      const res = await getPersonalizedFeed(page, 10);
      const data = res.data || res.Data || res;
      if (data.items || data.Items) {
        setThreads(data.items || data.Items);
        setTotalItems(data.totalCount || data.TotalCount || 0);
      } else if (Array.isArray(data)) {
        setThreads(data);
        setTotalItems(data.length);
      } else {
        setThreads([]);
      }
    } catch (err) {
      console.error('Failed to load feed:', err);
      setThreads([]);
    } finally {
      setLoading(false);
    }
  }, [page]);

  useEffect(() => {
    fetchFeed();

    const handleNotification = () => {
      fetchFeed();
    };

    window.addEventListener('askfm_notification', handleNotification);
    return () => window.removeEventListener('askfm_notification', handleNotification);
  }, [fetchFeed]);

  return (
    <div>
      <h2 style={{ fontSize: '18px', fontWeight: '800', margin: '20px 0 12px' }}>Your Feed</h2>

      {loading ? (
        <Spinner />
      ) : threads.length === 0 ? (
        <EmptyState
          title="Your feed is empty"
          message="Follow users or ask questions to see activity in your personalized feed!"
        />
      ) : (
        <div>
          {threads.map((thread) => (
            <ThreadCard key={thread.id || thread.Id} thread={thread} onUpdate={fetchFeed} />
          ))}
          <Pagination page={page} pageSize={10} totalItems={totalItems} onPageChange={setPage} />
        </div>
      )}
    </div>
  );
};
