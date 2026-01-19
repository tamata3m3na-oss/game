using System;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// GameStateRepository: نقطة تخزين الحالة الوحيدة (Single Source of Truth)
/// 
/// المسؤوليات:
/// - تخزين NetworkGameState الحالي
/// - تقديم لقطات غير قابلة للتعديل من حالة اللاعب
/// - التحقق من صحة التحولات بين الحالات
/// - إخطار المستمعين بالتغييرات
/// 
/// القواعد الصارمة:
/// - لا يمكن تعديل الحالة إلا عبر الطرق العامة المحددة
/// - لا تخزين للحالة في أي كلاس آخر
/// - جميع القراءة تتم من هنا فقط
/// </summary>
public class GameStateRepository : MonoBehaviour
{
    private static GameStateRepository instance;
    private static readonly object lockObject = new object();

    /// <summary>
    /// الوصول إلى النسخة الوحيدة من الـ Repository (Singleton)
    /// </summary>
    public static GameStateRepository Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameStateRepository>();
                if (instance == null)
                {
                    GameObject go = new GameObject("GameStateRepository");
                    instance = go.AddComponent<GameStateRepository>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }

    #region Private State Storage

    /// <summary>
    /// الحالة الكاملة للعبة الحالية
    /// يتم تخزينها داخلياً فقط - لا تُسمح بالوصول المباشر
    /// </summary>
    private NetworkGameState currentGameState;

    /// <summary>
    /// لقطات غير قابلة للتعديل لحالة كل لاعب
    /// يتم بناؤها من currentGameState عند الحاجة
    /// </summary>
    private Dictionary<int, PlayerStateSnapshot> playerSnapshots = new Dictionary<int, PlayerStateSnapshot>();

    /// <summary>
    /// آخر tick تمت معالجته بنجاح
    /// يستخدم للتحقق من التسلسل الزمني للـ snapshots
    /// </summary>
    private int lastProcessedTick = -1;

    /// <summary>
    /// Event للإخطار عن تغييرات الحالة
    /// جميع المستمعين يجب أن يشتركوا في هذا event
    /// </summary>
    public event Action<GameStateChangeEvent> OnStateChanged;

    #endregion

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        currentGameState = new NetworkGameState();
        playerSnapshots.Clear();
        lastProcessedTick = -1;
    }

    #region Public API - الوصول الآمن للحالة

    /// <summary>
    /// الحصول على الحالة الكاملة للعبة الحالية
    /// يتم إرجاع نسخة لمنع التعديل المباشر
    /// </summary>
    /// <returns>NetworkGameState أو null إذا لم تكن اللعبة نشطة</returns>
    public NetworkGameState GetCurrentState()
    {
        lock (lockObject)
        {
            if (currentGameState == null)
            {
                Debug.LogWarning("[GameStateRepository] Attempted to access null game state");
                return null;
            }
            
            // إرجاع نسخة لمنع التعديل من الخارج
            return new NetworkGameState(currentGameState);
        }
    }

    /// <summary>
    /// الحصول على لقطة غير قابلة للتعديل لحالة لاعب محدد
    /// يتم بناء اللقطة عند الطلب من الحالة الكاملة
    /// </summary>
    /// <param name="playerId">معرف اللاعب</param>
    /// <returns>PlayerStateSnapshot أو null</returns>
    public PlayerStateSnapshot GetPlayerState(int playerId)
    {
        lock (lockObject)
        {
            if (currentGameState == null)
            {
                Debug.LogWarning($"[GameStateRepository] Cannot get player {playerId} state - game state is null");
                return null;
            }

            // بناء أو الحصول على لقطة محفوظة
            if (!playerSnapshots.ContainsKey(playerId))
            {
                PlayerStateSnapshot snapshot = BuildPlayerSnapshot(playerId);
                if (snapshot != null)
                {
                    playerSnapshots[playerId] = snapshot;
                }
            }

            return playerSnapshots.ContainsKey(playerId) ? playerSnapshots[playerId] : null;
        }
    }

    /// <summary>
    /// تحديث الحالة الكاملة للعبة
    /// يتم التحقق من صحة التحول وإخطار المستمعين
    /// </summary>
    /// <param name="newState">الحالة الجديدة من السيرفر</param>
    public void UpdateGameState(NetworkGameState newState)
    {
        if (newState == null)
        {
            Debug.LogError("[GameStateRepository] Cannot update to null state");
            return;
        }

        lock (lockObject)
        {
            // التحقق من صحة التحول
            ValidateStateTransition(currentGameState, newState);

            // حفظ الحالة القديمة لـ event
            var oldState = currentGameState != null ? new NetworkGameState(currentGameState) : null;

            // تحديث الحالة
            currentGameState = new NetworkGameState(newState);
            lastProcessedTick = newState.tick;

            // مسح اللقطات القديمة - سيتم إعادة بنائها عند الحاجة
            playerSnapshots.Clear();

            // إخطار المستمعين
            var changeEvent = new GameStateChangeEvent
            {
                type = GameStateChangeType.FullStateUpdated,
                affectedPlayerId = 0, // كل اللاعبين
                oldValue = oldState,
                newValue = newState,
                tick = newState.tick
            };

            NotifyStateChanged(changeEvent);
        }
    }

    /// <summary>
    /// تحديث حالة لاعب محدد
    /// نادراً ما يُستخدم - معظم التحديثات تأتي كـ full snapshots
    /// </summary>
    public void UpdatePlayerState(int playerId, PlayerStateData playerData)
    {
        lock (lockObject)
        {
            if (currentGameState == null)
            {
                Debug.LogError($"[GameStateRepository] Cannot update player {playerId} - game state is null");
                return;
            }

            // تحديث اللاعب المحدد
            if (currentGameState.player1?.id == playerId)
            {
                currentGameState.player1 = new PlayerStateData(playerData);
            }
            else if (currentGameState.player2?.id == playerId)
            {
                currentGameState.player2 = new PlayerStateData(playerData);
            }
            else
            {
                Debug.LogWarning($"[GameStateRepository] Player {playerId} not found in current game state");
                return;
            }

            // مسح اللقطة القديمة - سيتم إعادة بنائها عند الطلب
            if (playerSnapshots.ContainsKey(playerId))
            {
                playerSnapshots.Remove(playerId);
            }

            // إخطار بالتغيير
            var changeEvent = new GameStateChangeEvent
            {
                type = GameStateChangeType.PlayerStateUpdated,
                affectedPlayerId = playerId,
                oldValue = null, // يمكن تحسينه لإرسال القيمة القديمة
                newValue = playerData,
                tick = currentGameState.tick
            };

            NotifyStateChanged(changeEvent);
        }
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// بناء لقطة غير قابلة للتعديل لحالة لاعب
    /// </summary>
    private PlayerStateSnapshot BuildPlayerSnapshot(int playerId)
    {
        if (currentGameState == null) return null;

        PlayerStateData playerData = null;
        if (currentGameState.player1?.id == playerId)
        {
            playerData = currentGameState.player1;
        }
        else if (currentGameState.player2?.id == playerId)
        {
            playerData = currentGameState.player2;
        }

        if (playerData == null) return null;

        return new PlayerStateSnapshot(playerData);
    }

    /// <summary>
    /// التحقق من صحة تحول الحالة
    /// يضمن أن التحديثات تأتي بالتسلسل الصحيح
    /// </summary>
    private void ValidateStateTransition(NetworkGameState oldState, NetworkGameState newState)
    {
        if (newState == null)
        {
            throw new ArgumentNullException(nameof(newState), "New state cannot be null");
        }

        if (oldState != null)
        {
            if (newState.tick < oldState.tick)
            {
                Debug.LogWarning($"[GameStateRepository] Received out-of-order snapshot: old tick {oldState.tick}, new tick {newState.tick}");
            }

            if (newState.matchId != oldState.matchId && oldState.matchId != 0)
            {
                Debug.LogWarning($"[GameStateRepository] Match ID changed from {oldState.matchId} to {newState.matchId}");
            }
        }
    }

    /// <summary>
    /// إخطار جميع المستمعين المسجلين بتغيير الحالة
    /// </summary>
    private void NotifyStateChanged(GameStateChangeEvent changeEvent)
    {
        try
        {
            OnStateChanged?.Invoke(changeEvent);
            Debug.Log($"[GameStateRepository] State changed: {changeEvent.type} at tick {changeEvent.tick}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[GameStateRepository] Error notifying state change: {e.Message}");
        }
    }

    #endregion
}