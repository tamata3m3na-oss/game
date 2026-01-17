# WebSocket PvP Matchmaking System

## Overview

The matchmaking system uses WebSocket connections with JWT authentication to match players for chess games. It uses Redis for queue management and implements a rating-based matching algorithm.

## WebSocket Connection

**Namespace:** `/pvp`  
**URL:** `ws://localhost:3000/pvp` (or `wss://` for production)

### Authentication

Clients must provide a JWT token when connecting:

```javascript
const socket = io('http://localhost:3000/pvp', {
  auth: {
    token: 'your-jwt-access-token'
  }
});
```

Or via Authorization header:
```javascript
const socket = io('http://localhost:3000/pvp', {
  extraHeaders: {
    authorization: 'Bearer your-jwt-access-token'
  }
});
```

## Client → Server Events

### `queue:join`

Join the matchmaking queue.

**Payload:** None

**Response:** Server emits `queue:status` event

```javascript
socket.emit('queue:join');
```

### `queue:leave`

Leave the matchmaking queue.

**Payload:** None

**Response:** Server emits `queue:status` with position 0

```javascript
socket.emit('queue:leave');
```

### `match:ready`

Acknowledge that the client is ready to start the match.

**Payload:**
```typescript
{
  matchId: number
}
```

**Response:** Server emits `match:start` when both players are ready

```javascript
socket.emit('match:ready', { matchId: 123 });
```

## Server → Client Events

### `queue:status`

Broadcast periodically with queue position and estimated wait time.

**Payload:**
```typescript
{
  position: number,      // Position in queue (1-based)
  estimatedWait: number  // Estimated wait time in seconds
}
```

```javascript
socket.on('queue:status', (data) => {
  console.log(`Position: ${data.position}, Wait: ${data.estimatedWait}s`);
});
```

### `match:found`

Emitted when a suitable opponent is found.

**Payload:**
```typescript
{
  matchId: number,
  opponent: {
    id: number,
    username: string,
    rating: number
  }
}
```

```javascript
socket.on('match:found', (data) => {
  console.log(`Match found! ID: ${data.matchId}`);
  console.log(`Opponent: ${data.opponent.username} (${data.opponent.rating})`);
  
  // Client should acknowledge readiness
  socket.emit('match:ready', { matchId: data.matchId });
});
```

### `match:start`

Emitted when both players have acknowledged readiness.

**Payload:**
```typescript
{
  matchId: number,
  opponent: {
    id: number,
    username: string,
    rating: number
  },
  color: 'white' | 'black'  // The color assigned to this player
}
```

```javascript
socket.on('match:start', (data) => {
  console.log(`Match starting! You are ${data.color}`);
  console.log(`Opponent: ${data.opponent.username}`);
});
```

## Matchmaking Algorithm

### Rating Brackets

Players are grouped into rating brackets of 200 points:
- 0-199
- 200-399
- 400-599
- etc.

### Matching Criteria

Two players are matched if:
1. **Rating difference ≤ 200 points**
2. **Wait time conditions:**
   - Both players have waited ≥ 3 seconds (default)
   - OR both players have waited ≤ 500ms AND there are only 2 players in queue
3. **Not recent opponents** (60-second cooldown)

### Queue Processing

- Queue is checked every **500ms**
- Players are matched in FIFO order (oldest waiting first)
- When matched, both players are removed from queue
- Match record is created in database with `status: 'pending'`

### Auto-Rematch Prevention

Players cannot be matched against each other again within 60 seconds of their last match to prevent immediate rematches and improve variety.

## Connection Lifecycle

### 1. Connection
```javascript
socket.on('connect', () => {
  console.log('Connected to matchmaking server');
});
```

### 2. Join Queue
```javascript
socket.emit('queue:join');
```

### 3. Monitor Queue Status
```javascript
socket.on('queue:status', (data) => {
  updateUI(data.position, data.estimatedWait);
});
```

### 4. Match Found
```javascript
socket.on('match:found', (data) => {
  showMatchFoundUI(data.opponent);
  socket.emit('match:ready', { matchId: data.matchId });
});
```

### 5. Match Start
```javascript
socket.on('match:start', (data) => {
  startGame(data.matchId, data.color, data.opponent);
});
```

### 6. Disconnection Handling

If a player disconnects before the match starts:
- They are automatically removed from queue
- If already matched but not started, the opponent is automatically re-queued
- The match status is updated to 'completed'

```javascript
socket.on('disconnect', () => {
  console.log('Disconnected from server');
});
```

## Configuration

### Environment Variables

All configuration is in `.env`:

```env
# CORS for WebSocket
CORS_ORIGIN=http://localhost:3000

# JWT for authentication
JWT_SECRET=your-secret-key

# Redis connection
REDIS_HOST=localhost
REDIS_PORT=6379
```

### Timeouts

- **Connection timeout:** 30 seconds
- **Ping interval:** 25 seconds
- **Ping timeout:** 30 seconds
- **Queue check interval:** 500ms
- **Min wait time (2+ players):** 3 seconds
- **Quick match wait (only 2 players):** 500ms

## Database Schema

### Matches Table Updates

New columns added:
- `status` (varchar): 'pending' | 'active' | 'completed'
- `match_started_at` (timestamp): When both players acknowledged readiness

```sql
ALTER TABLE matches 
ADD COLUMN status VARCHAR(20) DEFAULT 'pending',
ADD COLUMN match_started_at TIMESTAMP NULL;
```

## Redis Data Structures

### Queue Keys

- `matchmaking:brackets` - Set of active rating brackets
- `waiting_players:{bracket}` - Sorted set of player IDs (score = joinedAt timestamp)
- `matchmaking:waiting_player:{playerId}` - Player data JSON
- `matchmaking:player_match:{playerId}` - Current match ID for player
- `matchmaking:match:{matchId}` - Match state hash
- `matchmaking:recent_opponent:{playerId}` - Last opponent ID (TTL: 60s)

## Error Handling

### Connection Errors

```javascript
socket.on('connect_error', (error) => {
  console.error('Connection failed:', error.message);
  // Usually: invalid token or server unavailable
});
```

### Automatic Reconnection

Socket.IO handles automatic reconnection by default. The client will:
1. Attempt to reconnect with exponential backoff
2. Re-authenticate with the same token
3. Need to re-join the queue after reconnection

## Testing

### Manual Testing with Socket.IO Client

```javascript
const io = require('socket.io-client');

const socket = io('http://localhost:3000/pvp', {
  auth: { token: 'your-access-token' }
});

socket.on('connect', () => {
  console.log('Connected');
  socket.emit('queue:join');
});

socket.on('queue:status', console.log);
socket.on('match:found', console.log);
socket.on('match:start', console.log);
```

### Testing Flow

1. Start Docker containers: `docker compose up -d`
2. Start backend: `npm run start:dev`
3. Register two users via REST API
4. Get JWT tokens for both users
5. Connect both users via WebSocket
6. Both join queue
7. Verify match is created
8. Both acknowledge readiness
9. Verify match starts

## Monitoring

### Logs

The system logs:
- Client connections/disconnections
- Queue joins/leaves
- Matches found
- Matches started
- Errors during matchmaking

Example log output:
```
[MatchmakingGateway] Client connected: abc123 (User: 1 - alice)
[MatchmakingService] Player 1 joined queue with rating 1000
[MatchmakingService] Match 42 found: 1 vs 2
[MatchmakingService] Match 42 started
```

### Redis Inspection

Check queue state:
```bash
# Connect to Redis
docker exec -it chess_redis redis-cli

# View active brackets
SMEMBERS matchmaking:brackets

# View players in a bracket (e.g., 1000)
ZRANGE waiting_players:1000 0 -1 WITHSCORES

# View player data
GET matchmaking:waiting_player:1

# View match state
HGETALL matchmaking:match:42
```

## Production Considerations

1. **Secure WebSocket:** Use `wss://` with valid SSL certificate
2. **CORS:** Update `CORS_ORIGIN` to your production domain
3. **Load Balancing:** Use sticky sessions or Redis adapter for multi-instance deployments
4. **Rate Limiting:** Implement rate limiting on queue join/leave
5. **Monitoring:** Set up alerts for connection failures, queue length, match creation failures
6. **Scaling:** Consider dedicated Redis instance with persistence enabled
