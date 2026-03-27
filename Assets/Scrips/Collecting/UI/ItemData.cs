using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/ItemData")]
public class ItemData : ScriptableObject
{
    public int itemID; // 与 Item SO 的 ID 一致
    public string itemName;
    public Sprite icon2D;
    public GameObject prefab3D;
}
