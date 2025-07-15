using UnityEngine;

[CreateAssetMenu(menuName = "RTS/FSM/Condition/No Repair Target")]
public class NoRepairTargetCondition : ConditionDataScriptable
{
    public override bool Check(Unit _unit)
    {
        if (_unit.entityTarget)
        {
            var repairable = _unit.entityTarget as IRepairable;
            bool needsRepair = repairable.NeedsRepairing();
            return _unit.entityTarget != null
               && _unit.GetUnitData.CanRepair
               && _unit.entityTarget.GetTeam() == _unit.GetTeam()
               && !needsRepair;
        }
        return false;
    }
}