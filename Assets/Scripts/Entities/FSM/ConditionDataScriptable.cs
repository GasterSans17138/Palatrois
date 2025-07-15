using UnityEngine;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "RTS/FSM/Condition")]
#endif

public abstract class ConditionDataScriptable : ScriptableObject
{
    public abstract bool Check(Unit _unit);
}