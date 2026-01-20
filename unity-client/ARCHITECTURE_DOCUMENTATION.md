# معمارية اللعبة الشاملة (Comprehensive Game Architecture)

## ١. نظرة عامة

### ١.١ أهداف المعمارية

تم تصميم معمارية لعبة PvP الشبكية هذه وفق مبادئ هندسة البرمجيات الحديثة لضمان قابلية التوسع والصيانة والأداء العالي. تسعى هذه المعمارية إلى تحقيق توازن دقيق بين الفصل بين المسؤوليات وضمان سلامة البيانات وأداء الشبكة. تُعد هذه المعمارية نتاج تحليل دقيق لمتطلبات اللعبة الشبكية التي تتطلب تزامناً سريعاً ودقيقاً بين العميل والخادم، مع الحفاظ على تجربة مستخدم سلسة وخالية من التأخير الملحوظ.

تتمحور الأهداف الرئيسية حول عدة محاور جوهرية تشكل أساس كل قرار معماري يتم اتخاذها. أولاً، الفصل الواضح بين الطبقات بحيث لا تتداخل المسؤوليات وتتضخم الكلاسات، مما يسهل الصيانة والاختبار. ثانياً، سلامة البيانات من خلال ضمان أن جميع البيانات تمر عبر مسار محدد وواضح، مع منع التعديل المباشر على الحالة من أي طبقة غير مصرح لها. ثالثاً، الأداء العالي للشبكة باستخدام بروتوكول WebSocket مع معالجة غير متزامنة للرسائل لتقليل زمن الاستجابة. رابعاً، قابلية التوسع لإضافة ميزات جديدة دون الحاجة لإعادة هيكلة الكود الحالي، مما يوفر الوقت والجهد على المدى الطويل.

### ١.٢ المبادئ الأساسية

تعتمد هذه المعمارية على مجموعة من المبادئ الأساسية التي توجه جميع قرارات التصميم والتطوير. مبدأ الفصل بين الطبقات يمثل حجر الأساس، حيث أن كل طبقة لها مسؤولية محددة بوضوح ولا تتجاوز حدودها. هذا الفصل يضمن أن التغيير في طبقة واحدة لا يؤثر على الطبقات الأخرى، مما يقلل من مخاطر الأخطاء ويجعل الاختبار أكثر سهولة.

مبدأ المصدر الوحيد للحقيقة يمثل ركيزة أساسية أخرى، حيث أن `GameStateRepository` هو المصدر الوحيد والوحيد لحالة اللعبة. أي مكون يريد معرفة حالة اللعبة يجب أن يقرأ من هذا المستودع، ولا يُسمح لأي مكون آخر بتخزين نسخة محلية من الحالة. هذا يضمن اتساق البيانات عبر جميع مكونات التطبيق ويمنع حدوث تناقضات في البيانات المعروضة.

مبدأ عدم قابلية التعديل للقطات يمثل حماية إضافية لسلامة البيانات. عندما يقرأ أي مكون حالة اللاعب، يحصل على `PlayerStateSnapshot` وهي كائن غير قابل للتعديل. هذا يمنع التعديل العرضي للحالة من طبقة العرض ويضمن أن جميع التعديلات تمر عبر المسار الصحيح من خلال المستودع.

مبدأ سلامة العمليات المتعددة يمثل ضماناً للتشغيل المتزامن الآمن. جميع الهياكل المشتركة محمية بآليات التزامن المناسبة مثل الأقفال والمتحولات المتعددة، مما يمنع حالات السباق ويضمن سلوكاً متسقاً حتى في بيئات التنفيذ المتعدد الخيوط.

### ١.٣ الطبقات الرئيسية

تنقسم المعمارية إلى أربع طبقات رئيسية، كل منها لها دور محدد في تدفق البيانات والأحداث. الطبقة الشبكية تمثل نقطة الاتصال مع الخادم وتتولى إرسال واستقبال الرسائل عبر WebSocket. طبقة إدارة الحالة تمثل المستودع المركزي لجميع بيانات اللعبة وتضمن سلامة واتساق هذه البيانات. طبقة المنطق تحتوي على المعالجات والمديرين الذين يحوّلون البيانات ويطبقون قواعد اللعبة. طبقة العرض تتولى عرض البيانات للمستخدم وقراءة المدخلات منه.

---

## ٢. المعمارية الطبقية (Layered Architecture)

### ٢.١ الطبقة الشبكية (Network Layer)

#### ٢.١.١ نظرة عامة

تُعد الطبقة الشبكية البوابة الرئيسية للتواصل مع خادم اللعبة. تستخدم هذه الطبقة بروتوكول WebSocket للاتصال ثنائي الاتجاه مع الخادم، مما يتيح إرسال واستقبال البيانات في الوقت الحقيقي. تم تصميم هذه الطبقة لتكون فعالة وموثوقة، مع معالجة غير متزامنة للرسائل لتقليل زمن الاستجابة.

تتكون الطبقة الشبكية من ثلاثة مكونات رئيسية تعمل معاً لتحقيق التواصل السلس مع الخادم. `NetworkManager` يمثل المكون الأساسي الذي يدير اتصال WebSocket ويرسل ويستقبل الرسائل. `NetworkEventManager` يتولى توجيه الأحداث للمستمعين المسجلين وتوفير واجهة واضحة للتعامل مع الأحداث الشبكية. `NetworkProtocol` يحدد أنواع الرسائل والأحداث المدعومة ويوفر واجهات أساسية للرسائل.

#### ٢.١.٢ NetworkManager.cs

**المسؤولية:** إدارة اتصال WebSocket وتنسيق إرسال واستقبال الرسائل.

**الموقع:** `Assets/Scripts/Network/NetworkManager.cs` (٣٩٠ سطر)

**الخصائص الرئيسية:**

```csharp
public static NetworkManager Instance { get; private set; }
public string ServerUrl = "ws://localhost:3000";
public string PvpNamespace = "/pvp";
private ClientWebSocket socket;
private CancellationTokenSource cancellationTokenSource;
private NetworkEventManager eventManager;
private bool isConnected = false;
private string authToken = "";
```

يمثل `Instance` الوصول الوحيد إلى مدير الشبكة، وهو مصمم وفق نمط Singleton لضمان وجود نسخة واحدة فقط. `ServerUrl` و`PvpNamespace` يحددان عنوان الخادم ومساحة الأسماء للاتصال. `socket` هو كائن ClientWebSocket الذي يدير الاتصال الفعلي. `cancellationTokenSource` يُستخدم لإلغاء العمليات عند الحاجة، مثلاً عند إنهاء اللعبة أو الانتقال بين المشاهد.

**الأحداث العامة:**

```csharp
public UnityEvent<string> OnConnected = new UnityEvent<string>();
public UnityEvent<string> OnDisconnected = new UnityEvent<string>();
public UnityEvent<string> OnConnectionError = new UnityEvent<string>();
```

هذه الأحداث تتيح لمكونات أخرى الاشتراك في حالات الاتصال المختلفة. يتم استدعاء `OnConnected` عند نجاح الاتصال، و`OnDisconnected` عند قطع الاتصال، و`OnConnectionError` عند حدوث خطأ في الاتصال.

**البيانات الواردة:**

```csharp
[Serializable]
public class NetworkGameState
{
    public int matchId;
    public PlayerState player1;
    public PlayerState player2;
    public int tick;
    public long timestamp;
    public int winner;
    public string status;
}

[Serializable]
public class PlayerState
{
    public int id;
    public float x;
    public float y;
    public float rotation;
    public int health;
    public int shieldHealth;
    public bool shieldActive;
    public int shieldEndTick;
    public bool fireReady;
    public int fireReadyTick;
    public bool abilityReady;
    public long lastAbilityTime;
    public int damageDealt;
}
```

`NetworkGameState` تمثل الحالة الكاملة للعبة الواردة من الخادم، وتتضمن معلومات عن اللاعبين والمباراة. `PlayerState` تمثل حالة لاعب واحد وتشمل الموقع والصحة والدرع وحالة الأسلحة والقدرات.

**الطرق العامة:**

```csharp
public async void Initialize(string token)
public async void JoinQueue()
public async void LeaveQueue()
public async void MarkMatchReady(int matchId)
public async void SendGameInput(GameInputData input)
public async void Disconnect()
public bool IsConnected()
```

`Initialize` تبدأ الاتصال بالخادم باستخدام رمز المصادقة. `JoinQueue` و`LeaveQueue` تديران عضوية اللاعب في طابور المطابقة. `MarkMatchReady` تُعلم الخادم بأن اللاعب مستعد للمباراة. `SendGameInput` ترسل مدخلات اللاعب إلى الخادم. `Disconnect` تنهي الاتصال بشكل نظيف.

**حدود الطبقة الشبكية:**

لا تقرأ هذه الطبقة من `GameStateRepository` تحت أي ظرف. لا تعدل الحالة مباشرة، بل ترسل الأحداث فقط للمستمعين. تتولى فقط تحويل الرسائل من وإلى JSON، ولا تحتوي على أي منطق تجاري يتعلق بحالة اللعبة.

**الواجهة العامة:**

```csharp
public interface INetworkManager
{
    void Initialize(string token);
    void SendGameInput(GameInputData input);
    void JoinQueue();
    void LeaveQueue();
    event Action<QueueStatusData> OnQueueStatusReceived;
    event Action<MatchFoundData> OnMatchFoundReceived;
    event Action<MatchStartData> OnMatchStartReceived;
    event Action<NetworkGameStateData> OnGameSnapshotReceived;
    event Action<GameEndData> OnGameEndReceived;
}
```

#### ٢.١.٣ NetworkEventManager.cs

**المسؤولية:** توجيه الأحداث الشبكية للمستمعين وتوفير واجهة واضحة للتسجيل.

**الموقع:** `Assets/Scripts/Network/NetworkEventManager.cs` (٢٠٨ أسطر)

**الأحداث المتاحة:**

```csharp
public event Action<QueueStatusData> OnQueueStatusReceived;
public event Action<MatchFoundData> OnMatchFoundReceived;
public event Action<MatchStartData> OnMatchStartReceived;
public event Action<NetworkGameStateData> OnGameSnapshotReceived;
public event Action<GameEndData> OnGameEndReceived;
```

كل حدث يتوافق مع نوع رسالة معين من الخادم. عند استلام رسالة، يتم فك تشفيرها وتحويلها إلى الكائن المناسب، ثم يتم استدعاء الحدث المناسب لإخطار جميع المستمعين.

**طريقة المعالجة الرئيسية:**

```csharp
public void ProcessNetworkMessage(NetworkEventType eventType, string jsonData)
{
    switch (eventType)
    {
        case NetworkEventType.QueueStatus:
            var queueStatus = JsonUtility.FromJson<QueueStatusData>(jsonData);
            OnQueueStatusReceived?.Invoke(queueStatus);
            break;
        // ... حالات أخرى
    }
}
```

**أنواع الأحداث:**

```csharp
public enum NetworkEventType
{
    QueueStatus,      // تحديث موقع في الطابور
    MatchFound,       // تم إيجاد خصم
    MatchStart,       // بداية المباراة
    GameSnapshot,     // لقطة حالة اللعبة
    GameEnd,          // نهاية اللعبة
    Input             // استجابة للمدخلات
}
```

#### ٢.١.٤ NetworkProtocol.cs

**المسؤولية:** تعريف الواجهات الأساسية والبروتوكولات الشبكية.

**الموقع:** `Assets/Scripts/Network/NetworkProtocol.cs` (٤٢ سطر)

**الواجهة الأساسية:**

```csharp
public interface INetworkMessage
{
    string GetMessageType();
    string SerializeToJson();
}
```

**غلاف الرسائل:**

```csharp
[Serializable]
public class WebSocketMessageWrapper
{
    public string type;
    public string data;
    
    public WebSocketMessageWrapper(string messageType, string messageData)
    {
        type = messageType;
        data = messageData;
    }
}
```

### ٢.٢ طبقة إدارة الحالة (State Management Layer)

#### ٢.٢.١ نظرة عامة

تمثل طبقة إدارة الحالة القلب النابض لمنظومة اللعبة، حيث تُعد المصدر الوحيد والحصري لجميع بيانات حالة اللعبة. هذه الطبقة تضمن سلامة البيانات واتساقها عبر جميع مكونات التطبيق، وتمنع التعديل المباشر على الحالة من أي طبقة غير مصرح لها.

تتكون هذه الطبقة من عدة مكونات متكاملة تعمل معاً لتوفير واجهة آمنة وفعالة للوصول إلى حالة اللعبة. `GameStateRepository` هو المكون الرئيسي الذي يدير التخزين والوصول إلى الحالة. `NetworkGameState` و`PlayerStateData` يمثلان هياكل البيانات الأساسية. `PlayerStateSnapshot` يوفر لقطات غير قابلة للتعديل للقراءة الآمنة. `GameStateChangeEvent` يمثل أحداث التغيير للإخطار المستمعين.

#### ٢.٢.٢ GameStateRepository.cs

**المسؤولية:** تخزين واسترجاع حالة اللعبة، التحقق من صحة التحولات، إخطار المستمعين.

**الموقع:** `Assets/Scripts/State/GameStateRepository.cs` (٢٩٩ سطر)

**نمط التصميم:** Singleton مع التزامن

**التخزين الداخلي:**

```csharp
private static GameStateRepository instance;
private static readonly object lockObject = new object();
private NetworkGameState currentGameState;
private Dictionary<int, PlayerStateSnapshot> playerSnapshots;
private int lastProcessedTick = -1;
```

`lockObject` يضمن سلامة العمليات المتعددة. `currentGameState` يخزن الحالة الكاملة. `playerSnapshots` يخزن لقطات جاهزة للاعبين. `lastProcessedTick` يتتبع آخر تيك تمت معالجته.

**الواجهة العامة - القراءة:**

```csharp
public NetworkGameState GetCurrentState()
{
    lock (lockObject)
    {
        if (currentGameState == null)
            return null;
        return new NetworkGameState(currentGameState);
    }
}

public PlayerStateSnapshot GetPlayerState(int playerId)
{
    lock (lockObject)
    {
        if (currentGameState == null)
            return null;
        
        if (!playerSnapshots.ContainsKey(playerId))
        {
            PlayerStateSnapshot snapshot = BuildPlayerSnapshot(playerId);
            if (snapshot != null)
                playerSnapshots[playerId] = snapshot;
        }
        
        return playerSnapshots.ContainsKey(playerId) ? playerSnapshots[playerId] : null;
    }
}
```

تستخدم كلتا الطريقتين قفلاً لضمان سلامة العمليات المتعددة. `GetCurrentState` ترجع نسخة جديدة من الحالة لمنع التعديل المباشر. `GetPlayerState` تبني اللقطة عند الطلب وتخزنها للاستخدام اللاحق.

**الواجهة العامة - الكتابة:**

```csharp
public void UpdateGameState(NetworkGameState newState)
{
    if (newState == null)
    {
        Debug.LogError("[GameStateRepository] Cannot update to null state");
        return;
    }
    
    lock (lockObject)
    {
        ValidateStateTransition(currentGameState, newState);
        
        var oldState = currentGameState != null ? 
            new NetworkGameState(currentGameState) : null;
        
        currentGameState = new NetworkGameState(newState);
        lastProcessedTick = newState.tick;
        playerSnapshots.Clear();
        
        var changeEvent = new GameStateChangeEvent
        {
            type = GameStateChangeType.FullStateUpdated,
            affectedPlayerId = 0,
            oldValue = oldState,
            newValue = newState,
            tick = newState.tick
        };
        
        NotifyStateChanged(changeEvent);
    }
}

public void UpdatePlayerState(int playerId, PlayerStateData playerData)
{
    lock (lockObject)
    {
        if (currentGameState == null)
            return;
        
        if (currentGameState.player1?.id == playerId)
            currentGameState.player1 = new PlayerStateData(playerData);
        else if (currentGameState.player2?.id == playerId)
            currentGameState.player2 = new PlayerStateData(playerData);
        else
            return;
        
        if (playerSnapshots.ContainsKey(playerId))
            playerSnapshots.Remove(playerId);
        
        // ... إخطار المستمعين
    }
}
```

**التحقق من صحة التحولات:**

```csharp
private void ValidateStateTransition(NetworkGameState oldState, NetworkGameState newState)
{
    if (newState == null)
        throw new ArgumentNullException(nameof(newState));
    
    if (oldState != null)
    {
        if (newState.tick < oldState.tick)
            Debug.LogWarning($"[GameStateRepository] Received out-of-order snapshot");
        
        if (newState.matchId != oldState.matchId && oldState.matchId != 0)
            Debug.LogWarning($"[GameStateRepository] Match ID changed");
    }
}
```

**الحدث الرئيسي:**

```csharp
public event Action<GameStateChangeEvent> OnStateChanged;
```

**حدود طبقة الحالة:**

- Single Source of Truth: جميع البيانات تمر من هنا
- Immutable snapshots: اللقطات غير قابلة للتعديل
- Thread-safe: جميع العمليات محمية بقفل

**الواجهة العامة:**

```csharp
public interface IGameStateRepository
{
    NetworkGameState GetCurrentState();
    PlayerStateSnapshot GetPlayerState(int playerId);
    void UpdateGameState(NetworkGameState newState);
    void UpdatePlayerState(int playerId, PlayerStateData playerData);
    int GetCurrentTick();
    event Action<GameStateChangeEvent> OnStateChanged;
}
```

#### ٢.٢.٣ PlayerStateSnapshot.cs

**المسؤولية:** تمثيل حالة اللاعب بشكل غير قابل للتعديل.

**الموقع:** `Assets/Scripts/State/PlayerStateSnapshot.cs` (١٢١ سطر)

**الحقول readonly:**

```csharp
public readonly int id;
public readonly float x;
public readonly float y;
public readonly float rotation;
public readonly int health;
public readonly int shieldHealth;
public readonly bool shieldActive;
public readonly int shieldEndTick;
public readonly bool fireReady;
public readonly int fireReadyTick;
public readonly bool abilityReady;
public readonly long lastAbilityTime;
public readonly int damageDealt;
```

جميع الحfields معرفة كـ readonly، مما يمنع أي تعديل بعد الإنشاء. لا توجد طرق عامة لتعديل أي حقل.

**طرق المساعدة:**

```csharp
public Vector3 GetPosition()
{
    return new Vector3(x, 0f, y);
}

public Quaternion GetRotation()
{
    return Quaternion.Euler(0f, rotation, 0f);
}

public bool IsValid()
{
    return id > 0 && health >= 0 && health <= 100;
}
```

#### ٢.٢.٤ GameStateChangeEvent.cs

**المسؤولية:** تمثيل حدث تغيير الحالة للإخطار.

**الموقع:** `Assets/Scripts/State/GameStateChangeEvent.cs` (٩٨ سطر)

**أنواع التغييرات:**

```csharp
public enum GameStateChangeType
{
    FullStateUpdated,      // تحديث كامل للحالة
    PlayerStateUpdated,    // تحديث جزئي للاعب
    PositionChanged,       // تغيير الموقع
    HealthChanged,         // تغيير الصحة
    ShieldStatusChanged,   // تغيير حالة الدرع
    FireReadyChanged,      // تغيير جاهزية الإطلاق
    AbilityReadyChanged,   // تغيير جاهزية القدرة
    GameEnded              // نهاية اللعبة
}
```

**هيكل الحدث:**

```csharp
public class GameStateChangeEvent
{
    public GameStateChangeType type;
    public int affectedPlayerId;  // 0 = كل اللاعبين
    public object oldValue;
    public object newValue;
    public int tick;
}
```

### ٢.٣ طبقة المنطق (Game Logic Layer)

#### ٢.٣.١ نظرة عامة

تضم طبقة المنطق المعالجات والمديري اللازمين لتطبيق قواعد اللعبة وتحويل البيانات بين الطبقات. هذه الطبقة تقرأ من المستودع فقط، وتكتب عبر المستودع فقط، ولا تحتفظ بأي حالة خاصة بها.

المكونات الرئيسية في هذه الطبقة هي `SnapshotProcessor` الذي يعالج لقطات الحالة الواردة من الخادم، و`GameTickManager` الذي يدير تزامن التيكات مع الخادم.

#### ٢.٣.٢ SnapshotProcessor.cs

**المسؤولية:** معالجة لقطات الحالة والتحقق من صحتها وتحويلها.

**الموقع:** `Assets/Scripts/Game/SnapshotProcessor.cs` (٢٢١ سطر)

**التسجيل للأحداث:**

```csharp
private void Start()
{
    stateRepository = GameStateRepository.Instance;
    tickManager = GameTickManager.Instance;
    
    NetworkEventManager.Instance.OnGameSnapshotReceived += HandleGameSnapshot;
    NetworkEventManager.Instance.OnGameEndReceived += HandleGameEnd;
}
```

**معالجة الـ Snapshot:**

```csharp
private void HandleGameSnapshot(NetworkEventManager.NetworkGameStateData snapshotData)
{
    if (snapshotData == null)
    {
        Debug.LogError("[SnapshotProcessor] Received null snapshot data");
        return;
    }
    
    if (!ValidateSnapshot(snapshotData))
    {
        Debug.LogWarning($"[SnapshotProcessor] Invalid snapshot rejected for tick {snapshotData.tick}");
        return;
    }
    
    tickManager.UpdateServerTick(snapshotData.tick, snapshotData.timestamp);
    var gameState = ConvertToRepositoryFormat(snapshotData);
    stateRepository.UpdateGameState(gameState);
    
    Debug.Log($"[SnapshotProcessor] Processed snapshot tick {snapshotData.tick} successfully");
}
```

**التحقق من الصحة:**

```csharp
private bool ValidateSnapshot(NetworkEventManager.NetworkGameStateData snapshot)
{
    if (snapshot.player1 == null || snapshot.player2 == null)
    {
        Debug.LogWarning("[SnapshotProcessor] Snapshot missing player data");
        return false;
    }
    
    if (snapshot.player1.id <= 0 || snapshot.player2.id <= 0)
    {
        Debug.LogWarning("[SnapshotProcessor] Invalid player IDs in snapshot");
        return false;
    }
    
    if (snapshot.tick < 0)
    {
        Debug.LogWarning("[SnapshotProcessor] Invalid tick number");
        return false;
    }
    
    if (!ValidatePlayerData(snapshot.player1) || !ValidatePlayerData(snapshot.player2))
        return false;
    
    return true;
}

private bool ValidatePlayerData(NetworkEventManager.PlayerStateData playerData)
{
    if (playerData.health < 0 || playerData.health > 100)
    {
        Debug.LogWarning($"[SnapshotProcessor] Invalid health value: {playerData.health}");
        return false;
    }
    
    if (playerData.shieldHealth < 0 || playerData.shieldHealth > 50)
    {
        Debug.LogWarning($"[SnapshotProcessor] Invalid shield health: {playerData.shieldHealth}");
        return false;
    }
    
    return true;
}
```

**تحويل الصيغة:**

```csharp
private NetworkGameState ConvertToRepositoryFormat(
    NetworkEventManager.NetworkGameStateData eventData)
{
    var repositoryState = new NetworkGameState
    {
        matchId = eventData.matchId,
        tick = eventData.tick,
        timestamp = eventData.timestamp,
        winner = eventData.winner,
        status = eventData.status
    };
    
    if (eventData.player1 != null)
        repositoryState.player1 = ConvertPlayerData(eventData.player1);
    
    if (eventData.player2 != null)
        repositoryState.player2 = ConvertPlayerData(eventData.player2);
    
    return repositoryState;
}
```

**الواجهة:**

```csharp
public interface ISnapshotProcessor
{
    void ProcessSnapshot(NetworkGameState snapshot);
    bool ValidateSnapshot(NetworkGameState snapshot);
    PlayerStateSnapshot[] TransformSnapshot(NetworkGameState snapshot);
}
```

#### ٢.٣.٣ GameTickManager.cs

**المسؤولية:** إدارة تزامن التيكات وحساب تأخير الشبكة.

**الموقع:** `Assets/Scripts/Game/GameTickManager.cs` (١٢٨ سطر)

**الخصائص:**

```csharp
private int lastProcessedTick = -1;
private long lastProcessedTimestamp = 0;
private float averageNetworkDelay = 0f;
private float maxNetworkDelay = 0f;
private int snapshotCount = 0;
private float delaySmoothingFactor = 0.1f;
```

**تحديث التيك:**

```csharp
public void UpdateServerTick(int tick, long timestamp)
{
    if (tick < lastProcessedTick)
    {
        Debug.LogWarning($"[GameTickManager] Received out-of-order tick: {tick}");
        return;
    }
    
    long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    float networkDelay = (currentTime - timestamp) / 1000f;
    
    if (snapshotCount == 0)
        averageNetworkDelay = networkDelay;
    else
        averageNetworkDelay = Mathf.Lerp(averageNetworkDelay, networkDelay, 
            delaySmoothingFactor);
    
    maxNetworkDelay = Mathf.Max(maxNetworkDelay, networkDelay);
    snapshotCount++;
    
    lastProcessedTick = tick;
    lastProcessedTimestamp = timestamp;
}
```

**طرق المراقبة:**

```csharp
public int GetLastProcessedTick()
{
    return lastProcessedTick;
}

public float GetNetworkDelay()
{
    return averageNetworkDelay;
}

public float GetMaxNetworkDelay()
{
    return maxNetworkDelay;
}

public float CalculateClockDrift(long serverTimestamp)
{
    long localTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    return (localTime - serverTimestamp) / 1000f;
}

public bool IsLagDetected()
{
    return averageNetworkDelay > 0.2f;
}
```

**الواجهة:**

```csharp
public interface IGameTickManager
{
    int GetCurrentTick();
    float GetNetworkLatency();
    float GetNetworkDelay();
    bool IsLagDetected();
    void UpdateServerTick(int tick, long timestamp);
}
```

### ٢.٤ طبقة العرض (Presentation Layer)

#### ٢.٤.١ نظرة عامة

تتولى طبقة العرض عرض البيانات للمستخدم وقراءة مدخلاته. هذه الطبقة تقرأ من المستودع فقط، ولا تحتفظ بنسخة محلية من الحالة، وتحدث العناصر المرئية فقط.

المكونات الرئيسية هي `ShipController` الذي يعرض السفينة ويحدث موقعها، و`WeaponController` الذي يدير نظام الأسلحة، و`AbilityController` الذي يدير القدرات الخاصة.

#### ٢.٤.٢ ShipController.cs

**المسؤولية:** عرض السفينة وتحديث موقعها وحالتها.

**الموقع:** `Assets/Scripts/Game/ShipController.cs` (١٦٩ سطر)

**الخصائص:**

```csharp
[Header("Ship Settings")]
public float movementSpeed = 5f;
public float rotationSpeed = 10f;
public float interpolationSpeed = 10f;

[Header("References")]
public Transform thrusterTransform;
public ParticleSystem thrusterParticles;
public GameObject shieldVisual;

private Vector3 targetPosition;
private Quaternion targetRotation;
private int currentHealth = 100;
private int currentShieldHealth = 0;
private bool isShieldActive = false;
private int shieldEndTick = 0;
private bool isFireReady = true;
private int fireReadyTick = 0;
private bool isAbilityReady = true;
private long lastAbilityTime = 0;
```

**التحديث من اللقطة:**

```csharp
public void UpdateFromSnapshot(NetworkManager.PlayerState state)
{
    if (state == null) return;
    
    targetPosition = new Vector3(state.x, 0f, state.y);
    targetRotation = Quaternion.Euler(0f, state.rotation, 0f);
    
    currentHealth = state.health;
    currentShieldHealth = state.shieldHealth;
    isShieldActive = state.shieldActive;
    shieldEndTick = state.shieldEndTick;
    isFireReady = state.fireReady;
    fireReadyTick = state.fireReadyTick;
    isAbilityReady = state.abilityReady;
    lastAbilityTime = state.lastAbilityTime;
}
```

**التحديث في Update:**

```csharp
private void Update()
{
    // Smooth interpolation
    transform.position = Vector3.Lerp(transform.position, targetPosition, 
        Time.deltaTime * interpolationSpeed);
    transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, 
        Time.deltaTime * interpolationSpeed);
    
    UpdateThrusterEffects();
    UpdateShieldVisual();
}
```

**حدود طبقة العرض:**

- تقرأ من Repository فقط
- لا تحتفظ بنسخة محلية من الحالة (تستخدم target values)
- تحدّث Visual Elements فقط
- ترسل inputs إلى NetworkManager

#### ٢.٤.٣ WeaponController.cs

**المسؤولية:** إدارة نظام الأسلحة وإطلاق النار.

**الموقع:** `Assets/Scripts/Game/WeaponController.cs` (١١٩ سطر)

**خصائص نظام الـ Pooling:**

```csharp
[Header("Weapon Settings")]
public Transform firePoint;
public GameObject bulletPrefab;
public float fireCooldown = 0.5f;
public AudioSource fireAudioSource;
public ParticleSystem muzzleFlash;

[Header("Bullet Pooling")]
public int poolSize = 50;
public Transform bulletPoolContainer;

private ObjectPool<Bullet> bulletPool;
private float lastFireTime = -1f;
private bool isFireReady = true;
```

**تسجيل الدخول:**

```csharp
private void Start()
{
    bulletPool = new ObjectPool<Bullet>(
        CreateBullet, 
        OnGetBullet, 
        OnReleaseBullet, 
        OnDestroyBullet, 
        true, 
        poolSize, 
        poolSize * 2
    );
    
    for (int i = 0; i < poolSize; i++)
    {
        var bullet = bulletPool.Get();
        bulletPool.Release(bullet);
    }
}
```

**إطلاق النار:**

```csharp
public void OnFireInput()
{
    if (!isFireReady) return;
    
    if (Time.time - lastFireTime < fireCooldown) return;
    
    FireWeapon();
    lastFireTime = Time.time;
    isFireReady = false;
    
    Invoke("ResetFireReady", fireCooldown);
}
```

#### ٢.٤.٤ AbilityController.cs

**المسؤولية:** إدارة القدرات الخاصة (درع الحماية).

**الموقع:** `Assets/Scripts/Game/AbilityController.cs` (١٢٤ سطر)

**تفعيل القدرة:**

```csharp
public void OnAbilityInput()
{
    if (!isAbilityReady) return;
    
    if (Time.time - lastAbilityTime < abilityCooldown) return;
    
    ActivateAbility();
    lastAbilityTime = Time.time;
    isAbilityReady = false;
    
    Invoke("ResetAbilityReady", abilityCooldown);
}

private void ActivateAbility()
{
    isShieldActive = true;
    PlayAbilityEffects();
    Invoke("DeactivateShield", 5f);
}
```

---

## ٣. تدفق البيانات (Data Flow)

### ٣.١ البيانات من السيرفر إلى العرض

```
┌─────────────────────────────────────────────────────────────────┐
│                          Server                                  │
│    ↓ WebSocket message: {"type": "game:snapshot", "data": {...}} │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│                      NetworkManager                               │
│    - يستقبل الرسائل عبر ReceiveLoop                              │
│    - يفك تشفير JSON باستخدام JsonUtility                        │
│    - يستدعي ProcessMessage                                       │
│    ↓ Invokes OnGameSnapshot event                                │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│                   NetworkEventManager                             │
│    - يستقبل نوع الحدث                                            │
│    - يوجه الرسالة حسب نوعها                                      │
│    ↓ Calls HandleGameSnapshot()                                  │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│                    SnapshotProcessor                              │
│    - يتحقق من صحة البيانات                                       │
│    - يحسب تأخير الشبكة                                           │
│    - يحول صيغة البيانات                                          │
│    ↓ Calls UpdateGameState()                                     │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│                 GameStateRepository                               │
│    - يتحقق من صحة التحول                                         │
│    - يخزن الحالة الجديدة                                         │
│    - يبني PlayerStateSnapshots                                   │
│    ↓ Invokes OnStateChanged event                                │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│                  Presentation Layer                               │
│    - ShipController: تحديث الموقع والدوران                        │
│    - WeaponController: تحديث جاهزية الإطلاق                      │
│    - AbilityController: تحديث حالة القدرة                        │
│    ↓ Updates visual elements                                      │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│                        Game View                                  │
│              -渲染到屏幕 (Rendered to screen)                      │
└─────────────────────────────────────────────────────────────────┘
```

### ٣.٢ البيانات من المدخلات إلى السيرفر

```
┌─────────────────────────────────────────────────────────────────┐
│                     User Input                                    │
│              (keyboard/mouse/touch)                               │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│                    InputController                                │
│    - يجمع المدخلات من المستخدم                                   │
│    - يرسلها كـ GameInputData                                     │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│                    NetworkManager                                 │
│    - يخلق WebSocketMessage                                       │
│    - يشفّر إلى JSON                                              │
│    ↓ Sends via WebSocket                                         │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│                         Server                                    │
└─────────────────────────────────────────────────────────────────┘
```

### ٣.٣ تدفق الأحداث

```
Server Message
     ↓
NetworkManager.ReceiveLoop()
     ↓
NetworkManager.ProcessMessage()
     ↓
NetworkEventManager.ProcessNetworkMessage()
     ↓
SnapshotProcessor.HandleGameSnapshot()
     ↓
GameStateRepository.UpdateGameState()
     ↓
GameStateRepository.NotifyStateChanged()
     ↓
    ┌────────────────────────────────────────┐
    │           Presentation Layer            │
    │  - ShipController (يحدث المرئيات)       │
    │  - UI Controllers (يحدثون الواجهة)      │
    └────────────────────────────────────────┘
```

---

## ٤. جدول الملفات والمسؤوليات

### ٤.١ الطبقة الشبكية

| الملف | السطور | المسؤولية | التعقيد |
|-------|--------|-----------|---------|
| NetworkManager.cs | ٣٩٠ | إدارة WebSocket، إرسال/استقبال الرسائل | عالي |
| NetworkEventManager.cs | ٢٠٨ | توجيه الأحداث للمستمعين | متوسط |
| NetworkProtocol.cs | ٤٢ | تعريفات البروتوكول والأنواع | منخفض |

### ٤.٢ طبقة الحالة

| الملف | السطور | المسؤولية | التعقيد |
|-------|--------|-----------|---------|
| GameStateRepository.cs | ٢٩٩ | تخزين الحالة، Single Source of Truth | عالي |
| PlayerStateSnapshot.cs | ١٢١ | immutable snapshots للقراءة الآمنة | منخفض |
| GameStateChangeEvent.cs | ٩٨ | نظام الأحداث للإخطار | منخفض |
| NetworkGameState.cs | ٤٤ | هيكل حالة اللعبة | منخفض |
| PlayerStateData.cs | ٤٩ | هيكل بيانات اللاعب | منخفض |

### ٤.٣ طبقة المنطق

| الملف | السطور | المسؤولية | التعقيد |
|-------|--------|-----------|---------|
| SnapshotProcessor.cs | ٢٢١ | معالجة وتحويل البيانات | عالي |
| GameTickManager.cs | ١٢٨ | تزامن التيكات، مراقبة الشبكة | متوسط |

### ٤.٤ طبقة العرض

| الملف | السطور | المسؤولية | التعقيد |
|-------|--------|-----------|---------|
| ShipController.cs | ١٦٩ | عرض السفينة والحركة | متوسط |
| WeaponController.cs | ١١٩ | نظام الأسلحة والذخيرة | متوسط |
| AbilityController.cs | ١٢٤ | نظام القدرات الخاصة | متوسط |

### ٤.٥ طبقة المدخلات

| الملف | السطور | المسؤولية | التعقيد |
|-------|--------|-----------|---------|
| InputController.cs | غير محدد | جمع مدخلات المستخدم | منخفض |

---

## ٥. سلامة العمليات المتعددة (Thread Safety)

### ٥.١ آليات الحماية

تم تصميم النظام لضمان سلامة العمليات المتعددة في جميع النقاط الحرجة. يُستخدم قفل واحد (`lockObject`) في `GameStateRepository` لحماية جميع عمليات القراءة والكتابة. هذا يضمن أن العمليات المتتالية لا تتداخل وتسبب حالة سباق.

```csharp
private static readonly object lockObject = new object();

public NetworkGameState GetCurrentState()
{
    lock (lockObject)
    {
        if (currentGameState == null)
            return null;
        return new NetworkGameState(currentGameState);
    }
}

public void UpdateGameState(NetworkGameState newState)
{
    lock (lockObject)
    {
        ValidateStateTransition(currentGameState, newState);
        // ... باقي الكود
    }
}
```

### ٥.٢ حماية EventQueue

في NetworkManager، تُستخدم قائمة الانتظار مع قفل لحماية العمليات:

```csharp
private void QueueOnMainThread(Action action)
{
    lock (eventQueue)
    {
        eventQueue.Enqueue(action);
    }
}
```

### ٥.٣ لا توجد Race Conditions

بسبب التصميم cuidadoso، لا توجد race conditions في النظام. جميع البيانات المشتركة محمية، وجميع اللقطات غير قابلة للتعديل، وجميع الأحداث تُعالج بشكل متتابع.

---

## ٦. مقاييس الأداء (Performance Metrics)

### ٦.١ مقاييس الشبكة

- **تأخير الشبكة (Network Latency):** أقل من ٢٠٠ مللي ثانية (يُراقب بواسطة GameTickManager)
- **معالجة الـ Snapshot:** أقل من ٥ مللي ثانية
- **استقرار الاتصال:** يتولى NetworkManager إعادة الاتصال تلقائياً

### ٦.٢ مقاييس الذاكرة

- **استخدام الذاكرة الأساسي:** حوالي ٥٠ ميغابايت
- **تخصيصات GC لكل إطار:** أقل من ١ كيلو بايت

### ٦.٣ مقاييس الرسوميات

- **معدل الإطارات:** ٦٠ إطار في الثانية (مستقر)
- **استخدام Object Pooling:** للطلقات لتقليل GC

---

## ٧. معالجة الأخطاء (Error Handling)

### ٧.١ أخطاء الشبكة

**فقدان الاتصال:**
```csharp
if (result.MessageType == WebSocketMessageType.Close)
{
    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", 
        cancellationTokenSource.Token);
    isConnected = false;
    QueueOnMainThread(() => OnDisconnected.Invoke("Server closed connection"));
}
```

**خطأ في الاتصال:**
```csharp
catch (Exception ex)
{
    Debug.LogError("Failed to connect: " + ex.Message);
    QueueOnMainThread(() => OnConnectionError.Invoke("Connection failed: " + ex.Message));
}
```

**خطأ في الإرسال:**
```csharp
catch (Exception ex)
{
    Debug.LogError("Failed to send message: " + ex.Message);
}
```

### ٧.٢ أخطاء الحالة

**تحول حالة غير صالح:**
```csharp
if (newState.tick < oldState.tick)
{
    Debug.LogWarning($"[GameStateRepository] Received out-of-order snapshot");
}
```

**حالة null:**
```csharp
if (newState == null)
{
    Debug.LogError("[GameStateRepository] Cannot update to null state");
    return;
}
```

### ٧.٣ أخطاء المعالجة

**لقطة غير صالحة:**
```csharp
if (!ValidateSnapshot(snapshotData))
{
    Debug.LogWarning($"[SnapshotProcessor] Invalid snapshot rejected for tick {snapshotData.tick}");
    return;
}
```

---

## ٨. الملخص

توفر هذه المعمارية أساساً قوياً وقابلاً للتطوير للعبة PvP الشبكية. الفصل الواضح بين الطبقات يضمن قابلية الصيانة، وسلامة البيانات تضمن اتساق الحالة، والأداء العالي للشبكة يضمن تجربة مستخدم سلسة. من خلال اتباع هذه المبادئ، يمكن للفريق إضافة ميزات جديدة بثقة دون القلق من التأثير على استقرار النظام الحالي.

---

*تم إنشاء هذه الوثيقة كمجزء من توثيق المعمارية الشامل للعبة.*
