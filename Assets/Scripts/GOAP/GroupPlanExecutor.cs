using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupPlanExecutor : MonoBehaviour
{
    GOAP goap;
    PerceivedWorldState globalState;
    ETeam team;
    Vector3 target;
    BitArray goalState;

    List<Unit> units;
    GOAPActions[] plan;
    int current = -1;

    public void StartGroupPlan(GOAP goap,
        PerceivedWorldState globalState,
        ETeam team,
        List<Unit> assignedUnits,
        Vector3 target,
        BitArray goalState,
        GOAPActions[] planActions)
    {
        this.goap = goap;
        this.globalState = globalState;
        this.team = team;
        this.target = target;
        this.goalState = goalState;

        this.units = assignedUnits;
        this.plan = planActions;
        this.current = 0;
        EnterAction(plan[0]);
    }

    void Update()
    {
        if (units == null || current < 0) return;

        GOAPActions action = plan[current];

        bool anyFailed = false;

        action.Tick(goap.Controller);
        if (action.HasFailed(goap.Controller))
        {
            Debug.Log("ERROR " + action.name);
            anyFailed = true;
        }

        if (anyFailed)
        {
            Debug.LogWarning($"Action {action} a échoué, on replanifie !");
            Replan();
            return;
        }

        bool allDone = action.IsComplete(goap.Controller);
        if (!allDone) return;


        action.Exit(goap.Controller);


        current++;
        if (current < plan.Length)
        {
            goap.localWorldState.Analyze();
            EnterAction(plan[current]);
        }
        else
        {
            Finish();
        }
    }

    void EnterAction(GOAPActions action)
    {
        action.Enter(goap.Controller);
    }

    void Replan()
    {
        plan[current].Exit(goap.Controller);

        var localWS = new LocalWorldState(team, globalState, units, target);
        goap.SetupGoal();

        plan = goap.CreatePlanForward(goalState);
        if (plan == null || plan.Length == 0)
        {
            Debug.LogError("[GOAP] Re-planning a échoué : plus aucun plan possible.");
            Finish();
            return;
        }

        current = 0;
        EnterAction(plan[current]);
    }

    void Finish()
    {
        foreach (var unit in units)
        {
            unit.isNotLinkedToGoap = true;
        }

        goap.assignment = null;
        units = null;
        plan = null;
        current = -1;

        goap.isBusy = false;
        Debug.Log("Finish GOAP");
    }
}