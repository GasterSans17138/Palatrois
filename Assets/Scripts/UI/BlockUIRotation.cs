using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockUIRotation : MonoBehaviour
{
    [SerializeField] private Vector3 VectorRotationBlocked = new Vector3(90, 0, 0);
    void LateUpdate()
    {
        transform.rotation = Quaternion.Euler(VectorRotationBlocked);
    }
}
