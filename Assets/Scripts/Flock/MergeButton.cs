using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class MergeButton : MonoBehaviour
{
    [SerializeField] private TMP_Text text;

    public UnityEvent<Squad> OnClick = new UnityEvent<Squad>();

    public Squad squad;

    private void Start()
    {
        if (text == null)
        {
            text = GetComponentInChildren<TMP_Text>();
        }
    }

    public void Show(string _text, Squad _squad)
    {
        gameObject.SetActive(true);
        text.text = "=>" + _text;
        squad = _squad;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        text.text = "";
        squad = null;
    }


    public void PlayMerge()
    {
        if(squad.IsValid && squad.IsSelected) OnClick?.Invoke(squad);
    }
}
