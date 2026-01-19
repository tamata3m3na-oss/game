using System;

/// <summary>
/// لقطة غير قابلة للتعديل (immutable snapshot) من حالة اللاعب
/// تستخدم لمنع التعديل العرضي للحالة في طبقة العرض
/// 
/// القواعد:
/// - جميع الحقول readonly
/// - لا توجد طرق لتعديل الحالة
/// - تُبنى فقط من PlayerStateData الأصلي
/// </summary>
public class PlayerStateSnapshot
{
    #region Immutable Fields

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

    #endregion

    /// <summary>
    /// منع الإنشاء الافتراضي
    /// </summary>
    private PlayerStateSnapshot()
    {
    }

    /// <summary>
    /// إنشاء لقطة من بيانات اللاعب
    /// </summary>
    public PlayerStateSnapshot(PlayerStateData data)
    {
        if (data == null)
        {
            throw new ArgumentNullException(nameof(data), "Cannot create snapshot from null data");
        }

        this.id = data.id;
        this.x = data.x;
        this.y = data.y;
        this.rotation = data.rotation;
        this.health = data.health;
        this.shieldHealth = data.shieldHealth;
        this.shieldActive = data.shieldActive;
        this.shieldEndTick = data.shieldEndTick;
        this.fireReady = data.fireReady;
        this.fireReadyTick = data.fireReadyTick;
        this.abilityReady = data.abilityReady;
        this.lastAbilityTime = data.lastAbilityTime;
        this.damageDealt = data.damageDealt;
    }

    /// <summary>
    /// إنشاء نسخة من لقطة موجودة
    /// </summary>
    public PlayerStateSnapshot(PlayerStateSnapshot other)
    {
        if (other == null)
        {
            throw new ArgumentNullException(nameof(other), "Cannot copy null snapshot");
        }

        this.id = other.id;
        this.x = other.x;
        this.y = other.y;
        this.rotation = other.rotation;
        this.health = other.health;
        this.shieldHealth = other.shieldHealth;
        this.shieldActive = other.shieldActive;
        this.shieldEndTick = other.shieldEndTick;
        this.fireReady = other.fireReady;
        this.fireReadyTick = other.fireReadyTick;
        this.abilityReady = other.abilityReady;
        this.lastAbilityTime = other.lastAbilityTime;
        this.damageDealt = other.damageDealt;
    }

    #region Helper Methods

    /// <summary>
    /// الحصول على الموضع كـ Vector3
    /// </summary>
    public Vector3 GetPosition()
    {
        return new Vector3(x, 0f, y); // Y=0 للحركة على مستوي 2D
    }

    /// <summary>
    /// الحصول على الدوران كـ Quaternion
    /// </summary>
    public Quaternion GetRotation()
    {
        return Quaternion.Euler(0f, rotation, 0f);
    }

    /// <summary>
    /// التحقق من صحة البيانات
    /// </summary>
    public bool IsValid()
    {
        return id > 0 && health >= 0 && health <= 100;
    }

    public override string ToString()
    {
        return $"Player {id}: Pos({x:F2},{y:F2}) Health:{health} Shield:{shieldHealth}";
    }

    #endregion
}