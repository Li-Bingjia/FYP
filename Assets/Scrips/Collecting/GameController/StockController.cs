using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StockController : MonoBehaviour
{
    public GameObject stockPanel;
    public GameObject slockPrefab;
    public int stockCount;
    public GameObject[] itemPerfabs; // 索引与Item.ID一致
    public RectTransform slotsRoot;
    public static StockController Instance { get; private set; }
    Dictionary<int, int> itemsCountCache = new();
    public event Action OnStockChanged;

    // 新增：用于还原Item对象
    public Item[] allItems; // 请在Inspector中配置，索引与ID一致

    const string StockSaveKey = "StockSaveData";

    [Serializable]
    public class StockSaveData
    {
        public List<int> itemIDs = new();
        public List<int> quantities = new();
    }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // 初始化slot
        for (int i = 0; i < stockCount; i++)
        {
            Slot slot = Instantiate(slockPrefab, slotsRoot).GetComponent<Slot>();
            slot.currentItem = null;
        }
        LoadStock(); // 启动时自动加载
        RebuildItemCounts();
    }

    public void RebuildItemCounts()
    {
        itemsCountCache.Clear();
        foreach (Transform slotTransform in slotsRoot)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot.currentItem != null)
            {
                ItemBehaviour itemBehaviour = slot.currentItem.GetComponent<ItemBehaviour>();
                if (itemBehaviour != null && itemBehaviour.item != null)
                {
                    itemsCountCache[itemBehaviour.item.ID] = itemsCountCache.GetValueOrDefault(itemBehaviour.item.ID, 0) + itemBehaviour.quantity;
                }
            }
        }
        OnStockChanged?.Invoke();
    }

    public Dictionary<int, int> GetItemCounts() => itemsCountCache;

    public void AddItemToStock(Item item)
    {
        if (itemPerfabs == null || itemPerfabs.Length == 0)
            return;

        foreach (Transform child in slotsRoot)
        {
            Slot slot = child.GetComponent<Slot>();
            if (slot.currentItem == null)
            {
                GameObject itemObj = Instantiate(itemPerfabs[item.ID], slot.transform);
                itemObj.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                slot.currentItem = itemObj;

                var drag = itemObj.GetComponent<ItemDragHandler>();
                if (drag == null)
                    drag = itemObj.AddComponent<ItemDragHandler>();
                if (itemObj.GetComponent<CanvasGroup>() == null)
                    itemObj.AddComponent<CanvasGroup>();

                drag.item = item;

                var itemBehaviour = itemObj.GetComponent<ItemBehaviour>();
                if (itemBehaviour != null)
                {
                    itemBehaviour.item = item;
                    itemBehaviour.quantity = 1;
                }

                var img = itemObj.GetComponent<UnityEngine.UI.Image>();
                if (img != null && item.icon != null)
                    img.sprite = item.icon;

                break;
            }
        }
        RebuildItemCounts();
        SaveStock(); // 自动保存
    }

    public void RemoveItemFromStock(int itemID, int amountToRemove)
    {
        foreach (Transform slotTransform in slotsRoot)
        {
            if (amountToRemove <= 0) break;
            Slot slot = slotTransform.GetComponent<Slot>();
            var itemBehaviour = slot?.currentItem?.GetComponent<ItemBehaviour>();
            if (itemBehaviour != null && itemBehaviour.item != null && itemBehaviour.item.ID == itemID)
            {
                int removed = Mathf.Min(amountToRemove, itemBehaviour.quantity);
                itemBehaviour.quantity -= removed;
                amountToRemove -= removed;
                if (itemBehaviour.quantity == 0)
                {
                    Destroy(slot.currentItem);
                    slot.currentItem = null;
                }
            }
        }
        RebuildItemCounts();
        SaveStock(); // 自动保存
    }

    // 保存仓库内容
    public void SaveStock()
    {
        StockSaveData data = new();
        foreach (Transform slotTransform in slotsRoot)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            var itemBehaviour = slot.currentItem?.GetComponent<ItemBehaviour>();
            if (itemBehaviour != null && itemBehaviour.item != null)
            {
                data.itemIDs.Add(itemBehaviour.item.ID);
                data.quantities.Add(itemBehaviour.quantity);
            }
            else
            {
                data.itemIDs.Add(-1); // -1表示空
                data.quantities.Add(0);
            }
        }
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(StockSaveKey, json);
        PlayerPrefs.Save();
    }

    // 加载仓库内容
    public void LoadStock()
    {
        if (!PlayerPrefs.HasKey(StockSaveKey)) return;
        string json = PlayerPrefs.GetString(StockSaveKey);
        StockSaveData data = JsonUtility.FromJson<StockSaveData>(json);

        // 清空现有slot
        int slotIndex = 0;
        foreach (Transform slotTransform in slotsRoot)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot.currentItem != null)
            {
                Destroy(slot.currentItem);
                slot.currentItem = null;
            }
            // 还原
            if (slotIndex < data.itemIDs.Count && data.itemIDs[slotIndex] >= 0 && data.quantities[slotIndex] > 0)
            {
                int id = data.itemIDs[slotIndex];
                int qty = data.quantities[slotIndex];
                if (id >= 0 && id < itemPerfabs.Length && itemPerfabs[id] != null && allItems != null && id < allItems.Length && allItems[id] != null)
                {
                    GameObject itemObj = Instantiate(itemPerfabs[id], slot.transform);
                    itemObj.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                    var itemBehaviour = itemObj.GetComponent<ItemBehaviour>();
                    if (itemBehaviour != null)
                    {
                        itemBehaviour.item = allItems[id]; // 还原Item对象
                        itemBehaviour.quantity = qty;
                    }
                    slot.currentItem = itemObj;
                }
            }
            slotIndex++;
        }
        RebuildItemCounts();
    }
}