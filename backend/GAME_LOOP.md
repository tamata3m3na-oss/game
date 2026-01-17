# Game Loop Implementation

## Overview
Server-authoritative game loop system running at 20Hz (50ms per tick) for real-time PvP matches.

## Architecture

### Tick Rate
- **20Hz** = 50ms per tick
- Each active match runs its own independent tick loop
- Consistent timing with `setInterval`

### Game State Structure
```typescript
interface GameState {
  matchId: number;
  player1: PlayerState;
  player2: PlayerState;
  tick: number;
  timestamp: number;
  winner?: number;
  status: 'active' | 'completed';
}

interface PlayerState {
  id: number;
  x: number;           // Position (0-100)
  y: number;           // Position (0-100)
  rotation: number;    // Degrees
  health: number;      // 0-100
  abilityReady: boolean;
  lastAbilityTime: number;
}
```

## Movement System

### Constants
- **Player Speed**: 5 units/second
- **Movement per Tick**: 0.25 units (5 / 20)
- **Map Bounds**: 100x100 units
- **Starting Positions**: 
  - Player 1: (20, 50)
  - Player 2: (80, 50)

### Movement Calculation
1. Client sends normalized input: `{moveX: -1 to 1, moveY: -1 to 1}`
2. Server normalizes diagonal movement (magnitude = 1)
3. Server applies: `newPos = oldPos + (normalized * movePerTick)`
4. Server clamps to map bounds: `[0, 100]`
5. Server updates rotation based on movement direction

## Combat System

### Fire (Basic Attack)
- **Range**: 10 units
- **Damage**: 10 HP
- **Cooldown**: None (rate limited by tick rate)

### Ability (Special Attack)
- **Range**: 15 units
- **Damage**: 25 HP
- **Cooldown**: 5 seconds
- Server tracks `lastAbilityTime` per player

## Input Handling

### Client → Server
Event: `game:input`
```typescript
{
  moveX: number,      // -1 to 1
  moveY: number,      // -1 to 1
  fire: boolean,
  ability: boolean,
  timestamp: number
}
```

### Input Queue
- Each player has input queue (max 10 inputs)
- Server processes one input per tick (FIFO)
- Excess inputs dropped to prevent flooding

## Snapshot Broadcasting

### Server → Client
Event: `game:snapshot` (every 50ms)
```typescript
{
  matchId: number,
  player1: PlayerState,
  player2: PlayerState,
  tick: number,
  timestamp: number,
  winner?: number,
  status: 'active' | 'completed'
}
```

## Anti-Cheat Validation

### Input Validation
✓ Timestamp not in future (+ 1 second tolerance)
✓ Movement magnitude ≤ 1.1 (diagonal tolerance)
✓ MoveX/MoveY within [-1, 1]
✓ Ability only when ready (cooldown enforced server-side)
✓ All position updates calculated server-side
✓ No client-authoritative data accepted

## Match Lifecycle

### States
1. **pending** - Match found, waiting for both players to be ready
2. **active** - Game loop running, accepting inputs
3. **completed** - Winner determined, loop stopped

### Starting a Match
1. Both players send `match:ready` event
2. Server validates both ready
3. Server updates match status to 'active'
4. Server emits `match:start` to both players
5. **GameEngineService.startMatch()** called
6. Initial game state created
7. Tick loop started at 20Hz

### Ending a Match
1. Player health reaches 0 OR player disconnects
2. Winner determined
3. Match status updated to 'completed'
4. Player ratings updated (+25/-25)
5. Win/loss records updated
6. Server emits `game:end` to both players
7. Game loop stopped
8. Redis state cleaned up

## Redis Storage

### Game State Key
```
matchmaking:match:{matchId}:game
```
**Contents**: JSON serialized GameState
**TTL**: 1 hour + 60 seconds

### Match Metadata
```
matchmaking:match:{matchId}
```
**Contents**: Hash with player IDs, ready states
**TTL**: Updated to 1 hour when match starts

## Performance Considerations

### Tick Consistency
- Target: 50ms ± 5ms
- Monitored via internal timestamp tracking
- Input queue prevents processing backlog

### Memory Management
- Active match data stored in memory (Map)
- Game state persisted to Redis each tick
- Cleanup on match end or server restart

### Scalability
- Each match independent
- No cross-match synchronization required
- Can horizontally scale (matches distributed across instances)

## Events Protocol Summary

| Event | Direction | Payload | Description |
|-------|-----------|---------|-------------|
| `game:input` | Client → Server | GameInputDto | Player input (movement, fire, ability) |
| `game:snapshot` | Server → Client | GameState | Full world state (every 50ms) |
| `game:end` | Server → Client | { matchId, winner, finalState } | Match ended |
| `match:ready` | Client → Server | { matchId } | Player ready to start |
| `match:start` | Server → Client | { matchId, opponent, color } | Match starting |

## WebSocket Namespace
All game events occur on the `/pvp` namespace with JWT authentication.

## Example Flow

1. Players matched → `match:found`
2. Both send `match:ready`
3. Server sends `match:start` + starts game loop
4. Clients send `game:input` at their own rate (e.g., 60Hz)
5. Server processes inputs at 20Hz tick
6. Server broadcasts `game:snapshot` every 50ms to both clients
7. Health reaches 0 → Server sends `game:end`
8. Loop stopped, ratings updated

## Testing Considerations

- Test tick consistency (measure actual intervals)
- Test input validation (malformed/future timestamps)
- Test disconnection during active game
- Test simultaneous damage (both players hit at same tick)
- Load test with multiple concurrent matches
