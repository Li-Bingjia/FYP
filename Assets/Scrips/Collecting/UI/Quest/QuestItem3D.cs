using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestItem3D : MonoBehaviour
{
    public string objectiveID;
    public Item item; // 只用Item

    public void Init(string id, Item itemObj = null)
    {
        objectiveID = id;
        item = itemObj;
    }

    public void OnPickUp()
    {
        if (StockController.Instance != null && item != null)
            StockController.Instance.AddItemToStock(item);
        Destroy(gameObject);
    }
}