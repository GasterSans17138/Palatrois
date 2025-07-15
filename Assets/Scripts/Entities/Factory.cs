using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Factory : BaseEntity
{
    #region Fields

    [Header("Data")]
    [SerializeField] FactoryDataScriptable FactoryData = null;
    [SerializeField] private bool isMainFactory = false;
    [SerializeField] private UpgradeFactory prefabUpgradeFactory = null;
    private UpgradeFactory upgradeFactory = null;

    GameObject[] UnitPrefabs = null;
    GameObject[] LeaderPrefabs = null;
    GameObject[] FactoryPrefabs = null;
    int RequestedEntityBuildIndex = -1;
    bool RequestedEntityBuildLeader = false;
    Image BuildGaugeImage;
    float CurrentBuildDuration = 0f;
    float EndBuildDate = 0f;
    int SpawnCount = 0;
    /* !! max available unit count in menu is set to 9, available factories count to 3 !! */
    const int MaxAvailableUnits = 9;
    const int MaxAvailableLeaders = 9;
    const int MaxAvailableFactories = 3;


    [Header("Building")]
    [SerializeField]
    int MaxBuildingQueueSize = 5;
    Queue<int> BuildingQueue = new Queue<int>();
    public enum State
    {
        Available = 0,
        UnderConstruction,
        BuildingUnit,
    }

    #endregion

    #region Properties

    public bool IsUpgradable { get { return upgradeFactory != null; } }
    public bool IsMainFactory { get { return isMainFactory; } }
    public State CurrentState { get; private set; }
    public bool IsUnderConstruction { get { return CurrentState == State.UnderConstruction; } }
    public FactoryDataScriptable GetFactoryData { get { return FactoryData; } }
    public int AvailableUnitsCount { get { return Mathf.Min(MaxAvailableUnits, FactoryData.AvailableUnits.Length); } }
    public int AvailableLeadersCount { get { return Mathf.Min(MaxAvailableLeaders, FactoryData.AvailableLeaders.Length); } }
    public int AvailableFactoriesCount { get { return Mathf.Min(MaxAvailableFactories, FactoryData.AvailableFactories.Length); } }
    public Action<Unit> OnUnitBuilt;
    public Action<Factory> OnFactoryBuilt;
    public Action OnBuildCanceled;

    public bool IsBuildingUnit { get { return CurrentState == State.BuildingUnit; } }

    public UpgradeFactory UpgradeFactory { get { return upgradeFactory; } }
    public int UpgradeCost { get { return upgradeFactory.CurrentCost; } }

    public int LeaderMultiplicator { get { return upgradeFactory.LeaderCostMultiplicator; } }

    public bool CanCreateUnit { get { return BuildingQueue.Count < MaxBuildingQueueSize; } }

    #endregion

    #region Methods

    public override int Cost()
    {
        return FactoryData.Cost;
    }

    #region Unity Life Cycle
    protected override void Awake()
    {
        base.Awake();

        if(prefabUpgradeFactory) upgradeFactory = Instantiate(prefabUpgradeFactory);
        BuildGaugeImage = transform.Find("Canvas/BuildProgressImage").GetComponent<Image>();
        if (BuildGaugeImage)
        {
            BuildGaugeImage.fillAmount = 0f;
            BuildGaugeImage.color = GameServices.GetTeamColor(GetTeam());
        }

        if (FactoryData == null)
        {
            Debug.LogWarning("Missing FactoryData in " + gameObject.name);
        }
        HP = FactoryData.MaxHP;
        OnDeadEvent += Factory_OnDead;

        UnitPrefabs = new GameObject[FactoryData.AvailableUnits.Length];
        LeaderPrefabs = new GameObject[FactoryData.AvailableLeaders.Length];
        FactoryPrefabs = new GameObject[FactoryData.AvailableFactories.Length];

        // Load from resources actual Unit prefabs from template data
        for (int i = 0; i < FactoryData.AvailableUnits.Length; i++)
        {
            GameObject templateUnitPrefab = FactoryData.AvailableUnits[i];
            string path = "Prefabs/Units/" + templateUnitPrefab.name + "_" + Team.ToString();
            UnitPrefabs[i] = Resources.Load<GameObject>(path);
            if (UnitPrefabs[i] == null)
                Debug.LogWarning("could not find Unit Prefab at " + path);
        }

        // Load from resources actual Unit prefabs from template data
        for (int i = 0; i < FactoryData.AvailableLeaders.Length; i++)
        {
            GameObject templateUnitPrefab = FactoryData.AvailableLeaders[i];
            string path = "Prefabs/Units/" + templateUnitPrefab.name + "_" + Team.ToString();
            LeaderPrefabs[i] = Resources.Load<GameObject>(path);
            if (LeaderPrefabs[i] == null)
                Debug.LogWarning("could not find Unit Prefab at " + path);
        }

        // Load from resources actual Factory prefabs from template data
        for (int i = 0; i < FactoryData.AvailableFactories.Length; i++)
        {
            GameObject templateFactoryPrefab = FactoryData.AvailableFactories[i];
            string path = "Prefabs/Factories/" + templateFactoryPrefab.name + "_" + Team.ToString();
            FactoryPrefabs[i] = Resources.Load<GameObject>(path);
        }
    }
    protected override void Start()
    {
        base.Start();
        GameServices.GetGameState().IncreaseTeamScore(Team);
        Controller = GameServices.GetControllerByTeam(Team);

        if (prefabUpgradeFactory)
        {
            isMainFactory = true; // :)
        }
    }
    override protected void Update()
    {
        switch (CurrentState)
        {
            case State.Available:
                break;

            case State.UnderConstruction:
                // $$$ TODO : improve construction progress rendering
                if (Time.time > EndBuildDate)
                {
                    CurrentState = State.Available;
                    BuildGaugeImage.fillAmount = 0f;
                }
                else if (BuildGaugeImage)
                    BuildGaugeImage.fillAmount = 1f - (EndBuildDate - Time.time) / FactoryData.BuildDuration;
                break;

            case State.BuildingUnit:
                if (Time.time > EndBuildDate)
                {
                    OnUnitBuilt?.Invoke(BuildUnit());
                    OnUnitBuilt = null; // remove registered methods
                    CurrentState = State.Available;

                    // manage build queue : chain with new unit build if necessary
                    if (BuildingQueue.Count != 0)
                    {
                        int unitIndex = BuildingQueue.Dequeue();
                        StartBuildUnit(unitIndex, unitIndex >= 9);
                    }
                }
                else if (BuildGaugeImage)
                    BuildGaugeImage.fillAmount = 1f - (EndBuildDate - Time.time) / CurrentBuildDuration;
                break;
        }
    }
    #endregion

    #region Upgrade Methods

    /// <summary>
    /// Check if the leader can be created thanks to its index
    /// </summary>
    /// <param name="_index"></param>
    /// <returns></returns>
    public bool CanCreateLeader(int _index)
    {
        return upgradeFactory.CanCreateLeader(_index);

    }

    /// <summary>
    /// Create the leader if possible
    /// </summary>
    /// <param name="_index"></param>
    public void CreateLeader(int _index)
    {
        if (upgradeFactory.CanCreateLeader(_index))
        {
            upgradeFactory.CreateLeader(_index);
        }
    }

    /// <summary>
    /// Destroy the leader in order to create another one when the controller asks it
    /// </summary>
    /// <param name="_index"></param>
    public void DestroyLeader(int _index)
    {
        upgradeFactory.LeaderDead(_index);
    }

    /// <summary>
    /// Get the level of the Factory
    /// </summary>
    /// <returns></returns>
    public int GetLevel()
    {
        if (IsUpgradable)
        {
            return upgradeFactory.CurrentLevel;
        }
        return 0;
    }

    /// <summary>
    /// Upgrade the factory
    /// </summary>
    public void Upgrade()
    {
        if (IsUpgradable && upgradeFactory.CurrentCost <= Controller.TotalBuildPoints)
        {
            Controller.TotalBuildPoints -= upgradeFactory.CurrentCost;
            upgradeFactory.LevelUp();
        }
        else
        {
            Debug.Log("Cannot upgrade");
        }
    }

    public void ForceUpgrade()
    {
       upgradeFactory.LevelUp();
    }

    #endregion

    #region Dead Methods

    /// <summary>
    /// When the factory is destroyed
    /// </summary>
    void Factory_OnDead()
    {
        if (IsMainFactory)
        {
            Debug.Log("Lose conditions");
        }
        if (FactoryData.DeathFXPrefab)
        {
            GameObject fx = Instantiate(FactoryData.DeathFXPrefab, transform);
            fx.transform.parent = null;
        }

        GameServices.GetGameState().DecreaseTeamScore(Team);
        Destroy(gameObject);
    }

    #endregion

    #region IRepairable
    override public bool NeedsRepairing()
    {
        return HP < GetFactoryData.MaxHP;
    }
    override public void Repair(int amount)
    {
        HP = Mathf.Min(HP + amount, GetFactoryData.MaxHP);
        base.Repair(amount);
    }
    override public void FullRepair()
    {
        Repair(GetFactoryData.MaxHP);
    }
    #endregion

    #region Unit building methods

    /// <summary>
    /// Check if the leader index is valid
    /// </summary>
    /// <param name="_leaderIndex"></param>
    /// <returns></returns>
    private bool IsLeaderIndexValid(int _leaderIndex)
    {
        if (_leaderIndex < 0 || _leaderIndex >= LeaderPrefabs.Length)
        {
            Debug.LogWarning("Wrong unitIndex " + _leaderIndex);
            return false;
        }
        return true;
    }

    /// <summary>
    /// Check if the Unit index is valid
    /// </summary>
    /// <param name="unitIndex"></param>
    /// <param name="isLeader"></param>
    /// <returns></returns>
    bool IsUnitIndexValid(int unitIndex, bool isLeader = false)
    {
        if (isLeader)
        {
            IsLeaderIndexValid(unitIndex - 9);
        }
        else
        {
            if (unitIndex < 0 || unitIndex >= UnitPrefabs.Length)
            {
                Debug.LogWarning("Wrong unitIndex " + unitIndex);
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Get the leader data
    /// </summary>
    /// <param name="_leaderIndex"></param>
    /// <returns></returns>
    public UnitDataScriptable GetBuildableLeaderData(int _leaderIndex)
    {
        if (IsLeaderIndexValid(_leaderIndex) == false)
            return null;

        return LeaderPrefabs[_leaderIndex].GetComponent<Unit>().GetUnitData;
    }

    /// <summary>
    /// Get the unit data
    /// </summary>
    /// <param name="unitIndex"></param>
    /// <param name="isLeader"></param>
    /// <returns></returns>
    public UnitDataScriptable GetBuildableUnitData(int unitIndex, bool isLeader = false)
    {
        if (IsUnitIndexValid(unitIndex, isLeader) == false)
            return null;

        return isLeader ? LeaderPrefabs[unitIndex - 9].GetComponent<Unit>().GetUnitData : UnitPrefabs[unitIndex].GetComponent<Unit>().GetUnitData;
    }

    /// <summary>
    /// Get the cost of an unit
    /// </summary>
    /// <param name="unitIndex"></param>
    /// <param name="isLeader"></param>
    /// <returns></returns>
    public int GetUnitCost(int unitIndex, bool isLeader = false)
    {
        UnitDataScriptable data = GetBuildableUnitData(unitIndex, isLeader);
        if (data)
            return data.Cost;

        return 0;
    }

    /// <summary>
    /// Get the queued count of a unit
    /// </summary>
    /// <param name="unitIndex"></param>
    /// <returns></returns>
    public int GetQueuedCount(int unitIndex)
    {
        int counter = 0;
        foreach (int id in BuildingQueue)
        {
            if (id == unitIndex)
                counter++;
        }
        return counter;
    }

    public bool IsUnitInQueue(int _unitIndex)
    {
        foreach(int id in BuildingQueue)
        {
            if(id == _unitIndex) return true;
        }

        return false;
    }

    /// <summary>
    /// Request an unit build
    /// </summary>
    /// <param name="unitMenuIndex"></param>
    /// <returns></returns>
    public bool RequestUnitBuild(int unitMenuIndex, int _goapIndex = 0)
    {
        int cost = GetUnitCost(unitMenuIndex);
        if (Controller.TotalBuildPoints < cost || BuildingQueue.Count >= MaxBuildingQueueSize)
            return false;

        Controller.TotalBuildPoints -= cost;

        StartBuildUnit(unitMenuIndex, false, _goapIndex);

        return true;
    }

    /// <summary>
    /// Request a Leader build
    /// </summary>
    /// <param name="unitMenuIndex"></param>
    /// <returns></returns>
    public bool RequestLeaderBuild(int unitMenuIndex, int _goapIndex = 0)
    {
        Debug.Log("Leader create : " + CanCreateLeader(unitMenuIndex - 9));
        if (!IsUpgradable || !CanCreateLeader(unitMenuIndex - 9)) return false;

        CreateLeader(unitMenuIndex - 9);

        int cost = GetUnitCost(unitMenuIndex, true) * upgradeFactory.LeaderCostMultiplicator;

        if (Controller.TotalBuildPoints < cost || BuildingQueue.Count >= MaxBuildingQueueSize)
            return false;

        Controller.TotalBuildPoints -= cost;
        Debug.Log("StartBuildUnit leader");
        StartBuildUnit(unitMenuIndex, true, _goapIndex);

        return true;
    }

    /// <summary>
    /// Start building an unit
    /// </summary>
    /// <param name="unitMenuIndex"></param>
    /// <param name="isLeader"></param>
    void StartBuildUnit(int unitMenuIndex, bool isLeader = false, int _goapIndex = 0)
    {
        if (IsUnitIndexValid(unitMenuIndex, isLeader) == false) 
        { 
            Debug.Log("IsUnitIndexValid");
            return;
        }
        // Factory is being constucted
        if (CurrentState == State.UnderConstruction)
        {
            Debug.Log("Factory is being constucted");
            return;
        }

        // Build queue
        if (CurrentState == State.BuildingUnit)
        {
            if (BuildingQueue.Count < MaxBuildingQueueSize)
                BuildingQueue.Enqueue(unitMenuIndex);
            Debug.Log("Build queue");
            return;
        }

        CurrentBuildDuration = GetBuildableUnitData(unitMenuIndex, isLeader).BuildDuration;
        //Debug.Log("currentBuildDuration " + CurrentBuildDuration);

        CurrentState = State.BuildingUnit;
        EndBuildDate = Time.time + CurrentBuildDuration;

        RequestedEntityBuildIndex = unitMenuIndex;
        RequestedEntityBuildLeader = isLeader;

        int goap_index = _goapIndex;
        Debug.Log("goap_index" + goap_index);
        OnUnitBuilt += (Unit unit) =>
        {
            if (unit != null)
            {
                // Attribute the leader effects
                Debug.Log("Attribute the leader effects");

                unit.OnLeaderAttributed?.Invoke(isLeader);
                unit.OnCurrentLeaderAttributed?.Invoke(false);
                unit.fromFactory = this;
                Controller.AddUnit(unit);

                //_goap.localWorldState.assignedUnits.Add(unit);
                (Controller as AIController)?.AddUnitToGoap(unit, goap_index);
                (Controller as PlayerController)?.UpdateFactoryBuildQueueUI(RequestedEntityBuildIndex);
            }
        };
    }

    /// <summary>
    /// Finally spawn requested unit
    /// </summary>
    /// <returns></returns>
    Unit BuildUnit()
    {
        if (IsUnitIndexValid(RequestedEntityBuildIndex, RequestedEntityBuildLeader) == false)
            return null;

        CurrentState = State.Available;

        GameObject unitPrefab = RequestedEntityBuildLeader ? LeaderPrefabs[RequestedEntityBuildIndex - 9] : UnitPrefabs[RequestedEntityBuildIndex];

        if (BuildGaugeImage)
            BuildGaugeImage.fillAmount = 0f;

        int slotIndex = SpawnCount % FactoryData.NbSpawnSlots;
        // compute simple spawn position around the factory
        float angle = 2f * Mathf.PI / FactoryData.NbSpawnSlots * slotIndex;
        int offsetIndex = Mathf.FloorToInt(SpawnCount / FactoryData.NbSpawnSlots);
        float radius = FactoryData.SpawnRadius + offsetIndex * FactoryData.RadiusOffset;
        Vector3 spawnPos = transform.position + new Vector3(radius * Mathf.Cos(angle), 0f, radius * Mathf.Sin(angle));

        // !! Flying units require a specific layer to be spawned on !!
        bool isFlyingUnit = unitPrefab.GetComponent<Unit>().GetUnitData.IsFlying;
        int layer = isFlyingUnit ? LayerMask.NameToLayer("FlyingZone") : LayerMask.NameToLayer("Floor");

        // cast position on ground
        RaycastHit raycastInfo;
        Ray ray = new Ray(spawnPos, Vector3.down);
        if (Physics.Raycast(ray, out raycastInfo, 10f, 1 << layer))
            spawnPos = raycastInfo.point;

        Transform teamRoot = GameServices.GetControllerByTeam(GetTeam())?.GetTeamRoot();
        GameObject unitInst = Instantiate(unitPrefab, spawnPos, Quaternion.identity, teamRoot);
        unitInst.name = unitInst.name.Replace("(Clone)", "_" + SpawnCount.ToString());
        Unit newUnit = unitInst.GetComponent<Unit>();
        newUnit.Init(GetTeam());
        newUnit.Index = RequestedEntityBuildLeader ? RequestedEntityBuildIndex - 9 : RequestedEntityBuildIndex;

        SpawnCount++;

        // disable build cancelling callback
        OnBuildCanceled = null;

        return newUnit;
    }

    /// <summary>
    /// Check if the unit is a leader
    /// </summary>
    /// <param name="_index"></param>
    /// <returns></returns>
    private bool IsLeaderUnit(int _index)
    {
        return _index >= 9;
    }

    /// <summary>
    /// Cancel the current build
    /// </summary>
    public void CancelCurrentBuild()
    {
        if (CurrentState == State.UnderConstruction || CurrentState == State.Available)
            return;

        CurrentState = State.Available;



        // refund build points

        Controller.TotalBuildPoints += GetUnitCost(RequestedEntityBuildIndex, IsLeaderUnit(RequestedEntityBuildIndex)) * (IsLeaderUnit(RequestedEntityBuildIndex) ? LeaderMultiplicator : 1);
        if (IsLeaderUnit(RequestedEntityBuildIndex))
        {
            DestroyLeader(RequestedEntityBuildIndex - 9);
        }
        foreach (int unitIndex in BuildingQueue)
        {
            if (IsLeaderUnit(unitIndex))
            {
                Controller.TotalBuildPoints += GetUnitCost(unitIndex, true) * LeaderMultiplicator;
                DestroyLeader(unitIndex - 9);
            }
            else
            {
                Controller.TotalBuildPoints += GetUnitCost(unitIndex);
            }
        }
        BuildingQueue.Clear();

        BuildGaugeImage.fillAmount = 0f;
        CurrentBuildDuration = 0f;
        RequestedEntityBuildIndex = -1;
        RequestedEntityBuildLeader = false;

        OnBuildCanceled?.Invoke();
        OnBuildCanceled = null;
    }
    #endregion

    #region Factory building methods
    public GameObject GetFactoryPrefab(int factoryIndex)
    {
        return IsFactoryIndexValid(factoryIndex) ? FactoryPrefabs[factoryIndex] : null;
    }
    bool IsFactoryIndexValid(int factoryIndex)
    {
        if (factoryIndex < 0 || factoryIndex >= FactoryPrefabs.Length)
        {
            Debug.LogWarning("Wrong factoryIndex " + factoryIndex);
            return false;
        }
        return true;
    }
    public FactoryDataScriptable GetBuildableFactoryData(int factoryIndex)
    {
        if (IsFactoryIndexValid(factoryIndex) == false)
            return null;

        return FactoryPrefabs[factoryIndex].GetComponent<Factory>().GetFactoryData;
    }
    public int GetFactoryCost(int factoryIndex)
    {
        FactoryDataScriptable data = GetBuildableFactoryData(factoryIndex);
        if (data)
            return data.Cost;

        return 0;
    }
    public bool CanPositionFactory(int factoryIndex, Vector3 buildPos)
    {
        if (IsFactoryIndexValid(factoryIndex) == false)
            return false;

        if (GameServices.IsPosInPlayableBounds(buildPos) == false)
            return false;

        GameObject factoryPrefab = FactoryPrefabs[factoryIndex];

        Vector3 extent = factoryPrefab.GetComponent<BoxCollider>().size / 2f;

        float overlapYOffset = 0.1f;
        buildPos += Vector3.up * (extent.y + overlapYOffset);

        if (Physics.CheckBox(buildPos, extent))
        //foreach(Collider col in Physics.OverlapBox(buildPos, halfExtent))
        {
            //Debug.Log("Overlap");
            return false;
        }

        return true;
    }
    public Factory StartBuildFactory(int factoryIndex, Vector3 buildPos)
    {
        if (IsFactoryIndexValid(factoryIndex) == false)
            return null;

        if (CurrentState == State.BuildingUnit)
            return null;

        GameObject factoryPrefab = FactoryPrefabs[factoryIndex];
        Transform teamRoot = GameServices.GetControllerByTeam(GetTeam())?.GetTeamRoot();
        GameObject factoryInst = Instantiate(factoryPrefab, buildPos, Quaternion.identity, teamRoot);
        factoryInst.name = factoryInst.name.Replace("(Clone)", "_" + SpawnCount.ToString());
        Factory newFactory = factoryInst.GetComponent<Factory>();
        newFactory.Init(GetTeam());
        newFactory.StartSelfConstruction();

        return newFactory;
    }
    void StartSelfConstruction()
    {
        CurrentState = State.UnderConstruction;

        EndBuildDate = Time.time + FactoryData.BuildDuration;
    }

    #endregion

    #endregion
}
