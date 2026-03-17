using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; 

public class QuestUI : MonoBehaviour
{
    public Transform questListContainer;
    public GameObject questEntryPrefab;
    public GameObject objectTextPrefab;



    void Start()
    {

        UpdateQuestUI();
    }

    public void UpdateQuestUI()
    {
        //Debug.Log($"[QuestUI] QuestController.Instance={(QuestController.Instance != null ? "OK" : "null")}");
        if (QuestController.Instance != null)
            //Debug.Log($"[QuestUI] activeQuests count={QuestController.Instance.activeQuests?.Count ?? -1}");    
        foreach (Transform child in questListContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (var quest in QuestController.Instance.activeQuests)
        {
            if (quest == null)
            {
                //Debug.LogWarning("[QuestUI] activeQuests 有空元素！");
                continue;
            }
            //Debug.Log($"[QuestUI] Quest: {quest.quest.questName}, objectives count={quest.objectives?.Count ?? -1}");
            GameObject entry = Instantiate(questEntryPrefab, questListContainer);
            TMP_Text questNameText = entry.transform.Find("QuestNameText").GetComponent<TMP_Text>();
            Transform objectiveList = entry.transform.Find("ObjectiveList");
            questNameText.text = quest.quest.questName;
            foreach (var obj in quest.objectives)
            {
                GameObject objTextGO = Instantiate(objectTextPrefab, objectiveList);
                TMP_Text objText = objTextGO.GetComponent<TMP_Text>();
                objText.text = $"{obj.description} ({obj.currentAmount}/{obj.requiredAmount})";
            }
        }
    }
}