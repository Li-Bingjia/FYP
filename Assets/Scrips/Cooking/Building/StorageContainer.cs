using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class StorageSaveData
{
    public List<int> itemIDs = new();
}

public class StorageContainer : MonoBehaviour
{
    [Header("Storage Settings")]
    public List<Item> storedItems = new List<Item>();
    [SerializeField] private int maxCapacity = 10;
    [SerializeField] private float dropDistance = 0.8f;
    public static StorageContainer CurrentOpenContainer { get; set; }

    private Vector3 originalScale;
    private bool isPlayerInRange;
    private InGameUIManager uiManager;
    private Collider myCollider;

    // 需要在Inspector拖入所有Item资源（和背包allItems类似）
    public Item[] allItems;

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
        // 关闭储物柜时清空CurrentOpenContainer
        if (StorageUIController.Instance != null && StorageUIController.Instance.storagePanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                StorageUIController.Instance.CloseStorage();
                CurrentOpenContainer = null;
            }
            return;
        }

        // 打开储物柜
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (StorageUIController.Instance != null)
            {
                CurrentOpenContainer = this;
                LoadStorage(); // 打开时自动加载
                StorageUIController.Instance.OpenStorage();
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
        Item item = other.GetComponent<ItemBehaviour>()?.item;
        if (item != null && storedItems.Count < maxCapacity)
        {
            AddItem(item);
            other.gameObject.SetActive(false); // 物理物品消失
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

    public void AddItem(Item item)
    {
        if (storedItems.Count < maxCapacity)
        {
            storedItems.Add(item);
            StartCoroutine(PlayBounceEffect());
        }
    }

    public void RemoveItem(int index)
    {
        if (index >= 0 && index < storedItems.Count)
        {
            Item item = storedItems[index];
            storedItems.RemoveAt(index);

            // 这里可以实例化物理物品到场景
            if (item.prefab3D != null)
            {
                Vector3 dropPos = CalculateDropPosition(0.3f);
                GameObject obj = Instantiate(item.prefab3D, dropPos, Quaternion.identity);
                // 可选：给 obj 添加 ItemBehaviour 并赋值 item
                var behaviour = obj.GetComponent<ItemBehaviour>();
                if (behaviour != null)
                    behaviour.item = item;
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
        while (elapsed < duration)
        {
            transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        elapsed = 0;
        while (elapsed < duration)
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

    // ====== 储存与加载功能 ======
    private string GetSaveKey() => $"Storage_{gameObject.GetInstanceID()}";

    public void SaveStorage()
    {
        List<int> ids = new();
        foreach (var item in storedItems)
            ids.Add(item.ID);
        string json = JsonUtility.ToJson(new StorageSaveData { itemIDs = ids });
        PlayerPrefs.SetString(GetSaveKey(), json);
        PlayerPrefs.Save();
    }

    public void LoadStorage()
    {
        storedItems.Clear();
        string key = GetSaveKey();
        if (PlayerPrefs.HasKey(key))
        {
            string json = PlayerPrefs.GetString(key);
            StorageSaveData data = JsonUtility.FromJson<StorageSaveData>(json);
            foreach (int id in data.itemIDs)
            {
                if (allItems != null && id >= 0 && id < allItems.Length && allItems[id] != null)
                    storedItems.Add(allItems[id]);
            }
        }
    }
}