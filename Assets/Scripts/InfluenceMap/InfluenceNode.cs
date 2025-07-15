using System;
using System.Collections.Generic;
using UnityEngine;

public enum InfluenceType
{
    Military,
    Monetary,
    Factory
}

public class InfluenceNode : Node
{
    private Dictionary<InfluenceKey, float> influences = new();

    public void SetInfluenceForTeam(ETeam observer, ETeam source, InfluenceType type, float value)
    {
        var key = new InfluenceKey(observer, source, type);
        influences.TryGetValue(key, out float current);
        influences[key] = current + value;
    }

    public float GetInfluence(ETeam observer, ETeam source, InfluenceType type)
    {
        var key = new InfluenceKey(observer, source, type);
        return influences.TryGetValue(key, out var value) ? value : 0f;
    }

    public void ResetInfluence()
    {
        influences.Clear();
    }

    public ETeam GetDominantTeam(ETeam observer, InfluenceType type)
    {
        float maxInfluence = 0f;
        ETeam dominant = ETeam.Neutral;

        foreach (var pair in influences)
        {
            var key = pair.Key;
            if (key.Observer == observer && key.Type == type)
            {
                if (pair.Value > maxInfluence)
                {
                    maxInfluence = pair.Value;
                    dominant = key.Source;
                }
            }
        }

        return dominant;
    }

}
