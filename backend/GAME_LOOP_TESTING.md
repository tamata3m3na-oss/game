# Game Loop Testing Guide

## Test Environment Setup

### Prerequisites
- Backend server running
- Redis running
- Two authenticated users with JWT tokens
- WebSocket client (e.g., socket.io-client)

### Connect to WebSocket
```javascript
const io = require('socket.io-client');

const socket1 = io('http://localhost:3000/pvp', {
  auth: {
    token: '<player1_jwt_token>'
  }
});

const socket2 = io('http://localhost:3000/pvp', {
  auth: {
    token: '<player2_jwt_token>'
  }
});
```

## Test Flow

### 1. Join Matchmaking Queue
```javascript
// Player 1
socket1.emit('queue:join');
socket1.on('queue:status', (data) => {
  console.log('Player 1 queue status:', data);
  // Expected: { position: 1, estimatedWait: 0 }
});

// Player 2
socket2.emit('queue:join');
socket2.on('queue:status', (data) => {
  console.log('Player 2 queue status:', data);
  // Expected: { position: 1, estimatedWait: 0 }
});
```

### 2. Match Found
```javascript
socket1.on('match:found', (data) => {
  console.log('Player 1 match found:', data);
  // Expected: { matchId: <number>, opponent: { id, username, rating } }
});

socket2.on('match:found', (data) => {
  console.log('Player 2 match found:', data);
  // Expected: { matchId: <number>, opponent: { id, username, rating } }
});
```

### 3. Mark Ready
```javascript
// Both players must send ready
socket1.emit('match:ready', { matchId: <matchId> });
socket2.emit('match:ready', { matchId: <matchId> });
```

### 4. Match Start (Game Loop Begins)
```javascript
socket1.on('match:start', (data) => {
  console.log('Player 1 match start:', data);
  // Expected: { matchId, opponent: {...}, color: 'white' }
});

socket2.on('match:start', (data) => {
  console.log('Player 2 match start:', data);
  // Expected: { matchId, opponent: {...}, color: 'black' }
});
```

### 5. Send Player Input
```javascript
// Player 1 moves right and fires
setInterval(() => {
  socket1.emit('game:input', {
    moveX: 1,
    moveY: 0,
    fire: true,
    ability: false,
    timestamp: Date.now()
  });
}, 50); // Send at 20Hz or faster

// Player 2 moves left
setInterval(() => {
  socket2.emit('game:input', {
    moveX: -1,
    moveY: 0,
    fire: false,
    ability: false,
    timestamp: Date.now()
  });
}, 50);
```

### 6. Receive Game Snapshots
```javascript
let tickCount = 0;
let lastTimestamp = Date.now();

socket1.on('game:snapshot', (state) => {
  tickCount++;
  const now = Date.now();
  const delta = now - lastTimestamp;
  lastTimestamp = now;
  
  console.log(`Tick ${state.tick}: Player1(${state.player1.x.toFixed(2)}, ${state.player1.y.toFixed(2)}) HP:${state.player1.health}`);
  console.log(`Tick ${state.tick}: Player2(${state.player2.x.toFixed(2)}, ${state.player2.y.toFixed(2)}) HP:${state.player2.health}`);
  console.log(`Delta: ${delta}ms`);
  
  // Expected: ~50ms per snapshot
  // Player positions should update smoothly
  // Health should decrease when players are in range and firing
});

socket2.on('game:snapshot', (state) => {
  // Both players receive identical snapshots
  console.log('Player 2 received snapshot:', state.tick);
});
```

### 7. Game End
```javascript
socket1.on('game:end', (data) => {
  console.log('Player 1 game end:', data);
  // Expected: { matchId, winner: <playerId>, finalState: {...} }
});

socket2.on('game:end', (data) => {
  console.log('Player 2 game end:', data);
  // Expected: { matchId, winner: <playerId>, finalState: {...} }
});
```

## Test Cases

### Test 1: Basic Movement
**Objective**: Verify movement is server-authoritative and bounded

**Steps**:
1. Start match
2. Player 1 moves right (moveX: 1, moveY: 0)
3. Observe snapshots for 5 seconds

**Expected**:
- Player 1 X position increases by ~0.25 per tick
- Player 1 X never exceeds 100
- Player 2 position unchanged (no input)
- Rotation updates to match movement direction

### Test 2: Fire Damage
**Objective**: Verify combat mechanics

**Steps**:
1. Start match
2. Wait for players to move close (< 10 units)
3. Player 1 fires continuously
4. Observe health changes

**Expected**:
- Player 2 health decreases by 10 per successful hit
- No damage when distance > 10 units
- Health never goes below 0

### Test 3: Ability Cooldown
**Objective**: Verify ability cooldown enforcement

**Steps**:
1. Start match
2. Player 1 uses ability (ability: true)
3. Immediately use ability again
4. Wait 5 seconds
5. Use ability again

**Expected**:
- First ability: damage dealt, `abilityReady` becomes false
- Second ability: ignored (cooldown active)
- Third ability (after 5s): damage dealt, `abilityReady` was true

### Test 4: Tick Rate Consistency
**Objective**: Verify 20Hz tick rate

**Steps**:
1. Start match
2. Record timestamp of each snapshot for 60 seconds
3. Calculate average tick interval

**Expected**:
- Average interval: 50ms ± 5ms
- No intervals > 55ms (lag spike threshold)
- Tick counter increments by 1 each snapshot

### Test 5: Input Validation
**Objective**: Verify anti-cheat

**Steps**:
1. Send invalid inputs:
   - moveX: 10 (exceeds max)
   - moveY: -10 (exceeds max)
   - timestamp: Date.now() + 5000 (future)
2. Observe player position

**Expected**:
- Invalid inputs ignored
- Player position unchanged
- No errors/disconnections

### Test 6: Player Disconnect During Match
**Objective**: Verify graceful handling

**Steps**:
1. Start match
2. Player 1 disconnects socket
3. Observe Player 2 events

**Expected**:
- Player 2 receives `game:end` event
- Winner is Player 2
- Reason: 'opponent_disconnected'
- Match status updated to 'completed'
- Ratings updated

### Test 7: Simultaneous Damage
**Objective**: Verify deterministic combat

**Steps**:
1. Start match
2. Position players close to each other
3. Both players fire simultaneously

**Expected**:
- Both players take damage
- Damage applied in single tick
- If both die, first player in state is winner

### Test 8: Map Boundary Collision
**Objective**: Verify bounds enforcement

**Steps**:
1. Start match
2. Player 1 moves continuously left (moveX: -1)
3. Player 2 moves continuously right (moveX: 1)
4. Observe for 10 seconds

**Expected**:
- Player 1 X stops at 0
- Player 2 X stops at 100
- No negative or > 100 values
- Movement continues to be processed

### Test 9: High Input Rate
**Objective**: Verify input queue limiting

**Steps**:
1. Start match
2. Send 100 inputs in rapid succession
3. Observe processing

**Expected**:
- Only 10 inputs queued
- Excess inputs dropped
- No server errors
- Game continues smoothly

### Test 10: Diagonal Movement
**Objective**: Verify movement normalization

**Steps**:
1. Start match
2. Player 1 moves diagonally (moveX: 1, moveY: 1)
3. Measure distance traveled per tick

**Expected**:
- Movement magnitude = 0.25 per tick (same as cardinal)
- X and Y both change by ~0.177 per tick (0.25 / √2)
- No speed advantage for diagonal movement

## Performance Benchmarks

### Acceptable Ranges
- **Tick Interval**: 48-52ms (target: 50ms)
- **Snapshot Size**: < 500 bytes
- **Input Latency**: < 100ms (input to next snapshot)
- **Memory per Match**: < 10MB
- **CPU per Match**: < 5% (single core)

### Load Testing
```javascript
// Start 10 concurrent matches
const matches = [];
for (let i = 0; i < 10; i++) {
  // Create 2 sockets per match
  // Run match to completion
  // Measure tick consistency
}

// Expected: All matches maintain 50ms ± 5ms ticks
```

## Debugging Tips

### Check Active Matches
```bash
docker exec -it chess_redis redis-cli
KEYS matchmaking:match:*:game
GET matchmaking:match:1:game
```

### Monitor Tick Rate
```javascript
const ticks = [];
socket.on('game:snapshot', (state) => {
  ticks.push({ tick: state.tick, timestamp: state.timestamp });
  
  if (ticks.length >= 100) {
    const intervals = [];
    for (let i = 1; i < ticks.length; i++) {
      intervals.push(ticks[i].timestamp - ticks[i-1].timestamp);
    }
    
    const avg = intervals.reduce((a, b) => a + b) / intervals.length;
    const max = Math.max(...intervals);
    const min = Math.min(...intervals);
    
    console.log(`Avg: ${avg.toFixed(2)}ms, Min: ${min}ms, Max: ${max}ms`);
    ticks.length = 0;
  }
});
```

### Check Player State
```javascript
socket.on('game:snapshot', (state) => {
  console.table([
    { 
      Player: 'Player 1',
      X: state.player1.x.toFixed(2),
      Y: state.player1.y.toFixed(2),
      HP: state.player1.health,
      AbilityReady: state.player1.abilityReady
    },
    { 
      Player: 'Player 2',
      X: state.player2.x.toFixed(2),
      Y: state.player2.y.toFixed(2),
      HP: state.player2.health,
      AbilityReady: state.player2.abilityReady
    }
  ]);
});
```

## Common Issues

### Issue: Snapshots not received
**Cause**: Match not started (both players must be ready)
**Solution**: Verify both players sent `match:ready`

### Issue: Input ignored
**Cause**: Invalid input or match not active
**Solution**: Check input validation and match status

### Issue: Tick rate inconsistent
**Cause**: Server overload or Redis latency
**Solution**: Check server CPU/memory, Redis response times

### Issue: Position desync
**Cause**: Not possible (server-authoritative)
**Solution**: If observed, likely client-side rendering issue

## Integration with Unity/Frontend

### Client-Side Interpolation (Optional)
```javascript
// Store last 2 snapshots
let snapshot1 = null;
let snapshot2 = null;

socket.on('game:snapshot', (state) => {
  snapshot1 = snapshot2;
  snapshot2 = state;
});

// Render loop (60Hz)
function render() {
  if (snapshot1 && snapshot2) {
    const t = (Date.now() - snapshot2.timestamp) / 50;
    const interpX = lerp(snapshot2.player1.x, snapshot1.player1.x, t);
    const interpY = lerp(snapshot2.player1.y, snapshot1.player1.y, t);
    
    // Render at interpolated position
  }
  
  requestAnimationFrame(render);
}
```

### Input Sampling (Unity)
```csharp
// Send input every frame (Unity handles rate)
void Update() {
    float moveX = Input.GetAxis("Horizontal");
    float moveY = Input.GetAxis("Vertical");
    bool fire = Input.GetButton("Fire1");
    bool ability = Input.GetButton("Fire2");
    
    socket.Emit("game:input", new {
        moveX = moveX,
        moveY = moveY,
        fire = fire,
        ability = ability,
        timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds()
    });
}
```
