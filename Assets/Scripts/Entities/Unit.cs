using System.ComponentModel;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class Unit : BaseEntity
{
    #region Fields

    Transform bulletSlot;
    float lastActionDate = 0f;
    public bool isNotLinkedToGoap = true;

    public Factory fromFactory = null;

    #region Target

    //[HideInInspector]
    public BaseEntity entityTarget = null;
    //[HideInInspector]
    public TargetBuilding captureTarget = null;

    #endregion

    #region Unit

    [Header("Data")]
    [SerializeField] private UnitDataScriptable unitData = null;

    [HideInInspector] public NavMeshAgent navMeshAgent;
    [HideInInspector] public UnitDetection unitDetection;
    [HideInInspector] public Vector3 positionToMove = Vector3.zero;

    #endregion

    #region States

    [SerializeField] private bool canStopToShootWhileMoving = false;
    [HideInInspector] public bool isMoving = false;
    [HideInInspector] public bool movingToCapture = false;
    [HideInInspector] public bool movingToRepair = false;

    #endregion

    #region FSM

    [Header("FSM States")]
    public MoveState moveState;
    public AttackState attackState;
    public RepairState repairState;
    public CaptureState captureState;

    [HideInInspector]
    public UnitFSM fsm;

    #endregion

    #region Squad
    [Header("Squad")]
    [SerializeField, ReadOnly(true)]
    private bool isLeader = false;

    [SerializeField, ReadOnly(true)]
    private bool isLeaderActive = false;

    [SerializeField, ReadOnly(true)]
    private Squad squad = null;

    [SerializeField, ReadOnly(true)]
    private Vector3 offset = Vector3.zero;


    #endregion

    #endregion

    #region Properties

    #region States


    public bool CanStopToShootWhileMoving
    {
        get => canStopToShootWhileMoving;
        set => canStopToShootWhileMoving = value;
    }

    #endregion

    #region Squad
    public Squad Squad { get { return squad; } set { squad = value; } }
    public bool IsInSquad { get { return squad != null && squad.IsValid; } }

    public bool IsInSelectedSquad { get { return squad != null && squad.IsValid && squad.IsSelected; } }

    #region Leader

    public bool IsLeader
    {
        get
        {
            return isLeader;
        }
    }

    public bool IsLeaderActive { get { return isLeaderActive; } }

    public bool IsCurrentLeader { get { return isLeader && isLeaderActive && IsInSquad; } }

    #endregion

    public Vector3 Offset { get { return offset; } set { offset = value; } }

    #endregion

    #region Data & Bonuses

    public UnitDataScriptable GetUnitData { get { return unitData; } }
    public int GetTypeId { get { return unitData.TypeId; } }

    // Get the current speed
    public float GetSpeed
    {
        get
        {
            if (IsInSquad)
            {
                return squad.TeamSpeed + (squad.TeamSpeed * squad.Leader.GetUnitData.SpeedBonus / 100f);
            }
            return unitData.Speed;
        }
    }

    // Get the current damages per second
    public float GetDPS
    {
        get
        {
            if (IsInSquad)
            {
                return unitData.DPS + (unitData.DPS * squad.Leader.GetUnitData.AttackBonus / 100f);
            }
            return unitData.DPS;
        }
    }

    // Get the current range
    public float GetRange
    {
        get
        {
            if (IsInSquad)
            {
                return unitData.AttackDistanceMax + (unitData.AttackDistanceMax * squad.Leader.GetUnitData.RangeBonus / 100f);
            }
            return unitData.AttackDistanceMax;
        }
    }


    // Get the current capture range
    public float GetCaptureRange
    {
        get
        {
            if (IsInSquad)
            {
                return unitData.CaptureDistanceMax + (unitData.CaptureDistanceMax * squad.Leader.GetUnitData.CaptureRangeBonus / 100f);
            }
            return unitData.CaptureDistanceMax;
        }
    }

    // Get the current attack speed
    public float GetAttackSpeed
    {
        get
        {
            if (IsInSquad)
            {
                return unitData.AttackFrequency * squad.Leader.GetUnitData.AttackSpeedBonus / 100f;
            }
            return 0f;
        }
    }

    // Get the current health
    public int GetHealth
    {
        get
        {
            return Mathf.FloorToInt(GetMaxHealth * HP_Percent);
        }
    }

    // Get the current max health
    public int GetMaxHealth
    {
        get
        {
            if (IsInSquad)
            {
                return Mathf.FloorToInt(unitData.MaxHP + (unitData.MaxHP * squad.Leader.GetUnitData.HealthBonus / 100f));
            }
            return unitData.MaxHP;
        }
    }

    // Get the current repair per second
    public int GetRPS
    {
        get
        {
            if (IsInSquad)
            {
                return Mathf.FloorToInt(unitData.RPS + (unitData.RPS * squad.Leader.GetUnitData.RepairBonus / 100f));
            }
            return unitData.RPS;
        }
    }

    #endregion

    #endregion

    #region Events

    public UnityEvent<Squad> OnSquadJoined = new UnityEvent<Squad>();
    public UnityEvent<Squad> OnSquadLeaved = new UnityEvent<Squad>();

    public UnityEvent<bool> OnLeaderAttributed = new UnityEvent<bool>();
    public UnityEvent<bool> OnCurrentLeaderAttributed = new UnityEvent<bool>();

    #endregion

    public override int Cost()
    {
        return unitData.Cost;
    }

    #region Unity Life Cycle

    override protected void Awake()
    {
        base.Awake();

        navMeshAgent = GetComponent<NavMeshAgent>();
        unitDetection = GetComponentInChildren<UnitDetection>();
        bulletSlot = transform.Find("BulletSlot");

        // fill NavMeshAgent parameters
        navMeshAgent.speed = GetSpeed;
        navMeshAgent.angularSpeed = GetUnitData.AngularSpeed;
        navMeshAgent.acceleration = GetUnitData.Acceleration;

        fsm = GetComponent<UnitFSM>();

        PrepareEvents();
    }
    override protected void Start()
    {
        // Needed for non factory spawned units (debug)
        if (!IsInitialized)
            Init(Team);

        base.Start();
    }
    override protected void Update()
    {
    }

    #endregion

    private void PrepareEvents()
    {
        OnSquadJoined?.AddListener(Unit_OnSquadJoined);
        OnSquadLeaved?.AddListener(LeaveSquad);
        OnLeaderAttributed?.AddListener(Unit_OnLeaderAttributed);
        OnCurrentLeaderAttributed?.AddListener(Unit_OnCurrentLeaderAttributed);
    }

    private void ClearEvents()
    {
        OnSquadJoined?.RemoveAllListeners();
        OnSquadLeaved?.RemoveAllListeners();
        OnLeaderAttributed?.RemoveAllListeners();
        OnCurrentLeaderAttributed?.RemoveAllListeners();
    }

    #region Unit State Methods

    public override void SetSelected(bool selected)
    {
        base.SetSelected(selected);
    }

    #endregion

    #region Team

    override public void Init(ETeam _team)
    {
        if (IsInitialized)
            return;

        base.Init(_team);

        HP = GetMaxHealth;
        HP_Percent = HP / GetMaxHealth;
        OnDeadEvent += Unit_OnDead;
    }

    #endregion

    #region Dead

    /// <summary>
    /// On the dead of the unity
    /// </summary>
    void Unit_OnDead()
    {
        // The unit dead was in a squad
        if (IsInSquad)
        {
            squad.CompleteRemove(this);
            // We update the number of leaders creatable
            if (IsLeader)
                fromFactory.DestroyLeader(Index);
        }

        if (IsCapturing())
            StopCapture();

        if (GetUnitData.DeathFXPrefab)
        {
            GameObject fx = Instantiate(GetUnitData.DeathFXPrefab, transform);
            fx.transform.parent = null;
        }

        ClearEvents();
        Destroy(gameObject);
    }

    #endregion

    #region Squad Methods

    /// <summary>
    /// Add a unit to a squad, if it is already in a squad, ask for a force add
    /// </summary>
    /// <param name="_squad"></param>
    /// <param name="_forceSwitch"></param>
    private void Unit_OnSquadJoined(Squad _squad)
    {
        squad = _squad;
    }

    /// <summary>
    /// Set the unit as leader or not
    /// </summary>
    /// <param name="_state"></param>
    private void Unit_OnLeaderAttributed(bool _state)
    {
        isLeader = _state;
        Visibility.SetVisibleLeader(isLeader);
    }

    /// <summary>
    /// Set the leader as active or not
    /// </summary>
    /// <param name="_state"></param>
    private void Unit_OnCurrentLeaderAttributed(bool _state)
    {
        if (!isLeader) return;

        isLeaderActive = _state;
        Visibility.SetActiveLeader(isLeaderActive, squad.Name);
    }

    /// <summary>
    /// If the unit is in a squad and get another order, leave the squad
    /// </summary>
    /// <param name="_isSquadOrder"></param>
    public void CheckSeparatedOrder(bool _isSquadOrder)
    {
        if (IsInSquad && !_isSquadOrder)
        {
            Debug.Log("Separated Order");
            squad.CompleteRemove(this);
        }
    }

    /// <summary>
    /// Leave the squad
    /// </summary>
    private void LeaveSquad(Squad _squad)
    {
        if (IsLeader)
        {
            OnLeaderAttributed?.Invoke(true);
            OnCurrentLeaderAttributed?.Invoke(false);
        }
        UnreferenceSquad();
        ChangeStats();
    }

    /// <summary>
    /// Unreference the squad of the unit
    /// </summary>
    public void UnreferenceSquad()
    {
        squad = null;
        offset = Vector3.zero;
    }

    private void UpdateLeaderUI()
    {
        if (IsLeader)
        {
            Visibility.SetVisibleLeader(true);
            Unit_OnCurrentLeaderAttributed(IsCurrentLeader);
        }
    }

    /// <summary>
    /// Change the stats whether it is in a squad or not
    /// </summary>
    public void ChangeStats()
    {
        HP = Mathf.FloorToInt(GetMaxHealth * HP_Percent);
        UpdateHpUI();
    }

    #endregion

    #region IRepairable
    override public bool NeedsRepairing()
    {
        return HP < GetMaxHealth;
    }
    override public void Repair(int amount)
    {
        HP = Mathf.Min(HP + amount, GetMaxHealth);
        HP_Percent = HP / GetMaxHealth;
        base.Repair(amount);
    }
    override public void FullRepair()
    {
        Repair(GetMaxHealth);
    }
    #endregion

    #region Tasks methods : Moving, Capturing, Targeting, Attacking, Repairing ...

    // $$$ To be updated for AI implementation $$$

    // Moving Task
    public void SetTargetPos(Vector3 pos, bool _isSquadOrder = false)
    {
        CheckSeparatedOrder(_isSquadOrder);

        fsm.ChangeState(moveState);
        if (entityTarget != null)
            entityTarget = null;

        if (captureTarget != null)
            StopCapture();

        if (navMeshAgent)
        {
            navMeshAgent.stoppingDistance = 0.1f;
            navMeshAgent.speed = GetSpeed;
            // If in squad, we don't forget to add the offset (The target will be the place of the leader)
            navMeshAgent.SetDestination(pos + offset); // The Offset is defined when the formation is assigned, otherwise, offset is zero
            navMeshAgent.isStopped = false;
        }
    }

    // Targetting Task - attack
    public void SetAttackTarget(BaseEntity target, bool _isSquadOrder = false)
    {
        CheckSeparatedOrder(_isSquadOrder);

        if (!CanAttack(target))
        {
            positionToMove = target.transform.position;
            fsm.ChangeState(moveState);
            navMeshAgent.stoppingDistance = GetRange - 5; // need a litle offset
            return;
        }

        if (captureTarget != null)
        {
            StopCapture();
        }


        if (target.GetTeam() != GetTeam())
        {
            StartAttacking(target);
        }
    }

    // Targetting Task - capture
    public void SetCaptureTarget(TargetBuilding target, bool _isSquadOrder = false)
    {
        CheckSeparatedOrder(_isSquadOrder);

        if (!CanCapture(target))
        {
            navMeshAgent.stoppingDistance = GetCaptureRange;
            positionToMove = target.transform.position;
            captureTarget = target;
            movingToCapture = true;
            fsm.ChangeState(moveState);
            return;
        }

        if (entityTarget != null)
            entityTarget = null;

        if (IsCapturingTarget(target))
        {
            // Dont Stop
        }
        else
        {
            StopCapture();

            if (target.GetTeam() != GetTeam())
            {
                captureTarget = target;
                fsm.ChangeState(captureState);
            }
        }
    }

    public void SetCaptureTargetAI(TargetBuilding target, bool _isSquadOrder = false)
    {
        CheckSeparatedOrder(_isSquadOrder);

        if (IsCapturingTarget(target))
        {

        }
        else
        {
            StopCapture();

            if (target.GetTeam() != GetTeam())
            {
                navMeshAgent.SetDestination(target.transform.position);
                captureTarget = target;
                fsm.ChangeState(captureState);
            }
        }
    }

    // Targetting Task - repairing
    public void SetRepairTarget(BaseEntity entity, bool _isSquadOrder = false)
    {
        CheckSeparatedOrder(_isSquadOrder);

        if (CanRepair(entity) == false)
        {
            float range = unitData.RepairDistanceMax;
            navMeshAgent.stoppingDistance = range;
            entityTarget = entity;
            movingToRepair = true;
            positionToMove = entity.transform.position;
            fsm.ChangeState(moveState);
            return;
        }

        if (captureTarget != null)
            StopCapture();

        if (entity.GetTeam() == GetTeam())
        {
            entityTarget = entity;
            fsm.ChangeState(repairState);
            //StartRepairing(entity);
        }
    }
    public bool CanAttack(BaseEntity target)
    {
        if (target == null)
        {
            return false;
        }

        // distance check
        if ((target.transform.position - transform.position).sqrMagnitude > GetRange * GetRange)
        {
            return false;
        }

        return true;
    }

    // Attack Task
    public void StartAttacking(BaseEntity target)
    {
        entityTarget = target;
    }
    public void ComputeAttack()
    {
        if (CanAttack(entityTarget) == false)
        {
            return;
        }

        if (navMeshAgent)
            navMeshAgent.isStopped = true;

        transform.LookAt(entityTarget.transform);
        // only keep Y axis
        Vector3 eulerRotation = transform.eulerAngles;
        eulerRotation.x = 0f;
        eulerRotation.z = 0f;
        transform.eulerAngles = eulerRotation;

        if ((Time.time - lastActionDate) > unitData.AttackFrequency - GetAttackSpeed)
        {
            lastActionDate = Time.time;
            // visual only ?
            if (unitData.BulletPrefab)
            {
                GameObject newBullet = Instantiate(unitData.BulletPrefab, bulletSlot);
                newBullet.transform.parent = null;
                newBullet.GetComponent<Bullet>().ShootToward(entityTarget.transform.position - transform.position, this);
            }
            // apply damages
            int damages = Mathf.FloorToInt(unitData.DPS * unitData.AttackFrequency);
            entityTarget.AddDamage(damages);
        }
    }

    public override void AddDamage(int damageAmount)
    {
        damageAmount -= Mathf.FloorToInt(damageAmount * unitData.DefenseBonus / 100f);
        base.AddDamage(damageAmount);

        HP_Percent = HP / GetMaxHealth;
    }
    public bool CanCapture(TargetBuilding target)
    {
        if (target == null)
            return false;

        // distance check
        if ((target.transform.position - transform.position).sqrMagnitude > GetCaptureRange * GetCaptureRange)
            return false;

        return true;
    }

    public bool DistanceCheck(TargetBuilding target)
    {
       return (target.transform.position - transform.position).sqrMagnitude < GetCaptureRange * GetCaptureRange;
    }

    // Capture Task
    public void StartCapture(TargetBuilding target)
    {
        if (CanCapture(target) == false)
        {
            return;
        }


        if (navMeshAgent)
            navMeshAgent.isStopped = true;

        captureTarget = target;
        captureTarget.StartCapture(this);
    }
    public void StopCapture()
    {
        if (captureTarget == null)
            return;

        captureTarget.StopCapture(this);
        captureTarget = null;
    }

    public bool IsCapturingTarget(TargetBuilding _target)
    {
        return captureTarget == _target;
    }

    public bool IsCapturing()
    {
        return captureTarget != null;
    }

    // Repairing Task
    public bool CanRepair(BaseEntity target)
    {
        if (GetUnitData.CanRepair == false || target == null)
            return false;

        // distance check
        if ((target.transform.position - transform.position).sqrMagnitude > GetUnitData.RepairDistanceMax * GetUnitData.RepairDistanceMax)
            return false;

        return true;
    }
    public void StartRepairing(BaseEntity entity)
    {
        if (GetUnitData.CanRepair)
        {
            entityTarget = entity;
        }
    }

    // $$$ TODO : add repairing visual feedback
    public void ComputeRepairing()
    {
        if (CanRepair(entityTarget) == false)
            return;

        if (navMeshAgent)
            navMeshAgent.isStopped = true;

        transform.LookAt(entityTarget.transform);
        // only keep Y axis
        Vector3 eulerRotation = transform.eulerAngles;
        eulerRotation.x = 0f;
        eulerRotation.z = 0f;
        transform.eulerAngles = eulerRotation;

        if ((Time.time - lastActionDate) > unitData.RepairFrequency)
        {
            lastActionDate = Time.time;

            // apply reparing
            int amount = Mathf.FloorToInt(GetRPS * unitData.RepairFrequency);
            entityTarget.Repair(amount);
        }
    }
    #endregion
}
