using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardsController : MonoBehaviour
{
    public static RewardsController Instance { get; private set; }  

    private void Awake()
    {
        if(Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void GiveQuestRewards(Quest quest)
    {
        if(quest?.questRewards == null) return;
        foreach(var reward in quest.questRewards)
        {
            switch(reward.type)
            {
                case RewardType.Item:
                    GiveItemReward(reward.rewardID, reward.amount);
                    break;
                case RewardType.Experience:
                    // TODO: 经验奖励逻辑
                    break;  
                case RewardType.Gold:
                    // TODO: 金币奖励逻辑
                    break;
                case RewardType.Custom:
                    // TODO: 自定义奖励逻辑
                    break;   
            }
        }
    }

    public void GiveItemReward(int itemID, int amount)
    {
        var itemData = FindAnyObjectByType<ItemDictionary>()?.GetItemData(itemID);
        if(itemData == null) return;
        for(int i = 0; i < amount; i++)
        {
            StockController.Instance.AddItemToStock(itemData);
            // 如果背包满了，可以考虑掉落到场景
            // GameObject droppedItem = Instantiate(itemData.prefab3D, transform.position + Vector3.down, Quaternion.identity);
            // droppedItem.GetComponent<BounceEffect>()?.StartBounce();
        }
    }
}