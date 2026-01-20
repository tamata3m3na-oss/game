# دليل التكامل والاستخدام (Integration Guide)

## المقدمة

يهدف هذا الدليل إلى مساعدة المطورين الجدد على فهم بنية المشروع والقدرة على إضافة ميزات جديدة بسرعة وكفاءة. سنقوم بتصفح السيناريوهات الشائعة وتقديم أمثلة عملية خطوة بخطوة. تم تصميم المعمارية بحيث يكون إضافة ميزات جديدة عملية بسيطة ومباشرة لا تتطلب تغييرات في طبقات متعددة.

---

## الجزء الأول: فهم البنية

### ١.١ المبادئ الأساسية

قبل البدء في إضافة أي ميزة جديدة، من الضروري فهم المبادئ الأساسية التي تحكم هذه المعمارية. المبدأ الأول والأكثر أهمية هو "المصدر الوحيد للحقيقة"، حيث أن `GameStateRepository` هو المكان الوحيد الذي يُخزّن فيه حالة اللعبة. أي مكون يريد معرفة حالة أي لاعب أو اللعبة بشكل عام يجب أن يقرأ من هذا المستودع، ولا يُسمح لأي مكون آخر بتخزين نسخة محلية من الحالة.

المبدأ الثاني هو الفصل بين الطبقات. كل طبقة لها مسؤولية محددة بوضوح، ولا يجوز لأي طبقة تجاوز حدودها. الطبقة الشبكية تتولى الاتصال بالخادم فقط، طبقة الحالة تخزن البيانات وتتحقق من صحتها، طبقة المنطق تطبق القواعد وتحول البيانات، وطبقة العرض تعرض البيانات للمستخدم. هذا الفصل يجعل الكود أكثر قابلية للفهم والاختبار والصيانة.

المبدأ الثالث هو عدم قابلية تعديل اللقطات. عندما تقرأ حالة لاعب من المستودع، تحصل على `PlayerStateSnapshot` وهي كائن غير قابل للتعديل. هذا يمنع التعديل العرضي للحالة من طبقة العرض ويضمن أن جميع التعديلات تمر عبر المسار الصحيح من خلال المستودع والخادم.

### ١.٢ تدفق البيانات

لفهم كيفية عمل النظام، يجب فهم تدفق البيانات من الخادم إلى العرض. تبدأ البيانات من الخادم كرسالة WebSocket تصل إلى `NetworkManager` الذي يفك تشفيرها ويحولها إلى كائنات C#. ثم يتم توجيه هذه الرسالة إلى `NetworkEventManager` الذي يستدعي المعالج المناسب. `SnapshotProcessor` يستقبل اللقطات ويقوم بالتحقق من صحتها وتحويلها إلى الصيغة المطلوبة، ثم يستدعي `GameStateRepository.UpdateGameState()` لتخزينها. بعد ذلك، يتم إخطار جميع المستمعين المسجلين في حدث `OnStateChanged`، وأخيراً تقوم طبقة العرض (مثل `ShipController`) بتحديث العناصر المرئية بناءً على الحالة الجديدة.

### ١.٣ الهيكل التنظيمي

تنقسم شجرة المشروع إلى مجلدات واضحة حسب المسؤولية. مجلد `Network` يحتوي على جميع الملفات المتعلقة بالاتصال الشبكي مثل `NetworkManager` و`NetworkEventManager` و`NetworkProtocol`. مجلد `State` يحتوي على ملفات إدارة الحالة مثل `GameStateRepository` و`PlayerStateSnapshot` و`GameStateChangeEvent`. مجلد `Game` يحتوي على ملفات المنطق والعرض مثل `SnapshotProcessor` و`GameTickManager` و`ShipController` و`WeaponController` و`AbilityController`. مجلد `Input` يحتوي على ملفات إدارة المدخلات. مجلد `UI` يحتوي على واجهات المستخدم المختلفة.

---

## الجزء الثاني: السيناريوهات العملية

### ٢.١ السيناريو الأول: إضافة ميزة جديدة (Shield Regen)

#### المتطلبات
كل ٣ ثوانٍ، يرجع ٥ HP من الدرع (إن لم يكن نشطاً)، يتم حسابها على الخادم، والعميل يعرضها فقط.

#### الخطوة ١: تحديث الخادم (Node.js)
في ملف `game-engine.service.ts` أو ما يعادله في الخادم، نضيف منطق تجديد الدرع في حلقة اللعبة:

```typescript
// في gameTick() أو ما يعادله
player.shieldRegen = (Date.now() - player.lastShieldHitTime) / 1000 > 3
  ? Math.min(player.shieldHealth + 5, MAX_SHIELD)
  : player.shieldHealth;
```

أو إذا كان التجديد يُحسب عند كل snapshot:

```typescript
// عند بناء NetworkGameState
player.shieldHealth = Math.min(
  player.shieldHealth + (shouldRegenerate ? REGEN_RATE : 0),
  MAX_SHIELD
);
```

#### الخطوة ٢: تحديث NetworkGameState (Unity)
في `NetworkManager.cs` أو `NetworkEventManager.cs`، نضيف حقل `shieldRegen` إلى `PlayerState`:

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
    
    // الحقل الجديد
    public bool shieldRegenerating;
}
```

#### الخطوة ٣: تحديث PlayerStateSnapshot (Unity)
نضيف خاصية جديدة للقراءة فقط:

```csharp
public class PlayerStateSnapshot
{
    // الحقول الموجودة...
    public readonly bool shieldRegenerating;
    
    // في الكونستركتور:
    public PlayerStateSnapshot(PlayerStateData data)
    {
        // الحقول الموجودة...
        this.shieldRegenerating = data.shieldRegenerating;
    }
}
```

#### الخطوة ٤: عرض البيانات (Unity UI)
في `ShipController.cs` أو `AbilityController.cs`:

```csharp
private void UpdateShieldVisual()
{
    if (shieldVisual == null) return;
    
    var currentState = GameStateRepository.Instance.GetPlayerState(playerId);
    if (currentState == null) return;
    
    float shieldHealthRatio = (float)currentState.shieldHealth / MAX_SHIELD;
    shieldVisual.transform.localScale = Vector3.one * shieldHealthRatio;
    
    // عرض مؤشر التجديد
    if (currentState.shieldRegenerating)
    {
        shieldRegenIndicator.SetActive(true);
        shieldRegenIndicator.transform.localScale = Vector3.one * (1f - shieldHealthRatio);
    }
    else
    {
        shieldRegenIndicator.SetActive(false);
    }
}
```

#### الخطوة ٥: الاختبار
نتبع الخطوات التالية للاختبار: أولاً نشغل الخادم، ثم نشغل العميل، ثم ندخل لعبة، بعد ذلك ننتظر ٣ ثوانٍ، وأخيراً نتحقق من زيادة الدرع في واجهة المستخدم.

**النتيجة:** الميزة الجديدة تعمل بسلاسة دون تعديل طبقات أخرى! كل ما كان علينا فعله هو إضافة حقل واحد في المكان المناسب وعرضه في طبقة العرض.

---

### ٢.٢ السيناريو الثاني: إضافة event جديد من الخادم

#### المتطلبات
إضافة حدث جديد من الخادم، مثلاً `game:abilityUsed` للإشارة إلى استخدام قدرة.

#### الخطوة ١: إضافة نوع الحدث في NetworkProtocol.cs
```csharp
public enum NetworkEventType
{
    QueueStatus,
    MatchFound,
    MatchStart,
    GameSnapshot,
    GameEnd,
    Input,
    // النوع الجديد
    AbilityUsed
}
```

#### الخطوة ٢: إضافة فئة البيانات
```csharp
[Serializable]
public class AbilityUsedData
{
    public int playerId;
    public string abilityType;
    public int targetId;
    public long timestamp;
}
```

#### الخطوة ٣: تحديث NetworkEventManager.cs
```csharp
public event Action<AbilityUsedData> OnAbilityUsedReceived;

public void ProcessNetworkMessage(NetworkEventType eventType, string jsonData)
{
    switch (eventType)
    {
        // الأنواع الموجودة...
        case NetworkEventType.AbilityUsed:
            var abilityUsed = JsonUtility.FromJson<AbilityUsedData>(jsonData);
            if (abilityUsed != null)
            {
                Debug.Log($"[NetworkEventManager] Ability used: {abilityUsed.abilityType} by player {abilityUsed.playerId}");
                OnAbilityUsedReceived?.Invoke(abilityUsed);
            }
            break;
    }
}
```

#### الخطوة ٤: إنشاء معالج في SnapshotProcessor.cs
```csharp
private void HandleAbilityUsed(NetworkEventManager.AbilityUsedData abilityData)
{
    if (abilityData == null) return;
    
    Debug.Log($"[SnapshotProcessor] Player {abilityData.playerId} used {abilityData.abilityType}");
    
    // يمكن إضافة تأثيرات خاصة هنا
    // مثل: تشغيل صوت، إظهار تأثير بصري، إلخ
}

// في Start():
NetworkEventManager.Instance.OnAbilityUsedReceived += HandleAbilityUsed;

// في OnDestroy():
NetworkEventManager.Instance.OnAbilityUsedReceived -= HandleAbilityUsed;
```

---

### ٢.٣ السيناريو الثالث: إضافة نوع جديد من المدخلات

#### المتطلبات
إضافة مدخل جديد، مثلاً `specialMove` لحركة خاصة.

#### الخطوة ١: تحديث GameInputData
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
    // المدخل الجديد
    public bool specialMove;
}
```

#### الخطوة ٢: تحديث InputController.cs
```csharp
private void Update()
{
    // المدخلات الموجودة...
    
    // المدخل الجديد
    if (Input.GetKeyDown(KeyCode.Z)) // مثال: Z للحركة الخاصة
    {
        SendSpecialMoveInput();
    }
}

private void SendSpecialMoveInput()
{
    var input = new GameInputData
    {
        playerId = playerId,
        timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
        specialMove = true
    };
    
    NetworkManager.Instance.SendGameInput(input);
}
```

#### الخطوة ٣: تحديث NetworkManager.cs
```csharp
public async void SendGameInput(GameInputData input)
{
    WebSocketMessage message = new WebSocketMessage
    {
        type = "game:input",
        data = JsonUtility.ToJson(input)
    };
    
    string json = JsonUtility.ToJson(message);
    await SendMessageAsync(json);
}
```

---

### ٢.٤ السيناريو الرابع: تصحيح خطأ في بيانات الحالة

#### المشكلة
تلاحظ أن بيانات الموقع تتخطى حدود الخريطة بشكل غير طبيعي.

#### الحل
نقوم بتعديل التحقق من صحة التحول في `GameStateRepository.cs`:

```csharp
private void ValidateStateTransition(NetworkGameState oldState, NetworkGameState newState)
{
    if (newState == null)
        throw new ArgumentNullException(nameof(newState));
    
    if (oldState != null)
    {
        // التحقق من تسلسل التيكات
        if (newState.tick < oldState.tick)
            Debug.LogWarning($"[GameStateRepository] Received out-of-order snapshot");
        
        // التحقق من حدود الخريطة للموقع
        const float MAX_POSITION = 50f; // مثال: حدود الخريطة
        
        if (Mathf.Abs(newState.player1.x) > MAX_POSITION || 
            Mathf.Abs(newState.player1.y) > MAX_POSITION)
        {
            Debug.LogWarning($"[GameStateRepository] Player 1 out of bounds, clamping");
            newState.player1.x = Mathf.Clamp(newState.player1.x, -MAX_POSITION, MAX_POSITION);
            newState.player1.y = Mathf.Clamp(newState.player1.y, -MAX_POSITION, MAX_POSITION);
        }
        
        if (Mathf.Abs(newState.player2.x) > MAX_POSITION || 
            Mathf.Abs(newState.player2.y) > MAX_POSITION)
        {
            Debug.LogWarning($"[GameStateRepository] Player 2 out of bounds, clamping");
            newState.player2.x = Mathf.Clamp(newState.player2.x, -MAX_POSITION, MAX_POSITION);
            newState.player2.y = Mathf.Clamp(newState.player2.y, -MAX_POSITION, MAX_POSITION);
        }
        
        // التحقق من صحة الصحة
        if (newState.player1.health < 0) newState.player1.health = 0;
        if (newState.player1.health > 100) newState.player1.health = 100;
        
        if (newState.player2.health < 0) newState.player2.health = 0;
        if (newState.player2.health > 100) newState.player2.health = 100;
    }
}
```

---

### ٢.٥ السيناريو الخامس: مراقبة أداء النظام

#### المراقبة الأساسية
```csharp
// مراقبة تأخير الشبكة
float latency = GameTickManager.Instance.GetNetworkDelay();
Debug.Log($"Network Latency: {latency * 1000:F2}ms");

// مراقبة التيك الحالي
int tick = GameTickManager.Instance.GetLastProcessedTick();
Debug.Log($"Current Tick: {tick}");

// اكتشاف lag
if (GameTickManager.Instance.IsLagDetected())
{
    Debug.LogWarning("[Performance] Lag detected! Consider showing indicator to player");
}
```

#### مراقبة متقدمة
```csharp
public class PerformanceMonitor : MonoBehaviour
{
    private float[] latencySamples = new float[60];
    private int sampleIndex = 0;
    
    private void Update()
    {
        float currentLatency = GameTickManager.Instance.GetNetworkDelay();
        latencySamples[sampleIndex] = currentLatency;
        sampleIndex = (sampleIndex + 1) % latencySamples.Length;
    }
    
    private void OnGUI()
    {
        float avgLatency = GetAverageLatency();
        float maxLatency = GetMaxLatency();
        
        GUI.Label(new Rect(10, 10, 200, 20), $"Avg Latency: {avgLatency * 1000:F0}ms");
        GUI.Label(new Rect(10, 30, 200, 20), $"Max Latency: {maxLatency * 1000:F0}ms");
        
        if (avgLatency > 0.2f)
        {
            GUI.Label(new Rect(10, 50, 200, 20), "⚠ High Latency!");
        }
    }
    
    private float GetAverageLatency()
    {
        float sum = 0;
        foreach (var sample in latencySamples)
            sum += sample;
        return sum / latencySamples.Length;
    }
    
    private float GetMaxLatency()
    {
        float max = 0;
        foreach (var sample in latencySamples)
            max = Mathf.Max(max, sample);
        return max;
    }
}
```

---

## الجزء الثالث: الأسئلة الشائعة (FAQ)

### س: كيف أضيف حدث جديد من الخادم؟

ج: اتبع الخطوات التالية. أولاً، أضف نوع الحدث الجديد في `NetworkProtocol.cs` في `enum NetworkEventType`. ثانياً، أنشئ فئة بيانات مناسبة للحدث في `NetworkEventManager.cs`. ثالثاً، أضف handling للحدث في `ProcessNetworkMessage()`. رابعاً، أضف event public ليستطيع الآخرون الاشتراك. خامساً، أنشئ معالج للحدث في المكون المناسب (عادةً `SnapshotProcessor`). وأخيراً، اختبر استقبال الحدث بتسجيل رسائل debug.

### س: كيف أصحح خطأ في بيانات الحالة؟

ج: انتقل إلى `GameStateRepository.cs` وابحث عن طريقة `ValidateStateTransition()`. هذه الطريقة هي المكان المناسب لإضافة فحوصات إضافية. يمكنك إضافة فحوصات للحدود، وتحويل القيم غير الصالحة إلى قيم صالحة، وتسجيل تحذيرات عند اكتشاف مشاكل. مثال: إذا كانت الصحة سالبة، قم بتعيينها إلى صفر. وإذا كان الموقع خارج الخريطة، قم بتقييده للحدود.

### س: كيف أراقب أداء النظام؟

ج: استخدم `GameTickManager` للمراقبة الأساسية. الطريقة `GetNetworkDelay()` ترجع تأخير الشبكة الحالي بالثواني. الطريقة `GetMaxNetworkDelay()` ترجع أقصى تأخير تم رصده. الطريقة `IsLagDetected()` ترجع true إذا كان التأخير أكبر من ٢٠٠ مللي ثانية. يمكنك أيضاً إنشاء مكون مراقبة مخصص يجمع عينات ويرسم رسوماً بيانية.

### س: هل يمكنني تعديل حالة اللاعب مباشرة؟

ج: لا، لا يجوز تعديل الحالة مباشرة من أي طبقة غير طبقة الحالة. إذا حاولت تعديل حالة اللاعب من `ShipController` مثلاً، لن تجد طريقة لذلك لأن `PlayerStateSnapshot` غير قابل للتعديل. يجب إرسال المدخلات إلى الخادم عبر `NetworkManager.SendGameInput()`، والخادم هو من يقرر التغييرات ويحفظها في الحالة.

### س: كيف أتعامل مع lost connection؟

ج: `NetworkManager` يعالج lost connection تلقائياً. عند قطع الاتصال، يتم استدعاء `OnDisconnected` event. يمكنك الاشتراك في هذا الحدث وإعادة المحاولة:

```csharp
NetworkManager.Instance.OnDisconnected.AddListener((message) => {
    Debug.Log("Connection lost: " + message);
    StartCoroutine(ReconnectRoutine());
});

private IEnumerator ReconnectRoutine()
{
    yield return new WaitForSeconds(5f);
    NetworkManager.Instance.Initialize(authToken);
}
```

### س: كيف أضيف visual effect جديد؟

ج: أنشئ فئة جديدة في `Assets/Scripts/Game/` أو `Assets/Scripts/UI/Effects/`، ثم اشترك في الأحداث المناسبة من `NetworkEventManager` أو `GameStateRepository.OnStateChanged`. مثال:

```csharp
public class ShieldEffect : MonoBehaviour
{
    private void OnEnable()
    {
        GameStateRepository.Instance.OnStateChanged += HandleStateChanged;
    }
    
    private void OnDisable()
    {
        if (GameStateRepository.Instance != null)
            GameStateRepository.Instance.OnStateChanged -= HandleStateChanged;
    }
    
    private void HandleStateChanged(GameStateChangeEvent changeEvent)
    {
        if (changeEvent.type == GameStateChangeType.ShieldStatusChanged)
        {
            // تشغيل تأثير الدرع
            PlayEffect();
        }
    }
}
```

---

## الجزء الرابع: استكشاف الأخطاء وإصلاحها (Troubleshooting)

### ٤.١ جدول المشاكل والحلول

| المشكلة | السبب المحتمل | الحل |
|---------|--------------|------|
| البيانات لا تُحدّث | المستودع لم يُصادق على النسخة | تحقق من `ValidateStateTransition()` وتأكد من أن newState.tick >= oldState.tick |
| Null Reference Exception | قراءة من حالة null | استخدم `if (state == null) return;` قبل أي عملية قراءة |
| تأخير في العرض | Serialization بطيء | استخدم Object Pool للـ snapshots، تجنب `JsonUtility.ToJson()` في Update |
| لا يتم استقبال الأحداث | عدم التسجيل في الحدث | تأكد من استدعاء `+=` في `Start()` و`-=` في `OnDestroy()` |
| Race Condition | الوصول المتزامن بدون قفل | استخدم `lock` في `GameStateRepository` لجميع العمليات |
| WebSocket لا يتصل | عنوان خاطئ أو token منتهي | تحقق من `ServerUrl` و`PvpNamespace` وصلاحية `token` |
| loss memory | لا يتم تنظيف اللقطات | تأكد من استدعاء `playerSnapshots.Clear()` في `UpdateGameState()` |
| القيم غير صحيحة | لم يتم التحقق من الصحة | أضف فحوصات في `ValidatePlayerData()` |

### ٤.٢调试 التقنيات

#### استخدام Debug Logs
```csharp
// في NetworkManager
Debug.Log($"[NetworkManager] Received message: {message}");

// في GameStateRepository
Debug.Log($"[GameStateRepository] State updated: {currentGameState}");

// في SnapshotProcessor
Debug.Log($"[SnapshotProcessor] Processing tick {snapshotData.tick}");
```

#### استخدام breakpoints
ضع breakpoints في النقاط التالية لفهم تدفق البيانات:
- `NetworkManager.ProcessMessage()`: للتحقق من استقبال الرسائل
- `NetworkEventManager.ProcessNetworkMessage()`: للتحقق من توجيه الأحداث
- `SnapshotProcessor.HandleGameSnapshot()`: للتحقق من معالجة اللقطات
- `GameStateRepository.UpdateGameState()`: للتحقق من تخزين الحالة

#### مراقبة الأحداث
```csharp
// في أي كلاس
private void OnEnable()
{
    NetworkEventManager.Instance.OnGameSnapshotReceived += DebugSnapshot;
}

private void OnDisable()
{
    NetworkEventManager.Instance.OnGameSnapshotReceived -= DebugSnapshot;
}

private void DebugSnapshot(NetworkGameStateData snapshot)
{
    Debug.Log($"[Debug] Snapshot tick: {snapshot.tick}, players: {snapshot.player1?.id}, {snapshot.player2?.id}");
}
```

### ٤.٣ تحسين الأداء

#### تقليل allocations
```csharp
// بدلاً من:
new Vector3(x, 0, y)

// استخدم:
new Vector3(x, 0f, y) // مع الفاصلة العشرية
```

#### تجميع الذاكرة
```csharp
private void Start()
{
    // سخّن Object Pools
    bulletPool.Get();
    bulletPool.Release();
    // كرر عدة مرات
}
```

#### تأخير العمليات
```csharp
// إذا كانت العملية غير ضرورية، تأجلها
if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
    return;
```

---

## الجزء الخامس: أفضل الممارسات

### ٥.١ كتابة كود نظيف

اتبع هذه المبادئ عند كتابة كود جديد في المشروع. استخدم أسماء واضحة للمتغيرات والأساليب، فبدلاً من `p1` استخدم `player1`، وبدلاً من `data` استخدم `playerStateData`. أضف XML comments لجميع الأساليب العامة لتوليد توثيق تلقائي. قسّم الكلاسات الكبيرة إلى كلاسات أصغر ذات مسؤولية واحدة. تجنب التكرار باستخدام الدوال والأدوات المساعدة.

### ٥.٢ التعامل مع الأحداث

عند التعامل مع الأحداث، تذكر دائماً إلغاء التسجيل في `OnDestroy()` لتجنب memory leaks. استخدم weak references إذا كان الكائن قد يُدمّر قبل unsubscribe. تجنب إجراء عمليات ثقيلة في معالجات الأحداث، استخدم coroutines بدلاً منها.

```csharp
// مثال صحيح
private void OnEnable()
{
    NetworkEventManager.Instance.OnGameSnapshotReceived += HandleSnapshot;
}

private void OnDisable()
{
    NetworkEventManager.Instance.OnGameSnapshotReceived -= HandleSnapshot;
}

private void HandleSnapshot(NetworkGameStateData snapshot)
{
    StartCoroutine(ProcessSnapshotAsync(snapshot));
}
```

### ٥.٣ الاختبار

اختبر كل إضافة جديدة بشكل شامل. اختبر الحالات الطبيعية والحالات الحدية. اختبر الأخطاء المحتملة وتأكد من التعامل معها بشكل صحيح. اختبر الأداء تحت ظروف مختلفة (latency عالي، packet loss، إلخ).

---

## الجزء السادس: مثال تطبيقي كامل

### ٦.١ إضافة نظام Scoreboard

#### المتطلبات
عرض لوحة نتائج توضح النقاط والأحصائيات لكل لاعب.

#### الخطوات

**١. إضافة حقول النقاط:**
```csharp
// في PlayerStateData
public int score;
public int kills;
public int deaths;
public int damageDealt;

// في PlayerStateSnapshot
public readonly int score;
public readonly int kills;
public readonly int deaths;
public readonly int damageDealt;
```

**٢. إنشاء Scoreboard UI:**
```csharp
// في Assets/Scripts/UI/ScoreboardController.cs
public class ScoreboardController : MonoBehaviour
{
    public TextMeshProUGUI localPlayerScore;
    public TextMeshProUGUI opponentScore;
    public TextMeshProUGUI localPlayerKills;
    public TextMeshProUGUI opponentKills;
    
    private int localPlayerId;
    
    private void OnEnable()
    {
        GameStateRepository.Instance.OnStateChanged += UpdateScoreboard;
        localPlayerId = GetLocalPlayerId();
    }
    
    private void OnDisable()
    {
        if (GameStateRepository.Instance != null)
            GameStateRepository.Instance.OnStateChanged -= UpdateScoreboard;
    }
    
    private void UpdateScoreboard(GameStateChangeEvent changeEvent)
    {
        var state = GameStateRepository.Instance.GetCurrentState();
        if (state == null) return;
        
        var localPlayer = state.player1?.id == localPlayerId ? state.player1 : state.player2;
        var opponent = state.player1?.id == localPlayerId ? state.player2 : state.player1;
        
        if (localPlayer != null)
        {
            localPlayerScore.text = localPlayer.score.ToString();
            localPlayerKills.text = $"K: {localPlayer.kills} D: {localPlayer.deaths}";
        }
        
        if (opponent != null)
        {
            opponentScore.text = opponent.score.ToString();
            opponentKills.text = $"K: {opponent.kills} D: {opponent.deaths}";
        }
    }
    
    private int GetLocalPlayerId()
    {
        // الحصول على معرف اللاعب المحلي
        // قد يأتي من Login response أو MatchStart event
        return PlayerPrefs.GetInt("PlayerId", 1);
    }
}
```

---

## الخاتمة

يوفر هذا الدليل أساساً متيناً للبدء في العمل على المشروع. تذكر دائماً المبادئ الأساسية: الفصل بين الطبقات، المصدر الوحيد للحقيقة، وعدم قابلية تعديل اللقطات. باتباع هذه المبادئ، يمكنك إضافة ميزات جديدة بثقة مع الحفاظ على جودة الكود وقابليته للصيانة.

إذا واجهت أي مشكلة غير مغطاة في هذا الدليل، راجع قسم troubleshooting أو اطلب المساعدة من الفريق. تذكر أن التوثيق يعيش ويتطور، فإذا اكتشفت حلاً لمشكلة جديدة، أضفه إلى هذا الدليل ليستفيد منه الآخرون.

---

*تم إنشاء هذا الدليل لمساعدة المطورين على فهم وتكامل مع نظام معمارية اللعبة.*
