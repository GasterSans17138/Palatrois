using UnityEngine;

[CreateAssetMenu(menuName = "RTS/GOAP/Action/Get Leader")]
public class GetLeaderActionGOAP : GOAPActions
{
    public override void Enter(AIController _aiController)
    {
        base.Enter(_aiController);

        _hasFailed = !_aiController.HasAvailableLeader();
    }
    public override void Exit(AIController _aiController)
    {

    }

    public override bool IsComplete(AIController _aiController)
    {
        return true;
    }

    public override void Tick(AIController _aiController)
    {
        if (!_hasFailed)
        {
            Unit unit = _aiController.GetFirstAvailableLeader();
            if(unit != null)
            {
                goap.assignment.assignedUnits.Add(_aiController.GetFirstAvailableLeader());
            }
            else
            {
                _hasFailed = true;
            }
        }
    }
}