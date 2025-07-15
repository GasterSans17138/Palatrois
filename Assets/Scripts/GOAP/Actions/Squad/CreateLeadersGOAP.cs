using UnityEngine;

[CreateAssetMenu(menuName = "RTS/GOAP/Action/Create Leader")]
public class CreateLeaderActionGOAP : GOAPActions
{
    [SerializeField] private Unit Leader;
    private bool isActionDone = false;

    public override void Enter(AIController _aiController)
    {
        base.Enter(_aiController);
        isActionDone = false;
    }
    public override void Exit(AIController _aiController)
    {

    }

    public override bool IsComplete(AIController _aiController)
    {
        return _aiController.HasAvailableLeader();
    }

    public override void Tick(AIController _aiController)
    {
        if (_hasFailed && !isActionDone)
        {
            _aiController.CreateLeader(Leader.GetTypeId);
            isActionDone = true;
        }
    }
}