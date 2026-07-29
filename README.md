# AskFm Full-Stack Platform

![ASP.NET](https://img.shields.io/badge/ASP.NET-Core%209-purple)
![React](https://img.shields.io/badge/React-Frontend-61DAFB)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-blue)
![License](https://img.shields.io/badge/License-MIT-green)

A fully-featured full-stack anonymous Q&A platform inspired by Ask.fm. Built with a modern .NET backend Web API and a React frontend, this application follows a clean 3-tier architecture and provides robust features including real-time notifications, user moderation, and anonymous interactions.

## Demo Video

[Watch the AskFm Live Demo Video on YouTube](https://youtu.be/MUeUZzyzSoA?si=2UQhasBaNozVdtMS)

## Features

- **Authentication & Identity**: JWT-based authentication, refresh tokens, password reset, and email confirmation.
- **Q&A Threads**: Ask questions (anonymously or publicly), answer questions, and toggle thread visibility.
- **Social Interactions**: Follow/unfollow users, like threads, comment on threads, and like comments.
- **Real-Time Notifications**: Instant updates via SignalR for new questions, answers, likes, and follows, including unread badge counts.
- **User Moderation**: Block and mute users to prevent harassment in anonymous environments.
- **Caching**: Redis integration for high-performance token revocation and caching.
- **Soft Delete**: System-wide soft-delete mechanism implemented via `ITrackable` entities and EF Core Global Query Filters.
- **Full-Stack Application**: Includes both a RESTful API backend and a responsive React web interface.

## Architecture

The solution follows a clean 3-tier architecture with an integrated web client to enforce separation of concerns:
- **`AskFm.API`**: The presentation layer containing Controllers, SignalR Hubs, and Middleware.
- **`AskFm.BLL`**: The Business Logic Layer containing Services, DTOs, and custom business rules.
- **`AskFm.DAL`**: The Data Access Layer containing EF Core DbContext, Models, Entity Configurations, and the Unit of Work / Repository pattern implementations.
- **`AskFm.Web`**: The frontend web application built with React 19, Vite, React Router, and Axios.
- **`Shared`**: Common constants and utilities shared across layers.

## Technology Stack

### Backend
- **Framework**: .NET 9.0 (ASP.NET Core Web API)
- **Database**: SQL Server & Entity Framework Core
- **Real-time**: SignalR Hubs
- **Caching**: Redis
- **Authentication**: ASP.NET Core Identity & JWT Bearer
- **Email**: SMTP integration

### Frontend
- **Framework**: React 19 (JavaScript)
- **Build Tool**: Vite
- **Routing**: React Router
- **HTTP Client**: Axios
- **Icons**: Lucide React
- **Styling**: Vanilla CSS Design System

## Prerequisites

Ensure you have the following installed on your machine:

1. **Backend Prerequisites**:
   - [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
   - SQL Server (or SQL Server Express)
   - Redis Server (running on `localhost:6379` by default)

2. **Frontend Prerequisites**:
   - [Node.js (v18+)](https://nodejs.org/) & npm
   - Modern Web Browser (Google Chrome, Mozilla Firefox, Microsoft Edge) with WebSocket support

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

## Running the Application

To run the complete full-stack application locally, follow these exact steps:

### Step 1: Start the Backend API
Run from any directory in terminal 1:
```bash
dotnet run --project /home/mahmoud/Projects/AskFmBackend/AskFm/AskFm.API/AskFm.API.csproj
```
Or navigate to the project directory:
```bash
cd AskFm.API
dotnet run
```
Once running, you can access the **Swagger UI** to explore and test the endpoints visually:
- `http://localhost:5180/swagger`

### Step 2: Start the Frontend Web Application
Open terminal 2 and run:
```bash
cd /home/mahmoud/Projects/AskFmBackend/AskFm/AskFm.Web
npm install
npm run dev
```

### Step 3: Open the Web Application
Open your web browser and navigate to:
- `http://localhost:5173`

## API Endpoints Documentation

A complete breakdown of all available REST endpoints is provided in the [API_Documentation.md](./API_Documentation.md) file.

## Example Workflows

Examples of workflows that users can perform are documented in the [User_Flows.md](./User_Flows.md) file.
