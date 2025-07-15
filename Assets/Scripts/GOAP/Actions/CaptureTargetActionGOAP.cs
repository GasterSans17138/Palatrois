using UnityEngine;

[CreateAssetMenu(menuName = "RTS/GOAP/Action/Capture Target")]
public class CaptureTargetActionGOAP : GOAPActions
{
    private bool isActionDone = false;

    public override void Enter(AIController _aiController)
    {
        base.Enter(_aiController);
        isActionDone = false;
        Debug.Log(isActionDone);

        _aiController.CaptureTargetGOAP(goap.assignment.targetPosition, goap.localWorldState.assignedUnits);
    }
    public override void Tick(AIController _aiController)
    {
        if(!_hasFailed && !isActionDone)
        {
            isActionDone = !goap.assignment.assignedUnits[0].IsCapturing();
        }
    }

    public override bool IsComplete(AIController _aiController)
    {
        return isActionDone;
    }

    public override void Exit(AIController _aiController)
    {

    }
}