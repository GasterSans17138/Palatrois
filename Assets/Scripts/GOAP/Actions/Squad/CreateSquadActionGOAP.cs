using UnityEngine;

[CreateAssetMenu(menuName = "RTS/GOAP/Action/Create Squad")]
public class CreateSquadActionGOAP : GOAPActions
{
    [SerializeField] Formation formationToApply;

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
        return isActionDone;
    }

    public override void Tick(AIController _aiController)
    {
        if (!_hasFailed && !isActionDone)
        {
            _aiController.CreateSquad(formationToApply, goap.assignment.assignedUnits);
            isActionDone = true;
        }
    }
}