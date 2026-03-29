using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueController : MonoBehaviour
{
    public static DialogueController Instance { get; private set; }
    public GameObject dialoguePanelPrefab;
    public TMP_Text dialogueText, nameText;
    public Image portraitImage;
    public Transform choicesContainer;
    public GameObject choiceButtonPrefab;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void ShowDialogueUI(bool show)
    {
        dialoguePanelPrefab.SetActive(show);
    }

    public void SetNPCInfo(string npcName, Sprite portrait)
    {
        nameText.SetText(npcName);
        portraitImage.sprite = portrait;
    }

    public void SetDialogueText(string text)
    {
        dialogueText.text = text;
    }

    public void ClearDialogue()
    {
        foreach (Transform child in choicesContainer)
        {
            Destroy(child.gameObject);
        }
        choicesContainer.gameObject.SetActive(false); 
    }

    public GameObject CreateChoiceButton(string choiceText, System.Action onClick)
    {
        choicesContainer.gameObject.SetActive(true); 
        GameObject buttonObj = Instantiate(choiceButtonPrefab, choicesContainer);
        buttonObj.GetComponentInChildren<TMP_Text>().text = choiceText;
        buttonObj.GetComponent<Button>().onClick.AddListener(() => onClick());
        return buttonObj;
    }
}