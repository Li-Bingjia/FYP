using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestController : MonoBehaviour
{
    public static QuestController Instance { get; private set; }
    [HideInInspector]
    public List<QuestProgress> activeQuests = new();
    
    public QuestUI questUI;
    public bool hasStealthBuffQuest => activeQuests.Exists(q => q.quest.questID.StartsWith("stealth_buff_quest"));
    public List<string> handinQuestIDs = new(); 
    void Start() {
        questUI = FindFirstObjectByType<QuestUI>(); // 先赋值
        QuestController.Instance.ClearAllQuests();   // 再调用
        StockController.Instance.OnStockChanged += CheckStockForQuests;
    }
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    public void ClearAllQuests()
    {
        activeQuests.Clear();
        questUI.UpdateQuestUI();
    }    
    public void AcceptQuest(Quest quest)
    {
        if(IsQuestActive(quest.questID)) return;
        activeQuests.Add(new QuestProgress(quest));
        questUI.UpdateQuestUI();
        if (quest.questID.StartsWith("stealth_buff_quest")) {
            var player = FindAnyObjectByType<CharacterControl>();
            if (player != null) player.stealthBuffQuestActive = true;
        }
    }
    public bool IsQuestActive(string questID)
    {
        var quest = activeQuests.Find(q => q.quest.questID == questID);
        return quest != null && !quest.IsCompleted;
    }
    public void CheckStockForQuests()
    {
        Dictionary<int, int> itemCounts = StockController.Instance.GetItemCounts();
        foreach (var questProgress in activeQuests)
        {
            foreach (Quest.QuestObjective questObjective in questProgress.objectives)
            {
                if (questObjective.type != Quest.ObjectiveType.CollectItem) continue;

                int newAmount = 0;
                if (int.TryParse(questObjective.objectiveID, out int objID))
                {
                    //Debug.Log($"[QuestController] CheckStockForQuests: objectiveID={objID}");
                    if (itemCounts.TryGetValue(objID, out int count))
                    {
                        //Debug.Log($"[QuestController] itemCounts[{objID}]={count}");
                        newAmount = Mathf.Min(count, (int)questObjective.requiredAmount);
                    }
                }
                if (questObjective.currentAmount != newAmount)
                {
                    questObjective.currentAmount = newAmount;
                }
            }
        }
        questUI.UpdateQuestUI();
    }
    public bool IsQuestCompleted(string questID)
    {
        QuestProgress quest = activeQuests.Find(q => q.QuestID == questID);
        return quest != null && quest.objectives.TrueForAll(o => o.IsCompleted);
    }
    public void HandInQuest(string questID)
    {
        if(!RemoveRequiredItemsFromStock(questID))
        {
            return;
        }
        QuestProgress quest = activeQuests.Find(q => q.QuestID == questID);
        if(quest != null)
        {
            handinQuestIDs.Add(questID);
            activeQuests.Remove(quest);
            questUI.UpdateQuestUI();
        }
    }
    public bool IsQuestHandedIn(string questID)
    {
        return handinQuestIDs.Contains(questID);
    }
    public bool RemoveRequiredItemsFromStock(string questID)
    {
        QuestProgress quest = activeQuests.Find(q => q.QuestID == questID);
        if (quest == null)
            return false;

        Dictionary<int, int> requiredItems = new ();
        foreach(Quest.QuestObjective objective in quest.objectives)
        {
            if (objective.type == Quest.ObjectiveType.CollectItem && int.TryParse(objective.objectiveID, out int itemID))
            {
                requiredItems[itemID] = (int)objective.requiredAmount;
            }
        }
        Dictionary<int, int> itemCounts = StockController.Instance.GetItemCounts();
        foreach (var item in requiredItems)
        {
            if (itemCounts.GetValueOrDefault(item.Key) < item.Value)
            {
                return false; 
            }
        }
        foreach(var itemRequirment in requiredItems)
        {
            StockController.Instance.RemoveItemFromStock(itemRequirment.Key, itemRequirment.Value);
        }
        return true;
    }
}
