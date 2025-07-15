using System.Collections.Generic;
using UnityEngine;

public abstract class Formation : ScriptableObject
{
    [SerializeField] public string FormationName = "Formation";
    [SerializeField] private int maxCount = 20;
    public int MaxCount { get { return maxCount; } }
    public abstract List<Vector3> CalculateOffsets(int _agentNumber, float _distanceBetweenAgents);
}
