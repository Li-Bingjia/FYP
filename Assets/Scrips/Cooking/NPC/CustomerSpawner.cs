using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    [Header("Settings")]
    public GameObject npcPrefab;
    public Transform spawnPoint; 
    public Transform exitPoint; 
    
    [Header("Menu")]

    public List<DishData> menu; 

    [System.Serializable]
    public struct DishData
    {
        public string dishName;
        public int price;
        public Sprite icon;
    }

    private Building buildingSystem;
    void Start() {
        buildingSystem = FindObjectOfType<Building>();
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(10f);
            
            if (buildingSystem != null && buildingSystem.isBuildMode)
            {
                continue; 
            }
            
            TrySpawnCustomers();
        }
    }

    private void TrySpawnCustomers()
    {
        DiningChair[] allChairs = FindObjectsOfType<DiningChair>();
        List<DiningChair> validChairs = new List<DiningChair>();

        foreach (var chair in allChairs)
        {
            if (!chair.isOccupied && chair.HasTableNearby())
            {
                validChairs.Add(chair);
            }
        }

        if (validChairs.Count == 0) return;

        int spawnCount = (validChairs.Count >= 2 && Random.value > 0.5f) ? 2 : 1;
        spawnCount = Mathf.Min(spawnCount, validChairs.Count);

        for (int i = 0; i < spawnCount; i++)
        {
            int index = Random.Range(0, validChairs.Count);
            DiningChair selectedChair = validChairs[index];
            validChairs.RemoveAt(index);

            SpawnNPC(selectedChair);
        }
    }

    private void SpawnNPC(DiningChair chair)
    {
        if (menu.Count == 0) return;

        GameObject npcObj = Instantiate(npcPrefab, spawnPoint.position, Quaternion.identity);
        CustomerNPC npc = npcObj.GetComponent<CustomerNPC>();
        DishData dish = menu[Random.Range(0, menu.Count)];
        
        npc.Initialize(chair, exitPoint.position, dish.dishName, dish.price, dish.icon);
    }
}