using General;
using System.Collections;
using System.Collections.Generic;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MainMenu
{
    public class MainMenuManager : MonoBehaviour
    {
        #region Fields

        [SerializeField] private UISwitcher switcherWidget;

        [SerializeField] private Button backButton;

        #endregion

        #region Unity Life Cycle

        // Start is called before the first frame update
        void Start()
        {
            Menu();
        }

        // Update is called once per frame
        void Update()
        {

        }

        #endregion

        #region ButtonClicked

        public void Menu()
        {
            switcherWidget.SetActiveChildByIndex(0);
            DisplayBackButton(false);
        }
        public void Play()
        {
            switcherWidget.SetActiveChildByIndex(1);
            DisplayBackButton();
        }

        public void LaunchAI_VS_AI()
        {
            SceneManager.LoadScene("LargeBattleField"); // Add a parameter
        }

        public void LaunchAI_VS_Player()
        {
            SceneManager.LoadScene("LargeBattleField"); // Add a parameter
        }

        public void Settings()
        {
            switcherWidget.SetActiveChildByIndex(2);
            DisplayBackButton();
        }

        public void Quit()
        {
            RTS_Application.Quit(true);
        }

        public void DisplayBackButton(bool _state = true)
        {
            backButton.gameObject.SetActive(_state);
        }

        #endregion
    }

}