using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "RTS/FSM/State/Repair")]
public class RepairState : UnitStateDataScriptable
{
    public override void Enter(Unit _unit)
    {
        _unit.GetComponent<NavMeshAgent>().isStopped = true;
        _unit.movingToRepair = false;
    }

    public override void Tick(Unit _unit)
    {
        _unit.ComputeRepairing();
    }

    public override void Exit(Unit _unit)
    {
        _unit.navMeshAgent.stoppingDistance = 0;
        _unit.entityTarget = null;
    }
}
