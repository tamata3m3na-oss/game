using System;

/// <summary>
بيانات حالة اللاعب (قابلة للتعديل)
تستخدم داخلياً في الـ Repository
/// </summary>
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

    public PlayerStateData() { }

    public PlayerStateData(PlayerStateData other)
    {
        if (other == null) return;

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

    public override string ToString()
    {
        return $"Player {id}: Health={health}, Shield={shieldHealth}, Pos=({x:F1},{y:F1})";
    }
}