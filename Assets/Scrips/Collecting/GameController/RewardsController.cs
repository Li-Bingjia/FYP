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
                    GiveItemReward(reward.rewardID, (int)reward.amount);
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
        var item = FindAnyObjectByType<ItemDictionary>()?.GetItemByID(itemID);
        if(item == null) return;
        for(int i = 0; i < amount; i++)
        {
            StockController.Instance.AddItemToStock(item);
        }
    }
}