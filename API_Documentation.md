# AskFm API Endpoints

This document visualizes the available endpoints for the AskFm Backend API. All secured endpoints require a valid JWT token passed in the `Authorization: Bearer <token>` header.

---

## 🔐 Auth (`/api/Auth`)

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| `POST` | `/api/Auth/register` | Register a new user | No |
| `POST` | `/api/Auth/login` | Login and receive a JWT and Refresh Token | No |
| `POST` | `/api/Auth/refresh-token/{id}` | Refresh an expired JWT using a valid refresh token cookie | Yes |
| `POST` | `/api/Auth/logout/{id}` | Logout the user and invalidate the refresh token | Yes |
| `POST` | `/api/Auth/forgot-password` | Send a password reset email | No |
| `POST` | `/api/Auth/reset-password` | Reset password using a valid token | No |
| `POST` | `/api/Auth/send-email-confirmation`| Send an email confirmation link | No |
| `POST` | `/api/Auth/confirm-email` | Confirm an email using a valid token | No |

---

## 👤 User Profiles (`/api/User`)

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| `GET`  | `/api/User/profile` | Get the currently authenticated user's profile | Yes |
| `GET`  | `/api/User/profile/{userId}` | Get another user's public profile | Yes |
| `POST` | `/api/User/profile/update/{userId}`| Update user details (Bio, Name) | Yes |
| `DELETE`|`/api/User/profile/{userId}` | Delete user account | Yes |
| `POST` | `/api/User/profile/{userId}/avatar`| Upload a new profile avatar image | Yes |
| `GET`  | `/api/User/search?q={query}` | Search for users by username or name | Yes |
| `POST` | `/api/User/profile/update/pass/{userId}`| Update user password | Yes |
| `POST` | `/api/User/profile/update/email/{userId}`| Request to change email address | Yes |

---

## 🤝 Social & Following (`/api/User`)

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| `POST` | `/api/User/profile/{followerId}/follow/{targetUserId}` | Follow a user | Yes |
| `POST` | `/api/User/profile/{followerId}/unfollow/{targetUserId}`| Unfollow a user | Yes |
| `GET`  | `/api/User/profile/{userId}/followers`| Get a list of the user's followers | Yes |
| `GET`  | `/api/User/profile/{userId}/following`| Get a list of users the user is following | Yes |
| `GET`  | `/api/User/profile/{targetUserId}/follow-status` | Check if the current user is following the target | Yes |

---

## 💬 Threads & Q&A (`/api/Thread`)

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| `POST` | `/api/Thread/thread` | Ask a question (creates a new thread) | Yes |
| `GET`  | `/api/Thread/thread/{id}` | Get all threads asked to a specific user (User Profile) | Yes |
| `GET`  | `/api/Thread/threads/{id}` | Get a specific thread by its ID | Yes |
| `PUT`  | `/api/Thread/threads/{id}/answer` | Answer a pending question | Yes |
| `GET`  | `/api/Thread/threads` | Get a global feed of recently answered threads | Yes |
| `DELETE`| `/api/Thread/threads/{id}` | Delete a thread | Yes |
| `GET`  | `/api/Thread/threads/feed` | Get a personalized feed from followed users | Yes |
| `PUT`  | `/api/Thread/threads/{id}/visibility`| Toggle a thread's visibility (Answered <-> Hidden) | Yes |
| `POST` | `/api/Thread/threads/{id}/save` | Bookmark/Save a thread to your account | Yes |
| `DELETE`| `/api/Thread/threads/{id}/save` | Remove a thread from your saved bookmarks | Yes |
| `GET`  | `/api/Thread/threads/saved` | Get a list of your saved threads | Yes |

---

## ❤️ Likes (`/api/ThreadLike`, `/api/Comment`)

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| `POST` | `/api/ThreadLike/threads/{id}/likes`| Like an answered thread | Yes |
| `GET`  | `/api/ThreadLike/threads/{id}/likes`| Get all likes for a thread | Yes |
| `DELETE`| `/api/ThreadLike/threads/{id}/likes`| Unlike a thread | Yes |
| `POST` | `/api/Comment/{id}/likes` | Like a comment | Yes |
| `GET`  | `/api/Comment/{id}/likes` | Get all likes for a comment | Yes |
| `DELETE`| `/api/Comment/{id}/likes` | Unlike a comment | Yes |

---

## 📝 Comments (`/api/Comment`)

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| `POST` | `/api/Comment/threads/{id}/comments`| Add a new comment to a thread | Yes |
| `GET`  | `/api/Comment/threads/{id}/comments`| Get paginated comments for a thread | Yes |
| `DELETE`| `/api/Comment/threads/{threadId}/comments/{commentId}`| Delete a comment | Yes |

---

## 🛡️ Moderation (`/api/Moderation`)

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| `POST` | `/api/Moderation/block/{userId}` | Block a user (prevents them from asking questions) | Yes |
| `DELETE`| `/api/Moderation/block/{userId}` | Unblock a user | Yes |
| `GET`  | `/api/Moderation/blocked` | Get a paginated list of blocked users | Yes |
| `POST` | `/api/Moderation/mute/{userId}` | Mute a user (suppresses their activities in feeds) | Yes |
| `DELETE`| `/api/Moderation/mute/{userId}` | Unmute a user | Yes |

---

## 🔔 Notifications (`/api/Notification`)

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| `GET`  | `/api/Notification` | Get the current user's notifications (paginated) | Yes |
| `GET`  | `/api/Notification/unread-count` | Get the total count of unread notifications | Yes |
| `GET`  | `/api/Notification/type/{category}` | Get notifications filtered by type/category | Yes |
| `PUT`  | `/api/Notification/{notificationId}/read`| Mark a specific notification as read | Yes |
| `PUT`  | `/api/Notification/read-all` | Mark all notifications as read | Yes |
| `POST` | `/api/Notification` | Create a system notification | Yes (Admin) |

*(Note: Real-time notifications and initial unread counts are also pushed to clients via SignalR automatically when connecting to `wss://localhost:<port>/notificationHub`)*
