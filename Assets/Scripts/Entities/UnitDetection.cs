using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class UnitDetection : MonoBehaviour
{
    Unit unit;

    public List<BaseEntity> ennemiesInRange = new List<BaseEntity>();

    void Awake()
    {
        unit = GetComponentInParent<Unit>();

        SphereCollider collider = GetComponent<SphereCollider>();
        collider.isTrigger = true;
        collider.radius = unit.GetUnitData.AttackDistanceMax;
    }

    void OnTriggerEnter(Collider _other)
    {
        BaseEntity enemy = _other.GetComponent<BaseEntity>();
        if (enemy != null && enemy.GetTeam() != unit.GetTeam() && enemy.IsAlive)
        {
            ennemiesInRange.Add(enemy);
        }
    }

    void OnTriggerExit(Collider _other)
    {
        BaseEntity enemy = _other.GetComponent<BaseEntity>();
        if (enemy != null)
        {
            ennemiesInRange.Remove(enemy);
        }
    }

    public BaseEntity GetClosestEnemy()
    {
        BaseEntity closestEnemy = null;
        float closestEnemySqr = float.MaxValue;
        Vector3 pos = transform.position;

        for (int i = 0; i <= ennemiesInRange.Count - 1; i++)
        {
            BaseEntity unitEnemy = ennemiesInRange[i];

            if (!unitEnemy.IsAlive)
            {
                ennemiesInRange.RemoveAt(i);
                continue;
            }

            float sqr = (unitEnemy.transform.position - pos).sqrMagnitude;
            if (sqr < closestEnemySqr)
            {
                closestEnemySqr = sqr;
                closestEnemy = unitEnemy;
            }
        }
        return closestEnemy;
    }
}