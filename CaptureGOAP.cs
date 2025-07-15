using UnityEngine;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "RTS/GOAP/Action/Capture Building")]
#endif
public class CaptureActionSO : GOAPActions
{

    /*
    public override void Enter(Unit unit)
    {
        unit.fsm.ChangeState(unit.captureState);
    }

    public override void Tick(Unit unit)
    {
    }

    public override bool IsComplete(Unit unit)
    {
        return !unit.IsCapturing();
    }

    public override void Exit(Unit unit)
    {
        if (unit.IsCapturing())
        {
            unit.StopCapture();
        }
    }*/
}