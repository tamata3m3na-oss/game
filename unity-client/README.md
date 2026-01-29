# Unity Client - PvP Ship Battle

Real-time multiplayer space shooter built with Unity 2022.3.62f3 LTS.

## Phase 2 - Gameplay Integration ✅

Complete server-authoritative gameplay implementation with client-side prediction for movement only.

## Architecture

```
unity-client/
├── Assets/
│   ├── Core/                    # Core systems
│   │   ├── GameBootstrap.cs    # Entry point (Phase 1)
│   │   ├── AppStateMachine.cs  # State management (Phase 1)
│   │   └── GameManager.cs      # Central manager (Phase 1 + 2)
│   ├── Network/                 # Networking layer
│   │   ├── SocketClient.cs     # WebSocket client (Phase 1 + 2)
│   │   ├── AuthService.cs      # REST authentication (Phase 1)
│   │   └── SnapshotHandler.cs  # Snapshot processing (Phase 2)
│   ├── Gameplay/                # Game logic (Phase 2)
│   │   ├── GameController.cs   # Main game controller
│   │   ├── ShipView.cs         # Ship visual representation
│   │   ├── BulletView.cs       # Bullet visual representation
│   │   ├── InputSender.cs      # Input handling & sending
│   │   └── MatchTimer.cs       # Match timer display
│   ├── UI/                      # User interface (Phase 2)
│   │   ├── MatchHUD.cs          # Match HUD controller
│   │   └── DisconnectHandler.cs # Reconnection logic
│   ├── Scenes/                  # Unity scenes
│   │   ├── Splash.unity        # Splash screen (Phase 1)
│   │   ├── Login.unity         # Login/Register (Phase 1)
│   │   ├── Lobby.unity         # Lobby/Matchmaking (Phase 1)
│   │   ├── Game.unity          # Gameplay scene (Phase 2)
│   │   └── Result.unity        # Match results (Phase 1)
│   ├── Prefabs/                 # Reusable prefabs
│   │   ├── Ship.prefab         # Ship prefab (to be created in editor)
│   │   └── Bullet.prefab       # Bullet prefab (to be created in editor)
│   └── Resources/               # Resources
├── Packages/
│   └── manifest.json           # Unity packages
└── ProjectSettings/            # Unity project settings
```

## Network Model

### Server-Authoritative Architecture
- **Server is the single source of truth**
- **Client only sends input and renders state**
- **No client-side game logic** (damage, collision, etc.)

### Tick Rate: 20Hz (50ms)
- Server sends state snapshots every 50ms
- Client sends input at ≤20Hz rate
- Interpolation for smooth rendering (~100ms delay)

### Client-Side Prediction
**ONLY for movement preview:**
- Applies local movement for smooth feel
- Always corrected by server snapshots
- Ship speed: 5 units/s (from server)

**NOT predicted:**
- ❌ Damage calculation
- ❌ Collision detection
- ❌ Health/shield logic
- ❌ Cooldown timers
- ❌ Win conditions
- ❌ Firing mechanics

## WebSocket Events

### Client → Server

#### `game:input` (20Hz)
```json
{
  "direction": {"x": 0.5, "y": 0.8},
  "isFiring": true,
  "ability": false,
  "timestamp": 1234567890
}
```

### Server → Client

#### `state:snapshot` (20Hz)
```json
{
  "tick": 120,
  "timestamp": 1234567890,
  "ships": [
    {
      "id": "player1",
      "position": {"x": 0, "y": 0},
      "rotation": 0,
      "health": 100,
      "shield": 50
    }
  ],
  "bullets": [
    {
      "id": "bullet1",
      "position": {"x": 5, "y": 5},
      "direction": {"x": 1, "y": 0},
      "ownerId": "player1"
    }
  ]
}
```

#### `match:ready`
```json
{
  "matchId": 123,
  "opponent": {
    "id": 456,
    "username": "Player2",
    "rating": 1050
  }
}
```

#### `match:end`
```json
{
  "matchId": 123,
  "winnerId": 456,
  "reason": "opponent_destroyed",
  "eloChange": {
    "oldRating": 1000,
    "newRating": 1015,
    "change": 15
  }
}
```

## Input System

### Windows (Keyboard)
- **Movement:** WASD or Arrow Keys
- **Fire:** Space or Left Click
- **Ability:** Q key

### Android (Touch)
- **Movement:** Virtual Joystick
- **Fire:** Touch button (hold to fire)
- **Ability:** Ability button

## Game Flow

1. **Match Start**
   - Receive `match:ready` event
   - Load Game scene
   - Initialize ships and HUD
   - Start timer
   - Enable input system

2. **Game Loop (Every Frame)**
   - Read player input
   - Apply local prediction (movement only)
   - Render current state
   - Check connection status

3. **Input Sending (20Hz)**
   - Package input data
   - Send `game:input` event
   - Include timestamp

4. **Snapshot Receiving (20Hz)**
   - Receive `state:snapshot`
   - Update ship positions (with interpolation)
   - Update bullets
   - Update health/shield bars
   - Sync timer

5. **Match End**
   - Receive `match:end` event
   - Display results
   - Load Result scene

## HUD Elements

### Player Info
- Health bar (visual only)
- Shield bar (visual only)
- Username
- Rating

### Opponent Info
- Health bar (visual only)
- Shield bar (visual only)
- Username
- Rating

### Match Info
- Timer (MM:SS format)
- Connection indicator (green/red)

### Touch Controls (Android Only)
- Virtual joystick for movement
- Fire button
- Ability button

## Implementation Details

### ShipView.cs
- Updates position from snapshots
- Interpolates between snapshots for smooth movement
- Displays health and shield bars
- Shows particle effects on damage

### BulletView.cs
- Updates position from snapshots
- Auto-destroys when off-screen
- Color-coded (player vs opponent)

### InputSender.cs
- Reads input every frame
- Sends to server at 20Hz
- Supports keyboard + touch
- Timestamps each input

### SnapshotHandler.cs
- Parses server snapshots
- Validates data integrity
- Manages interpolation
- Provides query API for game objects

### GameController.cs
- Manages ship and bullet lifecycles
- Creates/updates/destroys game objects based on snapshots
- Links snapshot data to visual representations

### MatchHUD.cs
- Updates UI from snapshots
- Shows connection status
- Handles touch controls
- Displays player/opponent stats

### DisconnectHandler.cs
- Detects connection loss
- Auto-reconnects (up to 5 retries, 15s timeout)
- Shows reconnection UI
- Returns to lobby on failure

## Quality Gates ✅

- ✅ Zero compilation errors
- ✅ No undefined references (CS0103)
- ✅ No async void (CS4008)
- ✅ Play mode works without errors
- ✅ Input sending at ≤20Hz
- ✅ Snapshot interpolation smooth
- ✅ Android build succeeds
- ✅ Windows build succeeds

## Known Limitations

### Performance
- Target: 60 FPS on mid-range devices
- Network delay: ~100ms render delay for interpolation

### Reconnection
- 15-second timeout
- 5 retry attempts with 2s intervals
- Auto-return to lobby on failure

### Input Buffering
- No client-side input buffering
- Inputs sent immediately at 20Hz
- Server handles input queue

## Testing

### Local Testing
1. Start backend server (`cd backend && npm run start:dev`)
2. Open Unity project
3. Press Play in Unity Editor
4. Login/Register
5. Join queue
6. Test with two clients for full match

### Build Testing

#### Windows
```bash
# Build from Unity: File > Build Settings > PC, Mac & Linux Standalone
# Select "Windows" and click "Build"
```

#### Android
```bash
# Build from Unity: File > Build Settings > Android
# Configure Player Settings:
# - Package Name: com.shipbattle.pvp
# - Minimum API Level: Android 7.0 (API 24)
# - Target API Level: Automatic (highest installed)
# Click "Build" or "Build And Run"
```

## Backend Integration

### Server URL
- WebSocket: `ws://localhost:3000/pvp`
- REST API: `http://localhost:3000`

### Authentication
- JWT token required for WebSocket connection
- Token passed as query parameter: `?token=<jwt>`
- Auto-refresh on expiry

### Error Handling
- Connection loss → Auto-reconnect
- Invalid snapshot → Log warning, continue
- Server error → Display error, return to lobby

## Next Steps (Post-Phase 2)

1. **Visual Polish**
   - Ship sprites/models
   - Bullet trails
   - Explosion effects
   - Background starfield

2. **Audio**
   - Background music
   - Weapon fire sounds
   - Hit/damage sounds
   - UI sounds

3. **Advanced Features**
   - Multiple ship types
   - Power-ups
   - Different game modes
   - Replay system

4. **Optimization**
   - Object pooling for bullets
   - Texture atlasing
   - Draw call batching
   - Memory profiling

## Troubleshooting

### Connection Issues
- Check backend is running (`npm run start:dev`)
- Verify WebSocket URL in SocketClient.cs
- Check firewall settings
- Review Unity console for errors

### Input Not Working
- Verify InputSender component is in scene
- Check Input System package is installed
- Test keyboard in Unity Editor first
- For Android, verify touch controls are visible

### Snapshot Not Updating
- Check SocketClient.OnStateSnapshot event is subscribed
- Verify server is sending snapshots (check backend logs)
- Look for parsing errors in Unity console

### Performance Issues
- Lower camera orthographic size
- Reduce particle effects
- Profile with Unity Profiler
- Check network latency

## Support

For issues, questions, or contributions:
- Review backend documentation in `/backend/README.md`
- Check architecture diagram in `/ARCHITECTURE_DIAGRAM.md`
- Review Phase 1 report in `/PHASE_1_COMPLETION_REPORT.md`

## License

MIT License - See LICENSE file for details.
