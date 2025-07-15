using UnityEngine;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "RTS/GOAP/Action/Repair Ally")]
#endif
public class RepairActionSO : GOAPActions
{
    /*
    public override void Enter(Unit unit)
    {
        unit.fsm.ChangeState(unit.repairState);
    }

    public override void Tick(Unit unit)
    {
    }

    public override bool IsComplete(Unit unit)
    {
        BaseEntity entity = unit.entityTarget;
        return entity == null || !entity.NeedsRepairing() || !unit.CanRepair(entity);
    }

    public override void Exit(Unit unit)
    {
        unit.entityTarget = null;
    }*/
    public override void Exit(AIController _aiController)
    {
        throw new System.NotImplementedException();
    }

    public override bool IsComplete(AIController _aiController)
    {
        throw new System.NotImplementedException();
    }

    public override void Tick(AIController _aiController)
    {
        throw new System.NotImplementedException();
    }
}