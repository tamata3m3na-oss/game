# Script Execution Order & Lifecycle Reference

## 🎯 نظرة عامة / Overview

Quick reference guide for Unity Script Execution Order and lifecycle timing to prevent MonoManager NULL errors and ensure proper initialization.

**الهدف / Goal**: Provide clear, actionable reference for developers to maintain proper Unity lifecycle compliance.

---

## 📊 Script Execution Order & Lifecycle

### **ProjectSettings/ProjectSettings.asset Configuration:**

```json
{
  "m_ScriptingBackend": {
    "WebGL": "Il2Cpp",
    "Standalone": "Il2Cpp"
  },
  "m_ScriptingDefineSymbols": {
    "Standalone": "UNITY_POST_PROCESSING_STACK_V2"
  },
  "m_DefaultExecutionOrder": {
    "ThreadSafeEventQueue": -100,
    "BootstrapRunner": -100,
    "NetworkManager": -50,
    "AuthManager": -50,
    "InputController": -50,
    "GameManager": 0,
    "GameStateManager": 0,
    "InputController": 0,
    "SceneInitializer": 0
  }
}
```

---

## 🔄 Unity Lifecycle Timing

### **Initialization Sequence:**

```
Application Start
├── Unity Engine Initialization
├── Scene Loading
│   ├── Awake() Phase (Execution Order ascending)
│   │   ├── Order -100: ThreadSafeEventQueue.Awake()
│   │   ├── Order -100: BootstrapRunner.Awake()
│   │   ├── Order -50: NetworkManager.Awake()
│   │   ├── Order -50: AuthManager.Awake()
│   │   ├── Order -50: InputController.Awake()
│   │   └── Order 0: GameManager.Awake()
│   ├── OnEnable() Phase (same order as Awake)
│   ├── Start() Phase (same order as Awake)
│   ├── Update() Loops...
│   └── On Scene Change / Application Quit
│       ├── OnDisable() Phase (reverse order)
│       └── OnDestroy() Phase (reverse order)
```

### **Frame-by-Frame Flow:**

```
Frame N: Scene Active
├─ Awake() calls (Execution Order: -100 → 0)
├─ OnEnable() calls (same order)
├─ Start() calls (same order)  
├─ Update() calls (every frame)
├─ LateUpdate() calls (every frame)
├─ OnDisable() calls (if scene changes)
└─ OnDestroy() calls (if GameObject destroyed)
```

---

## 📋 Script Execution Order Table

| Order | Script | Type | Purpose | Lifecycle Phase |
|-------|--------|------|---------|----------------|
| **-100** | `ThreadSafeEventQueue` | Infrastructure | Main thread communication | Awake/Start |
| **-100** | `BootstrapRunner` | Entry Point | Manager initialization coordination | Awake |
| **-50** | `NetworkManager` | Global Service | WebSocket & network operations | Awake/Start |
| **-50** | `AuthManager` | Global Service | Authentication & user management | Awake/Start |
| **-50** | `InputController` | Global Service | Input system coordination | Awake/Start |
| **-50** | `NetworkEventManager` | Global Service | Network event processing | Awake/Start |
| **-50** | `GameStateRepository` | Global Service | Game state management | Awake/Start |
| **0** | `GameManager` | Coordinator | Scene coordination & validation | Awake/Start |
| **0** | `SceneInitializer` | Scene-Specific | Scene initialization | Awake |
| **0** | `GameStateManager` | Game Logic | Game state coordination | Awake/Start |
| **0** | `SnapshotProcessor` | Game Logic | Network snapshot processing | Awake/Start |
| **0** | `GameTickManager` | Game Logic | Game tick coordination | Awake/Start |
| **+50** | `UI Managers` | UI Layer | UI coordination & display | Start/Update |
| **+100** | `Scene-Specific Controllers` | UI Layer | Scene-specific UI logic | Start/Update |

---

## 🔧 Unity Lifecycle Best Practices

### **Awake() Phase:**
```csharp
[DefaultExecutionOrder(-100)]
public class ThreadSafeEventQueue : MonoBehaviour
{
    private void Awake()
    {
        // ✅ Infrastructure initialization
        // ✅ Singleton setup
        // ✅ DontDestroyOnLoad setup
        // ✅ Thread validation
        // ✅ Event queue initialization
    }
}

[DefaultExecutionOrder(-50)]
public class NetworkManager : MonoBehaviour
{
    private void Awake()
    {
        // ✅ Global service initialization
        // ✅ Network setup
        // ✅ WebSocket configuration
        // ✅ Event listener setup
    }
}

[DefaultExecutionOrder(0)]
public class GameManager : MonoBehaviour
{
    private void Awake()
    {
        // ✅ Singleton setup only
        // ✅ Coordinator initialization
        // ✅ Reference assignment
    }
}
```

### **Start() Phase:**
```csharp
[DefaultExecutionOrder(-100)]
public class ThreadSafeEventQueue : MonoBehaviour
{
    private void Start()
    {
        // ✅ Begin processing queue
        // ✅ Start main thread communication
    }
}

[DefaultExecutionOrder(0)]
public class GameManager : MonoBehaviour
{
    private void Start()
    {
        // ✅ Find existing managers (scene-based)
        // ✅ Validate dependencies
        // ✅ Setup coordination
        // ✅ Begin scene management
    }
}
```

### **Update() Phase:**
```csharp
[DefaultExecutionOrder(-100)]
public class ThreadSafeEventQueue : MonoBehaviour
{
    private void Update()
    {
        // ✅ Process queued actions
        // ✅ Handle main thread operations
        // ✅ Execute WebSocket callbacks
    }
}

[DefaultExecutionOrder(0)]
public class NetworkManager : MonoBehaviour
{
    private void Update()
    {
        // ✅ Network operations
        // ✅ Message processing
        // ✅ Connection management
    }
}
```

---

## 🧵 Thread Safety Reference

### **Thread Context Matrix:**

| Operation | Thread Context | Safe? | Implementation Method |
|-----------|----------------|-------|---------------------|
| **Debug.Log()** | WebSocket/Background | ❌ | `ThreadSafeEventQueue.Enqueue(() => Debug.Log())` |
| **transform.position** | WebSocket/Background | ❌ | `ThreadSafeEventQueue.Enqueue(() => transform.position = ...)` |
| **FindObjectOfType<T>()** | Main Thread | ✅ | Direct call |
| **GameObject.Find()** | Main Thread | ✅ | Direct call |
| **UI.text.text** | WebSocket/Background | ❌ | `ThreadSafeEventQueue.Enqueue(() => uiText.text = ...)` |
| **JSON Serialize/Parse** | Any Thread | ✅ | Direct call (no Unity APIs) |
| **Mathematical Operations** | Any Thread | ✅ | Direct call |
| **String Operations** | Any Thread | ✅ | Direct call |

### **Correct Implementation Pattern:**

```csharp
// ❌ INCORRECT - Direct Unity API calls from WebSocket thread
public void OnWebSocketMessage(string message)
{
    // This will crash!
    Debug.Log($"Message: {message}");
    authManager.ProcessMessage(message);
    uiController.UpdateDisplay(message);
}

// ✅ CORRECT - Using ThreadSafeEventQueue
public void OnWebSocketMessage(string message)
{
    // Queue for main thread execution
    ThreadSafeEventQueue.Enqueue(() =>
    {
        // Safe Unity API calls
        Debug.Log($"Message: {message}");
        authManager?.ProcessMessage(message);
        uiController?.UpdateDisplay(message);
    });
}
```

---

## 🏗️ Manager Hierarchy & Dependencies

### **Global Managers (DontDestroyOnLoad):**

```csharp
Bootstrap (RuntimeInitializeOnLoadMethod)
└── ThreadSafeEventQueue (-100)
    ├── NetworkManager (-50)
    │   ├── WebSocket operations
    │   ├── Message queuing
    │   └── Network event processing
    ├── AuthManager (-50)
    │   ├── Authentication state
    │   ├── Token management
    │   └── User session handling
    ├── InputController (-50)
    │   ├── Input buffering
    │   ├── Input event processing
    │   └── Input state management
    └── GameManager (0)
        ├── Scene coordination
        ├── Manager validation
        └── Game state orchestration
```

### **Scene-Specific Managers:**

```csharp
LoginScene
├── LoginUIController
├── RegisterUIController
└── LoadingUIController

GameScene  
├── GameUIController
├── HealthDisplay
├── ScoreDisplay
├── GameEndUIController
├── PlayerShipController
├── EnemyShipController
└── GameEnvironmentController
```

---

## 🚀 How to Add New Managers

### **Step 1: Global Manager (DontDestroyOnLoad)**

```csharp
// ✅ Create script with proper execution order
[DefaultExecutionOrder(-50)] // After infrastructure, before coordinators
public class NewGlobalManager : MonoBehaviour
{
    public static NewGlobalManager Instance { get; private set; }
    
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

// ✅ Add to BootstrapRunner
public class BootstrapRunner : MonoBehaviour
{
    private void Awake()
    {
        // ... existing managers ...
        EnsureSingletonComponent<NewGlobalManager>();
    }
}

// ✅ Add prefab to LoginScene
// Drag NewGlobalManager prefab to LoginScene hierarchy
```

### **Step 2: Scene-Specific Manager**

```csharp
// ✅ Create script without singleton pattern
[DefaultExecutionOrder(0)] // After global managers
public class NewSceneManager : MonoBehaviour
{
    public void InitializeScene()
    {
        Debug.Log("New scene manager initialized");
    }
}

// ✅ Add prefab to specific scene
// Drag NewSceneManager prefab to target scene

// ✅ Find via GameManager
public class GameManager : MonoBehaviour
{
    [Header("Scene-Specific Managers")]
    public NewSceneManager newSceneManager;
    
    private void Start()
    {
        FindManagers();
    }
    
    private void FindManagers()
    {
        // ... existing finds ...
        if (newSceneManager == null)
        {
            newSceneManager = FindObjectOfType<NewSceneManager>();
        }
    }
}
```

---

## ⚠️ Common Mistakes to Avoid

### **❌ Wrong Execution Order:**
```csharp
[DefaultExecutionOrder( late!
public class CriticalManager : MonoBehaviour100)] // Too
{
    private void Awake()
    {
        // Other managers might try to find this before it's ready!
        var other = FindObjectOfType<OtherManager>();
        other.Initialize(this); // NULL reference!
    }
}
```

### **❌ Missing DontDestroyOnLoad:**
```csharp
public class GlobalManager : MonoBehaviour
{
    private void Awake()
    {
        // ❌ Missing DontDestroyOnLoad - will be destroyed on scene change!
        Instance = this;
    }
}
```

### **❌ Wrong Thread Usage:**
```csharp
public async void OnWebSocketData(byte[] data)
{
    // ❌ Direct Unity API call from async context
    transform.position = ParsePosition(data);
    
    // ✅ Correct - use ThreadSafeEventQueue
    ThreadSafeEventQueue.Enqueue(() =>
    {
        transform.position = ParsePosition(data);
    });
}
```

### **❌ Constructor Unity API Calls:**
```csharp
public class BadManager
{
    public BadManager()
    {
        // ❌ Unity APIs not available in constructors!
        var manager = FindObjectOfType<SomeManager>();
    }
}
```

---

## ✅ Validation Checklist

### **Before Adding New Manager:**

- [ ] **Determine manager type**: Global (DontDestroyOnLoad) or Scene-specific?
- [ ] **Set proper Execution Order**: Infrastructure (-100), Services (-50), Coordinators (0)
- [ ] **Add to Bootstrap if global**: Add EnsureSingletonComponent call
- [ ] **Create prefab**: Drag to appropriate scene
- [ ] **Test initialization order**: Verify Awake/Start sequence
- [ ] **Test thread safety**: Ensure Unity APIs called from main thread
- [ ] **Test scene transitions**: Verify persistence/destruction as expected

### **Code Review Checklist:**

- [ ] **No `new GameObject()` calls**: Use prefabs instead
- [ ] **No runtime `AddComponent<T>()`**: Use scene prefabs
- [ ] **Proper Execution Order**: Set for dependencies
- [ ] **Thread safety**: Unity APIs only from main thread
- [ ] **Null handling**: Use null-conditional operators (?.)
- [ ] **Clear logging**: Initialization steps logged clearly

---

## 📚 Quick Reference Commands

### **Unity Console Output (Expected):**
```csharp
[ThreadSafeEventQueue] Initialized on main thread
[BootstrapRunner] Initializing global managers
[NetworkManager] Found in scene, reference assigned
[AuthManager] Found in scene, reference assigned  
[InputController] Found in scene, reference assigned
[GameManager] All managers validated successfully
[SceneInitializer] LoginScene initialized
```

### **Error Messages (What to Watch For):**
```csharp
// ❌ Warning: Manager not found
[GameManager] AuthManager not found! Please add AuthManager to LoginScene prefab.

// ❌ Error: Thread violation
[ThreadSafeEventQueue] Awake() was not called on the main thread.

// ❌ Error: Duplicate singleton
[NewManager] Instance already exists, destroying duplicate.
```

This reference guide ensures all developers understand the proper Unity lifecycle management and can add new managers correctly while maintaining system stability.