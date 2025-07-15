using System.Collections;
using UnityEngine;
using UnityEngine.UI;
public class TargetBuilding : MonoBehaviour, IInfluencer
{
    [Header("Capture")]
    [SerializeField]
    float CaptureGaugeStart = 100f;
    [SerializeField]
    float CaptureGaugeSpeed = 1f;
    [SerializeField]
    int BuildPointsCapture = 5;
    [SerializeField]
    Material BlueTeamMaterial = null;
    [SerializeField]
    Material RedTeamMaterial = null;

    Material NeutralMaterial = null;
    MeshRenderer BuildingMeshRenderer = null;
    Image GaugeImage;
    Image MinimapImage;

    [Header("Farming")]
    [SerializeField] private int BuildPointsPerTimer = 5;
    [SerializeField] private float TimerPoint = 5f;
    private Coroutine pointsCoroutine = null;

    [Header("Influence")]
    public int radius = 5;
    public float influence = 1f;
    public DropOff dropOff;
    public RadiusType radiusType;
    public float influenceThreshold = 2f;

    public ETeam teamForInfluence => GetTeam();
    public InfluenceType influenceType => InfluenceType.Monetary;
    public Vector3 positionForInfluence => transform.position;

    [SerializeField] int[] TeamScore;
    [SerializeField] float CaptureGaugeValue;
    [SerializeField] ETeam OwningTeam = ETeam.Neutral;
    [SerializeField] ETeam CapturingTeam = ETeam.Neutral;
    public ETeam GetTeam() { return OwningTeam; }

    private EntityVisibility _Visibility;
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

    #region MonoBehaviour methods
    void Start()
    {
        BuildingMeshRenderer = GetComponentInChildren<MeshRenderer>();
        NeutralMaterial = BuildingMeshRenderer.material;

        GaugeImage = GetComponentInChildren<Image>();
        if (GaugeImage)
            GaugeImage.fillAmount = 0f;
        CaptureGaugeValue = CaptureGaugeStart;
        TeamScore = new int[2];
        TeamScore[0] = 0;
        TeamScore[1] = 0;

        Transform minimapTransform = transform.Find("MinimapCanvas");
        if (minimapTransform != null)
            MinimapImage = minimapTransform.GetComponentInChildren<Image>();
    }
    void Update()
    {
        if (CapturingTeam == OwningTeam || CapturingTeam == ETeam.Neutral)
            return;

        CaptureGaugeValue -= TeamScore[(int)CapturingTeam] * CaptureGaugeSpeed * Time.deltaTime;

        GaugeImage.fillAmount = 1f - CaptureGaugeValue / CaptureGaugeStart;

        if (CaptureGaugeValue <= 0f)
        {
            CaptureGaugeValue = 0f;
            OnCaptured(CapturingTeam);
        }
    }
    #endregion

    #region Capture methods
    public void StartCapture(Unit unit)
    {
        if (unit == null)
            return;

        TeamScore[(int)unit.GetTeam()] += unit.Cost();

        if (CapturingTeam == ETeam.Neutral)
        {
            if (TeamScore[(int)GameServices.GetOpponent(unit.GetTeam())] == 0)
            {
                CapturingTeam = unit.GetTeam();
                GaugeImage.color = GameServices.GetTeamColor(CapturingTeam);
            }
        }
        else
        {
            if (TeamScore[(int)GameServices.GetOpponent(unit.GetTeam())] > 0)
                ResetCapture();
        }
    }
    public void StopCapture(Unit unit)
    {
        if (unit == null)
            return;

        TeamScore[(int)unit.GetTeam()] -= unit.Cost();
        if (TeamScore[(int)unit.GetTeam()] < 0) TeamScore[(int)unit.GetTeam()] = 0;
        if (TeamScore[(int)unit.GetTeam()] == 0)
        {
            ETeam opponentTeam = GameServices.GetOpponent(unit.GetTeam());
            if (TeamScore[(int)opponentTeam] == 0)
            {
                ResetCapture();
            }
            else
            {
                CapturingTeam = opponentTeam;
                GaugeImage.color = GameServices.GetTeamColor(CapturingTeam);
            }
        }
    }
    void ResetCapture()
    {
        CaptureGaugeValue = CaptureGaugeStart;
        CapturingTeam = ETeam.Neutral;
        GaugeImage.fillAmount = 0f;
    }
    void OnCaptured(ETeam newTeam)
    {
        UnitController team_controller;
        Debug.Log("target captured by " + newTeam.ToString());
        if (OwningTeam != newTeam)
        {
            team_controller = GameServices.GetControllerByTeam(newTeam);
            if (team_controller != null)
                team_controller.CaptureTarget(BuildPointsCapture, this);

            if (OwningTeam != ETeam.Neutral)
            {
                // remove points to previously owning team
                team_controller = GameServices.GetControllerByTeam(OwningTeam);
                if (team_controller != null)
                    team_controller.LoseTarget(BuildPointsCapture, this);
            }

            if (pointsCoroutine != null)
            {
                StopCoroutine(pointsCoroutine);
            }
        }
        ResetCapture();

        OwningTeam = newTeam;
        team_controller = GameServices.GetControllerByTeam(OwningTeam);
        if (team_controller != null && pointsCoroutine == null) pointsCoroutine = StartCoroutine(BuildPoints(team_controller));
        if (Visibility) { Visibility.Team = OwningTeam; }
        if (MinimapImage) { MinimapImage.color = GameServices.GetTeamColor(OwningTeam); }
        BuildingMeshRenderer.material = newTeam == ETeam.Blue ? BlueTeamMaterial : RedTeamMaterial;
    }
    #endregion

    #region Build Points Coroutine

    private IEnumerator BuildPoints(UnitController _teamController)
    {
        while (true)
        {
            Debug.Log("Build Points Loading !");

            yield return new WaitForSeconds(TimerPoint);
            Debug.Log("Getting Build Points!");

            _teamController.TotalBuildPoints += BuildPointsPerTimer;
        }
    }

    #endregion
    #region InfluenceMap methods : GetRadius, GetDropOff ...
    public float GetDropOff(int _locationDistance)
    {
        float i = dropOff switch
        {
            DropOff.CONSTANT => influence,
            DropOff.LINEAR => influence / (1 + _locationDistance),
            DropOff.SQRT => influence / Mathf.Sqrt(1 + _locationDistance),
            DropOff.POW => influence / ((1 + _locationDistance) * (1 + _locationDistance)),
            DropOff.CUSTOM => influence - influence / GetRadius() * _locationDistance,
            _ => throw new System.NotImplementedException()
        };

        return i;
    }

    public float GetRadius()
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
}
