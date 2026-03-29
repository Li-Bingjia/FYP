using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/ItemDictionary")]
public class ItemDictionary : ScriptableObject
{
    public List<Item> items; // 在Inspector里配置

    private Dictionary<int, Item> itemsDict;

    private void OnEnable()
    {
        itemsDict = new Dictionary<int, Item>();
        foreach (var item in items)
        {
            if (item != null && !itemsDict.ContainsKey(item.ID))
                itemsDict.Add(item.ID, item);
        }
    }

    public Item GetItemByID(int id)
    {
        if (itemsDict != null && itemsDict.ContainsKey(id))
            return itemsDict[id];
        return null;
    }
}
