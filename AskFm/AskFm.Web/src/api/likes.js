import apiClient from './client';

// Thread Likes
export const likeThread = async (threadId) => {
  const response = await apiClient.post(`/ThreadLike/threads/${threadId}/likes`);
  return response.data;
};

export const getThreadLikes = async (threadId) => {
  const response = await apiClient.get(`/ThreadLike/threads/${threadId}/likes`);
  return response.data;
};

export const unlikeThread = async (threadId) => {
  const response = await apiClient.delete(`/ThreadLike/threads/${threadId}/likes`);
  return response.data;
};

// Comment Likes
export const likeComment = async (commentId) => {
  const response = await apiClient.post(`/Comment/${commentId}/likes`);
  return response.data;
};

export const getCommentLikes = async (commentId) => {
  const response = await apiClient.get(`/Comment/${commentId}/likes`);
  return response.data;
};

export const unlikeComment = async (commentId) => {
  const response = await apiClient.delete(`/Comment/${commentId}/likes`);
  return response.data;
};
