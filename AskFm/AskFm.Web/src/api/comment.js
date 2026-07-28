import apiClient from './client';

export const addComment = async (threadId, content) => {
  const response = await apiClient.post(`/Comment/threads/${threadId}/comments`, { content });
  return response.data;
};

export const getThreadComments = async (threadId, page = 1, pageSize = 10) => {
  const response = await apiClient.get(`/Comment/threads/${threadId}/comments`, {
    params: { page, pageSize },
  });
  return response.data;
};

export const deleteComment = async (threadId, commentId) => {
  const response = await apiClient.delete(`/Comment/threads/${threadId}/comments/${commentId}`);
  return response.data;
};
