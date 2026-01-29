# PvP Ship Battle - Full Stack Application

Real-time multiplayer PvP game with NestJS backend and Unity client.

## Project Structure

```
.
├── backend/              # NestJS backend server
│   ├── src/
│   │   ├── auth/        # Authentication module (JWT, registration, login)
│   │   ├── player/      # Player profile module
│   │   ├── matchmaking/ # WebSocket matchmaking system
│   │   ├── ranking/     # ELO ranking system
│   │   ├── database/    # Database entities and configuration
│   │   └── main.ts      # Application entry point
│   ├── docker-compose.yml
│   ├── .env
│   └── package.json
├── unity-client/         # Unity 2022.3.62f3 client
│   ├── Assets/
│   │   ├── Scripts/     # C# game scripts
│   │   └── Scenes/      # Game scenes (Login, Lobby, Game, Result)
│   ├── Packages/
│   │   └── manifest.json
│   ├── README.md
│   ├── QUICK_START.md
│   ├── SCENE_SETUP_GUIDE.md
│   └── TESTING_GUIDE.md
└── README.md
```

## Features Implemented ✓

### Backend (NestJS)
- ✅ User registration with validation
- ✅ User login with JWT authentication
- ✅ JWT refresh token support
- ✅ Protected routes with JWT guard
- ✅ Player profile endpoints
- ✅ PostgreSQL database with TypeORM
- ✅ Redis for caching and matchmaking queues
- ✅ WebSocket real-time PvP (Socket.IO on /pvp namespace)
- ✅ Rating-based matchmaking system
- ✅ Server-authoritative game loop (20Hz tick rate)
- ✅ ELO ranking system with leaderboards
- ✅ Match result persistence
- ✅ Input validation with class-validator
- ✅ CORS enabled for frontend
- ✅ Password hashing with bcrypt

### Unity Client (2022.3.62f3) - Phase 2 Complete ✅
- ✅ Complete authentication system (Login/Register/Auto-login)
- ✅ WebSocket integration (System.Net.WebSockets)
- ✅ 5 scenes: Splash → Login → Lobby → Game → Result
- ✅ Real-time matchmaking with queue system
- ✅ Server-authoritative gameplay (Phase 2)
- ✅ Server snapshot processing (20Hz)
- ✅ Client input system (20Hz keyboard + touch)
- ✅ Ship and bullet rendering from server snapshots
- ✅ Client-side prediction (movement only)
- ✅ Health/shield HUD with real-time updates
- ✅ Match timer and opponent info display
- ✅ Disconnect handling with auto-reconnect
- ✅ Match result display with ELO changes
- ✅ Token persistence and refresh
- ✅ Windows + Android support
- ✅ No external dependencies (only Unity packages)

### Database Schema
- ✅ Users table with email, username, passwordHash, rating, wins, losses
- ✅ Matches table with player references and winner tracking
- ✅ Proper foreign key constraints
- ✅ Auto-generated timestamps

## Quick Start

### Prerequisites
- Node.js 18+
- Docker and Docker Compose
- Unity Hub + Unity 2022.3.62f3 LTS (for client)

### Backend Setup

1. Navigate to backend directory:
```bash
cd backend
```

2. Install dependencies:
```bash
npm install
```

3. Start PostgreSQL and Redis:
```bash
docker compose up -d
```

4. Start the server:
```bash
npm run start:dev
```

The server will be available at http://localhost:3000

### Unity Client Setup

1. Open Unity Hub
2. Add project from `unity-client` folder
3. Open with Unity 2022.3.62f3
4. Follow `unity-client/QUICK_START.md` for scene setup
5. Play and test!

**Quick Links:**
- [Unity Client README](unity-client/README.md) - Complete client documentation
- [Quick Start Guide](unity-client/QUICK_START.md) - 10-minute setup
- [Scene Setup Guide](unity-client/SCENE_SETUP_GUIDE.md) - Detailed scene creation
- [Testing Guide](unity-client/TESTING_GUIDE.md) - Comprehensive testing

## API Endpoints

### Authentication

#### POST /auth/register
Register a new user.

**Request:**
```json
{
  "email": "user@example.com",
  "username": "username",
  "password": "password123"
}
```

**Response:**
```json
{
  "user": {
    "id": 1,
    "email": "user@example.com",
    "username": "username",
    "rating": 1000,
    "wins": 0,
    "losses": 0
  },
  "accessToken": "eyJhbGc...",
  "refreshToken": "eyJhbGc..."
}
```

#### POST /auth/login
Login with existing credentials.

**Request:**
```json
{
  "email": "user@example.com",
  "password": "password123"
}
```

**Response:**
```json
{
  "user": {
    "id": 1,
    "email": "user@example.com",
    "username": "username",
    "rating": 1000,
    "wins": 0,
    "losses": 0
  },
  "accessToken": "eyJhbGc...",
  "refreshToken": "eyJhbGc..."
}
```

#### POST /auth/refresh
Refresh access token using refresh token.

**Request:**
```json
{
  "refreshToken": "eyJhbGc..."
}
```

**Response:**
```json
{
  "accessToken": "eyJhbGc...",
  "refreshToken": "eyJhbGc..."
}
```

### Player Profile

#### GET /player/me (Protected)
Get current user's profile. Requires JWT token.

**Headers:**
```
Authorization: Bearer <access_token>
```

**Response:**
```json
{
  "id": 1,
  "email": "user@example.com",
  "username": "username",
  "rating": 1000,
  "wins": 0,
  "losses": 0
}
```

#### GET /player/:id
Get any player's profile by ID (public endpoint).

**Response:**
```json
{
  "id": 1,
  "username": "username",
  "rating": 1000,
  "wins": 0,
  "losses": 0
}
```

## Testing

Run the provided test script:
```bash
cd backend
./test-api.sh
```

Or test manually with curl:
```bash
# Register
curl -X POST http://localhost:3000/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email": "test@example.com", "username": "testuser", "password": "password123"}'

# Login
curl -X POST http://localhost:3000/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email": "test@example.com", "password": "password123"}'

# Get profile (replace TOKEN with your access token)
curl -X GET http://localhost:3000/player/me \
  -H "Authorization: Bearer TOKEN"
```

## Database Management

Access PostgreSQL:
```bash
docker exec -it chess_postgres psql -U postgres -d chess_game
```

View tables:
```sql
\dt
\d users
\d matches
SELECT * FROM users;
```

## Environment Configuration

The `.env` file contains all configuration:
- Database connection settings
- Redis connection settings
- JWT secrets and expiration times
- Server port and CORS settings

**Important:** Change JWT secrets in production!

## Acceptance Criteria Status

### Backend
✅ Server runs on port 3000  
✅ Database synced automatically  
✅ Registration/Login works with validation  
✅ JWT tokens valid and properly configured  
✅ Protected routes working with JWT Guard  
✅ Player profile endpoints functional  
✅ CORS enabled for localhost:3000  
✅ Docker Compose setup for PostgreSQL + Redis  
✅ WebSocket /pvp namespace functional  
✅ Matchmaking queue system working  
✅ Server-authoritative game loop (20Hz)  
✅ ELO ranking system implemented  
✅ Match results persisted to database  

### Unity Client
✅ Unity 2022.3.62f3 project structure created  
✅ All required packages in manifest.json  
✅ Authentication system (register/login/auto-login) implemented  
✅ Token management (save/load/refresh) working  
✅ WebSocket connection to /pvp namespace functional  
✅ Queue join/leave functionality working  
✅ Match found and ready system implemented  
✅ 60 FPS input sending system  
✅ 20Hz snapshot processing  
✅ Ship movement interpolation  
✅ Health/weapon/ability systems complete  
✅ 4 scene architecture (Login/Lobby/Game/Result)  
✅ Touch + keyboard input support  
✅ No external dependencies (only Unity packages)  
⚠️ Scenes must be created manually in Unity Editor  

## Documentation

### Backend Docs
- [MATCHMAKING.md](backend/MATCHMAKING.md) - WebSocket matchmaking system
- [GAME_LOOP.md](backend/GAME_LOOP.md) - Server-authoritative game loop
- [ELO_RANKING_IMPLEMENTATION.md](backend/ELO_RANKING_IMPLEMENTATION.md) - Ranking system

### Unity Client Docs
- [README.md](unity-client/README.md) - Complete feature overview
- [QUICK_START.md](unity-client/QUICK_START.md) - 10-minute setup guide
- [SCENE_SETUP_GUIDE.md](unity-client/SCENE_SETUP_GUIDE.md) - Detailed scene creation
- [TESTING_GUIDE.md](unity-client/TESTING_GUIDE.md) - Comprehensive testing

## Architecture Overview

```
┌─────────────────┐         WebSocket         ┌──────────────────┐
│  Unity Client   │◄─────── /pvp ns ──────────►│  NestJS Backend  │
│  (2022.3.62f3)  │         Socket.IO          │   (Node 18)      │
└─────────────────┘                            └──────────────────┘
        │                                              │
        │ 60 FPS Input                                 │
        │ (movement, fire, ability)                    │
        │                                              │
        ▼                                              ▼
┌─────────────────┐                            ┌──────────────────┐
│  Input System   │                            │  Game Engine     │
│  Keyboard/Touch │                            │  20Hz Tick Loop  │
└─────────────────┘                            └──────────────────┘
                                                       │
                   20Hz Game Snapshots                 │
        ┌──────────────────────────────────────────────┤
        │                                              │
        ▼                                              ▼
┌─────────────────┐                            ┌──────────────────┐
│  Interpolation  │                            │  Redis Queue     │
│  Ship Rendering │                            │  Matchmaking     │
└─────────────────┘                            └──────────────────┘
                                                       │
                                                       ▼
                                               ┌──────────────────┐
                                               │   PostgreSQL     │
                                               │   Match Results  │
                                               │   ELO Rankings   │
                                               └──────────────────┘
```

## Technology Stack

### Backend
- **Framework:** NestJS (TypeScript)
- **Database:** PostgreSQL (TypeORM)
- **Cache/Queue:** Redis
- **WebSocket:** Socket.IO
- **Auth:** JWT + bcrypt
- **Container:** Docker Compose

### Unity Client
- **Engine:** Unity 2022.3.62f3 LTS
- **Language:** C# (.NET Standard 2.1)
- **Networking:** System.Net.WebSockets.ClientWebSocket
- **Serialization:** System.Text.Json
- **UI:** TextMeshPro + Unity UI
- **Input:** Unity Input System
- **Platforms:** Windows, Android

## Game Flow

1. **Login** - User authenticates via REST API
2. **Lobby** - WebSocket connects, player joins queue
3. **Matchmaking** - Server finds opponent based on rating
4. **Match Start** - Both players ready, game loop begins
5. **Gameplay** - Client sends input at 60 FPS, receives snapshots at 20Hz
6. **Match End** - Winner determined, ELO updated, results shown
7. **Repeat** - Return to lobby for next match
