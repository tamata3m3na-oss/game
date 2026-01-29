# Unity Client & Backend Integration Guide

## 🎯 Phase 2 - Complete Gameplay Integration

This guide explains how to integrate the Unity client with the NestJS backend for full gameplay functionality.

---

## 📐 Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│                     Unity Client (Phase 2)                  │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  ┌──────────────┐     ┌──────────────┐     ┌──────────────┐ │
│  │ Input System │────►│ SocketClient │◄────│   Snapshot   │ │
│  │   (20Hz)     │     │  (WebSocket) │     │   Handler    │ │
│  └──────────────┘     └──────────────┘     └──────────────┘ │
│                              │                      │        │
│                              │                      │        │
│                              ▼                      ▼        │
│  ┌──────────────┐     ┌──────────────┐     ┌──────────────┐ │
│  │     HUD      │     │    Game      │     │  Ship/Bullet │ │
│  │   Display    │◄────│  Controller  │────►│    Views     │ │
│  └──────────────┘     └──────────────┘     └──────────────┘ │
│                                                              │
└─────────────────────────────────────────────────────────────┘
                              │
                              │ WebSocket (Socket.IO)
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                   NestJS Backend (20Hz Tick)                │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  ┌──────────────┐     ┌──────────────┐     ┌──────────────┐ │
│  │   Gateway    │────►│  Game Engine │────►│    Physics   │ │
│  │   (/pvp)     │     │   (20Hz)     │     │   Collision  │ │
│  └──────────────┘     └──────────────┘     └──────────────┘ │
│         │                     │                      │       │
│         │                     │                      │       │
│         ▼                     ▼                      ▼       │
│  ┌──────────────┐     ┌──────────────┐     ┌──────────────┐ │
│  │  Match Queue │     │  Snapshot    │     │   Database   │ │
│  │   (Redis)    │     │  Broadcast   │     │  (Results)   │ │
│  └──────────────┘     └──────────────┘     └──────────────┘ │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

---

## 🔌 Backend Requirements

### Required WebSocket Events

The backend **MUST** implement these events for Phase 2 to work:

#### 1. Match Lifecycle

**`match:ready`** - Sent when both players accept match
```typescript
{
  matchId: number;
  opponent: {
    id: number;
    username: string;
    rating: number;
  };
}
```

**`match:end`** - Sent when match concludes
```typescript
{
  matchId: number;
  winnerId: number;
  reason: string; // "opponent_destroyed" | "timeout" | "disconnect"
  eloChange: {
    oldRating: number;
    newRating: number;
    change: number;
  };
}
```

#### 2. Gameplay (NEW in Phase 2)

**`state:snapshot`** - Sent at 20Hz (every 50ms) during match
```typescript
{
  tick: number;          // Server tick number
  timestamp: number;     // Unix timestamp in milliseconds
  ships: [
    {
      id: string;        // Player ID as string
      position: {
        x: number;       // Position in world space
        y: number;
      };
      rotation: number;  // Rotation in degrees
      health: number;    // 0-100
      shield: number;    // 0-100
    }
  ];
  bullets: [
    {
      id: string;        // Unique bullet ID
      position: {
        x: number;
        y: number;
      };
      direction: {
        x: number;       // Normalized direction vector
        y: number;
      };
      ownerId: string;   // Player ID who fired this bullet
    }
  ];
}
```

**`game:input`** - Received from client at 20Hz
```typescript
{
  direction: {
    x: number;           // -1 to 1
    y: number;           // -1 to 1
  };
  isFiring: boolean;
  ability: boolean;
  timestamp: number;     // Client timestamp for lag compensation
}
```

---

## 🎮 Backend Implementation Checklist

### Game Loop (20Hz Tick Rate)

```typescript
// Example game loop structure
class GameLoop {
  private tickRate = 20; // Hz
  private tickInterval = 1000 / this.tickRate; // 50ms
  
  start() {
    setInterval(() => {
      this.tick();
    }, this.tickInterval);
  }
  
  private tick() {
    // 1. Process player inputs from queue
    this.processInputs();
    
    // 2. Update game state (physics, collision)
    this.updateGameState();
    
    // 3. Broadcast snapshot to all players
    this.broadcastSnapshot();
  }
  
  private broadcastSnapshot() {
    const snapshot = {
      tick: this.currentTick,
      timestamp: Date.now(),
      ships: this.getShipsData(),
      bullets: this.getBulletsData()
    };
    
    this.gateway.server.emit('state:snapshot', snapshot);
  }
}
```

### Input Processing

```typescript
// Handle input from client
@SubscribeMessage('game:input')
handleInput(client: Socket, data: InputData) {
  const playerId = this.getPlayerIdFromSocket(client);
  
  // Add to input queue (process on next tick)
  this.inputQueue.push({
    playerId,
    input: data,
    timestamp: data.timestamp
  });
}
```

### Ship Movement (Server-Side)

```typescript
// Server calculates movement
class Ship {
  private speed = 5; // units per second
  
  update(deltaTime: number, input: InputData) {
    // Apply input direction
    const moveVector = {
      x: input.direction.x * this.speed * deltaTime,
      y: input.direction.y * this.speed * deltaTime
    };
    
    this.position.x += moveVector.x;
    this.position.y += moveVector.y;
    
    // Update rotation to face movement direction
    if (moveVector.x !== 0 || moveVector.y !== 0) {
      this.rotation = Math.atan2(moveVector.y, moveVector.x) * (180 / Math.PI);
    }
  }
}
```

### Firing Mechanics (Server-Side)

```typescript
// Server handles bullet creation
class Ship {
  private fireRate = 5; // bullets per second
  private lastFireTime = 0;
  
  handleFire(currentTime: number) {
    const cooldown = 1000 / this.fireRate; // 200ms
    
    if (currentTime - this.lastFireTime < cooldown) {
      return; // Still in cooldown
    }
    
    // Create bullet
    const bullet = {
      id: generateBulletId(),
      position: { ...this.position },
      direction: this.getForwardVector(),
      ownerId: this.playerId
    };
    
    this.game.addBullet(bullet);
    this.lastFireTime = currentTime;
  }
}
```

### Collision Detection (Server-Side)

```typescript
// Server checks all collisions
class CollisionSystem {
  checkBulletShipCollisions() {
    for (const bullet of this.bullets) {
      for (const ship of this.ships) {
        // Skip if bullet owner
        if (bullet.ownerId === ship.id) continue;
        
        if (this.checkCollision(bullet, ship)) {
          // Apply damage
          ship.takeDamage(10); // Example damage
          
          // Remove bullet
          this.removeBullet(bullet.id);
        }
      }
    }
  }
  
  private checkCollision(bullet: Bullet, ship: Ship): boolean {
    const distance = Math.sqrt(
      Math.pow(bullet.position.x - ship.position.x, 2) +
      Math.pow(bullet.position.y - ship.position.y, 2)
    );
    
    return distance < ship.radius + bullet.radius;
  }
}
```

---

## 🔧 Configuration

### Unity Client Configuration

Edit `SocketClient.cs` line 11 to set server URL:
```csharp
private string serverUrl = "ws://localhost:3000/pvp"; // Change for production
```

For production:
```csharp
private string serverUrl = "wss://your-server.com/pvp";
```

### Backend Configuration

Ensure WebSocket gateway is configured:
```typescript
// gateway.ts
@WebSocketGateway({
  namespace: '/pvp',
  cors: {
    origin: '*', // Configure for production
    credentials: true
  }
})
```

---

## 🧪 Testing Integration

### Local Testing Steps

1. **Start Backend:**
```bash
cd backend
npm run start:dev
```

2. **Open Unity Editor:**
```bash
# Open Unity Hub and select unity-client project
# Press Play in Unity Editor
```

3. **Create Test Accounts:**
- Register two accounts in Unity client
- Or use curl/Postman to create accounts

4. **Join Queue:**
- Join queue on both clients
- Backend should match them

5. **Verify Gameplay:**
- ✅ Both clients load Game scene
- ✅ Ships appear on screen
- ✅ Movement works (WASD or touch)
- ✅ Bullets fire and appear
- ✅ Health bars update when hit
- ✅ Timer counts up
- ✅ Connection indicator is green

### Debug Checklist

#### If no snapshots received:
- Check backend is sending `state:snapshot` at 20Hz
- Verify WebSocket connection is established
- Check Unity console for errors
- Use browser dev tools to monitor WebSocket messages

#### If ships don't appear:
- Check `GameController` is in Game scene
- Verify Ship.prefab exists and is assigned
- Check snapshot data structure matches expected format
- Look for parsing errors in Unity console

#### If input not working:
- Verify `InputSender` component is in Game scene
- Check Input System package is installed
- Test keyboard in Unity Editor first
- For Android, verify touch controls are visible

#### If bullets don't appear:
- Check `Bullet.prefab` exists and is assigned
- Verify bullets array in snapshot
- Check bullet creation in GameController
- Look for off-screen culling issues

---

## 📊 Performance Monitoring

### Unity Client Metrics

Monitor in Unity Profiler:
- **Target:** 60 FPS
- **Network messages:** ~20 per second (input + snapshots)
- **Memory:** < 100 MB
- **Draw calls:** < 50 (for 2 ships + bullets)

### Backend Metrics

Monitor in server logs:
- **Tick rate:** Exactly 20Hz (50ms intervals)
- **Player count:** Active connections
- **Snapshot size:** < 2KB per snapshot
- **CPU usage:** < 50% on mid-tier server

---

## 🐛 Common Issues & Solutions

### Issue: Stuttering movement
**Cause:** Interpolation not working
**Solution:** 
- Verify `SnapshotHandler.ProcessSnapshot()` is called
- Check last/current snapshot storage
- Ensure Time.deltaTime is used for lerp

### Issue: Input lag
**Cause:** Send rate too low or network latency
**Solution:**
- Verify 20Hz send rate in InputSender
- Check network ping to server
- Consider client-side prediction (movement only)

### Issue: Ships at wrong position
**Cause:** Snapshot coordinate mismatch
**Solution:**
- Verify world space coordinates
- Check camera orthographic size
- Ensure position.x and position.y are correct

### Issue: Bullets not hitting
**Cause:** Collision detection is client-side
**Solution:**
- ❌ Don't add collision on client
- ✅ Wait for server damage update
- ✅ Display hit effects from snapshot

---

## 🚀 Deployment

### Unity Build

**Windows:**
```bash
# In Unity: File > Build Settings > PC, Mac & Linux Standalone
# Target Platform: Windows
# Architecture: x86_64
# Click Build
```

**Android:**
```bash
# In Unity: File > Build Settings > Android
# Configure:
# - Package Name: com.shipbattle.pvp
# - Minimum API Level: Android 7.0 (API 24)
# Click Build And Run
```

### Backend Deployment

1. Deploy to cloud platform (AWS, Heroku, DigitalOcean)
2. Configure WebSocket URL in Unity
3. Enable HTTPS/WSS for production
4. Set up monitoring and logging

---

## 📚 Additional Resources

### Unity Documentation
- [Unity Client README](unity-client/README.md)
- [Phase 2 Completion Report](PHASE_2_COMPLETION_REPORT.md)
- [Architecture Diagram](ARCHITECTURE_DIAGRAM.md)

### Backend Documentation
- [Backend README](backend/README.md)
- [Game Loop Documentation](backend/GAME_LOOP.md)
- [Matchmaking System](backend/MATCHMAKING.md)

### Code Examples
- See `GameController.cs` for snapshot processing
- See `InputSender.cs` for input handling
- See `SnapshotHandler.cs` for interpolation

---

## ✅ Integration Checklist

Before deploying to production:

### Backend ✓
- [ ] Game loop running at 20Hz
- [ ] `state:snapshot` broadcasting every 50ms
- [ ] `game:input` processing implemented
- [ ] Collision detection working
- [ ] Damage calculation working
- [ ] Match end conditions implemented
- [ ] ELO updates on match end

### Unity Client ✓
- [ ] All Phase 2 scripts in place
- [ ] Ship.prefab created
- [ ] Bullet.prefab created
- [ ] Game scene configured
- [ ] Input working on Windows
- [ ] Input working on Android
- [ ] HUD displaying correctly
- [ ] Disconnect/reconnect working

### Testing ✓
- [ ] Two clients can play match
- [ ] Movement is smooth
- [ ] Bullets fire correctly
- [ ] Health decreases when hit
- [ ] Match ends properly
- [ ] Results screen shows

### Production ✓
- [ ] Server URL configured
- [ ] SSL/WSS enabled
- [ ] Monitoring in place
- [ ] Error logging enabled
- [ ] Performance tested

---

## 🆘 Support

For issues or questions:
1. Check Unity console for errors
2. Check backend logs for issues
3. Review this integration guide
4. Check Phase 2 completion report
5. Test with simple scenarios first

**Happy Gaming! 🎮**
