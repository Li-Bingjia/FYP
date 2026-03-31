using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class BackpackSaveData
{
    public List<int> itemIDs = new();
}

public class BackpackUIController : MonoBehaviour
{
    public static BackpackUIController Instance { get; private set; }

    [Header("UI References")]
    public GameObject backpackPanel;         // 背包栏主面板
    public GameObject slotPrefab;            // 背包格Prefab
    public RectTransform slotsRoot;          // 存放格子的父物体（挂GridLayoutGroup）
    public int slotCount = 20;               // 背包格数量
    public Vector2 cellSize = new Vector2(100, 100);
    public Vector2 spacing = new Vector2(10, 10);
    public GameObject[] itemUIPrefabs;       // UI物品Prefab，索引与Item.ID一致
    public Item[] allItems;                  // 所有Item资源，索引与ID一致

    private List<Slot> slots = new();
    private List<Item> backpackItems = new();

    public event System.Action OnBackpackChanged;

    const string BackpackSaveKey = "BackpackSaveData";

    void Awake()
    {
        Instance = this;

        if (backpackPanel == null)
        {
            Debug.LogError("BackpackUIController: backpackPanel is not assigned. Please assign it in the Inspector.");
            return;
        }

        backpackPanel.SetActive(false);

        var grid = slotsRoot.GetComponent<GridLayoutGroup>();
        if (grid != null)
        {
            grid.cellSize = cellSize;
            grid.spacing = spacing;
        }

        // 初始化格子
        for (int i = 0; i < slotCount; i++)
        {
            var slotObj = Instantiate(slotPrefab, slotsRoot);
            var slot = slotObj.GetComponent<Slot>();
            slots.Add(slot);
            slot.currentItem = null;
        }

        LoadBackpack();
        RefreshUI();
    }

    void Update()
    {
        if (backpackPanel == null) return;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (backpackPanel.activeSelf)
                CloseBackpack();
            else
                OpenBackpack();
        }
    }

    public void OpenBackpack()
    {
        if (backpackPanel == null) return;
        backpackPanel.SetActive(true);
    }

    public void CloseBackpack()
    {
        if (backpackPanel == null) return;
        backpackPanel.SetActive(false);
    }

    void RefreshUI()
    {
        // 先清空所有slot的currentItem
        foreach (var slot in slots)
        {
            if (slot.currentItem != null)
            {
                Destroy(slot.currentItem);
                slot.currentItem = null;
            }
        }

        // 重新生成UI物品
        for (int i = 0; i < backpackItems.Count && i < slots.Count; i++)
        {
            int prefabIndex = backpackItems[i].ID;
            if (prefabIndex >= 0 && prefabIndex < itemUIPrefabs.Length && itemUIPrefabs[prefabIndex] != null)
            {
                var itemObj = Instantiate(itemUIPrefabs[prefabIndex], slots[i].transform);
                var drag = itemObj.GetComponent<ItemDragHandler>() ?? itemObj.AddComponent<ItemDragHandler>();
                drag.item = backpackItems[i];
                slots[i].currentItem = itemObj;

                // 强制设置icon
                var img = itemObj.GetComponent<UnityEngine.UI.Image>();
                if (img != null && backpackItems[i].icon != null)
                    img.sprite = backpackItems[i].icon;
            }
            else
            {
                Debug.LogError($"itemUIPrefabs未配置，ID={prefabIndex} 超出范围或Prefab为null！");
            }
        }
    }

    // 拖拽到背包栏
    public void AddItemToBackpack(Item item)
    {
        if (backpackItems.Count >= slotCount) return;
        backpackItems.Add(item);
        RefreshUI();
        OnBackpackChanged?.Invoke();
        SaveBackpack();
    }

    // 从背包栏移除（如拖到仓库或丢弃）
    public void RemoveItemFromBackpack(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= backpackItems.Count) return;
        backpackItems.RemoveAt(slotIndex);
        RefreshUI();
        OnBackpackChanged?.Invoke();
        SaveBackpack();
    }

    // 支持跨UI拖拽（如仓库⇄背包）
    public void MoveItemToOtherUI(int slotIndex, System.Action<Item> addToOtherUI)
    {
        if (slotIndex < 0 || slotIndex >= backpackItems.Count) return;
        var item = backpackItems[slotIndex];
        addToOtherUI?.Invoke(item);
        RemoveItemFromBackpack(slotIndex);
        SaveBackpack();
    }

    public List<Item> GetBackpackItems() => backpackItems;

    public void SaveBackpack()
    {
        BackpackSaveData data = new();
        foreach (var item in backpackItems)
            data.itemIDs.Add(item.ID);
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(BackpackSaveKey, json);
        PlayerPrefs.Save();
    }

    public void LoadBackpack()
    {
        backpackItems.Clear();
        if (PlayerPrefs.HasKey(BackpackSaveKey))
        {
            string json = PlayerPrefs.GetString(BackpackSaveKey);
            BackpackSaveData data = JsonUtility.FromJson<BackpackSaveData>(json);
            foreach (int id in data.itemIDs)
            {
                if (allItems != null && id >= 0 && id < allItems.Length && allItems[id] != null)
                    backpackItems.Add(allItems[id]);
            }
        }
    }
}