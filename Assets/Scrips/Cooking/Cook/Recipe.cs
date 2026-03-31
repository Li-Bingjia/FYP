using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class IngredientEntry
{
    [Header("食材")]
    public Item item;
    [Header("数量")]
    public int count = 1;
}

[CreateAssetMenu(menuName = "Cooking/Recipe", fileName = "NewRecipe")]
public class Recipe : ScriptableObject
{
    [Header("所需食材（可设置数量）")]
    public List<IngredientEntry> ingredients = new List<IngredientEntry>();

    [Header("合成结果")]
    public Item result;
}