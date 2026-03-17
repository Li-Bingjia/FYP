using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestItem3D : MonoBehaviour
{
    // 任务目标ID（与QuestObjective.objectiveID一致）
    public string objectiveID;

    // 可选：引用ItemData或Quest相关数据
    public ItemData itemData;

    // 可选：初始化时赋值
    public void Init(string id, ItemData data = null)
    {
        objectiveID = id;
        itemData = data;
    }

    // 示例：玩家拾取时通知任务系统
    public void OnPickUp()
    {
        // 添加到背包
        if (StockController.Instance != null && itemData != null)
            StockController.Instance.AddItemToStock(itemData);

        //Debug.Log($"QuestItem3D Picked Up: objectiveID={objectiveID}");
        Destroy(gameObject);
    }
}
