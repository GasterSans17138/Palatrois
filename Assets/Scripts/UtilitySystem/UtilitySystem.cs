using System.Collections.Generic;
using System.Linq;
using Unity.Services.Analytics.Internal;
using UnityEngine;

public enum GoalType
{
    None,
    CaptureTower,
    DefendZone,
    ExploreZone,
    AttackZone,
    AccumulateMilitary,
    Diversion,
    RallyTroops,
    Reinforce,
    CreateFastUnit,
    CreateHeavyUnit,
    CreateLightUnit
}

public class UtilitySystem : MonoBehaviour
{
    [SerializeField] public PerceivedWorldState worldState;
    [SerializeField] private List<UtilityGoalDefinition> goalDefinitions;
    [SerializeField] private float updateInterval = 2f;

    public List<GOAP> goapAgents = new();
    private float lastUpdateTime;

    void Update()
    {
        if (Time.time - lastUpdateTime < updateInterval) return;
        lastUpdateTime = Time.time;
        EvaluateAndAssignGoals();
    }

    void EvaluateAndAssignGoals()
    {
        var context = new UtilityContext(worldState);

        foreach (var agent in goapAgents)
        {
            if (agent.isBusy) continue;

            var scoredGoals = goalDefinitions
                .Select(gd => new { goal = gd, score = gd.ComputeUtility(context) })
                .OrderByDescending(x => x.score)
                .ToList();

            UtilityGoalDefinition chosenGoal = null;
            UtilityGoalAssignment assignment = null;

            string debugGoalScores = $"[UTILITY EVAL] Agent: {agent.name}\n";
            foreach (var entry in scoredGoals)
            {
                debugGoalScores += $"  - {entry.goal.goalType}: {entry.score:F3}\n";
            }
            Debug.Log(debugGoalScores);

            foreach (var entry in scoredGoals)
            {
                var goal = entry.goal;
                Vector3? bestTargetPos = null;
                string bestTargetLabel = null;
                float requiredStrength = 0f;
                float targetThreat = 0f;
                int fastUnitsToProduce = 0;
                List<Factory> factories = null;

                switch (goal.goalType)
                {
                    case GoalType.AttackZone:
                        bestTargetPos = context.GetTargetForGoal(GoalType.AttackZone);
                        Debug.Log("ATTACK : " + bestTargetPos);
                        bestTargetLabel = "AttackTarget";
                        break;
                    case GoalType.CaptureTower:
                        var towers = worldState.clustersByTypeAndTeam[InfluenceType.Monetary][ETeam.Neutral];
                        bestTargetPos = (towers != null && towers.Count > 0)
                            ? towers.OrderBy(t => Vector3.Distance(worldState.basePosition, t.Position)).First().Position
                            : (Vector3?)null;
                        bestTargetLabel = "Tower";
                        targetThreat = bestTargetPos.HasValue ? worldState.EvaluateThreatAround(bestTargetPos.Value, 15f) : 0f;
                        break;
                    case GoalType.ExploreZone:
                        bestTargetPos = (worldState.LowExplorationZones != null && worldState.LowExplorationZones.Count > 0)
                            ? worldState.LowExplorationZones.OrderBy(z => worldState.EvaluateThreatAround(z, 15f)).First()
                            : (Vector3?)null;
                        bestTargetLabel = "Zone";
                        targetThreat = bestTargetPos.HasValue ? worldState.EvaluateThreatAround(bestTargetPos.Value, 15f) : 0f;
                        break;
                    case GoalType.CreateFastUnit:
                        fastUnitsToProduce = goal.GetRequiredUnitCount(context, UnitType.Fast);
                        factories = worldState.GetAvailableFactories();
                        assignment = new UtilityGoalAssignment
                        {
                            goalType = goal.goalType,
                            unitTypeToProduce = UnitType.Fast,
                            factoriesAssigned = factories,
                            unitCount = fastUnitsToProduce,
                        };
                        break;
                }

                if (goal.goalType == GoalType.CreateFastUnit)
                {
                    chosenGoal = goal;
                    break;
                }

                if (assignment == null)
                {
                    if (!bestTargetPos.HasValue)
                    {
                        continue;
                    }

                    requiredStrength = goal.GetRequiredStrength(context, bestTargetPos.Value, bestTargetLabel, targetThreat);
                    Debug.Log("ATTACK requiredStrength : " + requiredStrength + " for " + bestTargetPos.Value);

                    List<Unit> assignedUnits = SelectBestUnitsForGoal(bestTargetPos.Value, requiredStrength, buffer: GetInfluenceBufferForTarget(bestTargetLabel, bestTargetPos.Value));

                    if (assignedUnits.Sum(u => u.influence) < requiredStrength) { 
                        continue;
                    }

                    foreach (var unit in assignedUnits)
                        unit.isNotLinkedToGoap = false;

                    assignment = new UtilityGoalAssignment
                    {
                        goalType = goal.goalType,
                        targetPosition = bestTargetPos.Value,
                        targetLabel = bestTargetLabel,
                        assignedUnits = assignedUnits,
                        requiredStrength = requiredStrength
                    };

                    chosenGoal = goal;
                    break;
                }
            }

            if (assignment == null && chosenGoal == null)
                continue;

            DebugGoal(assignment);

            assignment.indexGOAP = goapAgents.IndexOf(agent);
            agent.AssignGoal(assignment);
        }
    }


    List<Unit> SelectBestUnitsForGoal(Vector3 targetPos, float requiredForce, float buffer = 1.2f, float maxRange = 1000f)
    {
        var unitList = GameServices.GetControllerByTeam(worldState.team).UnitList;
        var freeUnits = unitList.Where(u => u.IsAlive && u.isNotLinkedToGoap && u.IsCurrentLeader).ToList();
        
        freeUnits = freeUnits.OrderBy(u => Vector3.Distance(u.transform.position, targetPos)).ToList();

        List<Unit> selected = new List<Unit>();
        float sum = 0f;
        foreach (var unit in freeUnits)
        {
            selected.AddRange(unit.Squad.Units);
            sum += unit.Squad.GetInfluence;
            if (sum >= requiredForce * buffer)
                break;
        }

        return selected;
    }

    void DebugGoal(UtilityGoalAssignment assignment)
    {
        var debug = $"[UTILITY GOAL SELECTED]\n" +
                    $"Goal: {assignment.goalType}\n";

        if (assignment.targetLabel != null)
            debug += $"Target: {assignment.targetLabel} @ {assignment.targetPosition}\n";

        if (assignment.requiredStrength > 0)
            debug += $"Required Strength: {assignment.requiredStrength:F2}\n";

        if (assignment.assignedUnits != null && assignment.assignedUnits.Count > 0)
        {
            debug += $"Units Assigned: {assignment.assignedUnits.Count} " +
                     $"(Total force: {assignment.assignedUnits.Sum(u => u.influence):F2})\n";
            debug += $"Unit List: {string.Join(", ", assignment.assignedUnits.Select(u => u.name))}\n";
        }

        if (assignment.factoriesAssigned != null && assignment.factoriesAssigned.Count > 0)
        {
            debug += $"Factories Assigned: {assignment.factoriesAssigned.Count}\n";
            debug += $"Factory List: {string.Join(", ", assignment.factoriesAssigned.Select(f => f.name))}\n";
        }

        debug += $"Unit Type To Produce: {assignment.unitTypeToProduce}\n";

        if (assignment.unitCount > 0)
            debug += $"Units To Produce: {assignment.unitCount}\n";

        Debug.Log(debug);
    }

    float GetInfluenceBufferForTarget(string label, Vector3 pos)
    {
        float threat = worldState.EvaluateThreatAround(pos, 20f);
        if (threat > 8f) return 1.5f;
        if (threat > 2f) return 1.2f;
        return 1.0f;
    }
}

public class UtilityGoalAssignment
{
    public GoalType goalType;
    public Vector3 targetPosition;
    public string targetLabel;
    public List<Unit> assignedUnits = new();
    public List<Factory> factoriesAssigned = new();
    public UnitType unitTypeToProduce;
    public int unitCount; // To produce
    public float requiredStrength;
    public int indexGOAP;
}
public enum UnitType { Fast, Light, Heavy }
