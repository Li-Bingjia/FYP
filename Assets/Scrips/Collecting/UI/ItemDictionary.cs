using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDictionary : MonoBehaviour
{
    public List<ItemData> itemDataList; // 在 Inspector 里配置

    public ItemData GetItemData(int itemID)
    {
        return itemDataList.Find(i => i.itemID == itemID);
    }

    public GameObject GetItemPrefab(int itemID)
    {
        var data = GetItemData(itemID);
        return data != null ? data.prefab3D : null;
    }
}
