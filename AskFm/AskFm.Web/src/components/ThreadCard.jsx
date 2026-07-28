import React, { useState } from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { useNotification } from '../context/NotificationContext';
import { likeThread, unlikeThread } from '../api/likes';
import { saveThread, unsaveThread, deleteThread, toggleThreadVisibility } from '../api/thread';
import { parseApiError } from '../api/client';
import { Avatar } from './Avatar';
import { CommentSection } from './CommentSection';
import { AnswerModal } from './AnswerModal';
import { Heart, MessageSquare, Bookmark, Eye, EyeOff, Trash2, HelpCircle, User, CheckCircle2 } from 'lucide-react';

export const ThreadCard = ({ thread, onUpdate }) => {
  const { user } = useAuth();
  const { addToast } = useNotification();
  const [showComments, setShowComments] = useState(false);
  const [showAnswerModal, setShowAnswerModal] = useState(false);

  if (!thread) return null;

  const threadId = thread.id || thread.Id;
  const isAnswered = (thread.status === 1 || thread.Status === 1) || !!(thread.answerContent || thread.AnswerContent);

  const currentUserId = Number(user?.id || user?.Id);

  const askedId = Number(thread.askedId || thread.AskedId || thread.asked?.id || thread.asked?.Id);
  const askedName = thread.askedName || thread.AskedName || thread.asked?.name || thread.asked?.Name;
  const askedAvatar = thread.askedAvatarPath || thread.AskedAvatarPath || thread.asked?.avatarPath;

  const isAnonymous = thread.isAnonymous || thread.IsAnonymous;
  const askerId = isAnonymous ? null : Number(thread.askerId || thread.AskerId || thread.asker?.id || thread.asker?.Id);
  const askerName = isAnonymous ? 'Anonymous' : (thread.askerName || thread.AskerName || thread.asker?.name || thread.asker?.Name || 'User');
  const askerAvatar = isAnonymous ? null : (thread.askerAvatarPath || thread.AskerAvatarPath || thread.asker?.avatarPath);

  const isAskedMe = !!(askedId && currentUserId && askedId === currentUserId);
  const isAskerMe = !!(askerId && currentUserId && askerId === currentUserId);

  const [isLiked, setIsLiked] = useState(thread.isLikedByCurrentUser || false);
  const [likesCount, setLikesCount] = useState(thread.likesCount || thread.LikesCount || 0);
  const [isSaved, setIsSaved] = useState(thread.isSavedByCurrentUser || false);
  const [isHidden, setIsHidden] = useState(thread.isHidden || false);

  const handleToggleLike = async () => {
    try {
      if (isLiked) {
        await unlikeThread(threadId);
        setIsLiked(false);
        setLikesCount((prev) => Math.max(0, prev - 1));
      } else {
        await likeThread(threadId);
        setIsLiked(true);
        setLikesCount((prev) => prev + 1);
      }
    } catch (err) {
      addToast(parseApiError(err), 'error');
    }
  };

  const handleToggleSave = async () => {
    try {
      if (isSaved) {
        await unsaveThread(threadId);
        setIsSaved(false);
        addToast('Removed from saved threads', 'info');
      } else {
        await saveThread(threadId);
        setIsSaved(true);
        addToast('Saved thread!', 'success');
      }
    } catch (err) {
      addToast(parseApiError(err), 'error');
    }
  };

  const handleToggleVisibility = async () => {
    try {
      const res = await toggleThreadVisibility(threadId);
      setIsHidden(!isHidden);
      addToast(`Visibility set to ${!isHidden ? 'Hidden' : 'Public'}`, 'info');
      if (onUpdate) onUpdate();
    } catch (err) {
      addToast(parseApiError(err), 'error');
    }
  };

  const handleDelete = async () => {
    if (!window.confirm('Are you sure you want to delete this thread?')) return;
    try {
      await deleteThread(threadId);
      addToast('Thread deleted', 'info');
      if (onUpdate) onUpdate();
    } catch (err) {
      addToast(parseApiError(err), 'error');
    }
  };

  return (
    <div className="card" style={{ opacity: isHidden ? 0.75 : 1 }}>
      {/* Header */}
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '12px' }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
          {!isAnonymous && askerId ? (
            <Link to={`/profile/${askerId}`} style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
              <Avatar src={askerAvatar} name={askerName} size="sm" />
              <span style={{ fontWeight: '600', fontSize: '14px' }}>{askerName}</span>
            </Link>
          ) : (
            <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
              <Avatar name="Anonymous" size="sm" />
              <span style={{ fontWeight: '600', fontSize: '14px', color: 'var(--text-muted)' }}>Anonymous</span>
            </div>
          )}
          {askedId && askedName && (
            <span style={{ fontSize: '13px', color: 'var(--text-muted)' }}>
              asked{' '}
              <Link to={`/profile/${askedId}`} style={{ fontWeight: '600', color: 'var(--text-main)' }}>
                {askedName}
              </Link>
            </span>
          )}
        </div>

        <div style={{ display: 'flex', alignItems: 'center', gap: '6px' }}>
          {isHidden && <span className="badge" style={{ backgroundColor: '#f1f5f9', color: '#64748b' }}>Hidden</span>}
          {(isAskedMe || isAskerMe) && (
            <>
              {isAskedMe && (
                <button onClick={handleToggleVisibility} title="Toggle Visibility" style={{ padding: '4px', color: 'var(--text-muted)' }}>
                  {isHidden ? <EyeOff style={{ width: '16px', height: '16px' }} /> : <Eye style={{ width: '16px', height: '16px' }} />}
                </button>
              )}
              <button onClick={handleDelete} title="Delete Thread" style={{ padding: '4px', color: '#ef4444' }}>
                <Trash2 style={{ width: '16px', height: '16px' }} />
              </button>
            </>
          )}
        </div>
      </div>

      {/* Question Content */}
      <div style={{ background: '#f8fafc', padding: '14px', borderRadius: 'var(--radius-sm)', marginBottom: '12px' }}>
        <h3 style={{ fontSize: '16px', fontWeight: '700', color: 'var(--text-main)' }}>
          {thread.questionContent || thread.QuestionContent}
        </h3>
      </div>

      {/* Answer Content */}
      {isAnswered ? (
        <div style={{ marginBottom: '16px', paddingLeft: '4px' }}>
          <p style={{ fontSize: '14px', color: 'var(--text-main)', lineHeight: '1.6' }}>
            {thread.answerContent || thread.AnswerContent}
          </p>
        </div>
      ) : isAskedMe ? (
        <div style={{ marginBottom: '16px' }}>
          <button className="btn btn-primary" onClick={() => setShowAnswerModal(true)}>
            <CheckCircle2 style={{ width: '16px', height: '16px' }} /> Answer this question
          </button>
        </div>
      ) : (
        <p style={{ fontSize: '13px', fontStyle: 'italic', color: 'var(--text-muted)', marginBottom: '16px' }}>
          Waiting for an answer...
        </p>
      )}

      {/* Footer Actions */}
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', paddingTop: '12px', borderTop: '1px solid var(--border-color)' }}>
        <div style={{ display: 'flex', gap: '16px' }}>
          <button
            onClick={handleToggleLike}
            style={{
              display: 'flex',
              alignItems: 'center',
              gap: '6px',
              fontSize: '13px',
              fontWeight: '600',
              color: isLiked ? '#ef4444' : 'var(--text-muted)',
            }}
          >
            <Heart style={{ width: '18px', height: '18px', fill: isLiked ? '#ef4444' : 'none' }} />
            <span>{likesCount}</span>
          </button>

          <button
            onClick={() => setShowComments(!showComments)}
            style={{
              display: 'flex',
              alignItems: 'center',
              gap: '6px',
              fontSize: '13px',
              fontWeight: '600',
              color: showComments ? 'var(--primary)' : 'var(--text-muted)',
            }}
          >
            <MessageSquare style={{ width: '18px', height: '18px' }} />
            <span>{thread.commentsCount || thread.CommentsCount || 0}</span>
          </button>
        </div>

        <button
          onClick={handleToggleSave}
          style={{
            display: 'flex',
            alignItems: 'center',
            gap: '6px',
            fontSize: '13px',
            color: isSaved ? 'var(--primary)' : 'var(--text-muted)',
          }}
        >
          <Bookmark style={{ width: '18px', height: '18px', fill: isSaved ? 'var(--primary)' : 'none' }} />
        </button>
      </div>

      {/* Comments Section */}
      {showComments && <CommentSection threadId={threadId} />}

      {/* Answer Modal */}
      {showAnswerModal && (
        <AnswerModal
          thread={thread}
          onClose={() => setShowAnswerModal(false)}
          onAnswered={() => {
            if (onUpdate) onUpdate();
          }}
        />
      )}
    </div>
  );
};
