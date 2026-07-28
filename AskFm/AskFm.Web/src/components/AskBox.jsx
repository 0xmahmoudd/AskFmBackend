import React, { useState } from 'react';
import { askQuestion } from '../api/thread';
import { useNotification } from '../context/NotificationContext';
import { parseApiError } from '../api/client';
import { Send, EyeOff, UserCheck } from 'lucide-react';

export const AskBox = ({ targetUser, onQuestionAsked }) => {
  const [questionContent, setQuestionContent] = useState('');
  const [isAnonymous, setIsAnonymous] = useState(false);
  const [loading, setLoading] = useState(false);
  const { addToast } = useNotification();

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!questionContent.trim()) return;

    setLoading(true);
    try {
      await askQuestion({
        askedId: targetUser?.id || targetUser?.Id,
        questionContent: questionContent.trim(),
        isAnonymous,
      });
      addToast('Question sent successfully!', 'success');
      setQuestionContent('');
      if (onQuestionAsked) onQuestionAsked();
    } catch (err) {
      addToast(parseApiError(err), 'error');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="card" style={{ border: '2px solid var(--primary-light)' }}>
      <h3 style={{ fontSize: '16px', fontWeight: '700', marginBottom: '12px', display: 'flex', alignItems: 'center', gap: '8px' }}>
        <Send style={{ width: '18px', height: '18px', color: 'var(--primary)' }} />
        Ask {targetUser?.name || targetUser?.Name || 'a Question'}
      </h3>
      <form onSubmit={handleSubmit}>
        <div className="form-group">
          <textarea
            className="form-textarea"
            rows={3}
            placeholder={`Ask ${targetUser?.name || 'anything'}...`}
            value={questionContent}
            onChange={(e) => setQuestionContent(e.target.value)}
            disabled={loading}
          />
        </div>

        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <label style={{ display: 'flex', alignItems: 'center', gap: '8px', fontSize: '13px', cursor: 'pointer', userSelect: 'none' }}>
            <input
              type="checkbox"
              checked={isAnonymous}
              onChange={(e) => setIsAnonymous(e.target.checked)}
            />
            {isAnonymous ? (
              <span style={{ display: 'flex', alignItems: 'center', gap: '4px', color: 'var(--text-muted)' }}>
                <EyeOff style={{ width: '14px', height: '14px' }} /> Ask anonymously
              </span>
            ) : (
              <span style={{ display: 'flex', alignItems: 'center', gap: '4px', color: 'var(--text-muted)' }}>
                <UserCheck style={{ width: '14px', height: '14px' }} /> Ask publicly
              </span>
            )}
          </label>

          <button type="submit" className="btn btn-primary" disabled={loading || !questionContent.trim()}>
            {loading ? 'Asking...' : 'Ask Question'}
          </button>
        </div>
      </form>
    </div>
  );
};
