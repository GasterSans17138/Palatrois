using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum ETimeScale {
    LOW = 0,
    NORMAL = 1,
    FAST = 2,
    HYPER_FAST = 3,

}
public class FastForward : MonoBehaviour
{
    [SerializeField]
    public TextMeshProUGUI fastAndNormalText;
    private ETimeScale nextTimeScale = ETimeScale.FAST;

    public void FastAndNormal()
    {
        switch (nextTimeScale)
        {
            case ETimeScale.LOW:
                Time.timeScale = 0.5f;
                nextTimeScale = ETimeScale.NORMAL;
                fastAndNormalText.text = "Normal";
                break;
            case ETimeScale.NORMAL:
                Time.timeScale = 1f;
                nextTimeScale = ETimeScale.FAST;
                fastAndNormalText.text = "Fast";
                break;
            case ETimeScale.FAST:
                Time.timeScale = 2.0f;
                nextTimeScale = ETimeScale.HYPER_FAST;
                fastAndNormalText.text = "HyperFast";
                break;
            case ETimeScale.HYPER_FAST:
                Time.timeScale = 3.0f;
                nextTimeScale = ETimeScale.LOW;
                fastAndNormalText.text = "Low";
                break;
        }
    }
}