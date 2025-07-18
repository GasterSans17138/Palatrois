using UnityEngine;

public class EntityVisibility : MonoBehaviour
{
    public ETeam Team;
    public float Range;
    public Vector2 Position => new Vector2(transform.position.x, transform.position.z);

    private bool IsVisibleDefault = true;
    private bool IsVisibleUI = true;

    [SerializeField]
    private GameObject[] GameObjectDefaultLayer;
    [SerializeField]
    private GameObject[] GameObjectUILayer;
    [SerializeField]
    private GameObject[] GameObjectMinimapLayer;
    [SerializeField]
    private LeaderCanvas leaderCanvas;

    private void Start()
    {

    }
    #region Display Methods

    /// <summary>
    /// Show or not the leader squad name on the active leader
    /// </summary>
    /// <param name="_state"></param>
    /// <param name="_squadName"></param>
    public void SetActiveLeader(bool _state, string _squadName = "")
    {
        if(_state)leaderCanvas.SetSquadLeader(_squadName);
        else leaderCanvas.ResetSquadName();
    }

    /// <summary>
    /// Show or not the leader canvas
    /// </summary>
    /// <param name="_visible"></param>
    public void SetVisibleLeader(bool _visible)
    {
        if (leaderCanvas)
        {
            leaderCanvas.ResetSquadName();
            leaderCanvas.gameObject.SetActive(_visible);
        }
    }

    public void SetVisible(bool visible)
	{
        SetVisibleDefault(visible);
        SetVisibleUI(visible);
    }

    public void SetVisibleUI(bool visible)
	{
        if (IsVisibleUI == visible)
            return;

        if (visible)
		{
            IsVisibleUI = true;
            if (GameObjectUILayer.Length > 0)
            {
                SetLayer(GameObjectUILayer[0], LayerMask.NameToLayer("UI"));
            }
        }
        else
        {
            IsVisibleUI = false;
            if (GameObjectUILayer.Length > 0)
            {
                SetLayer(GameObjectUILayer[0], LayerMask.NameToLayer("Hidden"));
            }
        }
    }

    public void SetVisibleDefault(bool visible)
    {
        if (IsVisibleDefault == visible)
            return;

        if (visible)
        {
            IsVisibleDefault = true;
            if (GameObjectDefaultLayer.Length > 0)
            {
                SetLayer(GameObjectDefaultLayer[0], LayerMask.NameToLayer("Default"));
            }
            if (GameObjectMinimapLayer.Length > 0)
            {
                SetLayer(GameObjectMinimapLayer[0], LayerMask.NameToLayer("Minimap"));
            }
        }
        else
        {
            IsVisibleDefault = false;
            if (GameObjectDefaultLayer.Length > 0)
            {
                SetLayer(GameObjectDefaultLayer[0], LayerMask.NameToLayer("Hidden"));
            }
            if (GameObjectMinimapLayer.Length > 0)
            {
                SetLayer(GameObjectMinimapLayer[0], LayerMask.NameToLayer("Hidden"));
            }
        }
    }

    void SetLayer(GameObject root, int newLayer)
    {
        root.layer = newLayer;

        foreach (Transform t in root.transform)
        {
            SetLayer(t.gameObject, newLayer);
        }
    }

    #endregion
}
