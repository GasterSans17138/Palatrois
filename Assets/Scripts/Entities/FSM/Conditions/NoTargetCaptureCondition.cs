using UnityEngine;
using static UnityEngine.UI.CanvasScaler;

[CreateAssetMenu(menuName = "RTS/FSM/Condition/No Capture Target")]
public class NoTargetCaptureCondition : ConditionDataScriptable
{
    public override bool Check(Unit _unit)
    {
        if (_unit == null)
            return false;

        TargetBuilding cap = _unit.captureTarget;
        if (cap.GetTeam() == _unit.GetTeam()) 
        { 
            return true;
        }
        return false;
    }
}