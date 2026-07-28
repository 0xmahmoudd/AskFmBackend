import apiClient from './client';

export const askQuestion = async (data) => {
  // data: { askedId, questionContent, isAnonymous }
  const response = await apiClient.post('/Thread/thread', data);
  return response.data;
};

export const getThreadsForUser = async (userId, page = 1, pageSize = 10) => {
  const response = await apiClient.get(`/Thread/thread/${userId}`, {
    params: { page, pageSize },
  });
  return response.data;
};

export const getThreadById = async (threadId) => {
  const response = await apiClient.get(`/Thread/threads/${threadId}`);
  return response.data;
};

export const answerQuestion = async (threadId, answerContent) => {
  const response = await apiClient.put(`/Thread/threads/${threadId}/answer`, {
    answerContent,
  });
  return response.data;
};

export const getPublicThreads = async (page = 1, pageSize = 10) => {
  const response = await apiClient.get('/Thread/threads', {
    params: { page, pageSize },
  });
  return response.data;
};

export const deleteThread = async (threadId) => {
  const response = await apiClient.delete(`/Thread/threads/${threadId}`);
  return response.data;
};

export const getPersonalizedFeed = async (page = 1, pageSize = 10) => {
  const response = await apiClient.get('/Thread/threads/feed', {
    params: { page, pageSize },
  });
  return response.data;
};

export const saveThread = async (threadId) => {
  const response = await apiClient.post(`/Thread/threads/${threadId}/save`);
  return response.data;
};

export const unsaveThread = async (threadId) => {
  const response = await apiClient.delete(`/Thread/threads/${threadId}/save`);
  return response.data;
};

export const getSavedThreads = async (page = 1, pageSize = 10) => {
  const response = await apiClient.get('/Thread/threads/saved', {
    params: { page, pageSize },
  });
  return response.data;
};

export const toggleThreadVisibility = async (threadId) => {
  const response = await apiClient.put(`/Thread/threads/${threadId}/visibility`);
  return response.data;
};
