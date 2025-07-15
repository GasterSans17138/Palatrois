using System;
using UnityEngine;
using UnityEngine.AI;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "RTS/GOAP/Action/Move To Target")]
#endif
public class MoveGOAP : GOAPActions
{
    const float buffer = 0.1f;

    public override void Enter(AIController _aiController)
    {
        _hasFailed = false;
        foreach (Unit unit in goap.localWorldState.assignedUnits)
        {
            unit.SetTargetPos(goap.assignment.targetPosition, true);
            if (goap.assignment.goalType == GoalType.AttackZone)
            {
                unit.GetComponent<NavMeshAgent>().stoppingDistance = 20;
            }
        }
    }
    public override void Tick(AIController _aiController)
    {
        foreach (Unit unit in goap.localWorldState.assignedUnits)
        {
            if (unit == null)
                continue;
            NavMeshAgent agent = unit.GetComponent<NavMeshAgent>();
            if (agent == null)
            {
                return;
            }

            if (!unit.isMoving && !agent.pathPending)
            {
                float remaining = agent.remainingDistance;
                float stopDist = agent.stoppingDistance;

                if (remaining > stopDist + buffer)
                {
                    Debug.LogWarning(remaining + " | " + stopDist + " + " + buffer);
                    _hasFailed = true;
                }
            }
        }
    }

    public override void Exit(AIController _aiController)
    {
        foreach (Unit unit in goap.localWorldState.assignedUnits)
        {
            if (goap.assignment.goalType == GoalType.AttackZone)
            {
                unit.GetComponent<NavMeshAgent>().stoppingDistance = 0.1f;
            }
        }
    }

    public override bool IsComplete(AIController _aiController)
    {
        foreach (Unit unit in goap.localWorldState.assignedUnits)
        {
            return !unit.isMoving;
        }
        return false;
    }
}