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

    void Start()
    {
        Instance = this;
        Debug.Log($"itemPerfabs == null? {itemPerfabs == null}, Length: {(itemPerfabs == null ? -1 : itemPerfabs.Length)}");
        for (int i = 0; i < stockCount; i++)
        {
            Slot slot = Instantiate(slockPrefab, slotsRoot).GetComponent<Slot>();
            Debug.Log($"生成Slot: {i}, 名称: {slot.gameObject.name}");

            if (i < itemPerfabs.Length && itemPerfabs[i] != null)
            {
                GameObject itemObj = Instantiate(itemPerfabs[i], slot.transform);
                itemObj.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                slot.currentItem = itemObj;
            }
            else
            {
                slot.currentItem = null;
            }
        }
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
    }
}