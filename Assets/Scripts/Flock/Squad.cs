using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

[Serializable]
public class Squad
{
    #region Fields

    private SquadManager manager;
    [Header("Informations")]
    [SerializeField] private string name = string.Empty;

    [Header("Formation")]
    [SerializeField, ReadOnly(true)] private Formation currentFormation;

    [Header("Units")]
    [SerializeField] private List<Unit> units = new List<Unit>();
    [SerializeField] private Unit leader = null;

    [Header("Valid")]
    [SerializeField] private bool isValid = false;
    [SerializeField] private bool isSelected = false;

    [Header("Data")]
    [SerializeField] private float teamSpead = float.MaxValue; // We take the minimal speed

    #endregion

    #region Constructor / Destructor

    // Default Constructor set isValid to false because an instance of a pur C# class is created by default when it is setted as [Serializable]
    public Squad()
    {
        isValid = false;
    }

    // The Constructor used to create a squad
    public Squad(SquadManager _manager, List<Unit> _units, Formation _formation, string _name)
    {
        isValid = true;
        name = _name;
        manager = _manager;
        CreateSquad(_units, _formation);
    }

    ~Squad()
    {
        manager = null;
        currentFormation = null;
        units.Clear();
        leader = null;
    }

    #endregion

    #region Properties

    public string Name { get { return name; } }

    public List<Unit> Units { get { return units; } }

    public Unit Leader { get { return leader; } }

    public Formation CurrentFormation { get { return currentFormation; } }

    public float TeamSpeed { get { return teamSpead; } }

    public bool IsValid { get { return isValid; } }
    public bool IsSelected { get { return isSelected; } }
    public bool IsSquadUncomplete { get { return (units.Count < 2 || leader == null) && IsValid; } }

    public bool CanAddToSquad { get { return units.Count + 1 < currentFormation.MaxCount; } }

    public float GetInfluence
    {
        get
        {
            float influence = 0f;

            foreach(Unit unit in units)
            {
                influence += unit.influence;
            }

            return influence;
        }
    }

    #endregion

    #region Methods

    #region Conditions

    /// <summary>
    /// Check if the unit is the leader of the squad
    /// </summary>
    /// <param name="_unit"></param>
    /// <returns>The unit is the leader or not</returns>
    public bool IsUnitLeader(Unit _unit)
    {
        return leader == _unit;
    }

    /// <summary>
    /// Check if the unit is in the squad
    /// </summary>
    /// <param name="_unit"></param>
    /// <returns>The unit is in the squad or not</returns>
    public bool IsUnitInSquad(Unit _unit)
    {
        return _unit.Squad == this;
    }

    public static bool HasLeader(List<Unit> _units)
    {
        foreach (Unit unit in _units)
        {
            if (unit.IsLeader) return true;
        }

        return false;
    }

    /// <summary>
    /// Check if a unit list can be a squad
    /// </summary>
    /// <param name="_units"></param>
    /// <returns></returns>
    public static bool CanCreateSquad(List<Unit> _units)
    {
        return _units.Count > 1 && HasLeader(_units);
    }

    /// <summary>
    /// If a unit with the same speed
    /// </summary>
    /// <param name="_unit"></param>
    /// <returns></returns>
    public bool IsCheckSpeedNeeded(Unit _unit)
    {
        return _unit.GetSpeed == teamSpead;
    }

    #endregion

    #region Select

    /// <summary>
    /// Set the squad as selected or not
    /// </summary>
    /// <param name="_state"></param>
    public void SetSelected(bool _state)
    {
        isSelected = _state;
    }

    /// <summary>
    /// Set the squad as Selected
    /// </summary>
    /// <param name="_state"></param>
    public void Select()
    {
        isSelected = true;
    }

    /// <summary>
    /// Unset the squad as selected
    /// </summary>
    /// <param name="_state"></param>
    public void Unselect()
    {
        isSelected = false;
    }

    /// <summary>
    /// Set the units of the squad as selected or not
    /// </summary>
    /// <param name="_state"></param>
    public void SetSelectedUnits(bool _state)
    {
        foreach (Unit unit in units)
        {
            unit.SetSelected(_state);
        }
    }

    #endregion

    #region Action Squad

    /// <summary>
    /// Create the squad with the units and the formation
    /// </summary>
    /// <param name="_units"></param>
    /// <param name="_formation"></param>
    public void CreateSquad(List<Unit> _units, Formation _formation)
    {
        Select(); // The squad is selected by default

        currentFormation = _formation;

        for (int i = 0; i < _units.Count; i++)
        {
            if (!CanAddToSquad) break;
            AddUnitToSquad(_units[i]);
        }

        Debug.Log($"Squad {Name} created.\nLeader : {leader.name}\nCount : {units.Count}");

        // Place the Leader at the head of the squad
        if (_units.IndexOf(leader) != 0)
        {
            units.Remove(leader);
            units.Insert(0, leader);
        }

        // Change the stats of the units
        foreach (Unit unit in _units)
        {
            unit.ChangeStats();
        }

        // Attribute the offsets to the units
        AttributeOffsets();
    }

    /// <summary>
    /// Add a unit to a squad
    /// </summary>
    /// <param name="_unit"></param>
    /// <param name="_isSingleOne"></param>
    public void AddUnitToSquad(Unit _unit)
    {
        if (IsUnitInSquad(_unit))
        {
            Debug.Log("Already in the squad");
        }
        else
        {
            // Check if cann be added
            if (!CanAddToSquad)
            {
                Debug.Log("Cannot add unit to the squad");
                return;
            }

            // The unit join the squad
            units.Add(_unit);
            _unit.OnSquadJoined?.Invoke(this);

            // Check Leader
            if (leader == null && _unit.IsLeader)
            {
                leader = _unit;
                leader.OnLeaderAttributed?.Invoke(true);
                leader.OnCurrentLeaderAttributed?.Invoke(true);
            }
            else
            {
                _unit.OnCurrentLeaderAttributed?.Invoke(false);
            }

            // Check if speed is lower than the current one
            if (teamSpead > _unit.GetUnitData.Speed) teamSpead = _unit.GetUnitData.Speed;

            Debug.Log("Unit added");
        }
    }

    #region Remove Methods

    /// <summary>
    /// 
    /// </summary>
    /// <param name="_unit"></param>
    public void CompleteRemove(Unit _unit)
    {
        RemoveUnit(_unit);
        AttributeOffsets();
        if (IsCheckSpeedNeeded(_unit)) CheckSpeed();
    }

    /// <summary>
    /// Remove an unit from the squad
    /// </summary>
    /// <param name="_unit"></param>
    public void RemoveUnit(Unit _unit)
    {
        if (IsUnitInSquad(_unit))
        {
            // The unit leaves the squad
            units.Remove(_unit);
            _unit.OnSquadLeaved?.Invoke(this);


            Debug.Log("Unit is removed");

            // Search a new leader if the unit was one
            if (IsUnitLeader(_unit))
            {
                Debug.Log("Unit was leader, a new leader is required");
                leader = null;

                foreach (Unit unit in units)
                {
                    if (unit.IsLeader && leader == null)
                    {
                        Debug.Log("Leader Found!");
                        leader = unit;
                        leader.OnLeaderAttributed?.Invoke(true);
                        leader.OnCurrentLeaderAttributed?.Invoke(true);
                        foreach (Unit unit_stat in units)
                        {
                            unit_stat.ChangeStats();
                        }
                    }
                    else
                    {
                        unit.OnCurrentLeaderAttributed?.Invoke(false);
                    }
                }
            }

            // If the squad is uncomplete, then it would be destroyed
            if (IsSquadUncomplete)
            {
                manager.DestroySquad(this);
                return;
            }
        }
        else
        {
            Debug.Log("Unit not in squad");
            return;
        }
    }

    /// <summary>
    /// Clear the entire squad
    /// </summary>
    public void Clear()
    {
        foreach (Unit _unit in units)
        {
            _unit.OnSquadLeaved?.Invoke(this);
        }

        units.Clear();
        leader = null;
    }

    #endregion

    #region Formation Methods

    /// <summary>
    /// Switch the formation
    /// </summary>
    /// <param name="_formation"></param>
    public void SwitchFormation(Formation _formation)
    {
        if (currentFormation != _formation)
        {
            Debug.Log("New formation!");
            currentFormation = _formation;

            AttributeOffsets();
        }
        else
        {
            Debug.Log("Current Formation.");
        }
    }

    #endregion

    #endregion

    #region Targetting

    /// <summary>
    /// Targetting Task - move
    /// </summary>
    /// <param name="_target">Target setted</param>
    public void MoveToTarget(Vector3 _target)
    {
        foreach (Unit unit in units)
        {
            unit.SetTargetPos(_target, true);
        }
    }

    /// <summary>
    /// Targetting Task - attack
    /// </summary>
    /// <param name="_target">Target setted</param>
    public void AttackTarget(BaseEntity _target)
    {
        foreach (Unit unit in units)
        {
            unit.SetAttackTarget(_target, true);
        }
    }

    /// <summary>
    /// Targetting Task - capture
    /// </summary>
    /// <param name="_target">Target setted</param>
    public void CaptureTarget(TargetBuilding _target)
    {
        foreach (Unit unit in units)
        {
            unit.SetCaptureTarget(_target, true);
        }
    }

    /// <summary>
    /// Targetting Task - repairing
    /// </summary>
    /// <param name="_entity">Target setted</param>
    public void RepairTarget(BaseEntity _entity)
    {
        foreach (Unit unit in units)
        {
            unit.SetRepairTarget(_entity, true);
        }
    }

    #endregion

    #region Speed

    /// <summary>
    /// Check if a new speed is available
    /// </summary>
    public void CheckSpeed()
    {
        teamSpead = float.MaxValue;
        for (int i = 0; i < units.Count; i++)
        {

            if (teamSpead > units[i].GetUnitData.Speed) teamSpead = units[i].GetUnitData.Speed;
        }
    }

    #endregion

    #region Offset Methods

    /// <summary>
    /// Attribute the offsets to the units
    /// </summary>
    public void AttributeOffsets()
    {
        if (currentFormation == null)
        {
            Debug.Log("Formation doesn't exist");
            return;
        }

        List<Vector3> offsets = currentFormation.CalculateOffsets(units.Count, 10);

        for (int i = 0; i < units.Count; i++)
        {
            units[i].Offset = offsets[i];
        }
        Debug.Log($"Attribute Offset {Name}");
        foreach (Unit unit in units)
        {
            unit.SetTargetPos(leader.transform.position, true);
        }
    }

    #endregion

    #endregion
}
