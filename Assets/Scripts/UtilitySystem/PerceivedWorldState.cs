using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public enum WorldKey
{
    /* === Squad === */
    IsSquadCreated,
    HasEnoughUnitsToCreateSquad,
    HasLeaderInUnits,

    HasDiscoveredEnemyBase,
    HasUncapturedTowers,
    IsUnderThreat,

    HasEnoughStrengh,
    HasCurrentTarget,
    TargetIsCloseEnough,
    HasUnitBeenCreated,
    HasCapturedTarget,
    NoMoreThreatAtTarget,
}

public class PerceivedWorldState : MonoBehaviour 
{
    #region GOAP WorldState Flags
    public bool HasEnoughUnitsToCreateSquad => Controller.HasEnoughUnitsToCreateSquadInAllUnits();
    public bool HasLeaderInUnits => Controller.HasAvailableLeader();
    public bool HasDiscoveredEnemyBase => EnemyBasePosition.HasValue;
    public bool HasUncapturedTowers => clustersByTypeAndTeam[InfluenceType.Monetary][ETeam.Neutral].Count > 0;
    public bool IsUnderThreat => EvaluateThreatAround(basePosition) > 5f;

    public BitArray GetState()
    {
        var flags = new List<bool>
        {
            HasEnoughUnitsToCreateSquad,
            HasLeaderInUnits,
            HasDiscoveredEnemyBase,
            HasUncapturedTowers,
            IsUnderThreat
        };

        return new BitArray(flags.ToArray());
    }
    #endregion

    #region Data
    public float percentMapExplored; // pourcentage de la map qui a �t� explor�
    public float resourceAvailability; // buildings points
    public Dictionary<InfluenceType, Dictionary<ETeam, List<InfluenceCluster>>> clustersByTypeAndTeam = new(); // Dico des cluster de chaque type d'influence pour chaque team (y compris neutre pour mon�taire)
    private HashSet<InfluenceNode> unexploredNodes;
    public Vector3 basePosition; // pos base
    public Vector3? EnemyBasePosition { get; private set; } // pos base ennemie
    public List<Vector3> LowExplorationZones { get; private set; } = new(); // points a explorer
    #endregion

    #region Setup
    public ETeam team;
    private InfluenceMap map;
    private FogOfWarSystem fog;
    [SerializeField] private float updateInterval = 3.0f;
    private float lastUpdateTime;
    private AIController controller;

    public AIController Controller { get { return controller; }}

    private void Start()
    {
        controller = GetComponent<AIController>();
        map = InfluenceMap.Instance;
        fog = map.FOWSystem;
        
        basePosition = GameObject.FindObjectsOfType<Factory>().FirstOrDefault(f => f.Team == team && f.IsMainFactory).transform.position;
        EnemyBasePosition = GameObject.FindObjectsOfType<Factory>().FirstOrDefault(f => f.Team == team.GetOpponent() && f.IsMainFactory).transform.position;
        //new Vector3?();

        if (clustersByTypeAndTeam == null)
            clustersByTypeAndTeam = new Dictionary<InfluenceType, Dictionary<ETeam, List<InfluenceCluster>>>();

        if (LowExplorationZones == null)
            LowExplorationZones = new List<Vector3>();

        foreach (InfluenceType type in System.Enum.GetValues(typeof(InfluenceType)))
        {
            var teamDict = new Dictionary<ETeam, List<InfluenceCluster>>();
            foreach (ETeam t in System.Enum.GetValues(typeof(ETeam)))
            {
                if (t == ETeam.Neutral && type != InfluenceType.Monetary)
                    teamDict[t] = new List<InfluenceCluster>();
                else
                {
                    var clusters = ComputeClusters(type, t, _minInfluence: 0.3f);
                    teamDict[t] = clusters ?? new List<InfluenceCluster>();
                }
            }
            clustersByTypeAndTeam[type] = teamDict;
        }

        unexploredNodes = new HashSet<InfluenceNode>(
            map.NodeList
               .OfType<InfluenceNode>()
               .Where(inf => !fog.WasVisible((int)team, new Vector2(inf.Position.x, inf.Position.z)))
        );

        StartCoroutine(DetectLowExplorationZonesAsync());
        lastUpdateTime = Time.time;
    }
    #endregion

    #region Update
    private void Update()
    {
        if (Time.time - lastUpdateTime < updateInterval) return;

        lastUpdateTime = Time.time;
        Analyze();
    }

    private void Analyze()
    {
        int explored = 0, total = map.NodeList.Length;
        foreach (var node in map.NodeList)
        {
            if (node is not InfluenceNode inf) continue;
            if (fog.WasVisible(1 << (int)team, new Vector2(inf.Position.x, inf.Position.z)))
                explored++;
        }


        percentMapExplored = total > 0 ? (float)explored / total : 0f;

        resourceAvailability = GameServices.GetControllerByTeam(team)?.TotalBuildPoints ?? 0f;

        clustersByTypeAndTeam.Clear();


        foreach (InfluenceType type in System.Enum.GetValues(typeof(InfluenceType)))
        {
            var teamDict = new Dictionary<ETeam, List<InfluenceCluster>>();
            foreach (ETeam t in System.Enum.GetValues(typeof(ETeam)))
            {
                if (t == ETeam.Neutral && type != InfluenceType.Monetary)
                    teamDict[t] = new List<InfluenceCluster>();
                else
                {
                    var clusters = ComputeClusters(type, t, _minInfluence: 0.3f);
                    teamDict[t] = clusters ?? new List<InfluenceCluster>();
                }
            }
            clustersByTypeAndTeam[type] = teamDict;
        }

        if (!EnemyBasePosition.HasValue) 
        {
            //DetectEnemyBase();
        }

        var nowVisible = unexploredNodes
           .Where(inf => fog.WasVisible((int)team, new Vector2(inf.Position.x, inf.Position.z)))
           .ToList();
        foreach (var inf in nowVisible)
            unexploredNodes.Remove(inf);

        StopCoroutine(nameof(DetectLowExplorationZonesAsync));
        StartCoroutine(DetectLowExplorationZonesAsync());
    }
    #endregion

    #region Detection methods
    private List<InfluenceCluster> ComputeClusters(InfluenceType _type, ETeam _targetTeam, float _minInfluence)
    {
        var result = new List<InfluenceCluster>();
        var visited = new HashSet<InfluenceNode>();

        foreach (var node in map.NodeList)
        {
            if (node is not InfluenceNode inf) continue;
            if (visited.Contains(inf)) continue;

            // if (!fog.WasVisible((int)team, new Vector2(inf.Position.x, inf.Position.z))) continue;

            float influence = inf.GetInfluence(team, _targetTeam, _type);
            if (influence < _minInfluence) continue;

            var clusterNodes = new List<InfluenceNode>();
            var frontier = new Queue<InfluenceNode>();
            frontier.Enqueue(inf);
            visited.Add(inf);

            Vector3 center = Vector3.zero;
            float strength = 0;

            while (frontier.Count > 0)
            {
                var current = frontier.Dequeue();
                clusterNodes.Add(current);

                float val = current.GetInfluence(team, _targetTeam, _type);
                strength += val;
                center += current.Position;

                foreach (var neighbor in current.Neighbours)
                {
                    if (neighbor is not InfluenceNode ni || visited.Contains(ni)) continue;
                    float nVal = ni.GetInfluence(team, _targetTeam, _type);
                    if (nVal >= _minInfluence)
                    {
                        frontier.Enqueue(ni);
                        visited.Add(ni);
                    }
                }
            }

            if (clusterNodes.Count > 0)
            {
                center /= clusterNodes.Count;
                result.Add(new InfluenceCluster
                {
                    Team = _targetTeam,
                    Type = _type,
                    Strength = strength,
                    Position = center
                });
            }
        }

        return result;
    }

    private void DetectEnemyBase()
    {
        foreach (var cluster in clustersByTypeAndTeam[InfluenceType.Factory][team.GetOpponent()])
        {
            if (cluster.Strength >= 10f)
            {
                EnemyBasePosition = cluster.Position;
                return;
            }
        }
        EnemyBasePosition = null;
    }

    IEnumerator DetectLowExplorationZonesAsync()
    {
        LowExplorationZones.Clear();

        List<InfluenceNode> unexplored_nodes = new List<InfluenceNode>();

        foreach (Node node in map.NodeList)
        {
            if (node is not InfluenceNode influence_node) continue;

            if (influence_node.Position.y == 0
                && !fog.WasVisible(1 << (int)team, new Vector2(influence_node.Position.x, influence_node.Position.z))
                && IsNavMeshAccessible(influence_node.Position)
                )
            {
                unexplored_nodes.Add(influence_node);
            }
        }

        unexplored_nodes = unexplored_nodes.OrderBy(_ => UnityEngine.Random.value).ToList();

        const float clusterRadius = 10.0f;
        var processed = new HashSet<InfluenceNode>();

        foreach (var node in unexplored_nodes)
        {
            if (processed.Contains(node)) continue;

            List<InfluenceNode> cluster = new List<InfluenceNode>();

            foreach (InfluenceNode other in unexplored_nodes)
            {
                if (!processed.Contains(other) &&
                    (node.Position - other.Position).sqrMagnitude <= clusterRadius * clusterRadius)
                {
                    cluster.Add(other);
                }
            }

            if (cluster.Count > 0 && UnityEngine.Random.value < 0.3f)
            {
                var picked = cluster[UnityEngine.Random.Range(0, cluster.Count)];
                LowExplorationZones.Add(picked.Position);
            }
            else if (cluster.Count > 0)
            {
                Vector3 center = Vector3.zero;
                foreach (var n in cluster)
                    center += n.Position;
                center /= cluster.Count;
                LowExplorationZones.Add(center);
            }

            foreach (var n in cluster)
                processed.Add(n);

            if (LowExplorationZones.Count % 8 == 0)
                yield return null;
        }

        yield break;
    }

    private bool IsNavMeshAccessible(Vector3 pos)
    {
        NavMeshHit hit;
        return NavMesh.SamplePosition(pos, out hit, 1.0f, NavMesh.AllAreas);
    }

    private void FindValidCentersInCluster(List<InfluenceNode> cluster, List<(Vector3, int)> output)
    {
        if (cluster == null || cluster.Count == 0) return;

        Vector3 center = Vector3.zero;
        foreach (var node in cluster)
            center += node.Position;
        center /= cluster.Count;

        // Find the closest node
        var bestNode = cluster[0];
        float bestDistance = (bestNode.Position - center).sqrMagnitude;

        for (int i = 1; i < cluster.Count; i++)
        {
            float dist = (cluster[i].Position - center).sqrMagnitude;
            if (dist < bestDistance)
            {
                bestDistance = dist;
                bestNode = cluster[i];
            }
        }

        if (cluster.Contains(bestNode))
        {
            output.Add((bestNode.Position, cluster.Count));
            return;
        }

        // Divide Cluster
        float minX = float.MaxValue, maxX = float.MinValue;
        float minZ = float.MaxValue, maxZ = float.MinValue;

        foreach (var n in cluster)
        {
            Vector3 pos = n.Position;
            if (pos.x < minX) minX = pos.x;
            if (pos.x > maxX) maxX = pos.x;
            if (pos.z < minZ) minZ = pos.z;
            if (pos.z > maxZ) maxZ = pos.z;
        }

        float dx = maxX - minX;
        float dz = maxZ - minZ;

        if (dx >= dz)
        {
            float mid = (minX + maxX) / 2f;
            List<InfluenceNode> left = new();
            List<InfluenceNode> right = new();

            foreach (var n in cluster)
            {
                if (n.Position.x <= mid)
                    left.Add(n);
                else
                    right.Add(n);
            }

            FindValidCentersInCluster(left, output);
            FindValidCentersInCluster(right, output);
        }
        else
        {
            float mid = (minZ + maxZ) / 2f;
            List<InfluenceNode> top = new();
            List<InfluenceNode> bottom = new();

            foreach (var n in cluster)
            {
                if (n.Position.z <= mid)
                    top.Add(n);
                else
                    bottom.Add(n);
            }

            FindValidCentersInCluster(top, output);
            FindValidCentersInCluster(bottom, output);
        }
    }

    public List<Factory> GetAvailableFactories()
    {
        var ctrl = GameServices.GetControllerByTeam(team);
        return ctrl?.GetFactoryList?.Where(f => f.CanCreateUnit && !f.IsUnderConstruction).ToList() ?? new List<Factory>();
    }
    #endregion

    #region Evaluate Threat
    public float EvaluateThreatAround(Vector3 center, float radius = 10f)
    {
        float threat = 0f;
        foreach (var cluster in clustersByTypeAndTeam[InfluenceType.Military][team.GetOpponent()])
            if (Vector3.Distance(center, cluster.Position) <= radius)
                threat += cluster.Strength;
        return threat;
    }

    public Vector3 GetStrongestEnemyMilitaryClusterInRange(Vector3 center, float radius)
    {
        var clusters = clustersByTypeAndTeam[InfluenceType.Military][team.GetOpponent()];
        InfluenceCluster bestCluster = null;
        float maxStrength = float.MinValue;

        foreach (var cluster in clusters)
        {
            float distSqr = (center - cluster.Position).sqrMagnitude;
            if (distSqr <= radius * radius && cluster.Strength > maxStrength)
            {
                maxStrength = cluster.Strength;
                bestCluster = cluster;
            }
        }

        return bestCluster.Position;
    }

    public float EvaluateDefenseAround(Vector3 center, float radius = 10f)
    {
        float defense = 0f;
        float radiusSqr = radius * radius;

        foreach (var cluster in clustersByTypeAndTeam[InfluenceType.Military][team])
        {
            if ((center - cluster.Position).sqrMagnitude <= radiusSqr)
                defense += cluster.Strength;
        }
        return defense;
    }
    #endregion

    public class InfluenceCluster
    {
        public ETeam Team;
        public InfluenceType Type;
        public float Strength;
        public Vector3 Position;
    }
}