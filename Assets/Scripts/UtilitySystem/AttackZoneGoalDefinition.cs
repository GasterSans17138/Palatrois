using UnityEngine;

[CreateAssetMenu(menuName = "AI/Goals/AttackZoneGoal")]
public class AttackZoneGoalDefinition : UtilityGoalDefinition
{
    public override float ComputeUtility(UtilityContext context)
    {
        float bestScore = float.MinValue;
        Vector3? bestTarget = null;

        // 1. Test clusters militaires
        var clusters = context.state.clustersByTypeAndTeam[InfluenceType.Military][context.state.team.GetOpponent()];
        if (clusters != null)
        {
            foreach (var c in clusters)
            {
                context.currentTargetPos = c.Position;
                float score = 0f;
                foreach (var param in parameters)
                {
                    float value = context.GetValue(param.key);
                    float curveValue = param.curve.Evaluate(value) * param.weight;
                    Debug.Log($"[AttackZone] {param.key}: value={value:F3}, curve={curveValue:F3}, weight={param.weight:F3}");
                    score += curveValue;
                }
                Debug.Log($"[AttackZone] Cluster {c.Position} score={score}");
                if (score > bestScore)
                {
                    bestScore = score;
                    bestTarget = c.Position;
                }
            }
        }

        // 2. Test usines ennemies
        var factories = context.state.clustersByTypeAndTeam[InfluenceType.Factory][context.state.team.GetOpponent()];
        if (factories != null)
        {
            foreach (var f in factories)
            {
                context.currentTargetPos = f.Position;
                float score = 0f;
                foreach (var param in parameters)
                {
                    float value = context.GetValue(param.key);
                    float curveValue = param.curve.Evaluate(value) * param.weight;
                    score += curveValue;
                }
                if (score > bestScore)
                {
                    bestScore = score;
                    bestTarget = f.Position;
                }
            }
        }

        // 3. Test base ennemie
        if (context.state.HasDiscoveredEnemyBase && context.state.EnemyBasePosition.HasValue)
        {
            context.currentTargetPos = context.state.EnemyBasePosition.Value;
            float score = 0f;
            foreach (var param in parameters)
            {
                float value = context.GetValue(param.key);
                float curveValue = param.curve.Evaluate(value) * param.weight;
                Debug.Log($"[AttackZone] {param.key}: value={value:F3}, curve={curveValue:F3}, weight={param.weight:F3}");
                score += curveValue;
            }
            Debug.Log($"[AttackZone] EnemyBase {context.state.EnemyBasePosition.Value} score={score}");
            if (score > bestScore)
            {
                bestScore = score;
                bestTarget = context.state.EnemyBasePosition.Value;
            }
        }

        // 4. Test tours ennemies
        var enemyTowers = context.state.clustersByTypeAndTeam[InfluenceType.Monetary][context.state.team.GetOpponent()];
        if (enemyTowers != null)
        {
            foreach (var tow in enemyTowers)
            {
                context.currentTargetPos = tow.Position;
                float score = 0f;
                foreach (var param in parameters)
                {
                    float value = context.GetValue(param.key);
                    float curveValue = param.curve.Evaluate(value) * param.weight;
                    score += curveValue;
                }
                if (score > bestScore)
                {
                    bestScore = score;
                    bestTarget = tow.Position;
                }
            }
        }

        context.currentTargetPos = null;
        context.SetTargetForGoal(goalType, bestTarget);

        if (bestTarget == null)
            return 0f;
        return bestScore;
    }
}
