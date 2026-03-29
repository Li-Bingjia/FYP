using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackpackUIController : MonoBehaviour
{
    public static BackpackUIController Instance { get; private set; }

    [Header("UI References")]
    public GameObject backpackPanel;         // 背包栏主面板
    public GameObject slotPrefab;            // 背包格Prefab
    public RectTransform slotsRoot;          // 存放格子的父物体（挂GridLayoutGroup）
    public int slotCount = 20;               // 背包格数量
    public Vector2 cellSize = new Vector2(100, 100);
    public Vector2 spacing = new Vector2(10, 10);
    public GameObject[] itemUIPrefabs;       // UI物品Prefab，索引与Item.ID一致

    private List<Slot> slots = new();
    private List<Item> backpackItems = new();

    public event System.Action OnBackpackChanged;

    void Awake()
    {
        Instance = this;
        backpackPanel.SetActive(false);

        var grid = slotsRoot.GetComponent<GridLayoutGroup>();
        if (grid != null)
        {
            grid.cellSize = cellSize;
            grid.spacing = spacing;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (backpackPanel.activeSelf)
                CloseBackpack();
            else
                OpenBackpack();
        }
    }

    public void OpenBackpack()
    {
        backpackPanel.SetActive(true);
        RefreshUI();
    }

    public void CloseBackpack()
    {
        backpackPanel.SetActive(false);
    }

    void RefreshUI()
    {
        foreach (Transform child in slotsRoot)
            Destroy(child.gameObject);
        slots.Clear();

        for (int i = 0; i < slotCount; i++)
        {
            var slotObj = Instantiate(slotPrefab, slotsRoot);
            var slot = slotObj.GetComponent<Slot>();
            slots.Add(slot);

            if (i < backpackItems.Count)
            {
                int prefabIndex = backpackItems[i].ID;
                if (prefabIndex >= 0 && prefabIndex < itemUIPrefabs.Length && itemUIPrefabs[prefabIndex] != null)
                {
                    var itemObj = Instantiate(itemUIPrefabs[prefabIndex], slotObj.transform);
                    var drag = itemObj.GetComponent<ItemDragHandler>() ?? itemObj.AddComponent<ItemDragHandler>();
                    drag.item = backpackItems[i];
                    slot.currentItem = itemObj;
                }
                else
                {
                    Debug.LogError($"itemUIPrefabs未配置，ID={prefabIndex} 超出范围或Prefab为null！");
                }
            }
        }
    }

    // 拖拽到背包栏
    public void AddItemToBackpack(Item item)
    {
        if (backpackItems.Count >= slotCount) return;
        backpackItems.Add(item);
        RefreshUI();
        OnBackpackChanged?.Invoke();
    }

    // 从背包栏移除（如拖到仓库或丢弃）
    public void RemoveItemFromBackpack(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= backpackItems.Count) return;
        backpackItems.RemoveAt(slotIndex);
        RefreshUI();
        OnBackpackChanged?.Invoke();
    }

    // 支持跨UI拖拽（如仓库⇄背包）
    public void MoveItemToOtherUI(int slotIndex, System.Action<Item> addToOtherUI)
    {
        if (slotIndex < 0 || slotIndex >= backpackItems.Count) return;
        var item = backpackItems[slotIndex];
        addToOtherUI?.Invoke(item);
        RemoveItemFromBackpack(slotIndex);
    }

    public List<Item> GetBackpackItems() => backpackItems;
}
