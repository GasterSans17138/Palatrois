using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "RTS/FSM/State/Capture")]
public class CaptureState : UnitStateDataScriptable
{
    public bool needNewDestination = false;
    public override void Enter(Unit _unit)
    {
        Debug.Log("Enter Capture");
        needNewDestination = !_unit.DistanceCheck(_unit.captureTarget);
        if (!needNewDestination)
        {
            _unit.GetComponent<NavMeshAgent>().isStopped = true;
            _unit.StartCapture(_unit.captureTarget);
            _unit.movingToCapture = false;
        }
    }

    public override void Tick(Unit _unit)
    {
        if (_unit.DistanceCheck(_unit.captureTarget) && needNewDestination)
        {
            _unit.GetComponent<NavMeshAgent>().isStopped = true;
            _unit.StartCapture(_unit.captureTarget);
            _unit.movingToCapture = false;
            needNewDestination = false;
        }
    }

    public override void Exit(Unit _unit)
    {
        Debug.Log("Exit Capture");
        _unit.navMeshAgent.stoppingDistance = 0;
        if (_unit.IsCapturing())
        {
            _unit.StopCapture();
        }
    }
}