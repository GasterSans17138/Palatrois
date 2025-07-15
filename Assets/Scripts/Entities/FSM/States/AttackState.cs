using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.UI.CanvasScaler;

[CreateAssetMenu(menuName = "RTS/FSM/State/Attack")]
public class AttackState : UnitStateDataScriptable
{
    public override void Enter(Unit _unit)
    {
        _unit.GetComponent<NavMeshAgent>().isStopped = true;
    }

    public override void Tick(Unit _unit)
    {
        if (_unit.entityTarget != null)
        {
            _unit.ComputeAttack();
        }
        else
        {
            _unit.entityTarget = _unit.unitDetection.GetClosestEnemy();
        }
    }

    public override void Exit(Unit _unit)
    {
        if (_unit != null)
            _unit.navMeshAgent.stoppingDistance = 0.1f;
    }
}