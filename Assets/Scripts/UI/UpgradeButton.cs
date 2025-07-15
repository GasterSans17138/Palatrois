using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UpgradeButton : MonoBehaviour
{
    #region Fields

    public Factory factory;
    [SerializeField] private TMP_Text text;
    [SerializeField] private Button button;
    private string baseText = "Upgrade : ";

    #endregion

    #region Events

    public UnityEvent OnClick = new UnityEvent();

    #endregion

    #region Methods

    #region Unity Life Cycle

    private void Start()
    {
        if(text == null)
        {
            text = GetComponentInChildren<TMP_Text>();
            text.text = "Upgrade";
        }

        if(button == null)
        {
            button = GetComponent<Button>();
        }
    }

    #endregion

    #region Setter

    /// <summary>
    /// Link the factory to the button
    /// </summary>
    /// <param name="_factory"></param>
    public void SetFactory(Factory _factory)
    {
        factory = _factory;
    }

    /// <summary>
    /// Set the cost of the upgrade
    /// </summary>
    /// <param name="_cost"></param>
    public void SetCost(int _cost)
    {
        text.text = baseText + _cost;
    }
    #endregion

    #region On Click

    /// <summary>
    /// Upgrade the factory and set the new cost
    /// </summary>
    public void Play()
    {
        factory.Upgrade();
        SetCost(factory.UpgradeCost);
        OnClick?.Invoke();
    }

    public void SetEnabled(bool _enabled)
    {
        button.interactable = _enabled;
    }

    public void Enable()
    {
        button.interactable = true;
    }

    public void Disable()
    {
        button.interactable = false;
    }

    #endregion

    #endregion
}
