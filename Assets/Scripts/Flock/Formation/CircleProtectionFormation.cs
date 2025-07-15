using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Formations/CircleProtectionFormation")]
public class CircleProtectionFormation : Formation
{
    public override List<Vector3> CalculateOffsets(int _agentNumber, float _distanceBetweenAgents)
    {
        List<Vector3> offsets = new List<Vector3>(_agentNumber);

        int unitsRemaining = _agentNumber;

        // On commence par placer le centre
        if (unitsRemaining > 0)
        {
            offsets.Add(Vector3.zero);
            unitsRemaining--;
        }

        int layer = 1;
        float angleOffset = 0f;

        while (unitsRemaining > 0)
        {
            // Nombre maximal d'unités sur cette couche
            float radius = layer * _distanceBetweenAgents;
            int unitsInLayer = Mathf.CeilToInt(2 * Mathf.PI * radius / _distanceBetweenAgents);
            unitsInLayer = Mathf.Min(unitsInLayer, unitsRemaining);

            float angleStep = 360f / unitsInLayer;

            for (int i = 0; i < unitsInLayer; i++)
            {
                float angle = Mathf.Deg2Rad * (angleOffset + angleStep * i);
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;
                offsets.Add(offset);
            }

            unitsRemaining -= unitsInLayer;
            layer++;
            angleOffset += angleStep / 2f; // décalage pour un meilleur rendu visuel
        }

        return offsets;
    }


}
