using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameData : MonoBehaviour
{
    [SerializeField] private Formation[] formations;

    public Formation[] Formations { get { return formations; } }
}
