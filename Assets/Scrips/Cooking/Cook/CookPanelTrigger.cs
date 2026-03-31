using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CookPanelTrigger : MonoBehaviour
{
    public GameObject cookPanel; 

    private bool playerInRange = false;

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (cookPanel != null)
                cookPanel.SetActive(true); 
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}