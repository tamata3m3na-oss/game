# مرجع API الكامل (API Reference)

## المقدمة

يوفر هذا المرجع توثيقاً شاملاً لجميع الواجهات العامة والأساليب والأحداث في المشروع. يُقسم المرجع حسب الطبقات لتسهيل البحث والفهم. كل قسم يتضمن الواجهات الأساسية، الأساليب العامة، الخصائص، والأحداث المتاحة للاستخدام.

---

## الطبقة الشبكية (Network Layer)

### NetworkManager

المسؤولية: إدارة اتصال WebSocket وتنسيق إرسال واستقبال الرسائل.

#### الخصائص العامة

```csharp
public static NetworkManager Instance { get; private set; }
```

 الوصول إلى النسخة الوحيدة من مدير الشبكة. يُستخدم للوصول إلى جميع الأساليب والأحداث الأخرى. لا تقم بإنشاء نسخة جديدة باستخدام `new`، بل استخدم `NetworkManager.Instance` دائماً.

```csharp
public string ServerUrl = "ws://localhost:3000";
```

 عنوان URL الأساسي للخادم. يُستخدم عند بناء عنوان WebSocket الكامل. يمكن تعديل هذا الحقل في Unity Inspector أو في الكود حسب البيئة (تطوير/إنتاج).

```csharp
public string PvpNamespace = "/pvp";
```

 مساحة الأسماءPVP في عنوان WebSocket. تُضاف إلى ServerUrl لبناء العنوان الكامل للاتصال.

```csharp
public bool IsConnected()
```

 يُرجع قيمة منطقية توضح حالة الاتصال. تُرجع `true` إذا كان الاتصال نشطاً ومفتوحاً، و`false` في حالة عدم الاتصال أو إغلاقه.

#### الأحداث العامة

```csharp
public UnityEvent<string> OnConnected = new UnityEvent<string>();
```

 يُستدعى عند نجاح الاتصال بالخادم. المعامل المُمرر هو رسالة توضيحية، مثل "Connected to server". استخدم هذا الحدث لبدء العمليات التي تتطلب اتصالاً نشطاً، مثل تسجيل الدخول أو الانضمام للطابور.

```csharp
public UnityEvent<string> OnDisconnected = new UnityEvent<string>();
```

 يُستدعى عند قطع الاتصال بالخادم. المعامل المُمرر يوضح سبب قطع الاتصال. استخدم هذا الحدث لإعادة المحاولة أو إظهار رسالة للمستخدم.

```csharp
public UnityEvent<string> OnConnectionError = new UnityEvent<string>();
```

 يُستدعى عند حدوث خطأ في الاتصال. المعامل المُمرر يحتوي على رسالة الخطأ. استخدم هذا الحدث لتسجيل الأخطاء وإظهار رسائل المساعدة للمستخدم.

#### الأساليب العامة

```csharp
public async void Initialize(string token)
```

 تهيئة الاتصال بالخادم باستخدام رمز المصادقة. هذه الطريقة يجب أن تُستدعى قبل أي عمليات شبكية أخرى.

**المعاملات:**
- `token` (string): رمز المصادقة получен من خادم المصادقة. يُستخدم للتحقق من هوية اللاعب.

**مثال الاستخدام:**
```csharp
string authToken = await AuthService.Login(username, password);
NetworkManager.Instance.Initialize(authToken);
```

**سلوك:**
- يبني عنوان WebSocket الكامل: `ws://localhost:3000/pvp?token=...`
- يُنشئ ClientWebSocket ويبدأ الاتصال
- عند النجاح، يستدعي `OnConnected`
- عند الفشل، يستدعي `OnConnectionError`

```csharp
public async void JoinQueue()
```

 انضمام اللاعب لطابور المطابقة. يجب استدعاء هذه الطريقة بعد نجاح الاتصال.

**مثال الاستخدام:**
```csharp
NetworkManager.Instance.OnConnected.AddListener(() => {
    NetworkManager.Instance.JoinQueue();
});
```

```csharp
public async void LeaveQueue()
```

 مغادرة طابور المطابقة. يُستدعى إذا غير اللاعب رأيه قبل إيجاد خصم.

```csharp
public async void MarkMatchReady(int matchId)
```

 إعلام الخادم بأن اللاعب مستعد للمباراة. يُستدعى عادةً بعد عرض شاشة التأكيد للمستخدم.

**المعاملات:**
- `matchId` (int): معرف المباراة получен من حدث `OnMatchFound`.

```csharp
public async void SendGameInput(GameInputData input)
```

 إرسال مدخلات اللاعب إلى الخادم. هذه الطريقة الأساسية لإرسال أي إجراء يقوم به اللاعب.

**المعاملات:**
- `input` (GameInputData): كائن يحتوي على جميع بيانات المدخلات.

**مثال الاستخدام:**
```csharp
var input = new GameInputData
{
    playerId = playerId,
    timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
    moveX = horizontalAxis,
    moveY = verticalAxis,
    fire = fireButtonPressed
};
NetworkManager.Instance.SendGameInput(input);
```

```csharp
public async void Disconnect()
```

 إنهاء الاتصال بالخادم بشكل نظيف. يجب استدعاء هذه الطريقة عند مغادرة اللعبة أو الانتقال لشاشة رئيسية.

**سلوك:**
- يُلغي جميع العمليات المعلقة
- يُرسل رسالة إغلاق للسيرفر
- يُغلق WebSocket بشكل صحيح
- يُحدث حالة الاتصال إلى `false`

---

#### أنواع البيانات الداخلية

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
```

 تمثل الحالة الكاملة للعبة الواردة من الخادم.

| الحقل | النوع | الوصف |
|-------|-------|-------|
| matchId | int | معرف المباراة الحالية |
| player1 | PlayerState | حالة اللاعب الأول |
| player2 | PlayerState | حالة اللاعب الثاني |
| tick | int | رقم التيك الحالي |
| timestamp | long | الوقت بالخادم (Unix milliseconds) |
| winner | int | معرف الفائز (0 إذا لم ينتهِ بعد) |
| status | string | حالة اللعبة (playing, ended, إلخ) |

```csharp
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

 تمثل حالة لاعب واحد.

| الحقل | النوع | الوصف |
|-------|-------|-------|
| id | int | معرف اللاعب |
| x | float | الموقع على المحور الأفقي |
| y | float | الموقع على المحور الرأسي |
| rotation | float | الزاوية بالدجات |
| health | int | الصحة الحالية (0-100) |
| shieldHealth | int | صحة الدرع (0-50) |
| shieldActive | bool | هل الدرع نشط |
| shieldEndTick | int | التيك الذي ينتهي فيه الدرع |
| fireReady | bool | هل الإطلاق جاهز |
| fireReadyTick | int | التيك الذي يصبح فيه الإطلاق جاهزاً |
| abilityReady | bool | هل القدرة الخاصة جاهزة |
| lastAbilityTime | long | آخر وقت استخدام القدرة |
| damageDealt | int | إجمالي الضرر المُسَبَّب |

```csharp
[Serializable]
public class GameInputData
{
    public int playerId;
    public long timestamp;
    public float moveX;
    public float moveY;
    public bool fire;
    public bool ability;
}
```

 تمثل مدخلات اللاعب المرسلة للخادم.

---

### NetworkEventManager

المسؤولية: توجيه الأحداث الشبكية للمستمعين.

#### الخصائص

```csharp
public static NetworkEventManager Instance { get; }
```

 الوصول الوحيد لمدير الأحداث. يستخدم نمط Singleton.

#### الأحداث

```csharp
public event Action<QueueStatusData> OnQueueStatusReceived;
```

 يُستدعى عند استلام تحديث عن موقع اللاعب في الطابور.

```csharp
public event Action<MatchFoundData> OnMatchFoundReceived;
```

 يُستدعى عند إيجاد خصم مناسب.

```csharp
public event Action<MatchStartData> OnMatchStartReceived;
```

 يُستدعى عند بدء المباراة.

```csharp
public event Action<NetworkGameStateData> OnGameSnapshotReceived;
```

 يُستدعى عند استلام لقطة حالة اللعبة.

```csharp
public event Action<GameEndData> OnGameEndReceived;
```

 يُستدعى عند انتهاء اللعبة.

#### الأساليب

```csharp
public void ProcessNetworkMessage(NetworkEventType eventType, string jsonData)
```

 معالجة رسالة واردة وتوجيهها للمستمعين.

**المعاملات:**
- `eventType` (NetworkEventType): نوع الحدث
- `jsonData` (string): البيانات الخام بصيغة JSON

```csharp
public void RegisterEventListener(NetworkEventType eventType, Delegate listener)
```

 تسجيل مستمع لحدث معين. تُستخدم للتوثيق فقط في الإصدار الحالي.

```csharp
public void UnregisterEventListener(NetworkEventType eventType, Delegate listener)
```

 إلغاء تسجيل مستمع. يجب استدعاؤها لتجنب memory leaks.

---

#### أنواع البيانات

```csharp
[Serializable]
public class QueueStatusData
{
    public int position;
    public int estimatedWait;
}
```

| الحقل | النوع | الوصف |
|-------|-------|-------|
| position | int | الموقع في الطابور |
| estimatedWait | int | الوقت المتوقع بالثواني |

```csharp
[Serializable]
public class MatchFoundData
{
    public int matchId;
    public OpponentData opponent;
}
```

| الحقل | النوع | الوصف |
|-------|-------|-------|
| matchId | int | معرف المباراة |
| opponent | OpponentData | معلومات الخصم |

```csharp
[Serializable]
public class MatchStartData
{
    public int matchId;
    public OpponentData opponent;
    public string color;
}
```

| الحقل | النوع | الوصف |
|-------|-------|-------|
| matchId | int | معرف المباراة |
| opponent | OpponentData | معلومات الخصم |
| color | string | لون السفينة |

```csharp
[Serializable]
public class NetworkGameStateData
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

```csharp
[Serializable]
public class PlayerStateData
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

```csharp
[Serializable]
public class GameEndData
{
    public int matchId;
    public int winner;
    public NetworkGameStateData finalState;
}
```

| الحقل | النوع | الوصف |
|-------|-------|-------|
| matchId | int | معرف المباراة |
| winner | int | معرف الفائز |
| finalState | NetworkGameStateData | الحالة النهائية |

---

### NetworkProtocol

المسؤولية: تعريف البروتوكولات والأنواع الأساسية.

```csharp
public enum NetworkEventType
{
    QueueStatus,
    MatchFound,
    MatchStart,
    GameSnapshot,
    GameEnd,
    Input
}
```

---

## طبقة الحالة (State Layer)

### GameStateRepository

المسؤولية: تخزين واسترجاع حالة اللعبة، Single Source of Truth.

#### الخصائص

```csharp
public static GameStateRepository Instance { get; }
```

 الوصول الوحيد للمستودع. يستخدم نمط Singleton.

```csharp
public NetworkGameState CurrentState { get; }
```

 الحصول على الحالة الحالية. هذه الخاصية للقراءة فقط.

```csharp
public event Action<GameStateChangeEvent> OnStateChanged;
```

 يُستدعى عند حدوث أي تغيير في الحالة. يُعد الحدث الرئيسي للتفاعل مع التغييرات.

#### الأساليب العامة

```csharp
public NetworkGameState GetCurrentState()
```

 الحصول على الحالة الكاملة للعبة الحالية.

**القيمة المُرجَعة:**
- `NetworkGameState`: نسخة من الحالة الحالية، أو `null` إذا لم تكن اللعبة نشطة.

**ملاحظة:** تُرجع نسخة جديدة لمنع التعديل المباشر. أي تغييرات لن تؤثر على الحالة الحقيقية.

```csharp
public PlayerStateSnapshot GetPlayerState(int playerId)
```

 الحصول على لقطة غير قابلة للتعديل لحالة لاعب محدد.

**المعاملات:**
- `playerId` (int): معرف اللاعب المراد الحصول على حالته.

**القيمة المُرجَعة:**
- `PlayerStateSnapshot`: لقطة حالة اللاعب، أو `null` إذا لم يُ найден.

**مثال الاستخدام:**
```csharp
var myState = GameStateRepository.Instance.GetPlayerState(myPlayerId);
if (myState != null)
{
    int health = myState.health;
    Vector3 position = myState.GetPosition();
}
```

```csharp
public void UpdateGameState(NetworkGameState newState)
```

 تحديث الحالة الكاملة للعبة. هذه الطريقة للاستخدام الداخلي ومعالجة اللقطات الواردة.

**المعاملات:**
- `newState` (NetworkGameState): الحالة الجديدة من الخادم.

**سلوك:**
- يتحقق من صحة التحول
- يخزن الحالة الجديدة
- يبني PlayerStateSnapshots جديدة
- يستدعي OnStateChanged

```csharp
public void UpdatePlayerState(int playerId, PlayerStateData playerData)
```

 تحديث حالة لاعب محدد. نادراً ما يُستخدم لأن معظم التحديثات تأتي كـ full snapshots.

---

### PlayerStateSnapshot

المسؤولية: تمثيل حالة اللاعب بشكل غير قابل للتعديل.

#### الخصائص (readonly)

```csharp
public readonly int id;
```

 معرف اللاعب.

```csharp
public readonly float x;
```

 الموقع على المحور الأفقي.

```csharp
public readonly float y;
```

 الموقع على المحور الرأسي.

```csharp
public readonly float rotation;
```

 الزاوية بالدجات.

```csharp
public readonly int health;
```

 الصحة الحالية (0-100).

```csharp
public readonly int shieldHealth;
```

 صحة الدرع (0-50).

```csharp
public readonly bool shieldActive;
```

 هل الدرع نشط.

```csharp
public readonly int shieldEndTick;
```

 التيك الذي ينتهي فيه الدرع.

```csharp
public readonly bool fireReady;
```

 هل الإطلاق جاهز.

```csharp
public readonly int fireReadyTick;
```

 التيك الذي يصبح فيه الإطلاق جاهزاً.

```csharp
public readonly bool abilityReady;
```

 هل القدرة الخاصة جاهزة.

```csharp
public readonly long lastAbilityTime;
```

 آخر وقت استخدام القدرة (Unix milliseconds).

```csharp
public readonly int damageDealt;
```

 إجمالي الضرر المُسَبَّب.

#### الأساليب العامة

```csharp
public PlayerStateSnapshot(PlayerStateData data)
```

 إنشاء لقطة من بيانات اللاعب.

**المعاملات:**
- `data` (PlayerStateData): بيانات اللاعب الأصلية.

**يُلقي:** `ArgumentNullException` إذا كانت البيانات null.

```csharp
public PlayerStateSnapshot(PlayerStateSnapshot other)
```

 إنشاء نسخة من لقطة موجودة.

**المعاملات:**
- `other` (PlayerStateSnapshot): اللقطة المراد نسخها.

**يُلقي:** `ArgumentNullException` إذا كانت اللقطة null.

```csharp
public Vector3 GetPosition()
```

 الحصول على الموقع كـ Vector3.

**القيمة المُرجَعة:**
- `Vector3`: الموقع مع y = 0 (للحركة على مستوي 2D).

```csharp
public Quaternion GetRotation()
```

 الحصول على الدوران كـ Quaternion.

**القيمة المُرجَعة:**
- `Quaternion`: الدوران المحول من الزاوية.

```csharp
public bool IsValid()
```

 التحقق من صحة البيانات.

**القيمة المُرجَعة:**
- `bool`: true إذا كانت البيانات صالحة.

---

### GameStateChangeEvent

المسؤولية: تمثيل حدث تغيير الحالة.

#### الخصائص

```csharp
public GameStateChangeType type;
```

 نوع التغيير.

```csharp
public int affectedPlayerId;
```

 معرف اللاعب المتأثر (0 = كل اللاعبين).

```csharp
public object oldValue;
```

 القيمة القديمة قبل التغيير.

```csharp
public object newValue;
```

 القيمة الجديدة بعد التغيير.

```csharp
public int tick;
```

 التيك الحالي للعبة.

```csharp
public override string ToString()
```

 تمثيل نصي للحدث.

---

### GameStateChangeType

```csharp
public enum GameStateChangeType
{
    FullStateUpdated,      // تحديث كامل للحالة
    PlayerStateUpdated,    // تحديث جزئي للاعب
    PositionChanged,       // تغيير في الموقع
    HealthChanged,         // تغيير في الصحة
    ShieldStatusChanged,   // تغيير في حالة الدرع
    FireReadyChanged,      // تغيير في جاهزية الإطلاق
    AbilityReadyChanged,   // تغيير في جاهزية القدرة
    GameEnded              // نهاية اللعبة
}
```

---

## طبقة المنطق (Game Logic Layer)

### SnapshotProcessor

المسؤولية: معالجة لقطات الحالة والتحقق من صحتها.

#### الخصائص

```csharp
public static SnapshotProcessor Instance { get; }
```

 الوصول الوحيد للمعالج.

#### الأساليب العامة

```csharp
public void ProcessSnapshot(NetworkGameState snapshot)
```

 معالجة snapshot جديد. هذه الطريقة للاستخدام الداخلي.

---

### GameTickManager

المسؤولية: إدارة تزامن التيكات وحساب تأخير الشبكة.

#### الخصائص

```csharp
public static GameTickManager Instance { get; }
```

 الوصول الوحيد للمدير.

#### الأساليب العامة

```csharp
public void UpdateServerTick(int tick, long timestamp)
```

 تحديث التيك الحالي من السيرفر.

**المعاملات:**
- `tick` (int): رقم التيك من الخادم.
- `timestamp` (long): الوقت بالخادم (Unix milliseconds).

```csharp
public int GetLastProcessedTick()
```

 الحصول على آخر تيك تمت معالجته.

**القيمة المُرجَعة:**
- `int`: رقم آخر تيك، أو -1 إذا لم تتم معالجة أي تيك.

```csharp
public float GetNetworkDelay()
```

 الحصول على متوسط تأخير الشبكة بالثواني.

**القيمة المُرجَعة:**
- `float`: التأخير بالثواني.

```csharp
public float GetMaxNetworkDelay()
```

 الحصول على أقصى تأخير تم رصده.

**القيمة المُرجَعة:**
- `float`: أقصى تأخير بالثواني.

```csharp
public float GetNetworkLatency()
```

 الحصول على تأخير الشبكة بالمللي ثانية.

**القيمة المُرجَعة:**
- `float`: التأخير بالمللي ثانية (تُرجع `GetNetworkDelay() * 1000`).

```csharp
public float CalculateClockDrift(long serverTimestamp)
```

 حساب الفرق بين التوقيت المحلي والخادم.

**المعاملات:**
- `serverTimestamp` (long): الوقت بالخادم.

**القيمة المُرجَعة:**
- `float`: الفرق بالثواني (موجب يعني أن المحلي متأخر).

```csharp
public bool IsLagDetected()
```

 التحقق مما إذا كان هناك lag.

**القيمة المُرجَعة:**
- `bool`: true إذا كان التأخير أكبر من 200ms.

---

## طبقة العرض (Presentation Layer)

### ShipController

المسؤولية: عرض السفينة وتحديث موقعها.

#### الخصائص

```csharp
public float movementSpeed = 5f;
```

 سرعة الحركة.

```csharp
public float rotationSpeed = 10f;
```

 سرعة الدوران.

```csharp
public float interpolationSpeed = 10f;
```

 سرعة الاستيفاء (interpolation) للموقع والدوران.

```csharp
public Transform thrusterTransform;
```

 تحويل محرك الدفع.

```csharp
public ParticleSystem thrusterParticles;
```

 نظام جزيئات محرك الدفع.

```csharp
public GameObject shieldVisual;
```

 العنصر البصري للدرع.

#### الأساليب العامة

```csharp
public void Initialize(int id, bool isLocal)
```

 تهيئة المتحكم بمعرف اللاعب.

**المعاملات:**
- `id` (int): معرف اللاعب.
- `isLocal` (bool): هل هذا اللاعب هو اللاعب المحلي.

```csharp
public void UpdateFromSnapshot(NetworkManager.PlayerState state)
```

 تحديث الحالة من لقطة.

**المعاملات:**
- `state` (NetworkManager.PlayerState): حالة اللاعب من الخادم.

```csharp
public int GetPlayerId()
```

 الحصول على معرف اللاعب.

**القيمة المُرجَعة:**
- `int`: معرف اللاعب.

```csharp
public bool IsLocalPlayer()
```

 هل هذا اللاعب هو اللاعب المحلي.

**القيمة المُرجَعة:**
- `bool`: true إذا كان اللاعب المحلي.

```csharp
public int GetHealth()
```

 الحصول على الصحة الحالية.

**القيمة المُرجَعة:**
- `int`: الصحة (0-100).

```csharp
public int GetShieldHealth()
```

 الحصول على صحة الدرع.

**القيمة المُرجَعة:**
- `int`: صحة الدرع (0-50).

```csharp
public bool IsShieldActive()
```

 هل الدرع نشط.

**القيمة المُرجَعة:**
- `bool`: true إذا كان الدرع نشطاً.

```csharp
public bool IsFireReady()
```

 هل الإطلاق جاهز.

**القيمة المُرجَعة:**
- `bool`: true إذا كان الإطلاق جاهزاً.

```csharp
public bool IsAbilityReady()
```

 هل القدرة الخاصة جاهزة.

**القيمة المُرجَعة:**
- `bool`: true إذا كانت القدرة جاهزة.

---

### WeaponController

المسؤولية: إدارة نظام الأسلحة.

#### الخصائص

```csharp
public Transform firePoint;
```

 نقطة الإطلاق.

```csharp
public GameObject bulletPrefab;
```

 قالب الطلقة.

```csharp
public float fireCooldown = 0.5f;
```

 وقت الانتظار بين الإطلاقات.

```csharp
public int poolSize = 50;
```

 حجم Object Pool للطلقات.

#### الأساليب العامة

```csharp
public void OnFireInput()
```

 معالجة مدخل الإطلاق.

```csharp
public bool IsFireReady()
```

 هل الإطلاق جاهز.

**القيمة المُرجَعة:**
- `bool`: true إذا كان الإطلاق جاهزاً.

```csharp
public float GetFireCooldownProgress()
```

 الحصول على نسبة تقدم وقت الانتظار.

**القيمة المُرجَعة:**
- `float`: 0 إلى 1 (1 = جاهز).

---

### AbilityController

المسؤولية: إدارة القدرات الخاصة.

#### الخصائص

```csharp
public GameObject shieldVisual;
```

 العنصر البصري للدرع.

```csharp
public ParticleSystem shieldParticles;
```

 جزيئات الدرع.

```csharp
public float abilityCooldown = 5f;
```

 وقت الانتظار بين استخدام القدرات.

#### الأساليب العامة

```csharp
public void OnAbilityInput()
```

 معالجة مدخل القدرة الخاصة.

```csharp
public void UpdateShieldState(bool active, int shieldHealth, int maxShieldHealth)
```

 تحديث حالة الدرع.

```csharp
public bool IsAbilityReady()
```

 هل القدرة جاهزة.

**القيمة المُرجَعة:**
- `bool`: true إذا كانت القدرة جاهزة.

```csharp
public float GetAbilityCooldownProgress()
```

 الحصول على نسبة تقدم وقت الانتظار.

**القيمة المُرجَعة:**
- `float`: 0 إلى 1 (1 = جاهز).

```csharp
public bool IsShieldActive()
```

 هل الدرع نشط.

**القيمة المُرجَعة:**
- `bool`: true إذا كان الدرع نشطاً.

---

## ملخص الواجهات

### INetworkManager

```csharp
public interface INetworkManager
{
    void Initialize(string token);
    void SendGameInput(GameInputData input);
    void JoinQueue();
    void LeaveQueue();
    void MarkMatchReady(int matchId);
    void Disconnect();
    bool IsConnected();
    
    event Action<string> OnConnected;
    event Action<string> OnDisconnected;
    event Action<string> OnConnectionError;
}
```

### IGameStateRepository

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

### ISnapshotProcessor

```csharp
public interface ISnapshotProcessor
{
    void ProcessSnapshot(NetworkGameState snapshot);
    bool ValidateSnapshot(NetworkGameState snapshot);
    void TransformSnapshot(NetworkGameState snapshot);
}
```

### IGameTickManager

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

---

*تم إنشاء هذا المرجع ليكون مرجعاً شاملاً لجميع واجهات المشروع البرمجية.*
