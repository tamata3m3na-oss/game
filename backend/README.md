# Chess Game Backend

NestJS backend with authentication and real-time PvP game system.

## Features

- User registration and authentication with JWT
- Refresh token support
- Player profile management
- PostgreSQL database with TypeORM
- Redis for caching and matchmaking
- Input validation with class-validator
- CORS enabled for frontend
- **WebSocket-based PvP matchmaking system**
- **Server-authoritative game loop (20Hz tick rate)**
- Real-time match gameplay

## Setup

### Prerequisites

- Node.js 18+ and npm
- Docker and Docker Compose

### Installation

1. Install dependencies:
```bash
npm install
```

2. Start PostgreSQL and Redis:
```bash
docker-compose up -d
```

3. Configure environment variables:
Copy `.env` and adjust if needed.

4. Start the server:
```bash
npm run start:dev
```

The server will run on http://localhost:3000

## API Endpoints

### Authentication

- `POST /auth/register` - Register a new user
  ```json
  {
    "email": "user@example.com",
    "username": "username",
    "password": "password123"
  }
  ```

- `POST /auth/login` - Login
  ```json
  {
    "email": "user@example.com",
    "password": "password123"
  }
  ```

- `POST /auth/refresh` - Refresh access token
  ```json
  {
    "refreshToken": "your-refresh-token"
  }
  ```

### Player Profile

- `GET /player/me` - Get current user profile (Protected)
  - Requires: `Authorization: Bearer <access_token>`

- `GET /player/:id` - Get player profile by ID

## Database Schema

### Users Table
- id (Primary Key)
- email (Unique)
- username (Unique)
- passwordHash
- rating (Default: 1000)
- wins (Default: 0)
- losses (Default: 0)
- createdAt

### Matches Table
- id (Primary Key)
- player1Id (Foreign Key -> users.id)
- player2Id (Foreign Key -> users.id)
- winnerId (Foreign Key -> users.id)
- duration
- createdAt

## WebSocket Events

### Matchmaking (Namespace: `/pvp`)
- `queue:join` - Join matchmaking queue
- `queue:leave` - Leave matchmaking queue
- `match:ready` - Mark player ready for match
- **`game:input`** - Send player input (movement, fire, ability)

### Server Events
- `queue:status` - Queue position updates
- `match:found` - Match found notification
- `match:start` - Match starting (game loop begins)
- **`game:snapshot`** - Game state broadcast (every 50ms)
- **`game:end`** - Match ended with results

See [MATCHMAKING.md](./MATCHMAKING.md) and [GAME_LOOP.md](./GAME_LOOP.md) for detailed documentation.

## Scripts

- `npm run start:dev` - Start in development mode with hot reload
- `npm run build` - Build for production
- `npm run start:prod` - Start in production mode
- `npm run lint` - Run ESLint
- `npm run test` - Run tests
