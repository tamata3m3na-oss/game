# Migration Guide: What Changed & Why

## 📚 نظرة عامة / Overview

This guide explains the critical changes made to resolve Unity lifecycle violations and provides practical rules for future development to prevent similar issues.

**الهدف / Goal**: Ensure developers understand what changed and why, with clear rules for future development.

---

## ❌ الملفات المحذوفة / Deleted Files & Methods:

### 1️⃣ GameManager.InitializeManagers() ❌

**المشكلة الأصلية / Original Problem:**
```csharp
private void InitializeManagers()
{
    // ❌ هذه الطريقة كانت تنشئ GameObjects في Runtime
    GameObject authObj = new GameObject("AuthManager");
    authManager = authObj.AddComponent<AuthManager>();
    
    GameObject networkObj = new GameObject("NetworkManager");  
    networkManager = networkObj.AddComponent<NetworkManager>();
    
    GameObject inputObj = new GameObject("InputController");
    inputController = inputObj.AddComponent<InputController>();
}
```

**السبب / Reason:**
- تنشئ GameObjects في Runtime / Creates GameObjects at Runtime
- كان السبب المباشر لـ MonoManager NULL / Was the direct cause of MonoManager NULL
- يخترق Unity's Script Binding System / Violates Unity's Script Binding System
- لا يحترم Unity Lifecycle / Doesn't respect Unity Lifecycle

**التأثير / Impact:**
- ❌ MonoManager.GetManagerFromContext() = NULL
- ❌ Application crashes on startup
- ❌ Random timing issues
- ❌ Platform-dependent behavior

### 2️⃣ Dynamic AddComponent<T>() Calls ❌

**المشكلة الأصلية / Original Problem:**
```csharp
// ❌ في أي مكان في الكود
var manager = gameObject.AddComponent<T>(); // فجأة قد يكون NULL
```

**السبب / Reason:**
- MonoManager قد لا تكون جاهزة / MonoManager may not be ready
- يحدث خارج Scene Initialization context / Happens outside Scene Initialization context
- Race conditions بين different threads / Race conditions between different threads

### 3️⃣ Runtime GameObject Creation ❌

**المشكلة الأصلية / Original Problem:**
```csharp
// ❌ في أي مكان
var go = new GameObject("SomeManager");
var component = go.AddComponent<SomeManager>();
```

**السبب / Reason:**
- يخترق Unity's internal timing / Violates Unity's internal timing
- لا يحترم Script Execution Order / Doesn't respect Script Execution Order
- MonoBehaviour lifecycle violations / MonoBehaviour lifecycle violations

---

## ✅ الملفات الجديدة / New Files Added:

### 1️⃣ ThreadSafeEventQueue.cs

**الغرض / Purpose:** Queue آمنة للـ Main Thread operations

**الاستخدام / Usage:** WebSocket callbacks, async operations

**مثال الاستخدام / Usage Example:**
```csharp
// ✅ الطريقة الصحيحة للـ WebSocket callbacks
public void OnWebSocketMessageReceived(string message)
{
    // ❌ هذا خطير - قد يكون من WebSocket thread
    // Debug.Log($"Received: {message}");
    // authManager.ProcessMessage(message); // NULL reference!
    
    // ✅ الطريقة الآمنة
    ThreadSafeEventQueue.Enqueue(() =>
    {
        // هذا يُنفذ في Main Thread بشكل آمن
        Debug.Log($"Received: {message}");
        authManager?.ProcessMessage(message);
    });
}
```

**لماذا ضروري / Why Necessary:**
- WebSocket callbacks من ThreadPool / WebSocket callbacks from ThreadPool
- Unity APIs تحتاج Main Thread / Unity APIs require Main Thread
- يمنع Race conditions / Prevents race conditions

### 2️⃣ Bootstrap.cs

**الغرض / Purpose:** Entry Point الوحيد للمشروع

**التفويض / Responsibilities:**
- تهيئة جميع managers بالترتيب الصحيح / Initialize all managers in correct order
- ضمان احترام Unity Lifecycle / Ensure Unity Lifecycle respect
- منع التكرار / Prevent duplicates

**مثال / Example:**
```csharp
[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
private static void CreateBootstrapObject()
{
    if (Object.FindObjectOfType<BootstrapRunner>() != null)
    {
        return; // Prevent duplicates
    }

    var go = new GameObject("_Bootstrap");
    Object.DontDestroyOnLoad(go);
    go.AddComponent<BootstrapRunner>();
}
```

### 3️⃣ SceneInitializer.cs

**الغرض / Purpose:** تهيئة آمنة لكل Scene

**الاستخدام / Usage:** يُضاف على كل Scene

**مثال / Example:**
```csharp
[DefaultExecutionOrder(0)]
public class SceneInitializer : MonoBehaviour
{
    private void Awake()
    {
        var currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        Debug.Log($"Initializing scene: {currentScene.name}");
        
        switch (currentScene.name)
        {
            case "LoginScene":
                InitializeLoginScene();
                break;
            case "GameScene":  
                InitializeGameScene();
                break;
        }
    }
}
```

---

## 🔄 التغييرات في الكود / Code Changes:

### قبل / Before:

```csharp
public class GameManager : MonoBehaviour
{
    private void Start()
    {
        // ❌ الطريقة الخطيرة
        InitializeManagers(); // Creates GameObjects at runtime!
    }
    
    private void InitializeManagers()
    {
        // Creates GameObjects outside Unity's control
        authManager = new GameObject("AuthManager").AddComponent<AuthManager>();
        networkManager = new GameObject("NetworkManager").AddComponent<NetworkManager>();
        inputController = new GameObject("InputController").AddComponent<InputController>();
    }
}
```

### بعد / After:

```csharp
public class GameManager : MonoBehaviour
{
    [Header("Manager References (Scene-Based)")]
    public AuthManager authManager;
    public NetworkManager networkManager; 
    public InputController inputController;

    private void Start()
    {
        // ✅ الطريقة الآمنة - find existing managers only
        FindManagers();
        ValidateManagers();
    }
    
    private void FindManagers()
    {
        // ✅ Find existing managers without creating new ones
        if (authManager == null)
        {
            authManager = FindObjectOfType<AuthManager>(); // Safe, scene-based
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
        // ✅ Clear warnings instead of silent crashes
        if (authManager == null)
        {
            Debug.LogWarning("[GameManager] AuthManager not found! Please add AuthManager to LoginScene prefab.");
        }
    }
}
```

---

## 🛡️ كيفية تجنب تكرار الخطأ / How to Prevent the Error Recurring:

### القاعدة الذهبية / Golden Rule:

**لا تنشئ MonoBehaviours في Runtime. إن احتجتها، أضفها في prefabs.**

**Don't create MonoBehaviours at Runtime. If you need them, add them to prefabs.**

### Checklist للـ Future Development:

#### ✅ Do's (افعل):

```csharp
// ✅ Use scene prefabs
public class SomeManager : MonoBehaviour
{
    // Manager logic here
}

// ✅ Find existing managers
manager = FindObjectOfType<ManagerType>();

// ✅ Use ThreadSafeEventQueue for async operations
ThreadSafeEventQueue.Enqueue(() => 
{
    // Unity API calls here
});

// ✅ Use Script Execution Order attributes
[DefaultExecutionOrder(-100)]
public class InfrastructureManager : MonoBehaviour { }

// ✅ Use DontDestroyOnLoad for global managers
void Awake()
{
    DontDestroyOnLoad(gameObject);
}
```

#### ❌ Don'ts (لا تفعل):

```csharp
// ❌ Don't create GameObjects at runtime
var go = new GameObject("Manager");
var manager = go.AddComponent<Manager>(); // NULL reference!

// ❌ Don't mix async/await with Coroutines
IEnumerator SomeCoroutine()
{
    var result = await SomeAsyncOperation(); // Race condition!
    yield return null;
}

// ❌ Don't call Unity APIs from WebSocket callbacks directly
void WebSocketCallback(string message)
{
    // This might crash!
    transform.position = new Vector3(0, 0, 0);
}

// ❌ Don't use FindObjectOfType in constructors
public class BadManager 
{
    public BadManager()
    {
        // Unity APIs not available here!
        var manager = FindObjectOfType<SomeManager>();
    }
}

// ❌ Don't use Singleton pattern for scene-specific managers
public class SceneSpecificManager : MonoBehaviour
{
    // Scene-specific managers should not be singletons
    // They should be found via FindObjectOfType
}
```

---

## 🔧 Implementation Examples:

### 1️⃣ Adding a New Global Manager:

```csharp
// ✅ Step 1: Create the manager script
[DefaultExecutionOrder(-50)] // Order after infrastructure
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

// ✅ Step 2: Add to BootstrapRunner
public class BootstrapRunner : MonoBehaviour
{
    private void Awake()
    {
        // ... existing managers ...
        
        EnsureSingletonComponent<NewGlobalManager>(); // Add this line
    }
}

// ✅ Step 3: Add prefab to scene (LoginScene)
// Drag NewGlobalManager prefab into LoginScene hierarchy
```

### 2️⃣ Adding a New Scene-Specific Manager:

```csharp
// ✅ Step 1: Create the manager script (NO Singleton pattern)
public class GameSceneManager : MonoBehaviour
{
    // Scene-specific manager - no Instance property
    public void InitializeGameScene()
    {
        Debug.Log("Initializing Game Scene");
    }
}

// ✅ Step 2: Add prefab to GameScene
// Drag GameSceneManager prefab into GameScene hierarchy

// ✅ Step 3: Find in GameManager
public class GameManager : MonoBehaviour
{
    [Header("Scene-Specific Managers")]
    public GameSceneManager gameSceneManager;
    
    private void Start()
    {
        FindManagers();
        ValidateManagers();
    }
    
    private void FindManagers()
    {
        // ... existing manager finds ...
        
        if (gameSceneManager == null)
        {
            gameSceneManager = FindObjectOfType<GameSceneManager>();
        }
    }
}
```

### 3️⃣ Handling WebSocket Messages Safely:

```csharp
// ✅ Correct WebSocket message handling
public class NetworkManager : MonoBehaviour
{
    public event System.Action<string> OnMessageReceived;
    
    private void OnWebSocketDataReceived(byte[] data)
    {
        var message = System.Text.Encoding.UTF8.GetString(data);
        
        // ✅ Queue for main thread execution
        ThreadSafeEventQueue.Enqueue(() =>
        {
            try
            {
                // Safe Unity API calls here
                Debug.Log($"Processing message: {message}");
                OnMessageReceived?.Invoke(message);
                
                // Safe UI updates
                UpdateUI(message);
                
                // Safe game state changes
                UpdateGameState(message);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error processing message: {ex.Message}");
            }
        });
    }
}
```

---

## 🎯 Migration Checklist:

### For Developers:

- [ ] **Understand the problem**: Unity Lifecycle violations cause MonoManager NULL
- [ ] **Use Scene Prefabs**: Add managers to scenes via prefabs, not runtime creation
- [ ] **Use FindObjectOfType**: Find existing managers, don't create new ones
- [ ] **Use ThreadSafeEventQueue**: For all async operations and WebSocket callbacks
- [ ] **Respect Script Execution Order**: Use DefaultExecutionOrder attributes
- [ ] **Add proper logging**: Clear warnings instead of silent failures
- [ ] **Test thoroughly**: Test on different platforms and Unity versions

### For Code Reviews:

- [ ] **Check for `new GameObject()` calls**: Should be replaced with scene prefabs
- [ ] **Check for `AddComponent<T>()` calls**: Should be avoided or well-justified
- [ ] **Check WebSocket handling**: Should use ThreadSafeEventQueue
- [ ] **Check async/await usage**: Should not mix with Coroutines
- [ ] **Check Unity API calls**: Should be from Main Thread only
- [ ] **Check Script Execution Order**: Should be properly set for dependencies

---

## 📈 Benefits Achieved:

### Stability Improvements:
- ✅ **No more MonoManager NULL errors**: Eliminated through proper lifecycle management
- ✅ **Consistent behavior**: Works the same across Unity versions and platforms
- ✅ **Thread safety**: All Unity API calls happen on Main Thread
- ✅ **Clear error messages**: Developers know exactly what's missing

### Development Experience:
- ✅ **Predictable initialization**: Fixed order removes timing issues
- ✅ **Easy debugging**: Clear logging shows what's happening
- ✅ **Simple addition of managers**: Just add prefab and reference
- ✅ **Better error handling**: Graceful degradation instead of crashes

### Maintenance Benefits:
- ✅ **Clean separation**: Global vs scene-specific managers clearly separated
- ✅ **Better architecture**: Bootstrap system provides solid foundation
- ✅ **Future-proof**: Rules prevent regression of the original issue
- ✅ **Documentation**: Clear guidelines for future development

---

## 🚀 Future Development Rules:

### 1️⃣ Unity Lifecycle Respect:
```csharp
// ✅ Always respect Unity's lifecycle
void Awake()          // Use for initialization
void Start()          // Use for setup after all Awake() calls
void OnEnable()       // Use for event subscriptions
void OnDisable()     // Use for event unsubscriptions
void OnDestroy()     // Use for cleanup
```

### 2️⃣ Manager Management:
```csharp
// ✅ Global managers: Use Bootstrap + DontDestroyOnLoad
// ✅ Scene managers: Use FindObjectOfType + scene prefabs
// ❌ Never: new GameObject().AddComponent<T>()
// ❌ Never: Runtime GameObject creation
```

### 3️⃣ Async Operations:
```csharp
// ✅ Use ThreadSafeEventQueue for Unity API calls
// ✅ Use async/await for pure computation
// ✅ Use Coroutines for time-based operations
// ❌ Never: Mix async/await with IEnumerator
// ❌ Never: Call Unity APIs from background threads
```

### 4️⃣ Error Prevention:
```csharp
// ✅ Always validate manager references
// ✅ Provide clear error messages
// ✅ Use null-conditional operators (?.)
// ✅ Log initialization steps clearly
// ❌ Never: Silent NULL reference failures
// ❌ Never: Ignore missing manager warnings
```

Following these rules ensures the application remains stable and prevents the recurrence of Unity lifecycle violations that caused the original MonoManager NULL errors.