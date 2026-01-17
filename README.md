# Chess Game - Full Stack Application

This project contains a chess game backend built with NestJS, PostgreSQL, and Redis.

## Project Structure

```
.
├── backend/              # NestJS backend server
│   ├── src/
│   │   ├── auth/        # Authentication module (JWT, registration, login)
│   │   ├── player/      # Player profile module
│   │   ├── database/    # Database entities and configuration
│   │   └── main.ts      # Application entry point
│   ├── docker-compose.yml
│   ├── .env
│   └── package.json
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
- ✅ Redis setup for caching
- ✅ Input validation with class-validator
- ✅ CORS enabled for frontend
- ✅ Password hashing with bcrypt

### Database Schema
- ✅ Users table with email, username, passwordHash, rating, wins, losses
- ✅ Matches table with player references and winner tracking
- ✅ Proper foreign key constraints
- ✅ Auto-generated timestamps

## Quick Start

### Prerequisites
- Node.js 18+
- Docker and Docker Compose

### Setup Instructions

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

✅ Server runs on port 3000  
✅ Database synced automatically  
✅ Registration/Login works with validation  
✅ JWT tokens valid and properly configured  
✅ Protected routes working with JWT Guard  
✅ Player profile endpoints functional  
✅ CORS enabled for localhost:3000  
✅ Docker Compose setup for PostgreSQL + Redis  
✅ Postman collection included for testing  

## Next Steps

This implementation provides the foundation for:
- Real-time chess game functionality (WebSockets)
- Match-making system
- Game state management
- Move validation
- Rating calculation (ELO system)
- Leaderboards
- Game history and replay
