# AskFm User Flows

This document outlines common end-to-end user journeys within the AskFm backend, demonstrating how the various endpoints and real-time features interact.

---

## Flow 1: Onboarding and Social Discovery

This flow illustrates a new user joining the platform, setting up their profile, finding friends, and establishing a social connection.

```mermaid
sequenceDiagram
    actor Alice
    participant API as API (/api/Auth & /api/User)
    actor Bob

    Alice->>API: POST /api/Auth/register (Name, Username, Email, Password)
    API-->>Alice: 200 OK + JWT & Refresh Token
    Alice->>API: POST /api/User/profile/update/AliceId (Bio, Avatar)
    API-->>Alice: 200 OK
    
    Alice->>API: GET /api/User/search?q=Bob
    API-->>Alice: 200 OK (Returns Bob's Profile)
    
    Alice->>API: POST /api/User/profile/AliceId/follow/BobId
    API-->>Alice: 200 OK
    API-->>Bob: [SignalR] Push "New Follower" Notification
```

---

## Flow 2: The Anonymous Q&A Lifecycle

This is the core loop of the platform: asking a question anonymously, the recipient answering it, and the original asker engaging with the answer.

```mermaid
sequenceDiagram
    actor Alice
    participant API as API (/api/Thread & /api/Notification)
    actor Bob

    Note over Alice: Alice wants to ask Bob something anonymously.
    Alice->>API: POST /api/Thread/thread (AskedId: Bob, isAnonymous: true)
    API-->>Alice: 200 OK
    API-->>Bob: [SignalR] Push "New Question" + UnreadCount updated

    Note over Bob: Bob checks his pending questions and answers it.
    Bob->>API: GET /api/Thread/thread/BobId (Sees pending question)
    Bob->>API: PUT /api/Thread/threads/{threadId}/answer (Answer content)
    API-->>Bob: 200 OK (Thread status -> Answered)
    
    API-->>Alice: [SignalR] Push "Your question was answered!"
    
    Note over Alice: Alice sees the answer and likes it.
    Alice->>API: POST /api/ThreadLike/threads/{threadId}/likes
    API-->>Alice: 200 OK
```

---

## Flow 3: Feed and Commenting

Users have a personalized feed consisting of answered questions from people they follow. They can interact by leaving comments.

```mermaid
sequenceDiagram
    actor Charlie
    participant API as API (/api/Thread & /api/Comment)
    actor Bob

    Note over Charlie: Charlie is following Bob.
    Charlie->>API: GET /api/Thread/threads/feed
    API-->>Charlie: 200 OK (Returns Bob's newly answered thread)
    
    Charlie->>API: POST /api/Comment/threads/{threadId}/comments (Content)
    API-->>Charlie: 200 OK
    API-->>Bob: [SignalR] Push "Charlie commented on your thread"

    Note over Bob: Bob reads the comment and likes it.
    Bob->>API: POST /api/Comment/{commentId}/likes
    API-->>Bob: 200 OK
```

---

## Flow 4: Moderation and Abuse Handling

Because anonymity can lead to spam or harassment, users can easily block people who send them abusive questions, even if the sender is anonymous.

```mermaid
sequenceDiagram
    actor Troll
    participant API as API (/api/Thread & /api/Moderation)
    actor Alice

    Note over Troll: Troll asks Alice a mean anonymous question.
    Troll->>API: POST /api/Thread/thread (AskedId: Alice, isAnonymous: true)
    API-->>Troll: 200 OK
    API-->>Alice: [SignalR] Push "New Question"

    Note over Alice: Alice sees the question and decides to block the sender.
    Alice->>API: POST /api/Moderation/block/{TrollId} 
    Note right of API: The API resolves TrollId from the thread's AskerId
    API-->>Alice: 200 OK (Troll is blocked)
    
    Note over Troll: Troll tries to ask another question later.
    Troll->>API: POST /api/Thread/thread (AskedId: Alice, isAnonymous: true)
    API-->>Troll: 400 Bad Request ("You cannot ask this user questions")
```

---

## Flow 5: Managing Profile Visibility

Sometimes a user answers a question but later decides they don't want it visible on their public profile anymore without completely deleting it.

```mermaid
sequenceDiagram
    actor Alice
    participant API as API (/api/Thread)
    actor PublicUser

    Note over Alice: Alice wants to hide an old answered thread.
    Alice->>API: PUT /api/Thread/threads/{threadId}/visibility
    API-->>Alice: 200 OK (Status -> Hidden)
    
    Note over PublicUser: Someone views Alice's profile.
    PublicUser->>API: GET /api/Thread/thread/{AliceId}
    API-->>PublicUser: 200 OK (Hidden thread is NOT included in response)
```
