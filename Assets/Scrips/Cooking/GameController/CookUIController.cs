using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CookUIController : MonoBehaviour
{
    public List<Slot> cookSlots;
    public Slot resultSlot;
    public Button cookButton;
    public Button closeButton;
    public List<Recipe> recipes;
    public GameObject resultUIPrefab;

    void Start()
    {
        gameObject.SetActive(false); // 启动时隐藏
        cookButton.onClick.AddListener(OnCookButtonClicked);
        closeButton.onClick.AddListener(ClosePanel);
    }

    void ClosePanel()
    {
        Debug.Log("ClosePanel called");
        ClearResultSlot();
        gameObject.SetActive(false);
    }

    void OnCookButtonClicked()
    {
        // 清理旧的合成结果
        ClearResultSlot();

        List<Item> inputItems = new List<Item>();
        foreach (var slot in cookSlots)
        {
            if (slot.currentItem != null)
            {
                var drag = slot.currentItem.GetComponent<ItemDragHandler>();
                if (drag != null && drag.item != null)
                    inputItems.Add(drag.item);
            }
        }
        Item result = FindRecipeResult(inputItems);
        if (result != null)
        {
            var resultObj = Instantiate(resultUIPrefab, resultSlot.transform);
            var drag = resultObj.GetComponent<ItemDragHandler>();
            if (drag == null) drag = resultObj.AddComponent<ItemDragHandler>();
            drag.item = result; 
            resultSlot.currentItem = resultObj;

            var img = resultObj.GetComponent<Image>();
            if (img != null && result.icon != null)
                img.sprite = result.icon;
        }
        else
        {
            Debug.Log("没有匹配的菜谱！");
        }
    }

    void ClearResultSlot()
    {
        if (resultSlot.currentItem != null)
        {
            Destroy(resultSlot.currentItem);
            resultSlot.currentItem = null;
        }
        // 也可以清理所有子物体，防止异常残留
        foreach (Transform child in resultSlot.transform)
        {
            Destroy(child.gameObject);
        }
    }

    Item FindRecipeResult(List<Item> inputItems)
    {
        foreach (var recipe in recipes)
        {
            if (MatchRecipe(recipe, inputItems))
                return recipe.result;
        }
        return null;
    }

    bool MatchRecipe(Recipe recipe, List<Item> inputItems)
    {
        // 统计玩家输入的每种食材数量
        var inputCount = new Dictionary<int, int>();
        foreach (var item in inputItems)
        {
            if (item == null) continue;
            if (!inputCount.ContainsKey(item.ID))
                inputCount[item.ID] = 0;
            inputCount[item.ID]++;
        }

        // 检查每种配方食材是否都满足数量
        foreach (var entry in recipe.ingredients)
        {
            if (entry.item == null) return false;
            if (!inputCount.ContainsKey(entry.item.ID) || inputCount[entry.item.ID] < entry.count)
                return false;
            inputCount[entry.item.ID] -= entry.count;
        }

        // 检查是否有多余食材
        foreach (var kv in inputCount)
        {
            if (kv.Value > 0)
                return false;
        }
        return true;
    }
}