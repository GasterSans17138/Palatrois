using System;
using UnityEngine;
using UnityEngine.UI;

public enum DropOff
{
    CONSTANT,
    LINEAR,
    SQRT,
    POW,
    CUSTOM
}

public enum RadiusType
{
    CONSTANT,
    THRESHOLD
}


public abstract class BaseEntity : MonoBehaviour, ISelectable, IDamageable, IRepairable, IInfluencer
{
    #region Fields

    [Header("Team")]
    [SerializeField] public ETeam Team;
    protected UnitController controller = null;
    protected EntityVisibility _Visibility;

    [Header("HP")]
    protected float HP_Percent = 1f;
    protected int HP = 0;
    protected Action OnHpUpdated;
    protected GameObject SelectedSprite = null;
    protected Text HPText = null;

    [Header("State")]
    protected bool IsInitialized = false;
    protected int index = 0;

    [Header("Minimap")]
    protected UnityEngine.UI.Image MinimapImage;

    [Header("Influence")]
    public int radius = 5;
    public float influence = 1f;
    public DropOff dropOff;
    public RadiusType radiusType;
    public float influenceThreshold = 2f;
    public ETeam teamForInfluence => GetTeam();
    public InfluenceType influenceType => InfluenceType.Military;

    #endregion

    #region Properties

    public UnitController Controller { get { return controller; } protected set { controller = value; } }

    public EntityVisibility Visibility
    {
        get
        {
            if (_Visibility == null)
            {
                _Visibility = GetComponent<EntityVisibility>();
            }
            return _Visibility;
        }
    }
    public int Index { get { return index; } set { index = value; } }

    public Action OnDeadEvent;
    public bool IsSelected { get; protected set; }
    public bool IsAlive { get; protected set; }

    public Vector3 positionForInfluence => transform.position;

    #endregion

    #region Methods

    public abstract int Cost();

    #region Team Methods

    virtual public void Init(ETeam _team)
    {
        if (IsInitialized)
            return;

        Team = _team;

        if (Visibility) { Visibility.Team = _team; }

        Transform minimapTransform = transform.Find("MinimapCanvas");
        if (minimapTransform != null)
        {
            MinimapImage = minimapTransform.GetComponentInChildren<UnityEngine.UI.Image>();
            MinimapImage.color = GameServices.GetTeamColor(Team);
        }

        IsInitialized = true;
    }
    public Color GetColor()
    {
        return GameServices.GetTeamColor(GetTeam());
    }

    #endregion

    #region ISelectable

    virtual public void SetSelected(bool selected)
    {
        IsSelected = selected;
        SelectedSprite?.SetActive(IsSelected);
    }
    public ETeam GetTeam()
    {
        return Team;
    }

    #endregion

    #region IDamageable

    public virtual void AddDamage(int damageAmount)
    {
        if (IsAlive == false)
            return;

        HP -= damageAmount;

        OnHpUpdated?.Invoke();

        if (HP <= 0)
        {
            IsAlive = false;
            OnDeadEvent?.Invoke();
            Debug.Log("Entity " + gameObject.name + " died");
        }
    }

    public void Destroy()
    {
        AddDamage(HP);
    }
    #endregion

    #region IRepairable
    virtual public bool NeedsRepairing()
    {
        return true;
    }
    virtual public void Repair(int amount)
    {
        OnHpUpdated?.Invoke();
    }
    virtual public void FullRepair()
    {
    }

    #endregion

    #region IInfluencer
    public virtual float GetDropOff(int _locationDistance)
    {
        float i = dropOff switch
        {
            DropOff.CONSTANT => influence,
            DropOff.LINEAR => influence / (1 + _locationDistance),
            DropOff.SQRT => influence / Mathf.Sqrt(1 + _locationDistance),
            DropOff.POW => influence / ((1 + _locationDistance) * (1 + _locationDistance)),
            DropOff.CUSTOM => influence - (influence / GetRadius() * _locationDistance),
            _ => throw new System.NotImplementedException()
        };

        return i;
    }

    public virtual float GetRadius()
    {
        float r = radiusType switch
        {
            RadiusType.CONSTANT => radius,
            // method taken from Ian Millington's book Artificial Intelligence for Games, but doesn't seem to work properly
            RadiusType.THRESHOLD => influenceThreshold == 1f ? 0f : influence / (influenceThreshold - 1), // NOT WORKING
            _ => throw new System.NotImplementedException()
        };

        return r;
    }
    #endregion

    #region UI 

    protected void UpdateHpUI()
    {
        if (HPText != null)
            HPText.text = "HP : " + HP.ToString();
    }

    #endregion

    #region Unity Life Cycle

    virtual protected void Awake()
    {
        IsAlive = true;

        SelectedSprite = transform.Find("SelectedSprite")?.gameObject;
        SelectedSprite?.SetActive(false);

        Transform hpTransform = transform.Find("Canvas/HPText");
        if (hpTransform)
            HPText = hpTransform.GetComponent<Text>();

        OnHpUpdated += UpdateHpUI;
    }
    virtual protected void Start()
    {
        Init(GetTeam());
        UpdateHpUI();
    }
    virtual protected void Update()
    {
    }

    #endregion

    #endregion
}
