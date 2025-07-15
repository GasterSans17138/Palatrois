using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GOAPAgent : MonoBehaviour
{
    public bool IsBusy { get; private set; }
    private UtilityGoalAssignment currentGoal;
    private Coroutine goalCoroutine;

    // M�thode appel�e par le UtilitySystem
    public void AssignGoal(UtilityGoalAssignment assignment)
    {
        if (IsBusy)
        {
            Debug.LogWarning($"{name} a re�u un goal alors qu'il est occup� !");
            return;
        }
        currentGoal = assignment;
        IsBusy = true;
        Debug.Log($"[GOAPAgent] {name} d�bute le goal {assignment.goalType} (target: {assignment.targetLabel} @ {assignment.targetPosition}) avec {assignment.assignedUnits.Count} unit�s (force totale : {assignment.assignedUnits.Sum(u => u.influence):F2})");
        goalCoroutine = StartCoroutine(ExecuteGoalCoroutine());
    }

    private IEnumerator ExecuteGoalCoroutine()
    {
        float duration = 10f; // Simule une action de 10s
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        CompleteGoal();
    }

    private void CompleteGoal()
    {
        Debug.Log($"[GOAPAgent] {name} a termin� le goal {currentGoal.goalType}. Lib�ration des unit�s.");
        // Lib�re les unit�s
        foreach (var unit in currentGoal.assignedUnits)
        {
            unit.isNotLinkedToGoap = true;
        }
        IsBusy = false;
        currentGoal = null;
    }

    private void OnDisable()
    {
        if (goalCoroutine != null)
            StopCoroutine(goalCoroutine);
    }
}
