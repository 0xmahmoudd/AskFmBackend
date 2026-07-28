import React, { useState, useEffect, useCallback } from 'react';
import { getThreadsForUser } from '../api/thread';
import { useAuth } from '../context/AuthContext';
import { ThreadCard } from '../components/ThreadCard';
import { Spinner } from '../components/Spinner';
import { EmptyState } from '../components/EmptyState';
import { Pagination } from '../components/Pagination';
import { Inbox, CheckCircle, HelpCircle } from 'lucide-react';

export const InboxPage = () => {
  const { user } = useAuth();
  const [threads, setThreads] = useState([]);
  const [filter, setFilter] = useState('unanswered'); // 'unanswered' | 'all'
  const [page, setPage] = useState(1);
  const [totalItems, setTotalItems] = useState(0);
  const [loading, setLoading] = useState(true);

  const fetchInbox = useCallback(async () => {
    if (!user) return;
    setLoading(true);
    try {
      const res = await getThreadsForUser(user.id || user.Id, page, 10);
      const data = res.data || res.Data || res;
      let list = [];
      if (data.items || data.Items) {
        list = data.items || data.Items;
        setTotalItems(data.totalCount || data.TotalCount || 0);
      } else if (Array.isArray(data)) {
        list = data;
        setTotalItems(data.length);
      }

      if (filter === 'unanswered') {
        list = list.filter((t) => !(t.answerContent || t.AnswerContent));
      }

      setThreads(list);
    } catch (err) {
      console.error('Failed to load inbox threads:', err);
      setThreads([]);
    } finally {
      setLoading(false);
    }
  }, [user, page, filter]);

  useEffect(() => {
    fetchInbox();

    const handleNotification = (e) => {
      const notif = e.detail;
      if (!notif || notif.type === 'QUESTION' || notif.Type === 'QUESTION' || notif.type === 'ANSWER' || notif.Type === 'ANSWER') {
        fetchInbox();
      }
    };

    window.addEventListener('askfm_notification', handleNotification);
    return () => window.removeEventListener('askfm_notification', handleNotification);
  }, [fetchInbox]);

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '16px' }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
          <Inbox style={{ width: '24px', height: '24px', color: 'var(--primary)' }} />
          <h2 style={{ fontSize: '20px', fontWeight: '800' }}>Inbox (Questions for You)</h2>
        </div>

        <div style={{ display: 'flex', gap: '8px' }}>
          <button
            className={`btn ${filter === 'unanswered' ? 'btn-primary' : 'btn-secondary'}`}
            onClick={() => {
              setFilter('unanswered');
              setPage(1);
            }}
          >
            <HelpCircle style={{ width: '16px', height: '16px' }} /> Unanswered
          </button>
          <button
            className={`btn ${filter === 'all' ? 'btn-primary' : 'btn-secondary'}`}
            onClick={() => {
              setFilter('all');
              setPage(1);
            }}
          >
            <CheckCircle style={{ width: '16px', height: '16px' }} /> All
          </button>
        </div>
      </div>

      {loading ? (
        <Spinner />
      ) : threads.length === 0 ? (
        <EmptyState
          title={filter === 'unanswered' ? 'No pending questions' : 'No questions found'}
          message={filter === 'unanswered' ? 'You have answered all questions asked to you!' : 'Share your profile link to receive questions.'}
          icon={Inbox}
        />
      ) : (
        <div>
          {threads.map((thread) => (
            <ThreadCard key={thread.id || thread.Id} thread={thread} onUpdate={fetchInbox} />
          ))}
          <Pagination page={page} pageSize={10} totalItems={totalItems} onPageChange={setPage} />
        </div>
      )}
    </div>
  );
};
