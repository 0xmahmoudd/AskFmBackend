import React, { useState, useEffect, useCallback } from 'react';
import { getPublicThreads } from '../api/thread';
import { ThreadCard } from '../components/ThreadCard';
import { Spinner } from '../components/Spinner';
import { EmptyState } from '../components/EmptyState';
import { Pagination } from '../components/Pagination';
import { Compass } from 'lucide-react';

export const ExplorePage = () => {
  const [threads, setThreads] = useState([]);
  const [page, setPage] = useState(1);
  const [totalItems, setTotalItems] = useState(0);
  const [loading, setLoading] = useState(true);

  const fetchPublicFeed = useCallback(async () => {
    setLoading(true);
    try {
      const res = await getPublicThreads(page, 10);
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
      console.error('Failed to load explore feed:', err);
      setThreads([]);
    } finally {
      setLoading(false);
    }
  }, [page]);

  useEffect(() => {
    fetchPublicFeed();
  }, [fetchPublicFeed]);

  return (
    <div>
      <div style={{ display: 'flex', alignItems: 'center', gap: '8px', marginBottom: '16px' }}>
        <Compass style={{ width: '24px', height: '24px', color: 'var(--primary)' }} />
        <h2 style={{ fontSize: '20px', fontWeight: '800' }}>Explore Global Q&A</h2>
      </div>

      {loading ? (
        <Spinner />
      ) : threads.length === 0 ? (
        <EmptyState title="No public questions yet" message="Be the first to ask or answer a question publicly!" />
      ) : (
        <div>
          {threads.map((thread) => (
            <ThreadCard key={thread.id || thread.Id} thread={thread} onUpdate={fetchPublicFeed} />
          ))}
          <Pagination page={page} pageSize={10} totalItems={totalItems} onPageChange={setPage} />
        </div>
      )}
    </div>
  );
};
