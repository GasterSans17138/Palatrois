using UnityEngine;

[CreateAssetMenu(menuName = "RTS/FSM/Condition/Has Repair Target")]
public class HasRepairTargetCondition : ConditionDataScriptable
{
    public override bool Check(Unit _unit)
    {
        if( _unit.entityTarget)
        {
            IRepairable repairable = _unit.entityTarget;
            bool needsRepair = repairable.NeedsRepairing();
            return _unit.entityTarget != null
                && _unit.GetUnitData.CanRepair
                && _unit.entityTarget.GetTeam() == _unit.GetTeam()
                && needsRepair
                && _unit.CanRepair(_unit.entityTarget);
        }
        return false;
    }
}