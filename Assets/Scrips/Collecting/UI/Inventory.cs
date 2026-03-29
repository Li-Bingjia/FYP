using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public List<Item> items = new List<Item>();

    // 添加物品并自动更新相关任务目标
    public void AddItem(Item item, int count)
    {
        Item found = items.Find(i => i.ID == item.ID);
        if (found != null)
        {
            found.quantity += count;
        }
        else
        {
            Item newItem = Instantiate(item);
            newItem.quantity = count;
            items.Add(newItem);
        }
        // 通知背包刷新
        if (StockController.Instance != null)
            StockController.Instance.RebuildItemCounts();
    }

    // 可选：移除物品
    public bool RemoveItem(int itemId, int count)
    {
        Item found = items.Find(i => i.ID == itemId);
        if (found != null && found.quantity >= count)
        {
            found.quantity -= count;
            if (found.quantity == 0)
                items.Remove(found);
            return true;
        }
        return false;
    }
}