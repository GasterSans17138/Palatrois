using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "Upgrade_Factory", menuName = "RTS/UpgradeFactory")]
public class UpgradeFactory : ScriptableObject
{
    #region Fields

    [Header("Level")]
    [SerializeField] private int level = 0;
    [SerializeField] private int maxLevel = 9;

    [Header("Cost")]
    [SerializeField] private int initialCost = 100;
    [SerializeField] private int levelMultiplicator = 2;
    [SerializeField] private int leaderCostMultiplicator = 10;

    private int[] count_leader = new int[9];
    private int[] current_leaders = new int[9];

    #endregion

    #region Properties

    public int CurrentCost { get { return initialCost * (int)Mathf.Pow(levelMultiplicator, level); } }

    public bool CanLevelUp { get { return level < maxLevel; } }

    public int CurrentLevel { get { return level; } }

    public int MaxLevel { get { return maxLevel; } }

    public int LeaderCostMultiplicator { get { return leaderCostMultiplicator; } }

    #endregion

    #region Events

    public UnityEvent OnUpdate = new UnityEvent();

    #endregion

    #region Methods

    #region Unity Life Cycle

    /// <summary>
    /// Reset the data
    /// </summary>
    public void Reset()
    {
        level = 0;
        initialCost = 100;
        count_leader = new int[9];
        current_leaders = new int[9];
        OnUpdate.RemoveAllListeners();
    }

    #endregion

    #region Upgrade

    /// <summary>
    /// Level up the factory
    /// </summary>
    public void LevelUp()
    {
        if (!CanLevelUp) return;
        level++;
        Upgrade();
        OnUpdate?.Invoke();
    }

    /// <summary>
    /// Upgrade the factory, add new leaders and increase the number of previous ones
    /// </summary>
    public void Upgrade()
    {
        for (int i = 0; i < level; i++)
        {
            count_leader[i % maxLevel]++;
        }
    }

    #endregion

    #region Leader Methods

    /// <summary>
    /// Check if a leader can be created
    /// </summary>
    /// <param name="_index"></param>
    /// <returns></returns>
    public bool CanCreateLeader(int _index)
    {
        if (_index < 0 || _index >= count_leader.Length) return false;

        return current_leaders[_index] < count_leader[_index];
    }

    /// <summary>
    /// Create the leader
    /// </summary>
    /// <param name="_index"></param>
    public void CreateLeader(int _index)
    {
        current_leaders[_index]++;
        OnUpdate?.Invoke();
    }

    /// <summary>
    /// Remove a leader from the index array
    /// </summary>
    /// <param name="_index"></param>
    public void LeaderDead(int _index)
    {
        current_leaders[_index]--;
        if (current_leaders[_index] < 0) current_leaders[_index] = 0;
        OnUpdate?.Invoke();
    }

    #endregion

    #region String

    /// <summary>
    /// Get the remaining places of a leader
    /// </summary>
    /// <param name="_index"></param>
    /// <returns></returns>
    public string SlotString(int _index)
    {
        return $"{current_leaders[_index]}/{count_leader[_index]}";
    }

    #endregion

    #endregion
}
