using System;

/// <summary>
أنواع تغييرات الحالة الممكنة
يستخدم لتصفية والاستماع لأحداث محددة
/// </summary>
public enum GameStateChangeType
{
    /// <summary>
    /// تحديث كامل للحالة
    /// </summary>
    FullStateUpdated,

    /// <summary>
    /// تحديث جزئي لحالة لاعب واحد
    /// </summary>
    PlayerStateUpdated,

    /// <summary>
    /// تغيير في موضع اللاعب
    /// </summary>
    PositionChanged,

    /// <summary>
    /// تغيير في صحة اللاعب
    /// </summary>
    HealthChanged,

    /// <summary>
    /// تغيير في حالة الدرع
    /// </summary>
    ShieldStatusChanged,

    /// <summary>
    /// تغيير في جاهزية إطلاق النار
    /// </summary>
    FireReadyChanged,

    /// <summary>
    /// تغيير في جاهزية القدرة الخاصة
    /// </summary>
    AbilityReadyChanged,

    /// <summary>
    /// انتهاء اللعبة
    /// </summary>
    GameEnded
}

/// <summary>
/// بيانات حدث تغيير الحالة
/// تُرسل لجميع المستمعين المسجلين عند حدوث تغيير
/// </summary>
public class GameStateChangeEvent
{
    /// <summary>
    /// نوع التغيير الذي حدث
    /// </summary>
    public GameStateChangeType type;

    /// <summary>
    /// معرف اللاعب المتأثر (0 = كل اللاعبين)
    /// </summary>
    public int affectedPlayerId;

    /// <summary>
    /// القيمة القديمة قبل التغيير
    /// </summary>
    public object oldValue;

    /// <summary>
    /// القيمة الجديدة بعد التغيير
    /// </summary>
    public object newValue;

    /// <summary>
    /// التيك (tick) الحالي للعبة
    /// </summary>
    public int tick;

    public GameStateChangeEvent()
    {
    }

    public GameStateChangeEvent(GameStateChangeType changeType, int playerId, object oldVal, object newVal, int tickNum)
    {
        type = changeType;
        affectedPlayerId = playerId;
        oldValue = oldVal;
        newValue = newVal;
        tick = tickNum;
    }

    public override string ToString()
    {
        return $"Change {type} for player {affectedPlayerId} at tick {tick}";
    }
}