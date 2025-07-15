using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.UI.CanvasScaler;

public class LocalWorldState
{
    public GoalType goalType;
    public PerceivedWorldState globalState;
    public List<Unit> assignedUnits;
    public Vector3 currentTarget;
    public ETeam team;
    public int numberOfUnitToCreate;
    public bool IsSquadCreated { get; private set; }
    public float GroupStrength { get; private set; }
    public bool CanReachTarget { get; private set; }
    public bool HasCapturedTarget { get; private set; }
    public bool TargetIsCloseEnough { get; private set; }
    public bool NoMoreThreatAtTarget { get; private set; }
    public bool HasUnitBeenCreated { get; set; }

    public LocalWorldState(ETeam _team, PerceivedWorldState _globalState, List<Unit> _assignedUnits, Vector3 _target)
    {
        this.globalState = _globalState;
        this.assignedUnits = _assignedUnits;
        this.currentTarget = _target;
        this.team = _team;
    }

    public void Analyze()
    {
        IsSquadCreated = assignedUnits.Count > 1;
        HasUnitBeenCreated = false;
        HasCapturedTarget = false;
        GroupStrength = 0f;

        // Capture Target
        if(goalType == GoalType.CaptureTower)
        {
            HasCapturedTarget = GameServices.GetClosestTargetBuilding(currentTarget).GetTeam() == team;
        }

        // Unit Created
        if(numberOfUnitToCreate == assignedUnits.Count)
        {
            HasUnitBeenCreated = true;
        }

        // Squad Created
        foreach (Unit unit in assignedUnits)
        {
            if (!unit.IsInSquad)
            {
                IsSquadCreated = false;
                break;
            }
        }

        
        // More Influence
        foreach (var unit in assignedUnits)
        {
            GroupStrength += unit.influence; 
        }

        // Move
        if (currentTarget != Vector3.zero && assignedUnits.Count > 0)
        {
            bool allReached = true;
            foreach (Unit u in assignedUnits)
            {
                if (u == null)
                    continue;
                NavMeshAgent agent = u.GetComponent<NavMeshAgent>();
                float tol = agent.stoppingDistance;
                float sqrD = (u.transform.position - currentTarget).sqrMagnitude;
                if (sqrD > tol * tol)
                {
                    allReached = false;
                    continue;
                }
            }
            TargetIsCloseEnough = allReached;
        }
        else
        {
            TargetIsCloseEnough = false;
        }

        if (!NoMoreThreatAtTarget && globalState.EvaluateThreatAround(currentTarget) <= 1f) 
        {
            NoMoreThreatAtTarget = true;
        }
        else
        {
            NoMoreThreatAtTarget = false;
        }
    }

    public BitArray GetState()
    {
        BitArray globalBits = globalState.GetState();
        BitArray localBits = new BitArray(new bool[]
        {
            IsSquadCreated,
            GroupStrength > globalState.EvaluateThreatAround(currentTarget),
            currentTarget != Vector3.zero,
            TargetIsCloseEnough,
            HasUnitBeenCreated,
            HasCapturedTarget,
            NoMoreThreatAtTarget
        });

        BitArray combined = new BitArray(globalBits.Length + localBits.Length);
        for (int i = 0; i < globalBits.Length; i++) combined[i] = globalBits[i];
        for (int j = 0; j < localBits.Length; j++) combined[globalBits.Length + j] = localBits[j];

        return combined;
    }
}
