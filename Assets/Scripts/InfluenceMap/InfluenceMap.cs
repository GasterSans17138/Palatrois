using System;
using System.Collections.Generic;
using UnityEngine;


public class InfluenceMap : Graph
{
    static InfluenceMap _Instance = null;
    static public InfluenceMap Instance
    {
        get
        {
            if (_Instance == null)
                _Instance = FindObjectOfType<InfluenceMap>();
            return _Instance;
        }
    }

    private struct ObserverInfluence
    {
        public ETeam observer;
        public IInfluencer influencer;

        public ObserverInfluence(ETeam _observer, IInfluencer _influencer)
        {
            this.observer = _observer;
            this.influencer = _influencer;
        }
        public bool IsObserver(ETeam _eTeam)
        {
            return observer == _eTeam;
        }
    }

    private struct LastSeenInfo
    {
        public Vector3 Position;
        public bool IsVisibleNow;
        public bool HasBeenSeen;
    }

    [SerializeField]
    InfluenceType influenceTypeToDisplay = InfluenceType.Military;

    [SerializeField]
    private ETeam teamToDisplay = ETeam.Red;

    [SerializeField]
    public FogOfWarSystem FOWSystem;

    public float UpdateFrequency = 0.5f;
    private float LastUpdateTime = float.MinValue;

    private List<IInfluencer> influencerControllerList = new();

    private Dictionary<ObserverInfluence, LastSeenInfo> LastKnownPositions = new();

    private bool IsGraphCreated = false;
    private bool IsInitialized = false;


    #region Init & Update
    private void Awake()
    {
        CreateTiledGrid();
        OnGraphCreated += () => { IsGraphCreated = true; };
    }

    private void Update()
    {
        if (!IsGraphCreated)
            return;

        if (!IsInitialized)
        {
            influencerControllerList.Clear();

            UnitController[] controllers = GameServices.GetControllers();
            for (int i = 0; i < controllers.Length; i++)
            {
                influencerControllerList.AddRange(controllers[i].UnitList);
                influencerControllerList.AddRange(controllers[i].GetFactoryList);
            }
            influencerControllerList.AddRange(GameServices.GetTargetBuildings());

            IsInitialized = true;
        }

        if (Time.time - LastUpdateTime > UpdateFrequency)
        {
            ComputeInfluence();
            LastUpdateTime = Time.time;
        }
    }

    protected override Node CreateNode()
    {
        return new InfluenceNode();
    }
    #endregion      

    public void AddInfluencer(IInfluencer _influencer)
    {
        influencerControllerList.Add(_influencer);
    }

    public void RemoveInfluencer(IInfluencer _influencer)
    {
        influencerControllerList.Remove(_influencer);
    }

    #region Influence Map
    public void ComputeInfluence()
    {
        foreach (InfluenceNode node in NodeList)
            node.ResetInfluence();
        foreach (ETeam observer in Enum.GetValues(typeof(ETeam)))
        {
            if (observer == ETeam.Neutral) continue;
            foreach (IInfluencer influencer in influencerControllerList)
            {
                ModifyInfluencer(influencer, observer);
            }
            ClearMemoryOnVisibleNodes(observer);
        }
    }

    private void ModifyInfluencer(IInfluencer _influencer, ETeam _observer)
    {
        ObserverInfluence observer_influence = new ObserverInfluence(_observer, _influencer);
        LastSeenInfo memory;
        LastKnownPositions.TryGetValue(observer_influence, out memory);

        bool is_visible = IsVisibleNow(_influencer, _observer);
        if (!is_visible && !memory.HasBeenSeen) return;
        else if (is_visible)
        {
            memory.Position = _influencer.positionForInfluence;
            memory.IsVisibleNow = true;
            memory.HasBeenSeen = true;

            SpreadInfluence(_influencer, _observer);
        }
        else
        {
            memory.IsVisibleNow = false;
            SpreadInfluenceAt(_influencer, _observer, memory.Position);
        }
        LastKnownPositions[observer_influence] = memory;
    }

    private void SpreadInfluence(IInfluencer source, ETeam observer)
    {
        var pending = new List<InfluenceNode>();
        var visited = new HashSet<InfluenceNode>();
        pending.Add(GetNode(source.positionForInfluence) as InfluenceNode);

        for (int i = 1; i <= source.GetRadius(); i++)
        {
            var frontier = new List<InfluenceNode>();
            foreach (InfluenceNode node in pending)
            {
                if (node == null || visited.Contains(node)) continue;
                visited.Add(node);

                node.SetInfluenceForTeam(observer, source.teamForInfluence, source.influenceType, source.GetDropOff(i));
                foreach (Node n in node.Neighbours)
                    if (n is InfluenceNode infNode)
                        frontier.Add(infNode);
            }
            pending = frontier;
        }
    }

    private void SpreadInfluenceAt(IInfluencer source, ETeam observer, Vector3 atPosition)
    {
        var pending = new List<InfluenceNode>();
        var visited = new HashSet<InfluenceNode>();
        pending.Add(GetNode(atPosition) as InfluenceNode);

        for (int i = 1; i <= source.GetRadius(); i++)
        {
            var frontier = new List<InfluenceNode>();
            foreach (InfluenceNode node in pending)
            {
                if (node == null || visited.Contains(node)) continue;
                visited.Add(node);

                node.SetInfluenceForTeam(observer, source.teamForInfluence, source.influenceType, source.GetDropOff(i));

                foreach (Node n in node.Neighbours)
                    if (n is InfluenceNode infNode)
                        frontier.Add(infNode);
            }
            pending = frontier;
        }
    }

    private void ClearMemoryOnVisibleNodes(ETeam team)
    {
        var toClear = new List<ObserverInfluence>();

        foreach (var pair in LastKnownPositions)
        {
            if (!pair.Key.IsObserver(team)) continue;
            var info = pair.Value;

            // Ne purge que les entités non visibles mais déjà vues
            if (!info.IsVisibleNow && info.HasBeenSeen)
            {
                Vector2 lastSeen2D = new Vector2(info.Position.x, info.Position.z);
                int teamMask = 1 << (int)team;

                // Si la dernière position est à nouveau visible
                if (FOWSystem.IsVisible(teamMask, lastSeen2D))
                {
                    bool stillExists = influencerControllerList.Exists(inf =>
                        inf.positionForInfluence == info.Position &&
                        inf.teamForInfluence == pair.Key.influencer.teamForInfluence &&
                        inf.influenceType == pair.Key.influencer.influenceType);

                    if (!stillExists)
                        toClear.Add(pair.Key);
                }
            }
        }

        foreach (var key in toClear)
        {
            LastKnownPositions.Remove(key);
        }
    }
    #endregion

    #region Fog Of War Visibility
    private bool IsVisibleNow(IInfluencer influencer, ETeam team)
    {
        if (FOWSystem == null || influencer == null) return false;

        if (influencer is MonoBehaviour mono)
        {
            var visibility = mono.GetComponent<EntityVisibility>();
            if (visibility == null) return false;

            int teamMask = 1 << (int)team;
            Vector2 pos = visibility.Position;

            return FOWSystem.IsVisible(teamMask, pos);
        }

        return false;
    }

    private bool WasVisibleBefore(IInfluencer influencer, ETeam team)
    {
        if (FOWSystem == null || influencer == null) return false;

        if (influencer is MonoBehaviour mono)
        {
            var visibility = mono.GetComponent<EntityVisibility>();
            if (visibility == null) return false;

            int teamMask = 1 << (int)team;
            Vector2 pos = visibility.Position;

            return FOWSystem.WasVisible(teamMask, pos);
        }

        return false;
    }

    private bool WasVisibleToTeam(IInfluencer influencer, ETeam team)
    {
        if (FOWSystem == null || influencer == null) return false;

        if (influencer is MonoBehaviour mono)
        {
            var visibility = mono.GetComponent<EntityVisibility>();
            if (visibility == null) return false;

            int teamMask = 1 << (int)team;
            Vector2 pos = visibility.Position;

            return FOWSystem.WasVisible(teamMask, pos);
        }

        return false;
    }
    #endregion

    #region Gizmos
    protected override void DrawNodesGizmo()
    {
        for (int i = 0; i < NodeList.Length; i++)
        {
            InfluenceNode node = NodeList[i] as InfluenceNode;
            if (node != null)
            {
                ETeam dominant = node.GetDominantTeam(teamToDisplay, influenceTypeToDisplay);
                float alpha = node.GetInfluence(teamToDisplay, dominant, influenceTypeToDisplay);

                Color nodeColor = dominant switch
                {
                    ETeam.Blue => Color.blue,
                    ETeam.Red => Color.red,
                    _ => Color.gray
                };
                nodeColor.a = Mathf.Clamp01(alpha);

                Gizmos.color = nodeColor;
                Gizmos.DrawCube(node.Position, Vector3.one * SquareSize * 0.95f);
            }
        }
    }
    #endregion
}
