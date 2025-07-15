using TMPro;
using UnityEngine;

public class LeaderCanvas : MonoBehaviour
{
    #region Fields

    [SerializeField] private CanvasGroup _group;
    [SerializeField] private TMP_Text SquadText;
    [SerializeField] private Transform TextContainer;

    #endregion

    #region Properties

    public CanvasGroup Group
    {
        get 
        { 
            if(_group == null) _group = GetComponent<CanvasGroup>();
            return _group; 
        }
    }

    #endregion

    #region Unity Life Cycle

    void Start()
    {
        _group = GetComponent<CanvasGroup>();
        ResetSquadName();
    }


    void LateUpdate()
    {
        // Lock the rotation of the canvas to always be in front of the player
        transform.rotation = Quaternion.Euler(new Vector3(90, 0, 0));
    }

    #endregion

    #region Squad UI Information

    public void ResetSquadName()
    {
        SquadText.text = "";
        Group.alpha = 0.3f;
        TextContainer.gameObject.SetActive(false);
    }

    public void SetSquadLeader(string _squadName)
    {
        SquadText.text = _squadName;
        Group.alpha = 1.0f;
        TextContainer.gameObject.SetActive(true);
    }

    #endregion
}
