using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Battlefield : MonoBehaviour
{
    public Vector3 alliesPosition;
    public Vector3 enemiesPosition;

    public Vector3 distanceBetweenAllies;
    public Vector3 distanceBetweenEnemies;

    public void PlaceUnits(List<Transform> units, bool unitsAreAllies = true)
    {
        Vector3 unitPosition;

        if(unitsAreAllies)
        {
            unitPosition = alliesPosition - (distanceBetweenAllies * (units.Count - 1)/2);

            for(int i = 0; i < units.Count; i++)
            {
                units[i].position = unitPosition;
                unitPosition += distanceBetweenAllies;
            }
        }
        else
        {
            unitPosition = enemiesPosition - (distanceBetweenEnemies * (units.Count - 1)/2);

            for(int i = 0; i < units.Count; i++)
            {
                units[i].position = unitPosition;
                unitPosition += distanceBetweenEnemies;
            }
        }
    }

    public void PlaceUnit(Transform unit, Vector3 position)
    {
        Debug.LogWarning("Place Unit not implemented!");
    }
}
