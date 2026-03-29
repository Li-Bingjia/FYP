using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StorageContainer : MonoBehaviour
{
    [Header("Storage Settings")]
    public List<GameObject> storedItems = new List<GameObject>();
    [SerializeField] private int maxCapacity = 10;
    [SerializeField] private float dropDistance = 0.8f;
    
    private Vector3 originalScale;
    private bool isPlayerInRange;
    private InGameUIManager uiManager;
    private Collider myCollider;

    void Awake()
    {
        originalScale = transform.localScale;
        myCollider = GetComponent<Collider>(); 
    }

    void Start()
    {
        uiManager = Object.FindFirstObjectByType<InGameUIManager>();
    }

    void Update()
    {
        if (StorageUIController.Instance != null && StorageUIController.Instance.storagePanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                StorageUIController.Instance.CloseStorage();
            }
            return; 
        }

        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (StorageUIController.Instance != null)
            {
                List<Item> itemList = new List<Item>();
                foreach (var go in storedItems)
                {
                    var itemBehaviour = go.GetComponent<ItemBehaviour>();
                    if (itemBehaviour != null && itemBehaviour.item != null)
                        itemList.Add(itemBehaviour.item);
                }
                StorageUIController.Instance.OpenStorage(itemList);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 玩家检测
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            return;
        }

        // 物品放入逻辑
        if (other.gameObject.layer == LayerMask.NameToLayer("Pickable") && storedItems.Count < maxCapacity)
        {
            if (other.attachedRigidbody != null)
            {
                AddItem(other.gameObject);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            // 玩家离开时自动关闭 UI
            if (uiManager != null && uiManager.GetCurrentContainer() == this)
            {
                uiManager.ToggleStorageUI(this);
            }
        }
    }

    private void AddItem(GameObject item)
    {
        storedItems.Add(item);
        item.SetActive(false); 
        
        item.transform.SetParent(transform); 
        item.transform.localPosition = Vector3.zero;

        StartCoroutine(PlayBounceEffect());
    }

    public void RemoveItem(int index)
    {
        if (index >= 0 && index < storedItems.Count)
        {
            GameObject item = storedItems[index];
            storedItems.RemoveAt(index);
            
            item.SetActive(true);
            item.transform.SetParent(null);

            float itemRadius = 0.3f;
            Collider itemCol = item.GetComponent<Collider>();
            if (itemCol != null) itemRadius = itemCol.bounds.extents.x;

            item.transform.position = CalculateDropPosition(itemRadius);
            
            Rigidbody rb = item.GetComponent<Rigidbody>();
            if (rb)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }

    private Vector3 CalculateDropPosition(float itemRadius)
    {
        float randomAngle = Random.Range(0f, 360f);
        Vector3 randomDir = Quaternion.Euler(0, randomAngle, 0) * Vector3.forward;
        Vector3 spawnPos = transform.position + (randomDir * (dropDistance + itemRadius));
        
        if (myCollider != null) spawnPos.y = myCollider.bounds.center.y; 
        else spawnPos.y = transform.position.y + 0.5f;

        return spawnPos;
    }

    private IEnumerator PlayBounceEffect()
    {
        float duration = 0.15f;
        Vector3 targetScale = originalScale * 1.1f; 
        
        float elapsed = 0;
        while(elapsed < duration)
        {
            transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        elapsed = 0;
        while(elapsed < duration)
        {
            transform.localScale = Vector3.Lerp(targetScale, originalScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localScale = originalScale;
    }

    public bool IsEmpty()
    {
        return storedItems.Count == 0;
    }
}