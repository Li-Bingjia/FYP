using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NpcBubbleDropHandler : MonoBehaviour, IDropHandler
{
    public CustomerNPC npc; // 运行时由NPC脚本赋值

    public void OnDrop(PointerEventData eventData)
    {
        // 这里假设拖拽的物品有ItemDragHandler组件，包含icon和item数据
        var drag = eventData.pointerDrag?.GetComponent<ItemDragHandler>();
        if (drag != null && npc != null)
        {
            // 判断是否特殊菜品
            bool isSpecial = drag.item != null && drag.item.isSpecial; // 你可以自定义isSpecial字段
            // 传递icon和特殊标记
            npc.OnPlayerDropFood(drag.item.icon, isSpecial);
        }
    }
}
