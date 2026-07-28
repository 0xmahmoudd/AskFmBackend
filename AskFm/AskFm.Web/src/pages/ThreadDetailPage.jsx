import React, { useState, useEffect, useCallback } from 'react';
import { useParams, Link } from 'react-router-dom';
import { getThreadById } from '../api/thread';
import { ThreadCard } from '../components/ThreadCard';
import { Spinner } from '../components/Spinner';
import { EmptyState } from '../components/EmptyState';
import { ArrowLeft } from 'lucide-react';

export const ThreadDetailPage = () => {
  const { id } = useParams();
  const [thread, setThread] = useState(null);
  const [loading, setLoading] = useState(true);

  const fetchThread = useCallback(async () => {
    setLoading(true);
    try {
      const res = await getThreadById(id);
      setThread(res.data || res.Data || res);
    } catch (err) {
      console.error('Error fetching thread:', err);
      setThread(null);
    } finally {
      setLoading(false);
    }
  }, [id]);

  useEffect(() => {
    fetchThread();
  }, [fetchThread]);

  return (
    <div>
      <div style={{ marginBottom: '16px' }}>
        <Link to="/" style={{ display: 'inline-flex', alignItems: 'center', gap: '6px', color: 'var(--text-muted)', fontSize: '14px', fontWeight: '500' }}>
          <ArrowLeft style={{ width: '16px', height: '16px' }} /> Back
        </Link>
      </div>

      {loading ? (
        <Spinner />
      ) : !thread ? (
        <EmptyState title="Thread not found" message="This thread may have been deleted or is private." />
      ) : (
        <ThreadCard thread={thread} onUpdate={fetchThread} />
      )}
    </div>
  );
};
