# Scene-Local vs Singleton Patterns

## Overview
This document explains the migration from Singleton to Scene-Local patterns in the Unity client, specifically focusing on GameStateManager.

## Problem with Singleton Pattern

### Issues:
1. **Global State**: Singleton creates global state that persists across scenes
2. **Tight Coupling**: Components become tightly coupled to specific singleton instances
3. **Testing Difficulty**: Hard to test components in isolation
4. **Multiple Scene Conflicts**: Can cause conflicts when multiple scene instances exist
5. **Memory Leaks**: Singletons can persist when they should be destroyed

### Example of Problematic Singleton Pattern:
```csharp
public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
```

## Scene-Local Pattern Solution

### Benefits:
1. **Encapsulation**: Each scene manages its own instances
2. **Clean Lifecycle**: Objects are created/destroyed with their scenes
3. **Flexibility**: Different scenes can have different configurations
4. **Better Testing**: Easier to test scene-specific functionality
5. **Reduced Memory Usage**: No unnecessary objects persisting across scenes

### Scene-Local Pattern Implementation:
```csharp
public class GameStateManager : MonoBehaviour
{
    // No static Instance property
    
    private void Awake()
    {
        // Validate component is in scene
        if (GetComponent<GameStateManager>() == null)
        {
            Debug.LogError("GameStateManager component validation failed");
        }
    }
    
    private void OnEnable()
    {
        // Find dependencies from scene instead of using .Instance
        var networkEventManager = FindObjectOfType<NetworkEventManager>();
        if (networkEventManager != null)
        {
            networkEventManager.OnGameSnapshotReceived += HandleGameSnapshot;
        }
    }
}
```

## Migration Changes Made

### 1. Removed Singleton Dependencies
**Before:**
```csharp
var nem = NetworkEventManager.Instance;
var inputController = InputController.Instance;
var network = NetworkManager.Instance;
var authManager = AuthManager.Instance;
```

**After:**
```csharp
var nem = FindObjectOfType<NetworkEventManager>();
var inputController = FindObjectOfType<InputController>();
var network = FindObjectOfType<NetworkManager>();
var authManager = FindObjectOfType<AuthManager>();
```

### 2. Added Proper Validation
- Added validation in Awake() to ensure component is properly configured
- Added null checks with appropriate warning/error messages
- Graceful degradation when dependencies are missing

### 3. Scene-Local Lifecycle
- Removed DontDestroyOnLoad calls
- GameStateManager now properly follows scene lifecycle
- Automatically cleaned up when scene is unloaded

## Usage Patterns

### Finding GameStateManager
**Old Singleton Way:**
```csharp
var gameStateManager = GameStateManager.Instance;
```

**New Scene-Local Way:**
```csharp
var gameStateManager = FindObjectOfType<GameStateManager>();
if (gameStateManager != null)
{
    // Use gameStateManager
}
```

### Finding Dependencies in Other Managers
All managers that previously used .Instance should use FindObjectOfType:

```csharp
// Example in MatchUIController
private void Start()
{
    gameStateManager = FindObjectOfType<GameStateManager>();
    if (gameStateManager != null)
    {
        playerShip = gameStateManager.GetPlayerShip();
        opponentShip = gameStateManager.GetOpponentShip();
    }
}
```

## Benefits Achieved

1. **Better Memory Management**: Objects are properly cleaned up with scenes
2. **Reduced Coupling**: Components don't depend on global singletons
3. **Easier Testing**: Can test scene components in isolation
4. **Flexibility**: Different scenes can have different configurations
5. **No Conflicts**: Multiple scene instances won't conflict

## When to Use Each Pattern

### Use Scene-Local When:
- Managing scene-specific state
- Objects should be destroyed when scene unloads
- Different scenes need different configurations
- Testing individual scene components

### Use Singleton When:
- Truly global state is needed (e.g., audio manager)
- State must persist across all scenes
- Configuration is the same everywhere
- Single instance is conceptually correct

## Migration Checklist

- [x] Remove static Instance properties from GameStateManager
- [x] Replace .Instance calls with FindObjectOfType<>
- [x] Add proper null checking and validation
- [x] Remove DontDestroyOnLoad calls
- [x] Update documentation and comments
- [x] Test that all scene transitions work properly
- [x] Verify no memory leaks
- [x] Ensure all UI components can find GameStateManager

## Conclusion

The migration from Singleton to Scene-Local pattern for GameStateManager provides better encapsulation, cleaner lifecycle management, and improved flexibility while maintaining all the functionality of the original implementation.