# 🏗️ المخطط المعماري - Unity Client Architecture

## 📐 نظرة عامة على البنية

```
┌─────────────────────────────────────────────────────────────┐
│                    Unity Client Architecture                │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  ┌─────────────────┐    ┌─────────────────┐                │
│  │   Network Layer │    │   UI Layer      │                │
│  │                 │    │                 │                │
│  │ NetworkManager  │◄──►│ UIControllers   │                │
│  │ NetworkEventMgr │    │ Animations      │                │
│  │                 │    │ Effects         │                │
│  └─────────────────┘    └─────────────────┘                │
│           │                       │                        │
│           │                       │                        │
│           ▼                       ▼                        │
│  ┌─────────────────┐    ┌─────────────────┐                │
│  │  State Layer    │    │  Logic Layer    │                │
│  │                 │    │                 │                │
│  │ GameStateRepo   │◄──►│ SnapshotProc    │                │
│  │ NetworkGameState│    │ GameTickManager │                │
│  │ PlayerSnapshot  │    │                 │                │
│  └─────────────────┘    └─────────────────┘                │
│           │                       │                        │
│           │                       │                        │
│           └───────────┬───────────┘                        │
│                       │                                    │
│                       ▼                                    │
│  ┌─────────────────────────────────────────┐              │
│  │         Single Source of Truth          │              │
│  │                                        │              │
│  │        GameStateRepository              │              │
│  │                                        │              │
│  └─────────────────────────────────────────┘              │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

## 🔄 Data Flow Diagram

```
┌─────────────┐    ┌──────────────┐    ┌─────────────┐
│   Backend   │───►│   Network    │───►│    Event    │
│   Server    │    │   Manager    │    │   Manager   │
└─────────────┘    └──────────────┘    └─────────────┘
                                              │
                                              ▼
┌─────────────┐    ┌──────────────┐    ┌─────────────┐
│   Result    │◄───│   Snapshot   │◄───│  Validate  │
│   Scene     │    │  Processor   │    │   Snapshot │
└─────────────┘    └──────────────┘    └─────────────┘
                                              │
                                              ▼
┌─────────────┐    ┌──────────────┐    ┌─────────────┐
│   UI/Game   │◄───│   GameState  │◄───│     Save   │
│   Objects   │    │  Repository  │    │    State    │
└─────────────┘    └──────────────┘    └─────────────┘
```

## 🏛️ Layer Responsibilities

### 1. Network Layer
**المسؤوليات:**
- WebSocket connection management
- Message serialization/deserialization
- Connection error handling
- Event forwarding

**الملفات:**
- `NetworkManager.cs` - WebSocket client
- `NetworkEventManager.cs` - Event dispatcher

### 2. State Layer  
**المسؤوليات:**
- Single Source of Truth
- Thread-safe state management
- Data validation
- Immutable snapshots

**الملفات:**
- `GameStateRepository.cs` - Central state storage
- `NetworkGameState.cs` - State data structure
- `PlayerStateSnapshot.cs` - Immutable player data

### 3. Logic Layer
**المسؤوليات:**
- Data transformation
- Tick synchronization
- Business logic
- Validation rules

**الملفات:**
- `SnapshotProcessor.cs` - Data transformation
- `GameTickManager.cs` - Timing coordination

### 4. UI Layer
**المسؤوليات:**
- User interface
- Visual feedback
- Input handling
- Animation effects

**الملفات:**
- `*UIController.cs` - Scene controllers
- `*SceneUI.cs` - Scene-specific UI
- `Animations/*` - Animation effects

## 🔐 Data Models

### NetworkGameState
```csharp
[Serializable]
public class NetworkGameState
{
    public int matchId;
    public PlayerStateData player1;
    public PlayerStateData player2;
    public int tick;
    public long timestamp;
    public int winner;
    public string status;
}
```

### PlayerStateSnapshot (Immutable)
```csharp
public class PlayerStateSnapshot
{
    public readonly int id;
    public readonly float x;
    public readonly float y;
    public readonly float health;
    public readonly bool shieldActive;
    // ... all fields are readonly
}
```

## 🚦 Event Flow

```
1. Server sends game:snapshot
        ↓
2. NetworkManager receives message
        ↓
3. NetworkEventManager parses JSON
        ↓
4. SnapshotProcessor validates data
        ↓
5. GameStateRepository updates state
        ↓
6. UI/Game objects notified via events
        ↓
7. Visual updates applied
```

## 🔄 State Management Pattern

### Single Source of Truth
- **Only** `GameStateRepository` stores game state
- **All** reads go through repository methods
- **All** writes go through repository methods
- **No** direct state access from other components

### Thread Safety
```csharp
// GameStateRepository uses lock for thread safety
private static readonly object lockObject = new object();

public NetworkGameState GetCurrentState()
{
    lock (lockObject)
    {
        return new NetworkGameState(currentGameState);
    }
}
```

### Immutable Snapshots
```csharp
// UI gets immutable snapshots, cannot modify state
public PlayerStateSnapshot GetPlayerState(int playerId)
{
    // Returns immutable snapshot
    return playerSnapshots[playerId];
}
```

## 🛡️ Safety Mechanisms

### Data Validation
```csharp
private bool ValidateSnapshot(NetworkGameStateData snapshot)
{
    if (snapshot.player1 == null || snapshot.player2 == null)
        return false;
        
    if (snapshot.player1.id <= 0 || snapshot.player2.id <= 0)
        return false;
        
    // Additional validation...
}
```

### Error Handling
```csharp
try
{
    stateRepository.UpdateGameState(gameState);
}
catch (Exception e)
{
    Debug.LogError($"State update failed: {e.Message}");
    // Graceful degradation
}
```

### Connection Recovery
```csharp
private async void HandleConnectionError()
{
    // Automatic reconnection logic
    await Task.Delay(1000);
    await ConnectToServer();
}
```

## 📊 Performance Optimizations

### Object Pooling
```csharp
// Object reuse to reduce GC pressure
public class ObjectPool<T> where T : new()
{
    private readonly Queue<T> pool = new Queue<T>();
    
    public T Get() => pool.Count > 0 ? pool.Dequeue() : new T();
    public void Return(T item) => pool.Enqueue(item);
}
```

### Lazy Loading
```csharp
// Cache built snapshots for performance
if (!playerSnapshots.ContainsKey(playerId))
{
    playerSnapshots[playerId] = BuildPlayerSnapshot(playerId);
}
```

### Efficient Serialization
```csharp
// Use System.Text.Json for better performance
string json = JsonSerializer.Serialize(message);
```

## 🔧 Integration Points

### Backend Communication
- **WebSocket URL**: `ws://localhost:3000/pvp`
- **Auth Token**: JWT token in connection query
- **Message Format**: `{type: string, data: string}`

### Supported Events
- `queue:status` - Matchmaking updates
- `match:found` - Match discovery
- `match:start` - Game beginning
- `game:snapshot` - State updates (20Hz)
- `game:end` - Match conclusion

### Unity Integration
- **Scene Management**: GameManager handles scene transitions
- **Input System**: InputController processes player input
- **UI System**: TextMeshPro + Unity UI
- **Animation System**: Coroutines (no external dependencies)

## 🧪 Testing Strategy

### Unit Tests
- State repository operations
- Data validation logic
- Event handling

### Integration Tests
- Network communication
- End-to-end data flow
- Scene transitions

### Performance Tests
- Memory usage monitoring
- Frame rate stability
- Network latency handling

---

**آخر تحديث:** اليوم  
**الإصدار:** 1.0  
**المؤلف:** AI Development Agent  
