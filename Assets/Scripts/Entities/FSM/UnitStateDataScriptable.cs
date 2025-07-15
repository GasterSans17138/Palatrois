using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "RTS/FSM/State")]
#endif

public abstract class UnitStateDataScriptable : ScriptableObject
{
    [Tooltip("Liste des transitions possibles à partir de cet état")]
    public List<TransitionDataScriptable> transitions = new List<TransitionDataScriptable>();

    public abstract void Enter(Unit _unit);

    public abstract void Tick(Unit _unit);

    public abstract void Exit(Unit _unit);
}