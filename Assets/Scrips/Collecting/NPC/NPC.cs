using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class NPC : MonoBehaviour, IInteractable
{
    public NPCDialogue dialogueData;
    private DialogueController dialogueUI;

    private int dialogueIndex;
    private bool isDialogueActive;

    private enum QuestState { NotStarted, InProgress, Completed }
    private QuestState questState = QuestState.NotStarted;

    private void Start()
    {
        dialogueUI = DialogueController.Instance;
        //Debug.Log($"[NPC] {name} dialogueData={(dialogueData != null ? dialogueData.name : "null")}");

    }

    public bool CanInteract()
    {
        return !isDialogueActive;
    }

    public void Interact()
    {
        if (dialogueData == null || (PauseController.IsGamePaused && !isDialogueActive))
        {
            return;
        }
        if (isDialogueActive)
        {
            NextLine();
        }
        else
        {
            StartDialogue();
        }
    }

    void StartDialogue()
    {
        SyncQuestState();
        if(questState == QuestState.NotStarted)
        {
            dialogueIndex = 0;
        }
        else if(questState == QuestState.InProgress)
        {
            dialogueIndex =  dialogueData.questInProgressIndex;
        }
        else if(questState == QuestState.Completed)
        {
            dialogueIndex = dialogueData.questCompletedIndex;
        }
        isDialogueActive = true;

        dialogueUI.SetNPCInfo(dialogueData.npcName, dialogueData.npcPortrait);
        dialogueUI.ShowDialogueUI(true);
        PauseController.SetPause(true);
        StartCoroutine(TypeLine());
    }

    private void SyncQuestState()
    {
        if(dialogueData.quest == null) return;
        string questID = dialogueData.quest.questID;
        if (QuestController.Instance.IsQuestCompleted(questID) || QuestController.Instance.IsQuestHandedIn(questID))
        {
            questState = QuestState.Completed;
        }       
        else if (QuestController.Instance.IsQuestActive(questID))
        {
            questState = QuestState.InProgress;
        }
        else
        {
            questState = QuestState.NotStarted;
        }
    }
    void NextLine()
    {
        //Debug.Log($"[NPC] NextLine: dialogueIndex={dialogueIndex}, endDialogue={dialogueData.endDialogueLines.Length > dialogueIndex && dialogueData.endDialogueLines[dialogueIndex]}, hasChoice={dialogueData.choices != null && System.Array.Exists(dialogueData.choices, c => c.dialogueIndex == dialogueIndex)}");

        dialogueIndex++;

        if (dialogueIndex >= dialogueData.dialogueLines.Length)
        {
            EndDialogue();
            return;
        }

        dialogueUI.ClearDialogue();

        if (dialogueData.endDialogueLines.Length > dialogueIndex && dialogueData.endDialogueLines[dialogueIndex])
        {
            EndDialogue();
            return;
        }

        if (dialogueData.choices != null)
        {
            foreach (DialogueChoice dialogueChoice in dialogueData.choices)
            {
                if (dialogueChoice.dialogueIndex == dialogueIndex)
                {
                    //Debug.Log($"[NPC] At dialogueIndex={dialogueIndex}, found choice: {string.Join(",", dialogueChoice.choices)}");
                    for (int i = 0; i < dialogueChoice.choices.Length; i++)
                    {
                        int choiceIndex = i;
                        bool givesQuest = dialogueChoice.givesQuest[i];
                        dialogueUI.CreateChoiceButton(dialogueChoice.choices[i], () => OnChoiceSelected(choiceIndex, givesQuest));
                    }
                    break;
                }
            }
        }

        StartCoroutine(TypeLine());
    }

    IEnumerator TypeLine()
    {
        dialogueUI.SetDialogueText("");
        foreach (char letter in dialogueData.dialogueLines[dialogueIndex])
        {
            dialogueUI.SetDialogueText(dialogueUI.dialogueText.text + letter);
            yield return new WaitForSeconds(dialogueData.typingSpeed);
        }


        // 检查当前句子是否有选项
        bool hasChoice = false;
        if (dialogueData.choices != null)
        {
            foreach (DialogueChoice dialogueChoice in dialogueData.choices)
            {
                if (dialogueChoice.dialogueIndex == dialogueIndex)
                {
                    hasChoice = true;
                    break;
                }
            }
        }

        // 如果没有选项，且autoProgress为true，才自动下一句
        if (!hasChoice && dialogueData.autoProgressLines.Length > dialogueIndex && dialogueData.autoProgressLines[dialogueIndex])
        {
            yield return new WaitForSeconds(dialogueData.autoProgressDelay);
            NextLine();
        }
    }
    void OnChoiceSelected(int selectedIndex, bool givesQuest)
    {
        var choice = System.Array.Find(dialogueData.choices, c => c.dialogueIndex == dialogueIndex);
        if (choice != null && selectedIndex < choice.nextDialogueIndices.Length)
        {
            dialogueUI.ClearDialogue();
            dialogueIndex = choice.nextDialogueIndices[selectedIndex];
            // 只有玩家选择“接受”时才触发任务
            if (givesQuest && questState == QuestState.NotStarted)
            {
                QuestController.Instance.AcceptQuest(dialogueData.quest);
                questState = QuestState.InProgress;
            }
            StartCoroutine(TypeLine());
        }
    }

    public void EndDialogue()
    {
        if(questState == QuestState.Completed &&
           QuestController.Instance.IsQuestHandedIn(dialogueData.quest.questID))
        {
            HandleQuestCompletion();
        }


        StopAllCoroutines();
        isDialogueActive = false;
        dialogueUI.SetDialogueText("");
        dialogueUI.ShowDialogueUI(false);
        PauseController.SetPause(false);
    }
    void HandleQuestCompletion()
    {
        QuestController.Instance.HandInQuest(dialogueData.quest.questID);
        RewardsController.Instance.GiveQuestRewards(dialogueData.quest);
    }
}