using System.Collections;
using System.Collections.Generic;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class FactoryMenuPanel : MonoBehaviour
{
    #region Fields

    [Header("Informations")]
    [SerializeField] TMP_Text LevelFactoryText = null;

    [Header("Button Switcher")]
    [SerializeField] Button UnitsButton = null;
    [SerializeField] Button LeadersButton = null;
    [SerializeField] UISwitcher switcher = null;

    #endregion

    #region Methods

    #region Unity Life Cycle

    private void Start()
    {
        UnitsButton.interactable = false;
        LeadersButton.interactable = true;

        switcher.SetActiveChildByIndex(0);
    }

    #endregion

    #region Display

    /// <summary>
    /// Show the factory panel
    /// </summary>
    public void Show()
    {
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Hide the factory panel
    /// </summary>
    public void Hide()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Hide the leader buttons
    /// </summary>
    public void HideLeaderButtons()
    {
        UnitsButton.interactable = false;
        switcher.SetActiveChildByIndex(0);
        LeadersButton.gameObject.SetActive(false);
    }

    /// <summary>
    /// Show the leader buttons
    /// </summary>
    public void ShowLeaderButtons()
    {
        LeadersButton.interactable = true;
        switcher.SetActiveChildByIndex(0);
        LeadersButton.gameObject.SetActive(true);
    }

    #endregion

    #endregion
}
