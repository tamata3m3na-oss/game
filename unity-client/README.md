# PvP Unity Client

A complete Unity client for the PvP game with Socket.IO real-time communication and JWT authentication.

## Features

- **Authentication**: JWT-based login/register system
- **Real-time Multiplayer**: Socket.IO WebSocket communication
- **Game Mechanics**: Ship movement, weapons, abilities, and shields
- **Scene Management**: Login, Lobby, Game, and Result scenes
- **Performance Optimization**: Object pooling, Addressables, and 60 FPS target
- **Cross-platform**: Designed for Android with keyboard fallback for development

## Architecture

### Core Systems

1. **AuthManager**: Handles user authentication, token management, and API calls
2. **NetworkManager**: Manages WebSocket connection, events, and game state synchronization
3. **InputController**: Handles touch and keyboard input with 60 FPS updates
4. **GameStateManager**: Processes game snapshots and manages game state
5. **ShipController**: Handles ship movement, rotation, and visual effects
6. **WeaponController**: Manages firing mechanics and bullet pooling
7. **AbilityController**: Handles shield ability with cooldown management

### Scene Flow

```
LoginScene → LobbyScene → GameScene → ResultScene → LobbyScene
```

### Network Events

**Client → Server:**
- `queue:join` - Join matchmaking queue
- `queue:leave` - Leave matchmaking queue  
- `match:ready` - Acknowledge match readiness
- `game:input` - Send player input (movement, fire, ability)

**Server → Client:**
- `queue:status` - Queue position and estimated wait
- `match:found` - Match found with opponent info
- `match:start` - Match starting with color assignment
- `game:snapshot` - Full game state broadcast (20Hz)
- `game:end` - Match ended with final results

## Setup

### Requirements

- Unity 2022.3 LTS or later
- Android build support
- Universal Render Pipeline (URP)

### Installation

1. Clone this repository
2. Open in Unity 2022.3+
3. Install required packages via Package Manager:
   - Input System
   - TextMeshPro
   - Addressables
   - URP
   - Socket.IO Client for Unity
   - Newtonsoft JSON

### Configuration

1. Set up scenes in Build Settings:
   - LoginScene
   - LobbyScene
   - GameScene  
   - ResultScene

2. Configure server URL in NetworkManager:
   ```csharp
   public string ServerUrl = "http://your-server-url:3000";
   ```

3. Set up URP renderer and quality settings for mobile

## Development

### Input System

- **Touch Controls**:
  - Movement: Touch drag
  - Fire: Tap
  - Ability: Swipe

- **Keyboard Controls (Dev)**:
  - Movement: WASD
  - Fire: Space
  - Ability: E

### Performance Optimization

- **Object Pooling**: Bullets and effects are pooled
- **Addressables**: Assets loaded on demand
- **Zero GC**: No allocations in game loop
- **60 FPS Target**: Optimized for mobile devices

### Debug Features

- FPS counter
- Ping display
- Position validation
- Anti-cheat warnings

## Building

### Android Build

1. Set build target to Android
2. Configure player settings:
   - Minimum API Level: 28 (Android 9)
   - Target API Level: Latest
   - Scripting Backend: IL2CPP
   - Architecture: ARM64

3. Build and run on device

### Development Build

For testing on PC:
1. Use keyboard controls
2. Set server URL to localhost
3. Test all scene transitions

## Testing

### Test Cases

1. **Authentication Flow**:
   - Register new user
   - Login with credentials
   - Auto-login on app restart
   - Token refresh

2. **Matchmaking Flow**:
   - Join queue
   - Leave queue
   - Match found
   - Match ready acknowledgment

3. **Gameplay Flow**:
   - Input sending (movement, fire, ability)
   - Snapshot processing
   - Health and shield updates
   - Game end conditions

4. **Performance Testing**:
   - 60 FPS maintenance
   - Snapshot processing time <5ms
   - Input sending time <2ms
   - Memory usage monitoring

## Troubleshooting

### Common Issues

1. **Connection Failed**:
   - Check server URL
   - Verify JWT token
   - Check network connectivity

2. **Input Not Working**:
   - Verify Input System package
   - Check touch/keyboard settings
   - Test input events

3. **Performance Issues**:
   - Check object pooling
   - Monitor GC allocations
   - Optimize shaders and materials

## License

This project is proprietary software. All rights reserved.