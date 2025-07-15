using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;

public enum ContextValueKey
{
    FastUnitCount,  // Fast unit number
    LightUnitCount, // Light unit number
    HeavyUnitCount, // Heavy unit number
    FastUnitCountFree,  // Fast unit number not linked to a GOAP
    LightUnitCountFree, // Light unit number not linked to a GOAP
    HeavyUnitCountFree, // Heavy unit number not linked to a GOAP
    UncapturedTowersCount,  // How many uncaptured tower there is (based on fog of war vision)
    ResourceAvailability,   // How many building point we have
    HasDiscoveredEnemyBase, // If we know where is the enemy base
    PercentMapExplored,
    EnemyThreatAtBase,
    AllyDefenseAtBase,
    ClosestUncapturedTowerDistance,
    WeakestUncapturedTowerThreat,
    FreeMilitaryUnitStrength,   // Sum of the influence of each unit that is not linked to a GOAP
    LowExplorationZoneCount,    // How many LowExplorationZone there is
    WeakestUnexploredZoneThreat,
    EnemyBaseThreat,
    ClosestWeakEnemyFactoryThreat,
    ClosestWeakEnemyUnitClusterThreat,
    WeakEnemyFactoryNearEnemyBaseThreat,
    WeakEnemyFactoryNearOwnBaseThreat,
    WeakEnemyUnitClusterNearTowerThreat,
    WeakEnemyFactoryNearUncapturedTowerThreat,
    WeakEnemyUnitClusterNearUncapturedTowerThreat,
    AvailableFactoriesCount,
    AttackTargetDistance,
    TargetThreat
}

public class UtilityContext
{
    public PerceivedWorldState state;

    public Vector3? currentTargetPos;
    private Dictionary<GoalType, Vector3?> selectedTargets = new();

    public void SetTargetForGoal(GoalType type, Vector3? pos)
    {
        selectedTargets[type] = pos;
    }

    public Vector3? GetTargetForGoal(GoalType type)
    {
        return selectedTargets.TryGetValue(type, out var pos) ? pos : null;
    }

    private static readonly Dictionary<ContextValueKey, Func<UtilityContext, float>> valueGetters =
        new()
        {
            [ContextValueKey.FastUnitCount] = ctx => ctx.state.Controller.UnitList.Count(u => u.IsAlive && u.GetUnitData.unitType == UnitType.Fast),
            [ContextValueKey.LightUnitCount] = ctx => ctx.state.Controller.UnitList.Count(u => u.IsAlive && u.GetUnitData.unitType == UnitType.Light),
            [ContextValueKey.HeavyUnitCount] = ctx => ctx.state.Controller.UnitList.Count(u => u.IsAlive && u.GetUnitData.unitType == UnitType.Heavy),
            [ContextValueKey.FastUnitCountFree] = ctx => {
                return Mathf.Clamp01(ctx.state.Controller.UnitList.Count(u => u.IsAlive && u.GetUnitData.unitType == UnitType.Fast && u.isNotLinkedToGoap) / 6f);
            },
            [ContextValueKey.LightUnitCountFree] = ctx => ctx.state.Controller.UnitList.Count(u => u.IsAlive && u.GetUnitData.unitType == UnitType.Light && u.isNotLinkedToGoap),
            [ContextValueKey.HeavyUnitCountFree] = ctx => ctx.state.Controller.UnitList.Count(u => u.IsAlive && u.GetUnitData.unitType == UnitType.Heavy && u.isNotLinkedToGoap),
            [ContextValueKey.UncapturedTowersCount] = ctx => {
                int maxTowers = 12;
                int uncaptured = ctx.state.clustersByTypeAndTeam[InfluenceType.Monetary][ETeam.Neutral].Count;
                return Mathf.Clamp01((float)uncaptured / maxTowers);
            },
            [ContextValueKey.ResourceAvailability] = ctx => ctx.state.resourceAvailability,
            [ContextValueKey.ClosestUncapturedTowerDistance] = ctx =>
            {
                var towers = ctx.state.clustersByTypeAndTeam[InfluenceType.Monetary][ETeam.Neutral];
                if (towers == null || towers.Count == 0) return 1f;
                var closest = towers.OrderBy(c => Vector3.Distance(ctx.state.basePosition, c.Position)).First();
                float dist = Vector3.Distance(ctx.state.basePosition, closest.Position);
                return Mathf.Clamp01(dist / 100f);
            },
            [ContextValueKey.WeakestUncapturedTowerThreat] = ctx =>
            {
                var towers = ctx.state.clustersByTypeAndTeam[InfluenceType.Monetary][ETeam.Neutral];
                if (towers == null || towers.Count == 0) return 1f;
                float minThreat = float.MaxValue;
                foreach (var c in towers)
                    minThreat = Mathf.Min(minThreat, ctx.state.EvaluateThreatAround(c.Position, 15f));
                return Mathf.Clamp01((minThreat == float.MaxValue ? 10f : minThreat) / 10f);
            },
            [ContextValueKey.FreeMilitaryUnitStrength] = ctx =>
            {
                var controller = GameServices.GetControllerByTeam(ctx.state.team);
                if (controller == null) return 0f;
                return controller.UnitList.Where(u => u.IsAlive && u.isNotLinkedToGoap).Sum(u => u.influence);
            },
            [ContextValueKey.LowExplorationZoneCount] = ctx => ctx.state.LowExplorationZones.Count,
            [ContextValueKey.WeakestUnexploredZoneThreat] = ctx =>
            {
                if (ctx.state.LowExplorationZones == null || ctx.state.LowExplorationZones.Count == 0) return 1f;
                float minThreat = float.MaxValue;
                foreach (var pos in ctx.state.LowExplorationZones)
                    minThreat = Mathf.Min(minThreat, ctx.state.EvaluateThreatAround(pos, 15f));
                return Mathf.Clamp01((minThreat == float.MaxValue ? 10f : minThreat) / 10f);
            },
            [ContextValueKey.EnemyBaseThreat] = ctx =>
            {
                if (!ctx.state.EnemyBasePosition.HasValue) return 1f;
                float threat = ctx.state.EvaluateThreatAround(ctx.state.EnemyBasePosition.Value, 20f);
                return Mathf.Clamp01(threat / 15f); // 0 = safe, 1 = gros danger
            },
            [ContextValueKey.ClosestWeakEnemyFactoryThreat] = ctx =>
            {
                var factories = ctx.state.clustersByTypeAndTeam[InfluenceType.Factory][ctx.state.team.GetOpponent()];
                if (factories == null || factories.Count == 0) return 1f;
                float minThreat = float.MaxValue;
                foreach (var f in factories)
                    minThreat = Mathf.Min(minThreat, ctx.state.EvaluateThreatAround(f.Position, 15f));
                return Mathf.Clamp01((minThreat == float.MaxValue ? 10f : minThreat) / 10f);
            },
            [ContextValueKey.ClosestWeakEnemyUnitClusterThreat] = ctx =>
            {
                var clusters = ctx.state.clustersByTypeAndTeam[InfluenceType.Military][ctx.state.team.GetOpponent()];
                if (clusters == null || clusters.Count == 0) return 1f;
                float minThreat = float.MaxValue;
                foreach (var c in clusters)
                {
                    if (c.Strength > 10f) continue;
                    minThreat = Mathf.Min(minThreat, ctx.state.EvaluateThreatAround(c.Position, 15f));
                }
                return Mathf.Clamp01((minThreat == float.MaxValue ? 10f : minThreat) / 10f);
            },
            [ContextValueKey.WeakEnemyFactoryNearEnemyBaseThreat] = ctx =>
            {
                if (!ctx.state.EnemyBasePosition.HasValue) return 1f;
                var factories = ctx.state.clustersByTypeAndTeam[InfluenceType.Factory][ctx.state.team.GetOpponent()];
                if (factories == null || factories.Count == 0) return 1f;
                float minThreat = float.MaxValue;
                foreach (var f in factories)
                {
                    if (Vector3.Distance(f.Position, ctx.state.EnemyBasePosition.Value) > 30f) continue;
                    minThreat = Mathf.Min(minThreat, ctx.state.EvaluateThreatAround(f.Position, 15f));
                }
                return Mathf.Clamp01((minThreat == float.MaxValue ? 10f : minThreat) / 10f);
            },
            [ContextValueKey.WeakEnemyFactoryNearOwnBaseThreat] = ctx =>
            {
                var factories = ctx.state.clustersByTypeAndTeam[InfluenceType.Factory][ctx.state.team.GetOpponent()];
                if (factories == null || factories.Count == 0) return 1f;
                float minThreat = float.MaxValue;
                foreach (var f in factories)
                {
                    if (Vector3.Distance(f.Position, ctx.state.basePosition) > 30f) continue;
                    minThreat = Mathf.Min(minThreat, ctx.state.EvaluateThreatAround(f.Position, 15f));
                }
                return Mathf.Clamp01((minThreat == float.MaxValue ? 10f : minThreat) / 10f);
            },
            [ContextValueKey.WeakEnemyUnitClusterNearTowerThreat] = ctx =>
            {
                var clusters = ctx.state.clustersByTypeAndTeam[InfluenceType.Military][ctx.state.team.GetOpponent()];
                var towers = ctx.state.clustersByTypeAndTeam[InfluenceType.Monetary][ctx.state.team.GetOpponent()];
                if (clusters == null || clusters.Count == 0 || towers == null || towers.Count == 0) return 1f;
                float minThreat = float.MaxValue;
                foreach (var t in towers)
                {
                    foreach (var c in clusters)
                    {
                        if (Vector3.Distance(c.Position, t.Position) > 25f) continue;
                        minThreat = Mathf.Min(minThreat, ctx.state.EvaluateThreatAround(c.Position, 15f));
                    }
                }
                return Mathf.Clamp01((minThreat == float.MaxValue ? 10f : minThreat) / 10f);
            },
            [ContextValueKey.WeakEnemyFactoryNearUncapturedTowerThreat] = ctx =>
            {
                var factories = ctx.state.clustersByTypeAndTeam[InfluenceType.Factory][ctx.state.team.GetOpponent()];
                var neutralTowers = ctx.state.clustersByTypeAndTeam[InfluenceType.Monetary][ETeam.Neutral];
                if (factories == null || factories.Count == 0 || neutralTowers == null || neutralTowers.Count == 0) return 1f;
                float minThreat = float.MaxValue;
                foreach (var t in neutralTowers)
                {
                    foreach (var f in factories)
                    {
                        if (Vector3.Distance(f.Position, t.Position) > 25f) continue;
                        minThreat = Mathf.Min(minThreat, ctx.state.EvaluateThreatAround(f.Position, 15f));
                    }
                }
                return Mathf.Clamp01((minThreat == float.MaxValue ? 10f : minThreat) / 10f);
            },
            [ContextValueKey.WeakEnemyUnitClusterNearUncapturedTowerThreat] = ctx =>
            {
                var clusters = ctx.state.clustersByTypeAndTeam[InfluenceType.Military][ctx.state.team.GetOpponent()];
                var neutralTowers = ctx.state.clustersByTypeAndTeam[InfluenceType.Monetary][ETeam.Neutral];
                if (clusters == null || clusters.Count == 0 || neutralTowers == null || neutralTowers.Count == 0) return 1f;
                float minThreat = float.MaxValue;
                foreach (var t in neutralTowers)
                {
                    foreach (var c in clusters)
                    {
                        if (Vector3.Distance(c.Position, t.Position) > 25f) continue;
                        minThreat = Mathf.Min(minThreat, ctx.state.EvaluateThreatAround(c.Position, 15f));
                    }
                }
                return Mathf.Clamp01((minThreat == float.MaxValue ? 10f : minThreat) / 10f);
            },
            [ContextValueKey.AvailableFactoriesCount] = ctx =>
            {
                var ctrl = GameServices.GetControllerByTeam(ctx.state.team);
                return ctrl?.GetFactoryList?.Count(f => f.CanCreateUnit && !f.IsUnderConstruction) ?? 0;
            },
            [ContextValueKey.HasDiscoveredEnemyBase] = ctx => ctx.state.EnemyBasePosition.HasValue ? 1f : 0f,
            [ContextValueKey.PercentMapExplored] = ctx => ctx.state.percentMapExplored,
            [ContextValueKey.EnemyThreatAtBase] = ctx =>
            {
                float val = ctx.state.EvaluateThreatAround(ctx.state.basePosition, 30f);
                Debug.Log("EnemyThreatAtBase " + val + " at pos : " + ctx.state.basePosition);
                return Mathf.Clamp01(val / 20f);
            },
            [ContextValueKey.AllyDefenseAtBase] = ctx =>
            {
                float val = ctx.state.EvaluateDefenseAround(ctx.state.basePosition, 30f);
                return Mathf.Clamp01(val / 20f);
            },
            [ContextValueKey.AttackTargetDistance] = ctx =>
            {
                if (ctx.currentTargetPos.HasValue)
                {
                    float val = Mathf.Clamp01(Vector3.Distance(ctx.state.basePosition, ctx.currentTargetPos.Value) / 100f);
                    Debug.Log("AttackTargetDistance " + val + " for pos " + ctx.currentTargetPos.Value);
                    return val;
                }
                return 1f;
            },
            [ContextValueKey.TargetThreat] = ctx =>
            {
                if (ctx.currentTargetPos.HasValue) { 
                    float val = Mathf.Clamp01(ctx.state.EvaluateThreatAround(ctx.currentTargetPos.Value, 25f) / 10f); 
                    Debug.Log("TargetThreat " + val + " at pos : " + ctx.currentTargetPos.Value);
                    return val;
                }
                return 1f;
            },
        };

    public UtilityContext(PerceivedWorldState state) => this.state = state;

    public float GetValue(ContextValueKey key)
    {
        if (valueGetters.TryGetValue(key, out var getter))
            return getter(this);
        Debug.LogWarning($"[UtilityContext] Unknown key {key}");
        return 0f;
    }
}
