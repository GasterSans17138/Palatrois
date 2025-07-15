using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "AI/Goals/UtilityGoalDefinition")]
public class UtilityGoalDefinition : ScriptableObject
{
    public GoalType goalType;
    public List<UtilityParameter> parameters = new();

    public virtual float ComputeUtility(UtilityContext context)
    {
        float score = 0f;
        foreach (var param in parameters)
        {
            float value = context.GetValue(param.key);
            float curveValue = param.curve.Evaluate(value) * param.weight;
            score += curveValue;
        }
        return score;
    }

    public int GetRequiredUnitCount(UtilityContext context, UnitType _unitType)
    {
        int toProduce = Random.Range(4, 8);
        return toProduce;
    }

    public virtual float GetRequiredStrength(UtilityContext context, Vector3 targetPos, string targetLabel, float threatAtTarget)
    {
        float threat = threatAtTarget;

        float factoryBonus = 0f;
        var enemyFactories = context.state.clustersByTypeAndTeam[InfluenceType.Factory][context.state.team.GetOpponent()];
        foreach (var fac in enemyFactories)
        {
            float max_dist = 30f;
            float distSqr = (fac.Position - targetPos).sqrMagnitude;

            if (distSqr < max_dist * max_dist)
            {
                float dist = Mathf.Sqrt(distSqr);
                factoryBonus += Mathf.Lerp(4f, 0f, dist / max_dist);
            }
        }

        float multiEnemyBonus = 0f;
        var enemyClusters = context.state.clustersByTypeAndTeam[InfluenceType.Military][context.state.team.GetOpponent()];
        float maxDist = 20f;
        float maxDistSqr = maxDist * maxDist;

        int nearbyClusters = enemyClusters.Count(c =>
            (c.Position - targetPos).sqrMagnitude < maxDistSqr && c.Strength > 2f);

        if (nearbyClusters > 1)
            multiEnemyBonus = (nearbyClusters - 1) * 2f;


        float baseBonus = (targetLabel == "Base") ? 8f : 0f;

        float defenseAlly = context.state.EvaluateDefenseAround(targetPos, 15f);
        float defenseMalus = Mathf.Min(defenseAlly, 20f) * 0.4f;
        float buffer = 4f;

        float required = threat * 1.25f + buffer + factoryBonus + baseBonus + multiEnemyBonus - defenseMalus;

        switch (goalType)
        {
            case GoalType.ExploreZone: 
                return 1f;
            case GoalType.CaptureTower:
                return Mathf.Clamp(required, 1f, 25f);
            case GoalType.AttackZone:
            default:
                return Mathf.Clamp(required, 15f, 45f);
        }
    }
}

[System.Serializable]
public class UtilityParameter
{
    public ContextValueKey key;
    public AnimationCurve curve;
    public float weight = 1f;
}