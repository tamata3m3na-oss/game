# 📚 دليل الاستخدام والتوثيق - Unity Client

## 🎯 نظرة عامة

هذا الدليل يشرح كيفية استخدام النظام الجديد للـ Unity Client مع التركيز على:
- إدارة حالة اللعبة
- التكامل مع الشبكة
- أفضل الممارسات البرمجية

---

## 🚀 البدء السريع

### 1. إعداد المشروع

```bash
# فتح المشروع في Unity
1. Unity Hub → Add project from disk
2. Select: /path/to/unity-client
3. Open with Unity 2022.3 or later
4. Wait for package resolution

# تثبيت الحزم المطلوبة (إذا لم تثبت تلقائياً)
- DOTween من Asset Store (Free)
- TextMeshPro Essential Resources
```

### 2. إعداد السيرفر

```bash
# تشغيل السيرفر المحلي
cd backend
npm run start:dev

# السيرفر سيكون متاحاً على:
# HTTP: http://localhost:3000
# WebSocket: ws://localhost:3000/pvp
```

### 3. اختبار الاتصال

```csharp
// في أي C# script
public class TestConnection : MonoBehaviour
{
    private void Start()
    {
        // الشبكة تتصل تلقائياً عند بدء اللعبة
        // يجب أن ترى رسالة في Console:
        // "Attempting WebSocket connection to: ws://localhost:3000/pvp?token=..."
    }
}
```

---

## 🏗️ استخدام GameStateRepository

### الحصول على حالة اللعبة الحالية

```csharp
// ❌ خطأ - لا تصل مباشرة للحالة
var state = GameStateRepository.Instance.currentGameState;

// ✅ صحيح - استخدم الطرق المخصصة
var state = GameStateRepository.Instance.GetCurrentState();
if (state != null)
{
    Debug.Log($"Current match: {state.matchId}");
    Debug.Log($"Current tick: {state.tick}");
}
```

### الحصول على حالة لاعب محدد

```csharp
// الحصول على لقطة غير قابلة للتعديل
var playerState = GameStateRepository.Instance.GetPlayerState(playerId);

if (playerState != null)
{
    // استخدام القطة للقراءة فقط
    Vector3 position = playerState.GetPosition();
    int health = playerState.health;
    bool shieldActive = playerState.shieldActive;
    
    // ✅ هذا آمن - القطة immutable
    // ❌ هذا خطأ - لا يمكن تعديل القطة
    // playerState.health = 50; // Compiler error
}
```

### الاستماع لتغييرات الحالة

```csharp
public class GameUI : MonoBehaviour
{
    private void OnEnable()
    {
        // التسجيل لتلقي أحداث التغيير
        GameStateRepository.Instance.OnStateChanged += HandleStateChanged;
    }
    
    private void OnDisable()
    {
        // إلغاء التسجيل دائماً!
        GameStateRepository.Instance.OnStateChanged -= HandleStateChanged;
    }
    
    private void HandleStateChanged(GameStateChangeEvent changeEvent)
    {
        switch (changeEvent.type)
        {
            case GameStateChangeType.FullStateUpdated:
                UpdateUIForFullStateChange(changeEvent);
                break;
                
            case GameStateChangeType.PlayerStateUpdated:
                UpdateUIForPlayerChange(changeEvent.affectedPlayerId);
                break;
        }
    }
    
    private void UpdateUIForFullStateChange(GameStateChangeEvent changeEvent)
    {
        var newState = changeEvent.newValue as NetworkGameState;
        if (newState != null)
        {
            // تحديث UI بناءً على الحالة الجديدة
            UpdateHealthBars(newState);
            UpdateMatchStatus(newState.status);
        }
    }
}
```

---

## 🌐 استخدام NetworkEventManager

### استقبال أحداث الشبكة

```csharp
public class GameLogic : MonoBehaviour
{
    private void Start()
    {
        // التسجيل لاستقبال أحداث الشبكة
        NetworkEventManager.Instance.OnGameSnapshotReceived += HandleGameSnapshot;
        NetworkEventManager.Instance.OnGameEndReceived += HandleGameEnd;
        NetworkEventManager.Instance.OnMatchFoundReceived += HandleMatchFound;
    }
    
    private void OnDestroy()
    {
        // إلغاء التسجيل دائماً
        NetworkEventManager.Instance.OnGameSnapshotReceived -= HandleGameSnapshot;
        NetworkEventManager.Instance.OnGameEndReceived -= HandleGameEnd;
        NetworkEventManager.Instance.OnMatchFoundReceived -= HandleMatchFound;
    }
    
    private void HandleGameSnapshot(NetworkEventManager.NetworkGameStateData snapshot)
    {
        // هذه البيانات ستصل تلقائياً إلى GameStateRepository
        // غالباً لن تحتاج لمعالجتها مباشرة
        Debug.Log($"Received snapshot: tick {snapshot.tick}");
    }
    
    private void HandleGameEnd(NetworkEventManager.GameEndData endData)
    {
        Debug.Log($"Game ended! Winner: {endData.winner}");
        
        // يمكن الانتقال لشاشة النتائج هنا
        GameManager.Instance.LoadScene("ResultScene");
    }
    
    private void HandleMatchFound(NetworkEventManager.MatchFoundData matchData)
    {
        Debug.Log($"Match found! Opponent: {matchData.opponent.username}");
        
        // يمكن تحديث UI أو بدء تحميل شاشة اللعبة
    }
}
```

---

## 📡 استخدام SnapshotProcessor

### التحقق من صحة البيانات

```csharp
// SnapshotProcessor يعالج البيانات تلقائياً
// لكن يمكن استخدام وظائف التحقق يدوياً

public class DataValidator : MonoBehaviour
{
    [Header("Validation Settings")]
    public bool enableValidationLogging = true;
    
    private bool ValidatePlayerData(NetworkEventManager.PlayerStateData player)
    {
        // التحقق من صحة البيانات
        if (player.health < 0 || player.health > 100)
        {
            if (enableValidationLogging)
                Debug.LogWarning($"Invalid health: {player.health}");
            return false;
        }
        
        if (player.shieldHealth < 0 || player.shieldHealth > 50)
        {
            if (enableValidationLogging)
                Debug.LogWarning($"Invalid shield health: {player.shieldHealth}");
            return false;
        }
        
        return true;
    }
}
```

---

## ⏰ استخدام GameTickManager

### مراقبة أداء الشبكة

```csharp
public class NetworkMonitor : MonoBehaviour
{
    private void Update()
    {
        // عرض إحصائيات الشبكة في الوقت الفعلي
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ShowNetworkStats();
        }
    }
    
    private void ShowNetworkStats()
    {
        var tickManager = GameTickManager.Instance;
        
        Debug.Log($"=== Network Statistics ===");
        Debug.Log($"Last Tick: {tickManager.GetLastProcessedTick()}");
        Debug.Log($"Average Delay: {tickManager.GetNetworkDelay():F3}s");
        Debug.Log($"Max Delay: {tickManager.GetMaxNetworkDelay():F3}s");
        Debug.Log($"Lag Detected: {tickManager.IsLagDetected()}");
        
        if (tickManager.IsLagDetected())
        {
            Debug.LogWarning("⚠️ Network lag detected! Consider showing lag indicator to player.");
        }
    }
}
```

---

## 🎮 تكامل مع Gameplay

### تحديث شخصيات اللعبة

```csharp
public class PlayerController : MonoBehaviour
{
    private int playerId;
    private PlayerStateSnapshot currentSnapshot;
    
    private void Start()
    {
        // الحصول على معرف اللاعب (من authentication)
        playerId = PlayerData.Instance.PlayerId;
        
        // التسجيل لتحديثات الحالة
        GameStateRepository.Instance.OnStateChanged += HandleStateChange;
    }
    
    private void OnDestroy()
    {
        GameStateRepository.Instance.OnStateChanged -= HandleStateChange;
    }
    
    private void HandleStateChange(GameStateChangeEvent changeEvent)
    {
        if (changeEvent.affectedPlayerId != playerId && changeEvent.affectedPlayerId != 0)
            return;
            
        // الحصول على الحالة الجديدة
        var playerState = GameStateRepository.Instance.GetPlayerState(playerId);
        if (playerState != null)
        {
            currentSnapshot = playerState;
            UpdatePlayerTransform();
        }
    }
    
    private void UpdatePlayerTransform()
    {
        if (currentSnapshot == null) return;
        
        // تحديث موقع اللاعب
        Vector3 targetPosition = currentSnapshot.GetPosition();
        transform.position = Vector3.Lerp(transform.position, targetPosition, 0.1f);
        
        // تحديث دوران اللاعب
        Quaternion targetRotation = currentSnapshot.GetRotation();
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 0.1f);
    }
    
    // باقي منطق اللعبة...
}
```

### إدارة الطاقة والدرع

```csharp
public class HealthManager : MonoBehaviour
{
    [Header("UI References")]
    public Slider healthSlider;
    public Slider shieldSlider;
    public Image shieldActiveIndicator;
    
    private void Update()
    {
        // تحديث UI بناءً على حالة اللاعب
        UpdateHealthDisplay();
    }
    
    private void UpdateHealthDisplay()
    {
        // الحصول على حالة اللاعب الحالي
        var playerState = GameStateRepository.Instance.GetPlayerState(
            PlayerData.Instance.PlayerId
        );
        
        if (playerState != null)
        {
            // تحديث شريط الصحة
            healthSlider.value = playerState.health / 100f;
            
            // تحديث شريط الدرع
            shieldSlider.value = playerState.shieldHealth / 50f;
            
            // مؤشر حالة الدرع
            shieldActiveIndicator.enabled = playerState.shieldActive;
            
            // تغيير ألوان بناءً على الحالة
            if (playerState.health < 30)
            {
                healthSlider.fillRect.GetComponent<Image>().color = Color.red;
            }
            else
            {
                healthSlider.fillRect.GetComponent<Image>().color = Color.green;
            }
        }
    }
}
```

---

## 🛠️ معالجة الأخطاء

### استقبال الأخطاء

```csharp
public class ErrorHandler : MonoBehaviour
{
    private void Start()
    {
        // التسجيل لمعالجة أخطاء الشبكة
        NetworkManager.Instance.OnConnectionError.AddListener(HandleConnectionError);
        NetworkManager.Instance.OnDisconnected.AddListener(HandleDisconnection);
    }
    
    private void HandleConnectionError(string errorMessage)
    {
        Debug.LogError($"Connection error: {errorMessage}");
        
        // إظهار رسالة خطأ للاعب
        ShowErrorDialog("Connection Error", "Failed to connect to server. Please check your internet connection.");
    }
    
    private void HandleDisconnection(string reason)
    {
        Debug.LogWarning($"Disconnected: {reason}");
        
        // محاولة إعادة الاتصال
        StartCoroutine(AttemptReconnection());
    }
    
    private System.Collections.IEnumerator AttemptReconnection()
    {
        Debug.Log("Attempting to reconnect...");
        
        yield return new WaitForSeconds(2f);
        
        // إعادة محاولة الاتصال
        NetworkManager.Instance.Initialize(PlayerData.Instance.AuthToken);
    }
    
    private void ShowErrorDialog(string title, string message)
    {
        // تنفيذ UI dialog
        Debug.LogError($"{title}: {message}");
    }
}
```

---

## 📋 أفضل الممارسات

### 1. إدارة الذاكرة

```csharp
// ✅ صحيح - استخدام immutable snapshots
public void ReadPlayerState(int playerId)
{
    var snapshot = GameStateRepository.Instance.GetPlayerState(playerId);
    // استخدم القطة للقراءة - آمن من التعديل العرضي
    ProcessPlayerData(snapshot);
}

// ❌ خطأ - لا تحفظ references للحالة
public void CachePlayerState(int playerId)
{
    cachedState = GameStateRepository.Instance.GetPlayerState(playerId);
    // هذا قد يصبح outdated - لا تفعل هذا!
}
```

### 2. إلغاء التسجيل

```csharp
// ✅ صحيح - إلغاء التسجيل في OnDestroy
private void OnEnable()
{
    GameStateRepository.Instance.OnStateChanged += HandleStateChange;
}

private void OnDisable()
{
    GameStateRepository.Instance.OnStateChanged -= HandleStateChange;
}

// ❌ خطأ - قد يسبب memory leaks
private void OnEnable()
{
    GameStateRepository.Instance.OnStateChanged += HandleStateChange;
    // نسيت إلغاء التسجيل!
}
```

### 3. التحقق من صحة البيانات

```csharp
// ✅ صحيح - التحقق من البيانات
private void ProcessSnapshot(NetworkGameState state)
{
    if (state == null)
    {
        Debug.LogWarning("Received null state");
        return;
    }
    
    if (state.player1 == null || state.player2 == null)
    {
        Debug.LogWarning("Incomplete state data");
        return;
    }
    
    // معالجة البيانات...
}

// ❌ خطأ - عدم التحقق قد يسبب crashes
private void ProcessSnapshot(NetworkGameState state)
{
    // لا توجد فحوصات - خطير!
    ProcessPlayer(state.player1);
}
```

---

## 🔧 الإعدادات المتقدمة

### تخصيص معرفات اللاعبين

```csharp
public class PlayerIdentifier : MonoBehaviour
{
    public static int GetLocalPlayerId()
    {
        // يمكن تخصيص منطق تحديد اللاعب المحلي
        // مثلاً: من JWT token أو إعدادات اللعبة
        return PlayerData.Instance.PlayerId;
    }
    
    public static int GetOpponentId()
    {
        int localId = GetLocalPlayerId();
        var state = GameStateRepository.Instance.GetCurrentState();
        
        if (state?.player1?.id == localId)
            return state.player2?.id ?? 0;
        else
            return state?.player1?.id ?? 0;
    }
}
```

### مراقبة الأداء

```csharp
public class PerformanceMonitor : MonoBehaviour
{
    private float updateInterval = 1f;
    private float lastUpdateTime;
    
    private void Update()
    {
        if (Time.time - lastUpdateTime >= updateInterval)
        {
            LogPerformanceStats();
            lastUpdateTime = Time.time;
        }
    }
    
    private void LogPerformanceStats()
    {
        var tickManager = GameTickManager.Instance;
        
        Debug.Log($"Performance - FPS: {1f/Time.deltaTime:F1}, " +
                 $"Network Delay: {tickManager.GetNetworkDelay():F3}s, " +
                 $"Memory: {System.GC.GetTotalMemory(false)/1024/1024}MB");
    }
}
```

---

## 🐛 استكشاف الأخطاء

### مشاكل شائعة وحلولها

#### 1. لا يتم استقبال البيانات

```csharp
// تحقق من:
1. اتصال الشبكة
NetworkManager.Instance.IsConnected;

// تحقق من التسجيل في الأحداث
NetworkEventManager.Instance.OnGameSnapshotReceived += HandleSnapshot;

// تحقق من console للأخطاء
// ابحث عن "Attempting WebSocket connection"
```

#### 2. crashes عند استقبال البيانات

```csharp
// تحقق من:
1. Null checks في معالج الأحداث
private void HandleSnapshot(NetworkGameStateData data)
{
    if (data == null) return; // ✅ ضروري
    // معالجة البيانات...
}

2. إلغاء التسجيل في OnDestroy
private void OnDestroy()
{
    NetworkEventManager.Instance.OnGameSnapshotReceived -= HandleSnapshot;
}
```

#### 3. بيانات قديمة في UI

```csharp
// تحقق من:
1. استخدام آخر tick
var tickManager = GameTickManager.Instance;
int lastTick = tickManager.GetLastProcessedTick();

2. تحديث UI عند كل تغيير
GameStateRepository.Instance.OnStateChanged += UpdateUI;
```

---

## 📞 الدعم والمساعدة

### مصادر المعلومات

1. **Console Logs** - ابدأ بالبحث في Unity Console
2. **Network Tab** - استخدم Network tab في Chrome DevTools
3. **Profiler** - استخدم Unity Profiler لمراقبة الأداء

### رموز الأخطاء الشائعة

- `Connection timeout` - تحقق من اتصال الإنترنت
- `JSON parsing failed` - تحقق من تنسيق البيانات من السيرفر
- `Null reference exception` - تحقق من null checks
- `Missing reference` - تحقق من تسجيل الأحداث

---

**آخر تحديث:** اليوم  
**الإصدار:** 1.0  
**المؤلف:** AI Development Agent  
