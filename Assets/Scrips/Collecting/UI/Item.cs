using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    [Header("基础属性")]
    public int ID;           // 唯一ID，Inspector赋值，任务系统用
    public string itemName;  // 名称
    public int quantity = 1; // 默认数量

    [Header("描述与图标")]
    [TextArea]
    public string description;
    public Sprite icon;

    /// <summary>
    /// 从堆叠中移除指定数量
    /// </summary>
    public void RemoveFromStack(int amount)
    {
        quantity -= amount;
        if (quantity < 0) quantity = 0;
    }
}