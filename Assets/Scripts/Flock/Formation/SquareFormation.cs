using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Formations/SquareFormation")]
public class SquareFormation : Formation
{
    public override List<Vector3> CalculateOffsets(int _agentNumber, float _distanceBetweenAgents)
    {
        List<Vector3> offsets = new List<Vector3>(_agentNumber);

        if (_agentNumber <= 0)
            return offsets;

        // Ajouter le centre en premier
        offsets.Add(Vector3.zero);

        // Taille de grille minimale en carré (sans exclure le centre)
        int gridSize = Mathf.CeilToInt(Mathf.Sqrt(_agentNumber));

        Vector3 centerOffset = new Vector3(
            (gridSize - 1) * _distanceBetweenAgents / 2f,
            0f,
            (gridSize - 1) * _distanceBetweenAgents / 2f
        );

        int npcIndex = 1; // On a déjà ajouté le centre (première unité)
        for (int row = 0; row < gridSize; row++)
        {
            for (int col = 0; col < gridSize; col++)
            {
                // Calculer la position courante
                Vector3 offset = new Vector3(
                    col * _distanceBetweenAgents,
                    0f,
                    row * _distanceBetweenAgents
                ) - centerOffset;

                // Sauter la case centrale (Vector3.zero déjà prise)
                if (offset == Vector3.zero)
                    continue;

                if (npcIndex >= _agentNumber)
                    break;

                offsets.Add(offset);
                npcIndex++;
            }

            if (npcIndex >= _agentNumber)
                break;
        }

        return offsets;
    }


}