using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.UI.CanvasScaler;


[CreateAssetMenu(menuName = "RTS/FSM/State/Move")]
public class MoveState : UnitStateDataScriptable
{
    TargetBuilding tempCaptureTarget;
    BaseEntity tempRepairTarget;
    public override void Enter(Unit _unit)
    {
        if (_unit != null)
        { 
            if (_unit.entityTarget != null)
            {
                tempRepairTarget = _unit.entityTarget;
                _unit.entityTarget = null;
            }

        if (_unit.captureTarget != null)
        {
            tempCaptureTarget = _unit.captureTarget;
        }

            NavMeshAgent agent = _unit.GetComponent<NavMeshAgent>();
            agent.isStopped = false;
            _unit.isMoving = true;
            agent.SetDestination(_unit.positionToMove); 
        }
    }

    public override void Tick(Unit _unit)
    {
    }

    public override void Exit(Unit _unit)
    {
        _unit.GetComponent<NavMeshAgent>().isStopped = true;
        _unit.isMoving = false;
        /*if(_unit.GetComponent<NavMeshAgent>().stoppingDistance == _unit.GetRange - 5)
        {
            _unit.GetComponent<NavMeshAgent>().stoppingDistance = 0.1f;
        }*/
        if (_unit.movingToCapture && tempCaptureTarget) 
        {
            _unit.StartCapture(tempCaptureTarget);
            tempCaptureTarget = null;
        }
        if (_unit.movingToRepair && tempRepairTarget)
        {
            _unit.StartRepairing(tempRepairTarget);
            tempRepairTarget = null;
        }
    }
}