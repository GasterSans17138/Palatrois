using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "RTS/FSM/Condition/Arrived At Destination")]
public class ArrivedAtDestCondition : ConditionDataScriptable
{
    public override bool Check(Unit _unit)
    {
        NavMeshAgent agent = _unit.GetComponent<NavMeshAgent>();
        if (agent.pathPending)
        {
            return false;
        }
        return agent.remainingDistance <= agent.stoppingDistance
               && (!agent.hasPath || agent.velocity.sqrMagnitude == 0f);
    }
}