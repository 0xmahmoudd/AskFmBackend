# AskFm Backend

A fully-featured backend API for an anonymous Q&A platform inspired by Ask.fm. Built with modern .NET 9, this application follows a clean 3-tier architecture and provides robust features including real-time notifications, user moderation, and anonymous interactions.

##  Features

- **Authentication & Identity**: JWT-based authentication, refresh tokens, password reset, and email confirmation.
- **Q&A Threads**: Ask questions (anonymously or publicly), answer questions, and toggle thread visibility.
- **Social Interactions**: Follow/unfollow users, like threads, comment on threads, and like comments.
- **Real-Time Notifications**: Instant updates via SignalR for new questions, answers, likes, and follows, including unread badge counts.
- **User Moderation**: Block and mute users to prevent harassment in anonymous environments.
- **Caching**: Redis integration for high-performance token revocation and caching.
- **Soft Delete**: System-wide soft-delete mechanism implemented via `ITrackable` entities and EF Core Global Query Filters.

##  Architecture

The solution follows a classic 3-Tier architecture to enforce separation of concerns:
- **`AskFm.API`**: The presentation layer containing Controllers, SignalR Hubs, and Middleware.
- **`AskFm.BLL`**: The Business Logic Layer containing Services, DTOs, and custom business rules.
- **`AskFm.DAL`**: The Data Access Layer containing EF Core DbContext, Models, Entity Configurations, and the Unit of Work / Repository pattern implementations.
- **`Shared`**: Common constants and utilities shared across layers.

## 🛠️ Technology Stack

- **Framework**: .NET 9.0 (ASP.NET Core Web API)
- **Database**: SQL Server & Entity Framework Core
- **Real-time**: SignalR
- **Caching**: Redis
- **Authentication**: ASP.NET Core Identity & JWT Bearer
- **Email**: SMTP integration (e.g., SendGrid/Gmail)

##  Prerequisites

Ensure you have the following installed on your machine:
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- SQL Server (or SQL Server Express)
- Redis Server (running on `localhost:6379` by default)

## Setup & Configuration

1. **Clone the repository:**
   ```bash
   git clone <repository_url>
   cd AskFmBackend/AskFm
   ```

2. **Environment Variables (`.env`)**
   Create a `.env` file in the `AskFm.API` directory:
   ```env
   CONNECTION_STRING=Server=localhost;Database=AskFmDb;User Id=sa;Password=YourPassword123!;TrustServerCertificate=True;
   ISSUER=AskFmIssuer
   AUDIENCE=AskFmAudience
   SIGNINGKEY=YourSuperSecretAndLongSigningKeyHere123!
   ```

3. **Application Settings (`appsettings.json`)**
   Update the `AskFm.API/appsettings.json` file with your Redis connection and Email settings:
   ```json
   {
     "ConnectionStrings": {
       "Redis": "localhost:6379"
     },
     "EmailSettings": {
       "From": "noreply@yourdomain.com",
       "Client": "smtp.gmail.com",
       "Password": "your_app_password",
       "Port": 587
     }
   }
   ```

4. **Database Migration**
   Apply the EF Core migrations to create the database schema:
   ```bash
   dotnet ef database update -p AskFm.DAL -s AskFm.API
   ```

##  Running the Application

To run the application locally:

```bash
dotnet run --project AskFm.API
```

Once running, you can access the **Swagger UI** to explore and test the endpoints visually:
- `https://localhost:<port>/swagger`

##  API Endpoints Documentation

A complete breakdown of all available REST endpoints is provided in the [API_Documentation.md](./API_Documentation.md) file.

## Example WorkFlows
There is a some examples of some workflows that the user can do it
[User_Flows.md](./User_Flows.md) file.