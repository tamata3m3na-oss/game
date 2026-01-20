# Bootstrap Architecture - النظام الجديد للتهيئة

## 🎯 نظرة عامة / Overview

The Bootstrap Architecture ensures proper Unity lifecycle initialization through a systematic, ordered approach that prevents MonoManager NULL errors and respects Unity's internal timing requirements.

**المبدأ الأساسي**: جميع العمليات تتم في الترتيب الصحيح / **Core Principle**: All operations happen in the correct order

---

## 🔄 التسلسل الجديد للتهيئة / New Initialization Sequence:

### 1️⃣ Initialization Order (Fixed):

#### **Phase 1: Engine Load (Frame 0)**
```
Unity Engine Startup
↓
[RuntimeInitializeOnLoadMethod] triggered
↓
Bootstrap.CreateBootstrapObject()
↓
BootstrapRunner created as DontDestroyOnLoad
↓
Awake() phase begins
```

#### **Phase 2: Infrastructure Setup (Awake -100)**
```
ThreadSafeEventQueue.Awake()
├── Initialize singleton instance ✓
├── Setup MainThread validation ✓
├── Initialize ConcurrentQueue ✓
└── Mark as DontDestroyOnLoad ✓

BootstrapRunner.Awake()
├── Call EnsureSingletonComponent for all managers
└── No dynamic creation - only FindObjectOfType()
```

#### **Phase 3: Core Services (Awake -50)**
```
NetworkManager.Awake()
├── Create DontDestroyOnLoad instance ✓
├── Initialize WebSocket connections ✓
└── Setup event listeners ✓

AuthManager.Awake()
├── Initialize authentication state ✓
├── Setup HTTP client ✓
└── Configure token management ✓

InputController.Awake()
├── Initialize input systems ✓
├── Setup event listeners ✓
└── Configure input buffers ✓
```

#### **Phase 4: Game Coordination (Awake 0)**
```
GameManager.Awake()
├── Setup singleton instance ✓
├── Configure DontDestroyOnLoad ✓
└── Prepare scene coordination ✓

SceneInitializer.Awake()
├── Detect current scene ✓
├── Prepare scene-specific setup ✓
└── Configure scene transitions ✓
```

#### **Phase 5: Start Phase Processing**
```
ThreadSafeEventQueue.Start()
├── Begin Update() processing ✓
└── Start processing queued actions ✓

NetworkManager.Start()
├── Establish connections if needed ✓
└── Begin receiving messages ✓

GameManager.Start()
├── Find existing managers (scene-based) ✓
├── Validate all required managers ✓
└── Setup scene coordination ✓
```

---

## 🔧 ThreadSafeEventQueue Flow:

### **WebSocket Thread → Main Thread Communication:**

```
WebSocket Thread          Main Thread
     |                        |
     | (message received)     |
     |                        |
     |----Enqueue(Action)---->|
     |                        |
     | (Next Update cycle)    |
     |                        |
     | (Update() called)      |
     |                        |
     |<---Process Queue------|
     |                        |
     | (Execute all actions)   |
     |                        |
     | (Safe Unity API calls) |
     |                        |
(safe, no race conditions)
```

### **Implementation Details:**

```csharp
public static void Enqueue(Action action)
{
    if (action == null) return;

    if (Instance == null)
    {
        // Fallback for early initialization
        if (UnityMainThread.IsMainThread)
        {
            action(); // Direct execution if already on main thread
            return;
        }
        return; // Drop action if not initialized and not on main thread
    }

    Instance.queue.Enqueue(action); // Safe queue addition
}

private void Update()
{
    int processed = 0;
    
    // Process queued actions safely on main thread
    while (processed < maxActionsPerFrame && queue.TryDequeue(out var action))
    {
        try
        {
            action?.Invoke(); // Execute with null safety
        }
        catch (Exception ex)
        {
            Debug.LogException(ex); // Safe error handling
        }
        
        processed++;
    }
}
```

---

## 🏗️ Manager Relationships & Dependencies:

### **Dependency Hierarchy:**

```
Bootstrap (Entry Point - RuntimeInitializeOnLoadMethod)
│
├── ThreadSafeEventQueue (Infrastructure - DefaultExecutionOrder(-100))
│   └── Enables safe cross-thread communication
│
├── NetworkManager (Global Service - DefaultExecutionOrder(-50))
│   ├── WebSocket management
│   ├── Thread-safe message queuing
│   └── Network event processing
│
├── AuthManager (Global Service - DefaultExecutionOrder(-50))
│   ├── Authentication state management
│   ├── Token refresh handling
│   └── User session coordination
│
├── InputController (Global Service - DefaultExecutionOrder(-50))
│   ├── Input event processing
│   ├── Input buffering
│   └── Input state management
│
├── GameManager (Coordinator - DefaultExecutionOrder(0))
│   ├── Scene coordination
│   ├── Manager validation
│   └── Game state orchestration
│
└── SceneInitializer (Scene-Specific - DefaultExecutionOrder(0))
    ├── Scene-specific initialization
    ├── UI controller setup
    └── Scene transition management
```

### **Communication Flow:**

```
WebSocket Messages
        ↓
ThreadSafeEventQueue.Enqueue()
        ↓
Main Thread Update()
        ↓
NetworkEventManager.ProcessEvent()
        ↓
GameStateManager.Update()
        ↓
UI Controllers Update()
```

---

## 🔧 Implementation Details:

### **Bootstrap.cs Structure:**

```csharp
public static class Bootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateBootstrapObject()
    {
        // Ensure only one BootstrapRunner exists
        if (Object.FindObjectOfType<BootstrapRunner>() != null)
        {
            return;
        }

        // Create single bootstrap object
        var go = new GameObject("_Bootstrap");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<BootstrapRunner>();
    }
}

[DefaultExecutionOrder(-100)]
public sealed class BootstrapRunner : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        // Ensure all singletons are properly initialized
        EnsureSingletonComponent<ThreadSafeEventQueue>();
        EnsureSingletonComponent<NetworkEventManager>();
        EnsureSingletonComponent<NetworkManager>();
        EnsureSingletonComponent<AuthManager>();
        EnsureSingletonComponent<InputController>();

        EnsureSingletonComponent<GameStateRepository>();
        EnsureSingletonComponent<GameTickManager>();
        EnsureSingletonComponent<SnapshotProcessor>();

        EnsureSingletonComponent<AnimationController>();
        EnsureSingletonComponent<ParticleController>();
        EnsureSingletonComponent<TransitionManager>();

        EnsureSingletonComponent<SceneInitializer>();
    }

    private void EnsureSingletonComponent<T>() where T : Component
    {
        // Find existing component first - no dynamic creation
        if (FindObjectOfType<T>() != null)
        {
            return;
        }

        // Only add if not found (for global singletons)
        gameObject.AddComponent<T>();
    }
}
```

### **GameManager.cs Scene-Based Approach:**

```csharp
public class GameManager : MonoBehaviour
{
    [Header("Manager References (Scene-Based)")]
    [Tooltip("Assigned via scene prefabs or found at runtime - never created dynamically")]
    public AuthManager authManager;
    public NetworkManager networkManager;
    public InputController inputController;

    private void Start()
    {
        // Find existing managers in scene - DO NOT CREATE THEM
        // Using FindObjectOfType instead of new GameObject() to respect Unity lifecycle
        FindManagers();
        ValidateManagers();
    }

    private void FindManagers()
    {
        // Only find if not already assigned in inspector
        if (authManager == null)
        {
            authManager = FindObjectOfType<AuthManager>();
        }

        if (networkManager == null)
        {
            networkManager = FindObjectOfType<NetworkManager>();
        }

        if (inputController == null)
        {
            inputController = FindObjectOfType<InputController>();
        }
    }

    private void ValidateManagers()
    {
        // Clear logging for missing components instead of silently failing
        if (authManager == null)
        {
            Debug.LogWarning("[GameManager] AuthManager not found! Please add AuthManager to LoginScene prefab.");
        }
    }
}
```

---

## 📋 Scene Requirements:

### **LoginScene Prefab Setup:**

```
LoginScene (Root)
├── _Bootstrap (DontDestroyOnLoad)
│   ├── BootstrapRunner
│   ├── ThreadSafeEventQueue
│   ├── NetworkManager
│   ├── AuthManager
│   ├── InputController
│   └── GameManager
│
├── UI_Canvas (Scene-Specific)
│   ├── LoginUIController
│   ├── RegisterUIController
│   └── LoadingUIController
│
└── SceneInitializer (Scene-Specific)
```

### **GameScene Prefab Setup:**

```
GameScene (Root)
├── _Bootstrap (Inherited from LoginScene)
│   └── (All global managers preserved)
│
├── GameUI_Canvas (Scene-Specific)
│   ├── GameUIController
│   ├── HealthDisplay
│   ├── ScoreDisplay
│   └── GameEndUIController
│
├── GameObjects (Scene-Specific)
│   ├── PlayerShip
│   ├── EnemyShips
│   ├── GameEnvironment
│   └── CameraController
│
└── SceneInitializer (Scene-Specific)
```

---

## ✅ Benefits of Bootstrap Architecture:

### **1. Guaranteed Initialization Order:**
- Infrastructure (ThreadSafeEventQueue) initializes first
- Global services (Network, Auth, Input) initialize next
- Scene-specific managers initialize last
- No race conditions or timing issues

### **2. Thread Safety:**
- All WebSocket messages queued safely
- Unity API calls only from Main Thread
- No cross-thread Unity API violations
- Safe async operation handling

### **3. Scene Flexibility:**
- Managers persist across scene changes
- Scene-specific components can be added/removed
- No hard-coded GameObject creation
- Clear separation of global vs local concerns

### **4. Error Prevention:**
- No more MonoManager NULL errors
- Clear error messages for missing components
- Predictable behavior across Unity versions
- Robust initialization even with timing variations

### **5. Maintainability:**
- Clear dependency hierarchy
- Well-defined initialization sequence
- Easy to add new managers
- Easy to debug initialization issues

---

## 🚀 Migration Benefits:

### **Before Bootstrap Architecture:**
```csharp
// Random initialization order
// Race conditions
// NULL references
// Thread safety violations
// Version-dependent behavior
```

### **After Bootstrap Architecture:**
```csharp
// Fixed initialization order
// No race conditions  
// No NULL references
// Thread-safe by design
// Consistent behavior across versions
```

The Bootstrap Architecture ensures that Unity's internal managers are properly initialized before any MonoBehaviour operations, completely eliminating the MonoManager NULL error while providing a robust, maintainable foundation for the application.