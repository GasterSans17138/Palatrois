using UnityEngine;

[CreateAssetMenu(menuName = "RTS/FSM/Condition/No Enemy In Range")]
public class NoEnemyInRangeCondition : ConditionDataScriptable
{
    public override bool Check(Unit _unit)
    {
        if(_unit.unitDetection.GetClosestEnemy() == null)
        {
            return true;
        }
        return false;
    }
}