using UnityEngine;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "RTS/FSM/Transition")]
#endif

public class TransitionDataScriptable : ScriptableObject
{
    [Tooltip("État cible si la condition est validée")]
    public UnitStateDataScriptable toState;

    [Tooltip("Condition à remplir pour déclencher la transition")]
    public ConditionDataScriptable condition;
}