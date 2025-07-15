using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using UnityEngine;

// points system for units creation (Ex : light units = 1 pt, medium = 2pts, heavy = 3 pts)
// max points can be increased by capturing TargetBuilding entities
[RequireComponent(typeof(SquadManager))]
public class UnitController : MonoBehaviour
{
    [Header("Team")]
    [SerializeField]
    protected ETeam Team;
    public ETeam GetTeam() { return Team; }

    [Header("Points")]
    [SerializeField]
    protected int StartingBuildPoints = 15;
    [SerializeField, ReadOnly(true)]
    protected int _TotalBuildPoints = 0;

    protected Transform TeamRoot = null;
    public Transform GetTeamRoot() { return TeamRoot; }

    [Header("Units")]
    [SerializeField, ReadOnly(true)]
    protected List<Unit> unitList = new List<Unit>();

    [SerializeField] protected List<Unit> selectedUnitList = new List<Unit>();

    [Header("Factories")]
    [SerializeField, ReadOnly(true)] protected List<Factory> FactoryList = new List<Factory>();
    [SerializeField, ReadOnly(true)] protected Factory SelectedFactory = null;

    [Header("TargetBuildings")]
    [SerializeField] protected List<TargetBuilding> capturedTargets = new List<TargetBuilding>();

    public SquadManager SquadManager;

    private InfluenceMap influenceMap;

    #region Properties

    public int TotalBuildPoints
    {
        get { return _TotalBuildPoints; }
        set
        {
            Debug.Log("TotalBuildPoints updated");
            _TotalBuildPoints = value;
            OnBuildPointsUpdated?.Invoke();
        }
    }

    public int CapturedTargets
    {
        get { return capturedTargets.Count; }
    }

    public List<TargetBuilding> GetTargetBuilding
    {
        get { return capturedTargets; }
    }

    public List<Unit> UnitList
    {
        get => unitList;
        protected set => unitList = value;
    }
    public List<Unit> SelectedUnitList
    {
        get => selectedUnitList;
        protected set => selectedUnitList = value;
    }

    public List<Factory> GetFactoryList { get { return FactoryList; } }

    #endregion

    #region Events

    public Action OnBuildPointsUpdated;
    protected Action OnCaptureTarget;

    #endregion

    #region Cost

    public bool HasEnoughResources(int _cost)
    {
        return TotalBuildPoints >= _cost;
    }

    public bool HasEnoughResources(BaseEntity _baseEntity)
    {
        return TotalBuildPoints >= _baseEntity.Cost();
    }

    #endregion

    #region Unit methods

    #region Selection

    #region Select

    /// <summary>
    /// Select all the units of the controller
    /// </summary>
    public void SelectAllUnits()
    {
        UnselectCurrentFactory();
        SelectedUnitList.Clear();

        SelectedUnitList.AddRange(UnitList);

        foreach (Unit unit in UnitList)
            unit.SetSelected(true);
    }

    /// <summary>
    /// Select all the units of the controller by the id
    /// </summary>
    /// <param name="typeId">Id of the unit</param>
    public void SelectAllUnitsByTypeId(int typeId)
    {
        UnselectCurrentFactory();
        UnselectAllUnits();

        SelectedUnitList = UnitList.FindAll(unit => unit.GetTypeId == typeId);

        foreach (Unit unit in SelectedUnitList)
        {
            unit.SetSelected(true);
        }
    }

    /// <summary>
    /// Select a list of units
    /// </summary>
    /// <param name="_units">The list of units to select</param>
    public void SelectUnitList(List<Unit> _units)
    {
        foreach (Unit unit in _units)
            unit.SetSelected(true);
        SelectedUnitList.AddRange(_units);
    }

    /// <summary>
    /// Select an array of units
    /// </summary>
    /// <param name="_units">The array of units to select</param>
    public void SelectUnitList(Unit[] _units)
    {
        foreach (Unit unit in _units)
            unit.SetSelected(true);
        SelectedUnitList.AddRange(_units);
    }

    /// <summary>
    /// Select a unit
    /// </summary>
    /// <param name="_unit">The unit to select</param>
    /// <param name="_isSingleSelected">Check if the unit is Singled Selected for the leader</param>
    public void SelectUnit(Unit _unit, bool _isSingleSelected = false)
    {
        if (_isSingleSelected && _unit.IsCurrentLeader)
        {
            SquadManager.SetSelectedSquad(_unit.Squad);
            SelectUnitList(_unit.Squad.Units);

            return;
        }

        _unit.SetSelected(true);
        SelectedUnitList.Add(_unit);
    }

    #region Select Squad Methods

    /// <summary>
    /// Select a squad thanks to its name
    /// </summary>
    /// <param name="_index"></param>
    public void SelectSquadByIndex(string _name)
    {
        Squad squad = SquadManager.FindSquadByName(name);    
        if (squad == null)
        {
            Debug.LogWarning("Cannot find a squad, invalid name");
            return;
        }

        SquadManager.SetSelectedSquad(squad);
        SelectedUnitList.AddRange(squad.Units);
    }

    /// <summary>
    /// Select a squad thanks to its index
    /// </summary>
    /// <param name="_index"></param>
    public void SelectSquadByIndex(int _index)
    {
        if(_index >= SquadManager.CountSquads)
        {
            Debug.LogWarning("Cannot select squad, out of range");
            return;
        }

        SquadManager.SetSelectedSquad(_index);
        SelectedUnitList.AddRange(SquadManager.GetSquadByIndex(_index).Units);
    }

    /// <summary>
    /// Select a squad thanks to its ref
    /// </summary>
    /// <param name="_squad"></param>
    public void SelectSquad(Squad _squad)
    {
        SquadManager.SetSelectedSquad(_squad);
        SelectedUnitList.AddRange(_squad.Units);
    }

    #endregion

    #endregion

    #region Unselect

    /// <summary>
    /// Unselect all the units of a controller
    /// </summary>
    public void UnselectAllUnits()
    {
        foreach (Unit unit in SelectedUnitList)
            unit.SetSelected(false);
        SelectedUnitList.Clear();
    }

    /// <summary>
    /// Unselect a list of units of a controller
    /// </summary>
    /// <param name="_units">List to unselect</param>
    public void UnselectUnits(List<Unit> _units)
    {
        foreach (Unit unit in _units)
        {
            UnselectUnit(unit);
        }
    }

    /// <summary>
    /// Unselect an array of units of a controller
    /// </summary>
    /// <param name="_units">Array to unselect</param>
    public void UnselectUnits(Unit[] _units)
    {
        foreach (Unit unit in _units)
        {
            UnselectUnit(unit);
        }
    }

    /// <summary>
    /// Unselct an unit
    /// </summary>
    /// <param name="_unit">Unit to unselect</param>
    public void UnselectUnit(Unit _unit)
    {
        _unit.SetSelected(false);
        SelectedUnitList.Remove(_unit);
    }


    /// <summary>
    /// Unselect the current squads
    /// </summary>
    public void UnselectSelectedSquads()
    {
        UnselectSquads(SquadManager.SelectedSquads);
    }

    /// <summary>
    /// Unselect an entire list of squads
    /// </summary>
    /// <param name="_squads">Squads to unselect</param>
    public void UnselectSquads(List<Squad> _squads)
    {
        foreach (Squad squad in _squads)
        {
            UnselectSquad(squad);
        }
    }

    /// <summary>
    /// Unselect an entire squad
    /// </summary>
    /// <param name="_squad">Squad to unselect</param>
    public void UnselectSquad(Squad _squad)
    {
        _squad.Unselect();
        UnselectUnits(_squad.Units);
    }

    #endregion

    #endregion


    virtual public void AddUnit(Unit unit)
    {
        unit.OnDeadEvent += () =>
        {
            // Unselect the unit
            if (unit.IsSelected)
                SelectedUnitList.Remove(unit);

            // Remove the unit of the list
            influenceMap.RemoveInfluencer(unit);
            UnitList.Remove(unit);
        };
        influenceMap.AddInfluencer(unit);
        UnitList.Add(unit);
    }

    #region Target

    public void CaptureTarget(int _points, TargetBuilding _targetBuilding)
    {
        Debug.Log("CaptureTarget");
        TotalBuildPoints += _points;
        capturedTargets.Add(_targetBuilding);
    }
    public void LoseTarget(int _points, TargetBuilding _targetBuilding)
    {
        TotalBuildPoints -= _points;
        capturedTargets.Remove(_targetBuilding);
    }

    #endregion

    #region Squad Method

    public void ApplyFormationToSelectedUnit(Formation _formation)
    {
        ApplyFormationToSquad(_formation, SelectedUnitList);
    }

    /// <summary>
    /// Apply a new formation to a squad
    /// If the squad isn't created, then create it
    /// </summary>
    /// <param name="_formation"></param>
    public void ApplyFormationToSquad(Formation _formation, List<Unit> _selectedUnit)
    {
        // Not enough unit to be a squad
        if (_selectedUnit.Count < 2)
        {
            Debug.Log("Not enough units to be or create a squad");
            return;
        }

        // From the same squad, so we try to change the formation if it isn't the same one
        if (SquadManager.AreFromSameSquad(_selectedUnit))
        {
            _selectedUnit[0].Squad.SwitchFormation(_formation);
            Debug.Log("The units are from the same squad");
            return;
        }

        // No leader => No squad
        if (!SquadManager.HasLeader(_selectedUnit))
        {
            Debug.Log("Cannot create a squad without a Leader");
            return;
        }

        // Else, we have to create a new squad
        Squad new_squad = SquadManager.CreateNewSquad(_selectedUnit, _formation);
    }

    #region Merge Squads

    /// <summary>
    /// Merge the selected squads in the main squad
    /// </summary>
    /// <param name="_targetSquad"></param>
    public void MergeSquads(Squad _targetSquad)
    {
        SquadManager.MergeSelectedSquadsInSquad(_targetSquad);
    }

    /// <summary>
    /// Merge the list of squads in the main squad
    /// </summary>
    /// <param name="_targetSquad"></param>
    /// <param name="_squadsToMerge"></param>
    public void MergeSquads(Squad _targetSquad, List<Squad> _squadsToMerge)
    {
        SquadManager.MergeSquadsInSquad(_targetSquad, _squadsToMerge);
    }

    #endregion

    /// <summary>
    /// Destroy the selected squads
    /// </summary>
    public void DestroySelectedSquad()
    {
        if (SquadManager.HasSelectedSquads)
        {
            SquadManager.DestroySelectedSquads(); // Destroy the selected squad
            UnselectAllUnits();
        }
        else
        {
            Debug.Log("No Squad Selected");
        }
    }
    
    /// <summary>
    /// Destroy a squad
    /// </summary>
    /// <param name="_squadToDestroy"></param>
    public void DestroySquad(Squad _squadToDestroy)
    {
        UnselectSquad(_squadToDestroy);
        SquadManager.DestroySquad(_squadToDestroy);
        UnselectAllUnits();
    }
    
    #endregion

    #endregion

    #region Factory methods
    void AddFactory(Factory factory)
    {
        if (factory == null)
        {
            Debug.LogWarning("Trying to add null factory");
            return;
        }

        factory.OnDeadEvent += () =>
        {
            TotalBuildPoints += factory.Cost();
            if (factory.IsSelected)
                SelectedFactory = null;

            influenceMap.RemoveInfluencer(factory);
            FactoryList.Remove(factory);
        };
        influenceMap.AddInfluencer(factory);
        FactoryList.Add(factory);
    }
    virtual protected void SelectFactory(Factory factory)
    {
        if (factory == null || factory.IsUnderConstruction)
            return;

        SelectedFactory = factory;
        SelectedFactory.SetSelected(true);
        UnselectAllUnits();
    }
    virtual protected void UnselectCurrentFactory()
    {
        if (SelectedFactory != null)
            SelectedFactory.SetSelected(false);
        SelectedFactory = null;
    }

    protected bool RequestUnitBuild(int unitMenuIndex)
    {
        if (SelectedFactory == null)
            return false;

        return SelectedFactory.RequestUnitBuild(unitMenuIndex);
    }

    protected bool RequestUnitBuildGOAP(int unitMenuIndex, int _goapIndex = 0)
    {
        if (SelectedFactory == null)
            return false;

        return SelectedFactory.RequestUnitBuild(unitMenuIndex, _goapIndex);
    }

    protected bool RequestLeaderBuild(int unitMenuIndex)
    {
        if (SelectedFactory == null)
            return false;
        Debug.Log("Leader Request");
        return SelectedFactory.RequestLeaderBuild(unitMenuIndex);
    }

    protected bool RequestLeaderBuildGOAP(int unitMenuIndex, int _goapIndex = 0)
    {
        if (SelectedFactory == null)
            return false;
        Debug.Log("Leader Request");
        return SelectedFactory.RequestLeaderBuild(unitMenuIndex, _goapIndex);
    }

    protected bool RequestFactoryBuild(int factoryIndex, Vector3 buildPos)
    {
        if (SelectedFactory == null)
            return false;

        int cost = SelectedFactory.GetFactoryCost(factoryIndex);
        if (TotalBuildPoints < cost)
            return false;

        // Check if positon is valid
        if (SelectedFactory.CanPositionFactory(factoryIndex, buildPos) == false)
            return false;

        Factory newFactory = SelectedFactory.StartBuildFactory(factoryIndex, buildPos);
        if (newFactory != null)
        {
            AddFactory(newFactory);
            TotalBuildPoints -= cost;

            return true;
        }
        return false;
    }
    #endregion

    #region MonoBehaviour methods
    virtual protected void Awake()
    {
        UnitList = new List<Unit>();
        FactoryList = new List<Factory>();
        SelectedUnitList = new List<Unit>();
        capturedTargets = new List<TargetBuilding>();
        string rootName = Team.ToString() + "Team";
        TeamRoot = GameObject.Find(rootName)?.transform;
        if (TeamRoot)
            Debug.LogFormat("TeamRoot {0} found !", rootName);
        if (SquadManager == null) SquadManager = GetComponent<SquadManager>();
    }
    virtual protected void Start()
    {
        TotalBuildPoints = StartingBuildPoints;
        influenceMap = InfluenceMap.Instance;
        // get all team factory already in scene
        Factory[] allFactories = FindObjectsOfType<Factory>();
        foreach (Factory factory in allFactories)
        {
            if (factory.GetTeam() == GetTeam())
            {
                AddFactory(factory);
            }
        }

        Debug.Log("found " + FactoryList.Count + " factory for team " + GetTeam().ToString());
    }
    virtual protected void Update()
    {

    }
    #endregion
}
