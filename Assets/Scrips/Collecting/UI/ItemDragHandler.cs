using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    Transform originalParent;
    CanvasGroup canvasGroup;
    public Item item; 
    [Header("特殊属性")]
    public bool isSpecial; 
    public float misDropDistance = 2f;
    public float maxDropDistance = 3f;

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        RectTransform rt = GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.sizeDelta = new Vector2(100, 100);
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        transform.SetParent(transform.root);
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        if (originalParent == null)
        {
            Debug.LogWarning("[ItemDragHandler] originalParent is null in OnEndDrag!");
            return;
        }

        Slot originalSlot = originalParent.GetComponent<Slot>();
        if (originalSlot == null)
        {
            Debug.LogWarning("[ItemDragHandler] originalSlot is null in OnEndDrag! originalParent=" + originalParent.name);
            return;
        }

        Slot dropSlot = eventData.pointerEnter?.GetComponent<Slot>();
        if (dropSlot == null)
        {
            GameObject dropItem = eventData.pointerEnter;
            if (dropItem != null)
            {
                dropSlot = dropItem.GetComponent<Slot>();
            }
        }
        if (dropSlot != null)
        {
            if (dropSlot.currentItem != null)
            {
                dropSlot.currentItem.transform.SetParent(originalSlot.transform);
                originalSlot.currentItem = dropSlot.currentItem;
                dropSlot.currentItem = null;
                dropSlot.currentItem = gameObject;
            }
            else
            {
                originalSlot.currentItem = null;
                dropSlot.currentItem = gameObject;
            }
            transform.SetParent(dropSlot.transform);
        }
        else
        {
            if (!IsWithinStock(eventData.position))
            {
                DropItem(originalSlot);
            }
            else
            {
                transform.SetParent(originalParent);
            }
        }
        GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
    }

    bool IsWithinStock(Vector2 mousePosition)
    {
        if (originalParent == null || originalParent.parent == null)
        {
            Debug.LogWarning("[ItemDragHandler] originalParent or its parent is null in IsWithinStock!");
            return false;
        }
        RectTransform stockRect = originalParent.parent.GetComponent<RectTransform>();
        return RectTransformUtility.RectangleContainsScreenPoint(stockRect, mousePosition);
    }

    void DropItem(Slot orginalSlot)
    {
        orginalSlot.currentItem = null;
        Transform playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (playerTransform == null)
        {
            Debug.LogError("Missing player transform");
            return;
        }
        Vector3 dropOffset = Random.onUnitSphere;
        dropOffset.y = 0;
        dropOffset = dropOffset.normalized * Random.Range(misDropDistance, maxDropDistance);
        Vector3 dropPosition = playerTransform.position + dropOffset;

        if (item == null)
        {
            Debug.LogError($"[ItemDragHandler] item为null！请检查UI物品生成时是否正确赋值item字段。物品名:{gameObject.name}");
        }
        else if (item.prefab3D == null)
        {
            Debug.LogError($"[ItemDragHandler] item.prefab3D为null！请检查Item ScriptableObject的Prefab 3D字段是否赋值。物品名:{item.itemName}");
        }
        else
        {
            GameObject obj = Instantiate(item.prefab3D, dropPosition, Quaternion.identity);
            var questItem = obj.AddComponent<QuestItem3D>();
            questItem.Init(item.itemName, item);
            var spin = obj.GetComponent<GemSpin>();
            if (spin != null)
                spin.item = item;
        }
        Destroy(gameObject);
        StockController.Instance.RebuildItemCounts();
    }
}