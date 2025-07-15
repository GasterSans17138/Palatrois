using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GOAPAgent : MonoBehaviour
{
    public bool IsBusy { get; private set; }
    private UtilityGoalAssignment currentGoal;
    private Coroutine goalCoroutine;

    // Méthode appelée par le UtilitySystem
    public void AssignGoal(UtilityGoalAssignment assignment)
    {
        if (IsBusy)
        {
            Debug.LogWarning($"{name} a reçu un goal alors qu'il est occupé !");
            return;
        }
        currentGoal = assignment;
        IsBusy = true;
        Debug.Log($"[GOAPAgent] {name} débute le goal {assignment.goalType} (target: {assignment.targetLabel} @ {assignment.targetPosition}) avec {assignment.assignedUnits.Count} unités (force totale : {assignment.assignedUnits.Sum(u => u.influence):F2})");
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
        Debug.Log($"[GOAPAgent] {name} a terminé le goal {currentGoal.goalType}. Libération des unités.");
        // Libère les unités
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
