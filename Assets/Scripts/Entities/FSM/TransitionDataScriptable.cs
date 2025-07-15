using UnityEngine;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "RTS/FSM/Transition")]
#endif

public class TransitionDataScriptable : ScriptableObject
{
    [Tooltip("�tat cible si la condition est valid�e")]
    public UnitStateDataScriptable toState;

    [Tooltip("Condition � remplir pour d�clencher la transition")]
    public ConditionDataScriptable condition;
}