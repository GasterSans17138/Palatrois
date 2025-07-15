using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class SquadManager : MonoBehaviour
{
    #region Fields

    #region Squad Status & Name

    Dictionary<string, bool> squad_status = new Dictionary<string, bool>()
{
    { "Alpha", false },
    { "Bravo", false },
    { "Charlie", false },
    { "Delta", false },
    { "Echo", false },
    { "Foxtrot", false },
    { "Golf", false },
    { "Hotel", false },
    { "India", false },
    { "Juliett", false },
    { "Kilo", false },
    { "Lima", false },
    { "Mike", false },
    { "November", false },
    { "Oscar", false },
    { "Papa", false },
    { "Quebec", false },
    { "Romeo", false },
    { "Sierra", false },
    { "Tango", false },
    { "Uniform", false },
    { "Victor", false },
    { "Whiskey", false },
    { "X-ray", false },
    { "Yankee", false },
    { "Zulu", false }
};

    #endregion

    [SerializeField] private List<Squad> squadList = new List<Squad>();
    [SerializeField] private UnitController controller;
    #endregion

    #region Properties  

    public bool HasSquad { get { return squadList.Count > 0; } }

    public int CountSquads { get { return squadList.Count; } }  
    public List<Squad> Squads { get { return squadList; } }
    public List<Squad> SelectedSquads { get { return squadList.Where(s => s.IsSelected).ToList(); } }
    public bool HasSelectedSquads { get { return SelectedSquads.Count > 0; } }

    #endregion

    #region Methods

    #region Unit Life Cycle

    private void Start()
    {
        controller = GetComponent<UnitController>();
    }

    #endregion

    #region Squad Name & Status Methods
    private string GetFirstFalseKey(Dictionary<string, bool> _dict)
    {
        foreach (var pair in _dict)
        {
            if (!pair.Value)
            {
                return pair.Key;
            }
        }
        return null;
    }

    private void SetKey(string _key, bool _state)
    {
        squad_status[_key] = _state;
    }

    #endregion

    #region Create Squad Methods

    /// <summary>
    /// Create a new squad and add it to the list
    /// </summary>
    /// <param name="_units"></param>
    /// <param name="_formation"></param>
    /// <returns></returns>
    public Squad CreateNewSquad(List<Unit> _units, Formation _formation)
    {
        // Remove the units from their current squad (if it is the case)
        RemoveUnitsFromTheirSquad(_units);

        string name = GetFirstFalseKey(squad_status);
        SetKey(name, true);

        Squad new_squad = new Squad(this, _units, _formation, name);

        squadList.Add(new_squad);

        return new_squad;
    }

    #endregion

    #region Merge Methods

    /// <summary>
    /// Merge the selected squads in the main squad
    /// </summary>
    /// <param name="_motherSquad"></param>
    public void MergeSelectedSquadsInSquad(Squad _motherSquad)
    {
        MergeSquadsInSquad(_motherSquad, SelectedSquads);
    }

    /// <summary>
    /// Merge a list of squad in the main squad
    /// </summary>
    /// <param name="_motherSquad"></param>
    /// <param name="_squads"></param>
    public void MergeSquadsInSquad(Squad _motherSquad, List<Squad> _squads)
    {
        List<Unit> units_squadA = new List<Unit>();
        foreach (Squad squad in _squads)
        {
            if (squad == _motherSquad) continue;
            units_squadA.AddRange(squad.Units);
            DestroySquad(squad);
        }

        foreach (Unit unit in units_squadA)
        {
            _motherSquad.AddUnitToSquad(unit);
            unit.ChangeStats();
        }
        _motherSquad.AttributeOffsets();
        _motherSquad.CheckSpeed();
    }

    #endregion

    #region Conditions Methods

    /// <summary>
    /// Check if the units are all from the same squad
    /// </summary>
    /// <param name="_units"></param>
    /// <returns></returns>
    public static bool AreFromSameSquad(List<Unit> _units)
    {
        Squad squad_to_check = _units[0].Squad;

        if (squad_to_check == null) return false;

        if (squad_to_check.Units.Count != _units.Count) return false;

        for (int i = 1; i < _units.Count; i++)
        {
            if (squad_to_check != _units[i].Squad || _units[i].Squad == null)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Check if the unit is in a selected squad
    /// </summary>
    /// <param name="_unit"></param>
    /// <returns></returns>
    public bool IsUnitInSelectedSquad(Unit _unit)
    {
        List<Squad> selected_squads = SelectedSquads;
        if (selected_squads.Count == 0) return false;


        foreach (Squad squad in selected_squads)
        {
            if (squad.IsUnitInSquad(_unit)) return true;
        }

        return false;
    }

    /// <summary>
    /// Check if a list has a leader
    /// </summary>
    /// <param name="_units"></param>
    /// <returns></returns>
    public static bool HasLeader(List<Unit> _units)
    {
        foreach (Unit unit in _units)
        {
            if (unit.IsLeader) return true;
        }

        return false;
    }

    #endregion

    #region Selection Methods

    /// <summary>
    /// Compare the number of units in multiple squads to see if they fill them to select them
    /// </summary>
    /// <param name="_units"></param>
    public void SelectSquads(List<Unit> _units)
    {
        int[] count_unit_per_squad = new int[squadList.Count];

        foreach (Unit unit in _units)
        {
            if (unit.IsInSquad)
            {
                count_unit_per_squad[squadList.IndexOf(unit.Squad)]++;
            }
        }

        // Select the completed squads
        for (int i = 0; i < count_unit_per_squad.Length; i++)
        {
            squadList[i].SetSelected(squadList[i].Units.Count == count_unit_per_squad[i]);
        }
    }

    /// <summary>
    /// Set the squad as selected
    /// </summary>
    /// <param name="_squad"></param>
    public void SetSelectedSquad(Squad _squad)
    {
        _squad.Select();
    }

    /// <summary>
    /// Set the squad as selected
    /// </summary>
    /// <param name="_squad"></param>
    public void SetSelectedSquad(int _index)
    {
        squadList[_index].Select();
    }

    public Squad GetSquadByIndex(int _index)
    {
        return squadList[_index];
    }

    #endregion

    #region Remove & Destroy Methods
    /// <summary>
    /// Remove the units from their squads
    /// </summary>
    /// <param name="_units"></param>
    public void RemoveUnitsFromTheirSquad(List<Unit> _units)
    {
        List<Squad> squads_to_clean = new List<Squad>();

        foreach (var unit in _units)
        {
            if (unit.IsInSquad)
            {
                if (!squads_to_clean.Contains(unit.Squad))
                {
                    squads_to_clean.Add(unit.Squad);
                }
                unit.Squad.RemoveUnit(unit);
            }
        }

        // Clean the squad
        foreach(Squad squad_to_clean in squads_to_clean)
        {
            squad_to_clean.AttributeOffsets();
            squad_to_clean.CheckSpeed();
        }
    }

    /// <summary>
    /// Destroy a given squad
    /// </summary>
    /// <param name="_squad"></param>
    public void DestroySquad(Squad _squad)
    {
        SetKey(_squad.Name, false);
        _squad.Clear();
        squadList.Remove(_squad);
    }

    /// <summary>
    /// Destroy the selected squads
    /// </summary>
    public void DestroySelectedSquads()
    {
        if (!HasSelectedSquads) return;

        Debug.Log("Execute order 66");
        foreach (var squad in SelectedSquads)
        {
            DestroySquad(squad);
        }
    }

    #endregion

    #region Sort Methods

    /// <summary>
    /// Sort the squad by selected variable
    /// </summary>
    public void SortSquad()
    {
        squadList.Sort((a, b) => b.IsSelected.CompareTo(a.IsSelected));
    }

    #endregion

    #region Find Methods

    public Squad FindSquadByName(string _name)
    {
        foreach(Squad squad in squadList)
        {
            if(squad.Name == _name) return squad;
        }

        return null;
    }

    #endregion

    #endregion
}
