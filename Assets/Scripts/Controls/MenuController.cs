using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    #region Fields

    [SerializeField]
    Transform FactoryMenuCanvas = null;
    public GraphicRaycaster BuildMenuRaycaster { get; private set; }

    public UnitController Controller = null;
    [Header("Panels")]
    [SerializeField] FormationMenuPanel FormationsMenuPanel = null;
    [SerializeField] FactoryMenuPanel factoryMenuPanel = null;
    [SerializeField] Transform BuildUnitMenuPanel = null;
    [SerializeField] Transform BuildLeaderMenuPanel = null;
    [SerializeField] Transform BuildFactoryMenuPanel = null;

    [Header("Text")]
    [SerializeField] Text BuildPointsText = null;
    [SerializeField] Text CapturedTargetsText = null;
    [SerializeField] TMP_Text LevelFactoryText = null;
    Text[] BuildQueueTexts = null;

    [Header("Buttons")]
    Button[] BuildUnitButtons = null;
    Button[] BuildLeaderButtons = null;
    Button[] BuildFactoryButtons = null;
    [SerializeField] Button CancelBuildButton = null;
    [SerializeField] UpgradeButton UpgradeFactoryButton = null;

    #endregion

    #region Display Panel

    /// <summary>
    /// Hide everything related to the upgrade system
    /// </summary>
    public void HideUpgradeSystem()
    {
        if (UpgradeFactoryButton) UpgradeFactoryButton.gameObject.SetActive(false);
        if (factoryMenuPanel)
        {
            factoryMenuPanel.HideLeaderButtons();
        }
    }

    /// <summary>
    /// Show the upgrade system
    /// </summary>
    public void ShowUpgradeSystem()
    {
        if (UpgradeFactoryButton) UpgradeFactoryButton.gameObject.SetActive(true);
        if (factoryMenuPanel)
        {
            factoryMenuPanel.ShowLeaderButtons();
        }
    }

    /// <summary>
    /// Hide the factory menu
    /// </summary>
    public void HideFactoryMenu()
    {
        if (factoryMenuPanel)
            factoryMenuPanel.Hide();
    }

    /// <summary>
    /// Show the factory menu
    /// </summary>
    public void ShowFactoryMenu()
    {
        if (factoryMenuPanel)
            factoryMenuPanel.Show();
    }

    /// <summary>
    /// Hide the formations menu
    /// </summary>
    public void HideFormationsMenu()
    {
        if (FormationsMenuPanel)
            FormationsMenuPanel.Hide();
    }

    /// <summary>
    /// Show the formations menu
    /// </summary>
    public void ShowFormationsMenu()
    {
        if (FormationsMenuPanel)
        {
            Debug.Log("Show Information");
            FormationsMenuPanel.Show();
        }
    }

    /// <summary>
    /// Hide All the factory build queue
    /// </summary>
    public void HideAllFactoryBuildQueue()
    {
        foreach (Text text in BuildQueueTexts)
        {
            if (text)
                text.enabled = false;
        }
    }

#endregion

    #region Update Methods

    /// <summary>
    /// Update the Factory Build Queue UI
    /// </summary>
    /// <param name="i"></param>
    /// <param name="selectedFactory"></param>
    public void UpdateFactoryBuildQueueUI(int i, Factory selectedFactory)
    {
        if (selectedFactory == null)
            return;
        int queueCount = selectedFactory.GetQueuedCount(i);
        if (queueCount > 0)
        {
            BuildQueueTexts[i].text = "+" + queueCount;
            BuildQueueTexts[i].enabled = true;
        }
        else
        {
            BuildQueueTexts[i].enabled = false;
        }
    }

    /// <summary>
    /// Update the build points UI
    /// </summary>
    public void UpdateBuildPointsUI()
    {
        if (BuildPointsText != null)
            BuildPointsText.text = "Build Points : " + Controller.TotalBuildPoints;
    }

    /// <summary>
    /// Update the Captured Target UI
    /// </summary>
    public void UpdateCapturedTargetsUI()
    {
        if (CapturedTargetsText != null)
            CapturedTargetsText.text = "Captured Targets : " + Controller.CapturedTargets;
    }

    /// <summary>
    /// Unregister the build unit buttons, the build leader buttons, the build factory buttons & the upgrade factory button
    /// </summary>
    /// <param name="availableUnitsCount"></param>
    /// <param name="availableLeadersCount"></param>
    /// <param name="availableFactoriesCount"></param>
    public void UnregisterBuildButtons(int availableUnitsCount, int availableLeadersCount, int availableFactoriesCount, Factory _selectedFactory)
    {
        // unregister build buttons
        for (int i = 0; i < availableUnitsCount; i++)
        {
            BuildUnitButtons[i].onClick.RemoveAllListeners();
        }
        for (int i = 0; i < availableLeadersCount; i++)
        {
            BuildLeaderButtons[i].onClick.RemoveAllListeners();
        }

        for (int i = 0; i < availableFactoriesCount; i++)
        {
            BuildFactoryButtons[i].onClick.RemoveAllListeners();
        }

        if (UpgradeFactoryButton)
        {
            UpgradeFactoryButton.OnClick.RemoveAllListeners();
        }

        Controller.OnBuildPointsUpdated -= () => UpdateEnableUpgrade(_selectedFactory);

    }

    /// <summary>
    /// Update the factory menu thanks to the factory given
    /// </summary>
    /// <param name="selectedFactory"></param>
    /// <param name="requestUnitBuildMethod"></param>
    /// <param name="requestLeaderBuildMethod"></param>
    /// <param name="enterFactoryBuildModeMethod"></param>
    public void UpdateFactoryMenu(Factory selectedFactory, Func<int, bool> requestUnitBuildMethod, Func<int, bool> requestLeaderBuildMethod, Action<int> enterFactoryBuildModeMethod)
    {
        ShowFactoryMenu();

        // Unit build buttons
        // register available buttons
        int i = 0;
        for (; i < selectedFactory.AvailableUnitsCount; i++)
        {
            BuildUnitButtons[i].gameObject.SetActive(true);

            int index = i; // capture index value for event closure
            BuildUnitButtons[i].onClick.AddListener(() =>
            {
                if (requestUnitBuildMethod(index))
                    UpdateFactoryBuildQueueUI(index, selectedFactory);
            });

            Text[] buttonTextArray = BuildUnitButtons[i].GetComponentsInChildren<Text>();
            Text buttonText = buttonTextArray[0];//BuildUnitButtons[i].GetComponentInChildren<Text>();
            UnitDataScriptable data = selectedFactory.GetBuildableUnitData(i);
            buttonText.text = data.Caption + "(" + data.Cost + ")";

            // Update queue count UI
            BuildQueueTexts[i] = buttonTextArray[1];
            UpdateFactoryBuildQueueUI(i, selectedFactory);
        }
        // hide remaining buttons
        for (; i < BuildUnitButtons.Length; i++)
        {
            BuildUnitButtons[i].gameObject.SetActive(false);
        }

        // activate Cancel button
        CancelBuildButton.onClick.AddListener(() =>
                                                {
                                                    selectedFactory?.CancelCurrentBuild();
                                                    HideAllFactoryBuildQueue();
                                                });
        
        // Leader Buttons
        if (selectedFactory.IsUpgradable)
        {
            selectedFactory.UpgradeFactory.OnUpdate.RemoveAllListeners();
            i = 0;
            for (; i < selectedFactory.AvailableLeadersCount; i++)
            {
                Button button_leader = BuildLeaderButtons[i];
                button_leader.gameObject.SetActive(true);
                int index_no_modif = i;
                int index = i + BuildUnitButtons.Length; // capture index value for event closure
                button_leader.onClick.AddListener(() =>
                {
                    if (requestLeaderBuildMethod(index))
                    {
                        UpdateFactoryBuildQueueUI(index, selectedFactory);
                    }

                });

                Text[] buttonTextArray = button_leader.GetComponentsInChildren<Text>();
                Text buttonText = buttonTextArray[0];//BuildUnitButtons[i].GetComponentInChildren<Text>();
                UnitDataScriptable data = selectedFactory.GetBuildableLeaderData(i);
                button_leader.interactable = selectedFactory.CanCreateLeader(index_no_modif);
                buttonText.text = data.Caption + "(" + (data.Cost * selectedFactory.LeaderMultiplicator) + ")" + selectedFactory.UpgradeFactory.SlotString(index_no_modif);

                selectedFactory.UpgradeFactory.OnUpdate.AddListener(() =>
                {
                    button_leader.interactable = selectedFactory.CanCreateLeader(index_no_modif);
                    buttonText.text = data.Caption + "(" + (data.Cost * selectedFactory.LeaderMultiplicator) + ")" + selectedFactory.UpgradeFactory.SlotString(index_no_modif);
                });

                // Update queue count UI
                BuildQueueTexts[index] = buttonTextArray[1];
                UpdateFactoryBuildQueueUI(index, selectedFactory);
            }
            // hide remaining buttons
            for (; i < BuildLeaderButtons.Length; i++)
            {
                BuildLeaderButtons[i].gameObject.SetActive(false);
            }

            ShowUpgradeSystem();
        }
        else
        {
            HideUpgradeSystem();
        }


        // Factory build buttons
        // register available buttons
        i = 0;
        for (; i < selectedFactory.AvailableFactoriesCount; i++)
        {
            BuildFactoryButtons[i].gameObject.SetActive(true);

            int index = i; // capture index value for event closure
            BuildFactoryButtons[i].onClick.AddListener(() =>
            {
                enterFactoryBuildModeMethod(index);
            });

            Text buttonText = BuildFactoryButtons[i].GetComponentInChildren<Text>();
            FactoryDataScriptable data = selectedFactory.GetBuildableFactoryData(i);
            buttonText.text = data.Caption + "(" + data.Cost + ")";
        }
        // hide remaining buttons
        for (; i < BuildFactoryButtons.Length; i++)
        {
            BuildFactoryButtons[i].gameObject.SetActive(false);
        }

        if (selectedFactory.IsUpgradable)
        {
            LevelFactoryText.text = $"Level {selectedFactory.GetLevel()}";
            Controller.OnBuildPointsUpdated += () => UpdateEnableUpgrade(selectedFactory);

            UpgradeFactoryButton.gameObject.SetActive(true);
            UpdateEnableUpgrade(selectedFactory);
            UpgradeFactoryButton.SetFactory(selectedFactory);
            UpgradeFactoryButton.SetCost(selectedFactory.UpgradeCost);
            UpgradeFactoryButton.OnClick.AddListener(() =>
            {
                LevelFactoryText.text = $"Level {selectedFactory.GetLevel()}";
            });
        }
        else
        {
            LevelFactoryText.text = "Level 0";
            UpgradeFactoryButton.SetFactory(null);
            UpgradeFactoryButton.gameObject.SetActive(false);
        }
    }

    private void UpdateEnableUpgrade(Factory _selectedFactory)
    {
        UpgradeFactoryButton.SetEnabled(Controller.TotalBuildPoints >= _selectedFactory.UpgradeCost);
    }
#endregion

    #region Unity Life Cycle

    void Awake()
    {
        if (FactoryMenuCanvas == null)
        {
            Debug.LogWarning("FactoryMenuCanvas not assigned in inspector");
        }
        else
        {

            factoryMenuPanel.gameObject.SetActive(false);

            BuildMenuRaycaster = FactoryMenuCanvas.GetComponent<GraphicRaycaster>();
        }

        Controller = GetComponent<UnitController>();
    }

    void Start()
    {
        BuildUnitButtons = BuildUnitMenuPanel.GetComponentsInChildren<Button>();
        BuildLeaderButtons = BuildLeaderMenuPanel.GetComponentsInChildren<Button>();
        BuildFactoryButtons = BuildFactoryMenuPanel.GetComponentsInChildren<Button>();
        BuildQueueTexts = new Text[BuildUnitButtons.Length + BuildLeaderButtons.Length];
    }

    #endregion
}


