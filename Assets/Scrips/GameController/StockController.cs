using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StockController : MonoBehaviour
{
    public GameObject stockPanel;
    public GameObject slockPrefab;
    public int stockCount;
    public GameObject[] itemPerfabs; // 这些Prefab要有ItemDragHandler和ItemData

    public static StockController Instance { get; private set; }
    Dictionary<int, int> itemsCountCache = new();
    public event Action OnStockChanged;

    void Start()
    {
        Instance = this;
        for (int i = 0; i < stockCount; i++)
        {
            Slot slot = Instantiate(slockPrefab, stockPanel.transform).GetComponent<Slot>();
            if (i < itemPerfabs.Length)
            {
                GameObject item = Instantiate(itemPerfabs[i], slot.transform);
                item.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                slot.currentItem = item;
            }
        }
        RebuildItemCounts();
    }

    public void RebuildItemCounts()
    {
        itemsCountCache.Clear();
        foreach (Transform slotTransform in stockPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot.currentItem != null)
            {
                ItemBehaviour itemBehaviour = slot.currentItem.GetComponent<ItemBehaviour>();
                if (itemBehaviour != null && itemBehaviour.item != null)
                {
                    //Debug.Log($"[StockController] RebuildItemCounts: item.ID={itemBehaviour.item.ID}, quantity={itemBehaviour.quantity}");
                    itemsCountCache[itemBehaviour.item.ID] = itemsCountCache.GetValueOrDefault(itemBehaviour.item.ID, 0) + itemBehaviour.quantity;
                }
            }
        }
        //Debug.Log($"[StockController] RebuildItemCounts: itemsCountCache.Count={itemsCountCache.Count}");
        OnStockChanged?.Invoke();
    }

    public Dictionary<int, int> GetItemCounts() => itemsCountCache;

    public void AddItemToStock(ItemData itemData)
    {
        if (itemPerfabs == null || itemPerfabs.Length == 0)
        {
            //Debug.LogError("itemPerfabs is empty! Please assign at least one prefab in the inspector.");
            return;
        }

        foreach (Transform child in stockPanel.transform)
        {
            Slot slot = child.GetComponent<Slot>();
            if (slot.currentItem == null)
            {
                GameObject item = Instantiate(itemPerfabs[0], slot.transform);
                item.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                slot.currentItem = item;

                var drag = item.GetComponent<ItemDragHandler>();
                if (drag == null)
                    drag = item.AddComponent<ItemDragHandler>();
                if (item.GetComponent<CanvasGroup>() == null)
                    item.AddComponent<CanvasGroup>();

                drag.itemData = itemData;

                // 关键：给 ItemBehaviour 赋值
                var itemBehaviour = item.GetComponent<ItemBehaviour>();
                if (itemBehaviour != null)
                {
                    Item itemSO = Resources.Load<Item>("Items/" + itemData.itemName);
                    if (itemSO != null)
                    {
                        itemBehaviour.item = itemSO;
                        itemBehaviour.quantity = 1;
                        // 确认 itemSO.ID == itemData.itemID
                    }
                    else
                    {
                        //Debug.LogWarning("找不到与ItemData对应的Item ScriptableObject: " + itemData.itemName);
                    }
                }

                var img = item.GetComponent<UnityEngine.UI.Image>();
                if (img != null && itemData.icon2D != null)
                    img.sprite = itemData.icon2D;

                break;
            }
        }
        RebuildItemCounts();
    }
    public void RemoveItemFromStock(int itemID, int amountToRemove)
    {
        foreach (Transform slotTransform in stockPanel.transform)
        {
            if(amountToRemove <= 0) break;
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot?.currentItem?.GetComponent<Item>() is Item item && item.ID == itemID)
            {
                int removed = Mathf.Min(amountToRemove, item.quantity);
                item.RemoveFromStack(removed);
                amountToRemove -= removed;
                if (item.quantity == 0)
                {
                    Destroy(slot.currentItem);
                    slot.currentItem = null;
                }
            }
        }
        RebuildItemCounts();
    }
}