using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StorageUIController : MonoBehaviour
{
    public static StorageUIController Instance { get; private set; }

    [Header("UI References")]
    public GameObject storagePanel;           // 储物箱弹窗
    public GameObject slotPrefab;             // 储物格Prefab
    public RectTransform slotsRoot;           // 存放格子的父物体（挂GridLayoutGroup）
    public int slotCount = 12;                // 格子数量
    public Vector2 cellSize = new Vector2(100, 100); // 格子大小
    public Vector2 spacing = new Vector2(10, 10);    // 格子间距
    public GameObject[] itemUIPrefabs;        // UI物品Prefab，索引与Item.ID一致

    private List<Slot> slots = new();
    private List<Item> storedItems = new();

    public event System.Action OnStorageChanged;

    void Awake()
    {
        Instance = this;
        storagePanel.SetActive(false);

        // 自动设置GridLayoutGroup参数
        var grid = slotsRoot.GetComponent<GridLayoutGroup>();
        if (grid != null)
        {
            grid.cellSize = cellSize;
            grid.spacing = spacing;
        }
    }

    public void OpenStorage(List<Item> items)
    {
        storagePanel.SetActive(true);
        storedItems = new List<Item>(items);
        RefreshUI();
    }

    public void CloseStorage()
    {
        storagePanel.SetActive(false);
    }

    void RefreshUI()
    {
        // 清空旧格子
        foreach (Transform child in slotsRoot)
            Destroy(child.gameObject);
        slots.Clear();

        // 生成新格子
        for (int i = 0; i < slotCount; i++)
        {
            var slotObj = Instantiate(slotPrefab, slotsRoot);
            var slot = slotObj.GetComponent<Slot>();
            slots.Add(slot);

            if (i < storedItems.Count)
            {
                int prefabIndex = storedItems[i].ID;
                if (prefabIndex >= 0 && prefabIndex < itemUIPrefabs.Length && itemUIPrefabs[prefabIndex] != null)
                {
                    var itemObj = Instantiate(itemUIPrefabs[prefabIndex], slotObj.transform);
                    var drag = itemObj.GetComponent<ItemDragHandler>() ?? itemObj.AddComponent<ItemDragHandler>();
                    drag.item = storedItems[i];
                    slot.currentItem = itemObj;
                }
                else
                {
                    Debug.LogError($"itemUIPrefabs数组未正确配置，ID={prefabIndex} 超出范围或Prefab为null！");
                }
            }
        }
    }

    public void MoveItemToStock(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= storedItems.Count) return;
        var item = storedItems[slotIndex];
        StockController.Instance.AddItemToStock(item);
        storedItems.RemoveAt(slotIndex);
        RefreshUI();
        OnStorageChanged?.Invoke();
    }

    public void DropItemToWorld(int slotIndex, Vector3 worldPos)
    {
        if (slotIndex < 0 || slotIndex >= storedItems.Count) return;
        var item = storedItems[slotIndex];
        var prefab = itemUIPrefabs[item.ID];
        Instantiate(prefab, worldPos, Quaternion.identity);
        storedItems.RemoveAt(slotIndex);
        RefreshUI();
        OnStorageChanged?.Invoke();
    }

    public void AddItemFromWorld(Item item)
    {
        if (storedItems.Count >= slotCount) return;
        storedItems.Add(item);
        RefreshUI();
        OnStorageChanged?.Invoke();
    }

    public List<Item> GetStoredItems() => storedItems;
    public void RemoveItemFromStorage(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= storedItems.Count) return;
        storedItems.RemoveAt(slotIndex);
        RefreshUI();
        OnStorageChanged?.Invoke();
    }
}