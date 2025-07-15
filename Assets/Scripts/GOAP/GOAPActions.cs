using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "RTS/GOAP/Actions")]
#endif
public abstract class GOAPActions : ScriptableObject
{
    public GOAP goap;
    public int indexGOAP = 0;
    protected bool _hasFailed = false;
    protected bool _isDone = false;

    [Header("Prérequis")]
    public List<WorldKey> positivePreconditions = new List<WorldKey>();
    public List<WorldKey> negativePreconditions = new List<WorldKey>();

    [Header("Effets")]
    public List<WorldKey> positiveEffects = new List<WorldKey>();
    public List<WorldKey> negativeEffects = new List<WorldKey>();

    BitArray preconditionsPos, preconditionsNeg, effectsPos, effectsNeg;


    void OnEnable()
    {
        int n = Enum.GetValues(typeof(WorldKey)).Length;
        preconditionsPos = new BitArray(n);
        preconditionsNeg = new BitArray(n);
        effectsPos = new BitArray(n);
        effectsNeg = new BitArray(n);

        foreach (var k in positivePreconditions) preconditionsPos.Set((int)k, true);
        foreach (var k in negativePreconditions) preconditionsNeg.Set((int)k, true);
        foreach (var k in positiveEffects) effectsPos.Set((int)k, true);
        foreach (var k in negativeEffects) effectsNeg.Set((int)k, true);
    }

    public bool IsValid(BitArray _worldState)
    {
        for (int i = 0; i < preconditionsPos.Length; i++)
            if (preconditionsPos[i] && !_worldState[i]) 
            { 
                return false;
            }

        for (int i = 0; i < preconditionsNeg.Length; i++)
            if (preconditionsNeg[i] && _worldState[i]) 
            { 
                return false;
            }
        return true;
    }

    public BitArray ApplyEffects(BitArray _worldState)
    {
        BitArray newState = new BitArray(_worldState);

        for (int i = 0; i < effectsPos.Length; i++)
            if (effectsPos[i])
            {
                newState.Set(i, true);
            }

        for (int i = 0; i < effectsNeg.Length; i++)
            if (effectsNeg[i])
                newState.Set(i, false);

        return newState;
    }

    /// <summary>
    /// Start of the goap action
    /// </summary>
    /// <param name="_aiController"></param>
    public virtual void Enter(AIController _aiController)
    {
        _hasFailed = false;
    }

    /// <summary>
    /// Update of the goap action
    /// </summary>
    /// <param name="_aiController"></param>
    public abstract void Tick(AIController _aiController);

    /// <summary>
    /// Check if it is complete
    /// </summary>
    /// <param name="_aiController"></param>
    /// <returns></returns>
    public abstract bool IsComplete(AIController _aiController);

    /// <summary>
    /// Exit the goap actions
    /// </summary>
    /// <param name="_aiController"></param>
    public abstract void Exit(AIController _aiController);

    /// <summary>
    /// Check if the goap actions has failed
    /// </summary>
    /// <param name="_aiController"></param>
    /// <returns></returns>
    public virtual bool HasFailed(AIController _aiController)
    {
        return _hasFailed;
    }
}