import React, { useState } from 'react';
import { answerQuestion } from '../api/thread';
import { useNotification } from '../context/NotificationContext';
import { parseApiError } from '../api/client';
import { X, Check } from 'lucide-react';

export const AnswerModal = ({ thread, onClose, onAnswered }) => {
  const [answerContent, setAnswerContent] = useState('');
  const [loading, setLoading] = useState(false);
  const { addToast } = useNotification();

  if (!thread) return null;

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!answerContent.trim()) return;

    setLoading(true);
    try {
      await answerQuestion(thread.id || thread.Id, answerContent.trim());
      addToast('Answer published!', 'success');
      if (onAnswered) onAnswered();
      onClose();
    } catch (err) {
      addToast(parseApiError(err), 'error');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="modal-overlay">
      <div className="modal-content">
        <div className="modal-header">
          <h3 className="modal-title">Answer Question</h3>
          <button className="modal-close" onClick={onClose}>
            <X style={{ width: '20px', height: '20px' }} />
          </button>
        </div>

        <div style={{ background: '#f8fafc', padding: '12px 16px', borderRadius: 'var(--radius-sm)', marginBottom: '16px', borderLeft: '4px solid var(--primary)' }}>
          <p style={{ fontSize: '13px', color: 'var(--text-muted)', marginBottom: '4px' }}>Question:</p>
          <p style={{ fontWeight: '600', fontSize: '15px' }}>{thread.questionContent || thread.QuestionContent}</p>
        </div>

        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label className="form-label">Your Answer</label>
            <textarea
              className="form-textarea"
              rows={4}
              placeholder="Write your answer here..."
              value={answerContent}
              onChange={(e) => setAnswerContent(e.target.value)}
              disabled={loading}
              required
            />
          </div>

          <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '10px' }}>
            <button type="button" className="btn btn-secondary" onClick={onClose} disabled={loading}>
              Cancel
            </button>
            <button type="submit" className="btn btn-primary" disabled={loading || !answerContent.trim()}>
              <Check style={{ width: '16px', height: '16px' }} />
              {loading ? 'Publishing...' : 'Publish Answer'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};
