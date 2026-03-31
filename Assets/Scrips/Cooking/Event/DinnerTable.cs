using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiningTable : MonoBehaviour
{
    public List<CustomerNPC> currentCustomers = new List<CustomerNPC>();

    public void AddCustomer(CustomerNPC customer)
    {
        if (!currentCustomers.Contains(customer))
        {
            currentCustomers.Add(customer);
        }
    }

    public void RemoveCustomer(CustomerNPC customer)
    {
        if (currentCustomers.Contains(customer))
        {
            currentCustomers.Remove(customer);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        CheckFood(collision.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        CheckFood(other.gameObject);
    }

    private void CheckFood(GameObject item)
    {
        foreach (var customer in currentCustomers)
        {
            if (customer == null) continue;

            if (customer.currentState == CustomerNPC.State.WaitingForFood || customer.currentState == CustomerNPC.State.Sitting)
            {
                if (item.name.Contains(customer.wantedDishName))
                {
                    // 这里补全参数
                    customer.ReceiveFood(item, customer.wantedDishIcon, false);
                    return; 
                }
            }
        }
    }


    public bool CanBeMoved()
    {
        return currentCustomers.Count == 0;
    }
}