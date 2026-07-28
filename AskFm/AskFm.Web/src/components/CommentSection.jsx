import React, { useState, useEffect, useCallback } from 'react';
import { getThreadComments, addComment, deleteComment } from '../api/comment';
import { likeComment, unlikeComment } from '../api/likes';
import { useAuth } from '../context/AuthContext';
import { useNotification } from '../context/NotificationContext';
import { parseApiError } from '../api/client';
import { Avatar } from './Avatar';
import { Pagination } from './Pagination';
import { Spinner } from './Spinner';
import { Heart, Trash2, Send } from 'lucide-react';

export const CommentSection = ({ threadId }) => {
  const { user } = useAuth();
  const { addToast } = useNotification();
  const [comments, setComments] = useState([]);
  const [page, setPage] = useState(1);
  const [totalItems, setTotalItems] = useState(0);
  const [loading, setLoading] = useState(true);
  const [newCommentText, setNewCommentText] = useState('');
  const [submitting, setSubmitting] = useState(false);

  const fetchComments = useCallback(async () => {
    setLoading(true);
    try {
      const res = await getThreadComments(threadId, page, 5);
      // Backend return ServiceResult<PaginatedList<ReadCommentDTO>> or List
      const data = res.data || res.Data || res;
      if (data.items || data.Items) {
        setComments(data.items || data.Items);
        setTotalItems(data.totalCount || data.TotalCount || 0);
      } else if (Array.isArray(data)) {
        setComments(data);
        setTotalItems(data.length);
      } else {
        setComments([]);
      }
    } catch (err) {
      console.error('Error fetching comments:', err);
    } finally {
      setLoading(false);
    }
  }, [threadId, page]);

  useEffect(() => {
    fetchComments();
  }, [fetchComments]);

  const handleAddComment = async (e) => {
    e.preventDefault();
    if (!newCommentText.trim()) return;

    setSubmitting(true);
    try {
      await addComment(threadId, newCommentText.trim());
      addToast('Comment added!', 'success');
      setNewCommentText('');
      fetchComments();
    } catch (err) {
      addToast(parseApiError(err), 'error');
    } finally {
      setSubmitting(false);
    }
  };

  const handleDeleteComment = async (commentId) => {
    if (!window.confirm('Delete this comment?')) return;
    try {
      await deleteComment(threadId, commentId);
      addToast('Comment deleted', 'info');
      fetchComments();
    } catch (err) {
      addToast(parseApiError(err), 'error');
    }
  };

  const handleToggleLike = async (comment) => {
    const isLiked = comment.isLikedByCurrentUser;
    try {
      if (isLiked) {
        await unlikeComment(comment.id || comment.Id);
      } else {
        await likeComment(comment.id || comment.Id);
      }
      fetchComments();
    } catch (err) {
      addToast(parseApiError(err), 'error');
    }
  };

  return (
    <div style={{ marginTop: '16px', paddingTop: '16px', borderTop: '1px solid var(--border-color)' }}>
      <h4 style={{ fontSize: '14px', fontWeight: '700', marginBottom: '12px' }}>Comments</h4>

      <form onSubmit={handleAddComment} style={{ display: 'flex', gap: '8px', marginBottom: '16px' }}>
        <input
          type="text"
          className="form-input"
          placeholder="Write a comment..."
          value={newCommentText}
          onChange={(e) => setNewCommentText(e.target.value)}
          disabled={submitting}
          style={{ height: '38px', fontSize: '13px' }}
        />
        <button type="submit" className="btn btn-primary" disabled={submitting || !newCommentText.trim()}>
          <Send style={{ width: '14px', height: '14px' }} />
        </button>
      </form>

      {loading ? (
        <Spinner center={false} />
      ) : comments.length === 0 ? (
        <p style={{ fontSize: '13px', color: 'var(--text-muted)' }}>No comments yet.</p>
      ) : (
        <div style={{ display: 'flex', flexDirection: 'column', gap: '10px' }}>
          {comments.map((comment) => {
            const commentId = comment.id || comment.Id;
            const author = comment.user || comment.User || {};
            const isOwner = user?.id === author.id || user?.Id === author.Id;

            return (
              <div
                key={commentId}
                style={{
                  background: '#f8fafc',
                  padding: '10px 14px',
                  borderRadius: 'var(--radius-sm)',
                  display: 'flex',
                  justifyContent: 'space-between',
                  alignItems: 'flex-start',
                }}
              >
                <div style={{ display: 'flex', gap: '10px' }}>
                  <Avatar src={author.avatarPath || author.AvatarPath} name={author.name || author.Name} size="sm" />
                  <div>
                    <span style={{ fontWeight: '600', fontSize: '13px', marginRight: '8px' }}>
                      {author.name || author.Name || 'User'}
                    </span>
                    <p style={{ fontSize: '13px', color: 'var(--text-main)', marginTop: '2px' }}>
                      {comment.content || comment.Content}
                    </p>
                  </div>
                </div>

                <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                  <button
                    onClick={() => handleToggleLike(comment)}
                    style={{
                      display: 'flex',
                      alignItems: 'center',
                      gap: '4px',
                      fontSize: '12px',
                      color: comment.isLikedByCurrentUser ? '#ef4444' : 'var(--text-muted)',
                    }}
                  >
                    <Heart style={{ width: '14px', height: '14px', fill: comment.isLikedByCurrentUser ? '#ef4444' : 'none' }} />
                    <span>{comment.likesCount || comment.LikesCount || 0}</span>
                  </button>

                  {isOwner && (
                    <button
                      onClick={() => handleDeleteComment(commentId)}
                      style={{ color: 'var(--text-muted)', padding: '2px' }}
                    >
                      <Trash2 style={{ width: '14px', height: '14px' }} />
                    </button>
                  )}
                </div>
              </div>
            );
          })}
          <Pagination page={page} pageSize={5} totalItems={totalItems} onPageChange={setPage} />
        </div>
      )}
    </div>
  );
};
