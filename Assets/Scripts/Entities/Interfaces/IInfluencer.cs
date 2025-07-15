using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInfluencer
{
    ETeam teamForInfluence { get; }
    InfluenceType influenceType { get; }
    Vector3 positionForInfluence { get; }
    float GetDropOff(int distance);
    float GetRadius();
}