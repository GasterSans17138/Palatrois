using UnityEngine;

[CreateAssetMenu(menuName = "RTS/FSM/Condition/Has Capture Target")]
public class HasCaptureTargetCondition : ConditionDataScriptable
{
    public override bool Check(Unit _unit)
    {
        TargetBuilding cap = _unit.captureTarget;
        return cap != null
            && cap.GetTeam() != _unit.GetTeam()
            && _unit.CanCapture(cap);
    }
}