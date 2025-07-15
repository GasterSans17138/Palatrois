using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "RTS/GOAP/Action/Create Unit")]
#endif
public class CreateUnitsGOAP : GOAPActions
{
    public override void Enter(AIController _aiController)
    {
        _hasFailed = false;
        _aiController.CreateLeaderGOAP(0, goap.assignment.indexGOAP);

        for (int i = 1; i <= goap.assignment.unitCount - 1; i++)
        {
            _aiController.CreateUnitGOAP(goap.assignment.factoriesAssigned[0], 0, goap.assignment.indexGOAP);
        }
    }
    public override void Tick(AIController _aiController)
    {
        if (goap.localWorldState.numberOfUnitToCreate <= goap.localWorldState.assignedUnits.Count)
        {
            _isDone = true;
        }
        else
        {
            _isDone = false;
        }
    }
    public override void Exit(AIController _aiController)
    {

    }

    public override bool IsComplete(AIController _aiController)
    {
        return _isDone;
    }

}