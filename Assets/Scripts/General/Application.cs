using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace General
{
    public class RTS_Application : MonoBehaviour
    {
        private bool state = true;

        public bool State { get { return state; } set { state = value; } }
        public static void SceneTransition(string _levelName)
        {
            SceneManager.LoadScene(_levelName);
        }

        public static void Quit(bool _hasAuthority = false)
        {
            if (_hasAuthority)
            {
                Application.Quit();
            }
            else
            {
                // TODO Change
                Application.Quit();
            }
        }
    }
}