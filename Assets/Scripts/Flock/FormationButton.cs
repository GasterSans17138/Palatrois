using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class FormationButton : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private Formation formation;

    public UnityEvent OnClick = new UnityEvent();

    private void Start()
    {
        if(text == null)
        {
            text = GetComponentInChildren<TMP_Text>();
            if (text) text.text = formation.FormationName;
        }
    }

    public void SetFormation(Formation _formation)
    {
        formation = _formation;
        gameObject.name = formation.FormationName;
        if(text) text.text = formation.FormationName;
    }

    public void PlayFormation()
    {
        OnClick?.Invoke();
    }
}
