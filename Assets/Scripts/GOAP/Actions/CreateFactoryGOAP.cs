using UnityEngine;

[CreateAssetMenu(menuName = "RTS/GOAP/Action/Create Factory")]
public class CreateFactory : GOAPActions
{
    public override void Enter(AIController _aiController)
    {
        base.Enter(_aiController);

        //_hasFailed = !_aiController.RequestBuildFactory(0);
    }
    public override void Exit(AIController _aiController)
    {
        
    }

    public override bool IsComplete(AIController _aiController)
    {
        return !_hasFailed;
    }

    public override void Tick(AIController _aiController)
    {

    }
}