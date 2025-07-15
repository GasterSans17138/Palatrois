using UnityEngine;

[System.Flags]
public enum BonusFlags
{
    None = 0,
    Attack = 1 << 0,   
    Speed = 1 << 1,    
    Health = 1 << 2,   
    Defense = 1 << 3,  
    Range = 1 << 4,
    Repair = 1 << 5,
    AttackSpeed = 1 << 6,
    CaptureRange = 1 << 7,
}

[CreateAssetMenu(fileName = "Unit_Data", menuName = "RTS/UnitData", order = 0)]
public class UnitDataScriptable : EntityDataScriptable
{
    [Header("UnitType")]
    public UnitType unitType;

    [Header("Combat")]
    public int DPS = 10;
    public float AttackFrequency = 1f;
    public float AttackDistanceMax = 10f;
    public float CaptureDistanceMax = 10f;

    [Header("Repairing")]
    public bool CanRepair = false;
    public int RPS = 10;
    public float RepairFrequency = 1f;
    public float RepairDistanceMax = 10f;

    [Header("Movement")]
    [Tooltip("Overrides NavMeshAgent steering settings")]
    public float Speed = 10f;
    public float AngularSpeed = 200f;
    public float Acceleration = 20f;
    public bool IsFlying = false;

    [Header("FX")]
    public GameObject BulletPrefab = null;
    public GameObject DeathFXPrefab = null;

    [Header("Bonus")]
    public BonusFlags Bonus = BonusFlags.None;

    [Tooltip("Damage percent bonus")]
    public float AttackBonus = 0f;
    [Tooltip("Speed percent bonus")]
    public float SpeedBonus = 0f;
    [Tooltip("Heal percent bonus")]
    public float HealthBonus = 0f;
    [Tooltip("Defense percent bonus")]
    public float DefenseBonus = 0f;
    [Tooltip("Range percent bonus")]
    public float RangeBonus = 0f;
    [Tooltip("Repair percent bonus")]
    public float RepairBonus = 0f;
    [Tooltip("Attack speed percent bonus")]
    public float AttackSpeedBonus = 0f;
    [Tooltip("Capture range percent bonus")]
    public float CaptureRangeBonus = 0f;

    public bool HasBonus(BonusFlags _bonus)
    {
        return (Bonus & _bonus) != 0;
    }

    public void AddBonus(BonusFlags _bonus)
    {
        Bonus |= _bonus;
    }

    public void RemoveBonus(BonusFlags _bonus)
    {
        Bonus &= ~_bonus;
    }
}
