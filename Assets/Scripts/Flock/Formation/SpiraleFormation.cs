using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Formations/SpiraleFormation")]
public class SpiraleFormation : Formation
{
    public override List<Vector3> CalculateOffsets(int _agentNumber, float _distanceBetweenAgents)
    {
        List<Vector3> offsets = new List<Vector3>(_agentNumber);

        float angleStep = 30f; // angle en degrés entre chaque agent
        float currentAngle = 0f;
        float radiusFactor = 2.5f; // facteur pour espacer la spirale (plus grand = plus d'espace)

        for (int i = 0; i < _agentNumber; i++)
        {
            if (i == 0)
            {
                offsets.Add(Vector3.zero); // premier agent au centre
                continue;
            }

            // Le rayon augmente plus vite avec radiusFactor
            float radius = radiusFactor * _distanceBetweenAgents * (currentAngle / 360f);

            float rad = Mathf.Deg2Rad * currentAngle;
            float x = Mathf.Cos(rad) * radius;
            float z = Mathf.Sin(rad) * radius;

            offsets.Add(new Vector3(x, 0f, z));

            currentAngle += angleStep;
        }

        return offsets;
    }


}
