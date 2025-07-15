using UnityEngine;

public class UnitFSM : MonoBehaviour
{
    [Tooltip("État de départ")]
    public UnitStateDataScriptable initialState;

    public UnitStateDataScriptable currentState;
    Unit unit;

    void Awake()
    {
        unit = GetComponent<Unit>();
        currentState = initialState;
        currentState.Enter(unit);
    }

    void Update()
    {
        foreach (TransitionDataScriptable transition in currentState.transitions)
        {
            if (transition.condition.Check(unit))
            {
                currentState.Exit(unit);
                currentState = transition.toState;
                currentState.Enter(unit);
                break;
            }
        }
        currentState.Tick(unit);
    }

    public void ChangeState(UnitStateDataScriptable _next)
    {
        //if (currentState == _next) return;
        currentState.Exit(unit);
        currentState = _next;
        currentState.Enter(unit);
    }
}