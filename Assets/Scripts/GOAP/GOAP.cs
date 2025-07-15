using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Services.Analytics.Internal;
using UnityEngine;

public class GOAP : MonoBehaviour
{
    [Header("Assets GOAP Actions")]
    [SerializeField] List<GOAPActions> actions;
    public ETeam team;
    public PerceivedWorldState globalState;
    public LocalWorldState localWorldState;
    public bool isBusy = false;
    public GroupPlanExecutor groupPlanExecutor;
    public UtilityGoalAssignment assignment = null;
    public AIController controller;
    public AIController Controller { get { return controller; } }

    private void Awake()
    {
        LinkGoapAndActions();
    }

    private void LinkGoapAndActions()
    {
        foreach (var action in actions)
        {
            action.goap = this;
        }
    }

    public void AssignGoal(UtilityGoalAssignment _assignment) 
    {
        assignment = _assignment;
        SetupGoal();
    }

    public void SetupGoal()
    {
        localWorldState = new LocalWorldState(team, globalState, assignment.assignedUnits, assignment.targetPosition);
        localWorldState.numberOfUnitToCreate = assignment.unitCount;
        localWorldState.goalType = assignment.goalType;
        isBusy = true;

        int keyCount = System.Enum.GetValues(typeof(WorldKey)).Length;
        BitArray goal = new BitArray(keyCount);

        switch (assignment.goalType)
        {
            case GoalType.CreateFastUnit:
                goal.Set((int)WorldKey.HasUnitBeenCreated, true);
                goal.Set((int)WorldKey.IsSquadCreated, true);
                break;

            case GoalType.CaptureTower:
                goal.Set((int)WorldKey.HasCapturedTarget, true);
                break;

            case GoalType.ExploreZone:
                goal.Set((int)WorldKey.HasCurrentTarget, true);
                goal.Set((int)WorldKey.TargetIsCloseEnough, true);
                break;

            case GoalType.AttackZone:
                goal.Set((int)WorldKey.HasCurrentTarget, true);
                goal.Set((int)WorldKey.TargetIsCloseEnough, true);
                goal.Set((int)WorldKey.NoMoreThreatAtTarget, true);
                break;

            default:
                goal.Set((int)WorldKey.HasCurrentTarget, true);
                goal.Set((int)WorldKey.TargetIsCloseEnough, true);
                break;
        }

        GOAPActions[] plan = CreatePlanForward(goal);

        if (plan == null || plan.Length == 0)
        {
            Debug.LogWarning("GOAP: aucun plan trouvé pour cet objectif");
            foreach (var u in assignment.assignedUnits)
                u.isNotLinkedToGoap = true;

            assignment = null;
            isBusy = false;
            return;
        }

        Debug.Log($"[GOAP] Plan généré: {assignment.goalType} => " + string.Join(" -> ", plan.Select(a => a.name)));

        groupPlanExecutor.StartGroupPlan(
            this,
            globalState,
            team,
            assignment.assignedUnits,
            assignment.targetPosition,
            goal,
            plan
        );
    }

    public GOAPActions[] CreatePlanForward(BitArray _goal)
    {
        NodeGOAP start = new NodeGOAP(null, localWorldState.GetState(), null);
        List<NodeGOAP> leaves = new List<NodeGOAP>();

        BuildGraphForward(start, leaves, actions, _goal);

        if (leaves.Count == 0) return null;

        NodeGOAP best = null;
        float minDepth = float.MaxValue;
        foreach (var leaf in leaves)
        {
            if (leaf.depth < minDepth)
            {
                minDepth = leaf.depth;
                best = leaf;
            }
        }

        // Reverse
        List<GOAPActions> plan = new List<GOAPActions>();
        for (NodeGOAP n = best; n != null && n.action != null; n = n.parent)
        {
            plan.Add(n.action);
        }
        plan.Reverse();

        return plan.ToArray();
    }


    private void BuildGraphForward(NodeGOAP _parent, List<NodeGOAP> _leaves, List<GOAPActions> _usableActions, BitArray _goal)
    {
        foreach (GOAPActions action in _usableActions)
        {
            if (action.IsValid(_parent.worldState))
            {
                BitArray newWorldState = action.ApplyEffects(_parent.worldState);
                NodeGOAP node = new NodeGOAP(_parent, newWorldState, action);
                if (GoalAchieved(newWorldState, _goal))
                {
                    _leaves.Add(node);
                }
                else
                {
                    List<GOAPActions> actionsSubSet = CreateActionSubSet(_usableActions, action);
                    BuildGraphForward(node, _leaves, actionsSubSet, _goal);
                }
            }
        }
    }

    private bool GoalAchieved(BitArray _worldState, BitArray _goal)
    {
        for (int i = 0; i < _goal.Length; i++)
        {
            if (_goal[i] && !_worldState[i])
            {
                return false;
            }
        }
        return true;
    }

    private List<GOAPActions> CreateActionSubSet(List<GOAPActions> _usableActions, GOAPActions _action)
    {
        List<GOAPActions> subset = new List<GOAPActions>();
        foreach (GOAPActions act in _usableActions)
        {
            if (act != _action)
            {
                subset.Add(act);
            }
        }
        return subset;
    }
}