using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Quests/Quest")]
public class Quest : ScriptableObject
{
    public string questID; 
    public string questName;
    public string description;
    public List<QuestObjective> objectives; 
    public List<QuestReward> questRewards;

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(questID))
        {
            questID = questName + Guid.NewGuid().ToString(); 
        }
    }
    [Serializable]
    public class QuestObjective
    {
        public string objectiveID;
        public string description;
        public ObjectiveType type;
        public float requiredAmount;
        public float currentAmount;   
        public bool isCompleted;
        public bool IsCompleted => currentAmount >= requiredAmount; 
   }

   public enum ObjectiveType
   {
       CollectItem,
       DefeatEnemy,
       ReachLocation,
       TalkToNPC,
       Custom,
       TimeBased
   }
}
// ...前略...
[Serializable]
public class QuestProgress
{
    public Quest quest;
    public List<Quest.QuestObjective> objectives;

    public QuestProgress(Quest quest)
    {
        this.quest = quest;
        objectives = new List<Quest.QuestObjective>();
        foreach (var obj in quest.objectives)
        {
            objectives.Add(new Quest.QuestObjective
            {
                objectiveID = obj.objectiveID,
                description = obj.description,
                type = obj.type,
                requiredAmount = obj.requiredAmount,
                currentAmount = 0,
            });
        }
    }
    public bool IsCompleted => objectives.TrueForAll(o => o.IsCompleted);
    public string QuestID => quest.questID;
}

[Serializable]
public class QuestReward
{
    public RewardType type;
    public int rewardID;
    public float amount = 1;   
}
public enum RewardType
{
    Experience,
    Item,
    Gold,
    Custom
}