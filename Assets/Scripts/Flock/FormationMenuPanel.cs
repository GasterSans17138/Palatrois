using System.Collections.Generic;
using UnityEngine;

public class FormationMenuPanel : MonoBehaviour
{
    #region Fields

    public MenuController menuController;
    [SerializeField] private Transform Content;

    [SerializeField] private FormationButton prefabFormationButton;
    List<FormationButton> FormationButtons = new List<FormationButton>();

    [SerializeField] private MergeButton[] mergeButtons;

    #endregion

    #region Methods

    #region Unity Life Cycle

    void Start()
    {
        CreateMergeButton();
        CreateFormationsMenu(GameServices.GetGameData().Formations);
        Hide();
    }

#endregion

    #region Display

    /// <summary>
    /// Show the formations menu panel
    /// </summary>
    public void Show()
    {
        ShowMergeButtons();
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Hide the formations menu panel
    /// </summary>
    public void Hide()
    {
        HideMergeButtons();

        gameObject.SetActive(false);
    }

    /// <summary>
    /// Show the merge buttons
    /// </summary>
    private void ShowMergeButtons()
    {
        List<Squad> selected_squad_list = menuController.Controller.SquadManager.SelectedSquads;

        if (selected_squad_list.Count > 1)
        {
            for (int i = 0; i < selected_squad_list.Count; i++)
            {
                if (i == mergeButtons.Length) break;
                mergeButtons[i].Show(selected_squad_list[i].Name, selected_squad_list[i]);
            }
        }
    }

    /// <summary>
    /// Hide the merge buttons
    /// </summary>
    private void HideMergeButtons()
    {
        for (int i = 0; i < mergeButtons.Length; i++)
        {
            mergeButtons[i].Hide();
        }
    }

    #endregion

    #region Create Methods

    /// <summary>
    /// Create the merge buttons to merge squads
    /// </summary>
    private void CreateMergeButton()
    {
        for(int i = 0;i < mergeButtons.Length; i++)
        {
            UnitController controller = menuController.Controller;
            mergeButtons[i].OnClick.AddListener((Squad squad) =>
            {
                controller.MergeSquads(squad);
                HideMergeButtons();
            });
        }
    }

    /// <summary>
    /// Create the formations menu
    /// </summary>
    /// <param name="_formations"></param>
    private void CreateFormationsMenu(Formation[] _formations)
    {
        FormationButtons.Clear();

        for (int i = 0; i < _formations.Length; i++)
        {
            int index = i; // copie de i
            FormationButton new_button = Instantiate<FormationButton>(prefabFormationButton, Vector3.zero, Quaternion.identity, Content);
            new_button.SetFormation(_formations[index]);
            new_button.OnClick.AddListener(() =>
            {
                menuController.Controller.ApplyFormationToSelectedUnit(_formations[index]);
                HideMergeButtons();
                ShowMergeButtons();
            });
            FormationButtons.Add(new_button);
        }
    }

    #endregion

    #region On Unform Click Methods
    public void DestroySelectedSquads()
    {
        Debug.Log("Destroy squad");
        menuController.Controller.DestroySelectedSquad();
        Hide();
    }
    #endregion

    #endregion
}
