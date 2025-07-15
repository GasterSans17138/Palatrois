using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Formations/VFormation")]
public class VFormation : Formation
{
    public override List<Vector3> CalculateOffsets(int _agentNumber, float _distanceBetweenAgents)
    {
        _distanceBetweenAgents /= 2f;
        List<Vector3> offsets = new List<Vector3>(_agentNumber);

        for (int i = 0; i < _agentNumber; i++)
        {
            if (i == 0)
            {
                offsets.Add(Vector3.zero); // Le leader au sommet du V
            }
            else
            {
                int index = i - 1;
                float row = Mathf.Floor(index / 2f) + 1;
                float sideMultiplier = (index % 2 == 0) ? 1f : -1f;

                Vector3 localOffset =
                      Vector3.back * row * _distanceBetweenAgents
                    + Vector3.right * sideMultiplier * row * _distanceBetweenAgents;

                offsets.Add(localOffset);
            }
        }

        return offsets;
    }
}
