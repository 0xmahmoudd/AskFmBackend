import React, { useState, useEffect, useCallback } from 'react';
import { getSavedThreads } from '../api/thread';
import { ThreadCard } from '../components/ThreadCard';
import { Spinner } from '../components/Spinner';
import { EmptyState } from '../components/EmptyState';
import { Pagination } from '../components/Pagination';
import { Bookmark } from 'lucide-react';

export const SavedThreadsPage = () => {
  const [threads, setThreads] = useState([]);
  const [page, setPage] = useState(1);
  const [totalItems, setTotalItems] = useState(0);
  const [loading, setLoading] = useState(true);

  const fetchSaved = useCallback(async () => {
    setLoading(true);
    try {
      const res = await getSavedThreads(page, 10);
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
      console.error('Failed to load saved threads:', err);
      setThreads([]);
    } finally {
      setLoading(false);
    }
  }, [page]);

  useEffect(() => {
    fetchSaved();
  }, [fetchSaved]);

  return (
    <div>
      <div style={{ display: 'flex', alignItems: 'center', gap: '8px', marginBottom: '16px' }}>
        <Bookmark style={{ width: '24px', height: '24px', color: 'var(--primary)' }} />
        <h2 style={{ fontSize: '20px', fontWeight: '800' }}>Saved Threads</h2>
      </div>

      {loading ? (
        <Spinner />
      ) : threads.length === 0 ? (
        <EmptyState
          title="No saved threads"
          message="Click the bookmark icon on any thread to save it for quick reference here!"
          icon={Bookmark}
        />
      ) : (
        <div>
          {threads.map((thread) => (
            <ThreadCard key={thread.id || thread.Id} thread={thread} onUpdate={fetchSaved} />
          ))}
          <Pagination page={page} pageSize={10} totalItems={totalItems} onPageChange={setPage} />
        </div>
      )}
    </div>
  );
};
