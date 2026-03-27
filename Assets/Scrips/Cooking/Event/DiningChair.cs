using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiningChair : MonoBehaviour
{
    public bool isOccupied = false;
    public DiningTable linkedTable; 

    public bool HasTableNearby()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, 1.5f);
        foreach (var hit in hits)
        {
            DiningTable table = hit.GetComponentInParent<DiningTable>(); 
            if (table != null)
            {
                linkedTable = table;
                return true;
            }
        }
        return false;
    }

    public bool CanBeMoved()
    {
        return !isOccupied;
    }
}