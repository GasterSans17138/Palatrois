using System.Collections.Generic;
using UnityEngine;


// $$$ TO DO :)

public sealed class AIController : UnitController
{
    [SerializeField] private float howManyGoapToInstantiate = 1f;
    public GOAP GOAPToInstantiate;
    [SerializeField] private UtilitySystem utilitySystem;

    [SerializeField] private List<Transform> AvailableSpots;

    #region Build Factory

    /// <summary>
    /// Find a Factory position
    /// </summary>
    /// <param name="_startPosition"></param>
    /// <returns></returns>
    private Vector3? FloorFactoryPosition(Transform _startPosition)
    {
        Vector3? floor_pos = new Vector3?();

        int floorMask = 1 << LayerMask.NameToLayer("Floor");
        if (Physics.Raycast(_startPosition.position, Vector3.down, out RaycastHit hit, Mathf.Infinity, floorMask))
        {
            floor_pos = hit.point;
        }

        return floor_pos;
    }

    #endregion

    #region Actions

    public void AttackTarget(BaseEntity _baseEntityTarget)
    {
        // Direct call to attacking task $$$ to be improved by AI behaviour
        foreach (Unit unit in SelectedUnitList)
            unit.SetAttackTarget(_baseEntityTarget, unit.IsInSelectedSquad);
    }

    public void RepairTarget(BaseEntity _baseEntity)
    {
        // Direct call to reparing task $$$ to be improved by AI behaviour
        foreach (Unit unit in SelectedUnitList)
            unit.SetRepairTarget(_baseEntity, unit.IsInSelectedSquad);
    }

    public void CaptureTarget(Vector3 _targetPosition)
    {
        TargetBuilding target = GameServices.GetClosestTargetBuilding(_targetPosition);
        // Direct call to capturing task $$$ to be improved by AI behaviour
        foreach (Unit unit in SelectedUnitList)
            unit.SetCaptureTargetAI(target, unit.IsInSelectedSquad);
    }

    public void CaptureTargetGOAP(Vector3 _targetPosition, List<Unit> _units)
    {
        TargetBuilding target = GameServices.GetClosestTargetBuilding(_targetPosition);
        // Direct call to capturing task $$$ to be improved by AI behaviour
        foreach (Unit unit in _units)
            unit.SetCaptureTargetAI(target, true);
    }

    public void CaptureTarget(TargetBuilding _targetBuilding)
    {
        // Direct call to capturing task $$$ to be improved by AI behaviour
        foreach (Unit unit in SelectedUnitList)
            unit.SetCaptureTarget(_targetBuilding, unit.IsInSelectedSquad);
    }

    public void MoveTo(Vector3 _position)
    {
        foreach (Unit unit in SelectedUnitList)
        {
            unit.positionToMove = _position;
            unit.SetTargetPos(unit.positionToMove, unit.IsInSelectedSquad);
        }
    }

    #endregion

    #region Useful Conditions GOAP

    /// <summary>
    /// Check if there ie a leader available in all the units
    /// </summary>
    /// <returns></returns>
    public bool HasAvailableLeader()
    {
        foreach (Unit unit in unitList)
        {
            if (unit.IsLeader && !unit.IsInSquad) return true;
        }
        return false;
    }

    /// <summary>
    /// Check if there is a leader in the units
    /// </summary>
    /// <param name="_units"></param>
    /// <returns></returns>
    public bool HasLeaderInUnits(List<Unit> _units)
    {
        return SquadManager.HasLeader(_units);
    }

    /// <summary>
    /// Check if there is enough units to create a squad
    /// </summary>
    /// <param name="_units"></param>
    /// <returns></returns>
    public bool HasEnoughUnitsToCreateSquad(List<Unit> _units)
    {
        return _units.Count > 1; // 2 units Required at least
    }

    /// <summary>
    /// Check if there is enough units to create a squad in all the units
    /// </summary>
    /// <param name="_units"></param>
    /// <returns></returns>
    public bool HasEnoughUnitsToCreateSquadInAllUnits()
    {
        int count = 0;
        foreach (Unit unit in unitList)
        {
            if (!unit.IsInSquad) count++;

            if (count > 1) return true;
        }
        return false;
    }

    #region Create Conditions

    /// <summary>
    /// Check if the selected factory can create unit
    /// </summary>
    /// <param name="_factory"></param>
    /// <returns></returns>
    public bool CanCreateUnitInMainFactory()
    {
        return CanCreateUnit(GetMainFactory());
    }

    /// <summary>
    /// Check if the selected factory can create unit
    /// </summary>
    /// <param name="_factory"></param>
    /// <returns></returns>
    public bool CanCreateUnitInSelectedFactory()
    {
        return CanCreateUnit(SelectedFactory);
    }

    /// <summary>
    /// Check if the factory can create unit
    /// </summary>
    /// <param name="_factory"></param>
    /// <returns></returns>
    public bool CanCreateUnit(Factory _factory)
    {
        if (_factory)
        {
            Debug.Log($"{_factory.name} can create unit : {_factory.CanCreateUnit}");
            return _factory.CanCreateUnit;
        }

        Debug.Log("No Factory");
        return false;
    }

    /// <summary>
    /// Check if a Factory can create leaders
    /// </summary>
    /// <param name="_factory"></param>
    /// <returns></returns>
    public bool CanCreateLeader(Factory _factory)
    {
        return CanCreateUnit(_factory) && _factory.IsMainFactory;
    }

    #endregion

    #region Unit Build Conditions

    /// <summary>
    /// Check if an unit is in a building queue
    /// </summary>
    /// <param name="_index"></param>
    /// <returns></returns>
    public bool IsUnitInBuildingQueue(int _index)
    {
        if (FactoryList.Count == 0) return false;

        foreach (Factory _factory in FactoryList)
        {
            if (_factory.IsUnitInQueue(_index)) return true;
        }

        return false;
    }

    /// <summary>
    /// Check if there is a available spot remaining to create a factory
    /// </summary>
    /// <returns></returns>
    public bool CanBuildFactory()
    {
        return AvailableSpots.Count > 0;
    }

    #endregion

    #endregion

    #region Useful Actions GOAP

    #region Add to GOAP

    public void AddUnitToGoap(Unit _unit, int _index)
    {
        if(_index >= utilitySystem.goapAgents.Count)
        {
            Debug.Log("Goap index false");
            return;
        }
        utilitySystem.goapAgents[_index].assignment.assignedUnits.Add(_unit);
    }

    #endregion

    #region Create Actions

    /// <summary>
    /// Create a squad with units and a formation
    /// </summary>
    /// <param name="_formation"></param>
    /// <param name="_selectedUnit"></param>
    public void CreateSquad(Formation _formation, List<Unit> _selectedUnit)
    {
        Squad new_squad = SquadManager.CreateNewSquad(_selectedUnit, _formation);
    }

    /// <summary>
    /// Create a leader thanks to an index
    /// </summary>
    /// <param name="_index"></param>
    public void CreateLeader(int _index = 0)
    {
        SelectedFactory = GetMainFactory();
        if (_index < 9) _index += 9;

        RequestLeaderBuild(_index);
    }

    public void CreateLeaderGOAP(int _index = 0, int _goapIndex = 0)
    {
        SelectedFactory = GetMainFactory();
        if (_index < 9) _index += 9;

        RequestLeaderBuildGOAP(_index, _goapIndex);
    }

    /// <summary>
    /// Create a leader thanks to an index
    /// </summary>
    /// <param name="_index"></param>
    public void CreateUnit(Factory _factory, int _index = 0)
    {
        SelectedFactory = _factory;
        RequestUnitBuild(_index);
    }

    public void CreateUnitGOAP(Factory _factory, int _index = 0, int _goapIndex = 0)
    {
        SelectedFactory = _factory;
        RequestUnitBuildGOAP(_index, _goapIndex);
    }

    public void CreateFactory(int _index = 0)
    {
        SelectedFactory = GetMainFactory();
        Vector3? pos = FloorFactoryPosition(AvailableSpots[0]);
        AvailableSpots.RemoveAt(0);

        if (pos == null)
        {
            Debug.Log("No position found");
            return;
        }

        if (RequestFactoryBuild(_index, (Vector3)pos))
        {
            return;
        }


        return;
    }

    #endregion

    #region Apply Actions

    /// <summary>
    /// Apply a new formation to the squad
    /// </summary>
    /// <param name="_formation"></param>
    /// <param name="_squad"></param>
    public void ApplyFormationToSquad(Formation _formation, Squad _squad)
    {
        _squad.SwitchFormation(_formation);
    }

    #endregion

    #region Getter Actions

    /// <summary>
    /// Get the main Factory
    /// </summary>
    /// <returns></returns>
    public Factory GetMainFactory()
    {
        foreach (Factory factory in FactoryList)
        {
            if (factory.IsMainFactory)
            {
                Debug.Log("Main Factory Found");
                return factory;
            }
        }

        Debug.LogWarning("Main Factory not Found");
        return null;
    }

    /// <summary>
    /// Get the first available Leader
    /// </summary>
    /// <returns></returns>
    public Unit GetFirstAvailableLeader()
    {
        foreach (Unit unit in unitList)
        {
            if (unit.IsLeader && !unit.IsInSquad) return unit;
        }

        Debug.LogWarning("Leader not Found");
        return null;
    }

    #endregion

    #region Setter Actions

    public void SetMainFactoryAsSelected()
    {
        if (FactoryList.Count > 0)
        {
            SelectedFactory = GetMainFactory();
            if (SelectedFactory != null) Debug.Log("Main factory setted");
            else Debug.Log("No main factory setted");
        }
        else
        {
            Debug.Log("No main Factory");
        }

    }

    #endregion

    #endregion

    #region MonoBehaviour methods

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();

        for (int i = 0; i < howManyGoapToInstantiate; i++)
        {
            GOAP goap = Instantiate(GOAPToInstantiate);
            goap.team = utilitySystem.worldState.team;
            goap.globalState = utilitySystem.worldState;
            goap.controller = this;
            utilitySystem.goapAgents.Add(goap);
        }

        for (int i = 0; i < 9; i++)
        {
            FactoryList[0].ForceUpgrade();
        }
    }

    protected override void Update()
    {
        base.Update();

        if (Input.GetKeyUp(KeyCode.B))
        {
            //FactoryList[0].RequestUnitBuild(0);
            SelectFactory(FactoryList[0]);
            RequestUnitBuild(0);
        }

        //Debug.Log("UnitList.Count = " + UnitList.Count);
    }

    #endregion
}
