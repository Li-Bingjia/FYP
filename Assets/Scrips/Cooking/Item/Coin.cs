using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
    public int value = 5;
    private InGameUIManager uiManager;

    void Start()
    {
        uiManager = Object.FindFirstObjectByType<InGameUIManager>();
    }

    public void Collect()
    {
        if (uiManager != null)
        {
            uiManager.AddMoney(value);
        }
        Destroy(gameObject);
    }
}