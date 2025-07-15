using UnityEngine;

[CreateAssetMenu(menuName = "RTS/GOAP/Action/Attack Zone")]
public class AttackZone : GOAPActions
{
    public override void Enter(AIController _aiController)
    {
        foreach (Unit unit in goap.localWorldState.assignedUnits)
        {
            unit.CanStopToShootWhileMoving = true;
        }
    }
    public override void Tick(AIController _aiController)
    {
        if(goap.globalState.EvaluateThreatAround(goap.localWorldState.currentTarget) != 0f)
        {
            foreach(Unit unit in goap.localWorldState.assignedUnits)
            {
                if(unit.unitDetection.ennemiesInRange == null)
                {
                    unit.positionToMove = goap.globalState.GetStrongestEnemyMilitaryClusterInRange(goap.localWorldState.currentTarget, 10f);
                     
                    unit.fsm.ChangeState(unit.moveState);
                }
            }
            _isDone = false;
        }
        else
        {
            _isDone = true;
        }

        if (goap.localWorldState.assignedUnits.Count <= 1f) 
        {
            _hasFailed = true;
        }
    }
    public override void Exit(AIController _aiController)
    {
        foreach (Unit unit in goap.localWorldState.assignedUnits)
        {
            unit.CanStopToShootWhileMoving = false;
        }
    }

    public override bool IsComplete(AIController _aiController)
    {
        return _isDone;
    }

}