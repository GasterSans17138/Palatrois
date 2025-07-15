using UnityEngine;

[CreateAssetMenu(menuName = "RTS/FSM/Condition/Enemy In Range")]
public class EnemyInRangeCondition : ConditionDataScriptable
{
    public override bool Check(Unit _unit)
    {
        BaseEntity enemy = _unit.unitDetection.GetClosestEnemy();

        if (enemy != null && _unit.CanAttack(enemy) && (!_unit.isMoving || _unit.CanStopToShootWhileMoving))
        {
            _unit.entityTarget = enemy;
            return true;
        }
        return false;
    }
}