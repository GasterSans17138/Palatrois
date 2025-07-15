using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    public class UISwitcher : MonoBehaviour
    {


        #region Fields

        [Tooltip("Enfant actif au démarrage")]
        [SerializeField] private RectTransform currentWidget = null;
        [SerializeField] private List<RectTransform> children = new List<RectTransform>();

        #endregion

        #region Methods

        #region Unity Life Cycle

        private void Start()
        {
            currentWidget.gameObject.SetActive(true);
        }

        private void OnValidate()
        {

            children.Clear();

            for (int i = 0; i < transform.childCount; i++)
            {
                RectTransform child = (RectTransform)transform.GetChild(i);
                children.Add(child);
                child.gameObject.SetActive(false);
            }

            if (currentWidget == null && children.Count > 0)
            {
                currentWidget = children[0];
            }

            if(currentWidget != null ) currentWidget.gameObject.SetActive(true);
        }

        #endregion

        #region Set Active

        /// <summary>
        /// Affiche uniquement l'enfant à l'index donné et désactive les autres.
        /// </summary>
        public void SetActiveChild(RectTransform _activeWidget)
        {
            if (!Contains(_activeWidget))
            {
                Debug.LogWarning("Do not contains widget : " + _activeWidget.name);
                return;
            }

            SwitchWidget(_activeWidget);
        }

        /// <summary>
        /// Affiche uniquement l'enfant à l'index donné et désactive les autres.
        /// </summary>
        public void SetActiveChildByIndex(int _index)
        {
            if (_index < 0 || _index >= children.Count)
            {
                Debug.LogWarning("Index en dehors des bornes !");
                return;
            }

            SwitchWidget(children[(int)_index]);
        }

        /// <summary>
        /// Affiche un enfant en fonction de son nom (optionnel).
        /// </summary>
        public void SetActiveChildByName(string _childName)
        {
            for (int i = 0; i < children.Count; i++)
            {
                RectTransform child = children[i];

                if (child.name == _childName)
                {
                    SwitchWidget(child);
                    return;
                };
            }

            Debug.LogWarning("Do not contains widget : " + _childName);
        }

        #endregion

        #region Switch Widget

        /// <summary>
        /// Switch the current widget by the new one
        /// </summary>
        /// <param name="_widget"></param>
        public void SwitchWidget(RectTransform _widget)
        {
            currentWidget.gameObject.SetActive(false);

            currentWidget = _widget;

            currentWidget.gameObject.SetActive(true);
        }
        #endregion

        #region Contains

        /// <summary>
        /// Check if the children contains a widget
        /// </summary>
        /// <param name="_child"></param>
        /// <returns></returns>
        public bool Contains(RectTransform _child)
        {
            return children.Contains(_child);
        }

        #endregion

        #endregion
    }

}