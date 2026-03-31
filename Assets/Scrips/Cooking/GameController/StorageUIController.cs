using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StorageUIController : MonoBehaviour
{
    public static StorageUIController Instance { get; private set; }

    [Header("UI References")]
    public GameObject storagePanel;
    public GameObject slotPrefab;
    public RectTransform slotsRoot;
    public int slotCount = 12;
    public Vector2 cellSize = new Vector2(100, 100);
    public Vector2 spacing = new Vector2(10, 10);
    public GameObject[] itemUIPrefabs;
    public Item[] allItems; // 在Inspector拖入所有Item资源

    private List<Slot> slots = new();

    public event System.Action OnStorageChanged;

    void Awake()
    {
        Instance = this;
        storagePanel.SetActive(false);

        var grid = slotsRoot.GetComponent<GridLayoutGroup>();
        if (grid != null)
        {
            grid.cellSize = cellSize;
            grid.spacing = spacing;
        }

        for (int i = 0; i < slotCount; i++)
        {
            var slotObj = Instantiate(slotPrefab, slotsRoot);
            var slot = slotObj.GetComponent<Slot>();
            slots.Add(slot);
            slot.currentItem = null;
        }
    }

    public void OpenStorage()
    {
        if (StorageContainer.CurrentOpenContainer != null)
            StorageContainer.CurrentOpenContainer.LoadStorage();

        storagePanel.SetActive(true);
        RefreshUI();
    }

    public void CloseStorage()
    {
        if (StorageContainer.CurrentOpenContainer != null)
            StorageContainer.CurrentOpenContainer.SaveStorage();

        storagePanel.SetActive(false);
    }

    void RefreshUI()
    {
        List<Item> storedItems = new List<Item>();
        if (StorageContainer.CurrentOpenContainer != null)
        {
            storedItems = StorageContainer.CurrentOpenContainer.storedItems;
        }

        // 先清空所有slot下的UI物体
        foreach (var slot in slots)
        {
            for (int i = slot.transform.childCount - 1; i >= 0; i--)
            {
                Destroy(slot.transform.GetChild(i).gameObject);
            }
            slot.currentItem = null;
        }

        // 重新生成UI物品
        for (int i = 0; i < storedItems.Count && i < slots.Count; i++)
        {
            var item = storedItems[i];
            if (item == null || item.ID < 0) continue;
            int prefabIndex = item.ID - 1;
            if (prefabIndex >= 0 && prefabIndex < itemUIPrefabs.Length && itemUIPrefabs[prefabIndex] != null)
            {
                var itemObj = Instantiate(itemUIPrefabs[prefabIndex], slots[i].transform);
                slots[i].currentItem = itemObj;

                var drag = itemObj.GetComponent<ItemDragHandler>() ?? itemObj.AddComponent<ItemDragHandler>();
                drag.item = item;

                var img = itemObj.GetComponent<UnityEngine.UI.Image>();
                if (img != null && item.icon != null)
                    img.sprite = item.icon;
            }
        }
    }

    public void RemoveItemFromStorage(int slotIndex)
    {
        if (StorageContainer.CurrentOpenContainer == null) return;
        StorageContainer.CurrentOpenContainer.RemoveItem(slotIndex);
        RefreshUI();
        OnStorageChanged?.Invoke();
        if (StorageContainer.CurrentOpenContainer != null)
            StorageContainer.CurrentOpenContainer.SaveStorage();
    }

    public void AddItemToStorage(Item item)
    {
        if (StorageContainer.CurrentOpenContainer == null) return;
        if (item == null) return; // 防止null
        StorageContainer.CurrentOpenContainer.AddItem(item);
        RefreshUI();
        OnStorageChanged?.Invoke();
        if (StorageContainer.CurrentOpenContainer != null)
            StorageContainer.CurrentOpenContainer.SaveStorage();
    }

    public List<Item> GetStoredItems()
    {
        if (StorageContainer.CurrentOpenContainer != null)
            return StorageContainer.CurrentOpenContainer.storedItems;
        return new List<Item>();
    }
}