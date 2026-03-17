using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    Transform originalParent;
    CanvasGroup canvasGroup;
    public ItemData itemData; // 只挂数据

    public float misDropDistance = 2f;
    public float maxDropDistance = 3f;

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
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

        Slot dropSlot = eventData.pointerEnter?.GetComponent<Slot>();
        if (dropSlot == null)
        {
            GameObject dropItem = eventData.pointerEnter;
            if (dropItem != null)
            {
                dropSlot = dropItem.GetComponent<Slot>();
            }
        }
        Slot originalSlot = originalParent.GetComponent<Slot>();
        if (dropSlot != null)
        {
            if (dropSlot.currentItem != null)
            {
                dropSlot.currentItem.transform.SetParent(originalSlot.transform);
                originalSlot.currentItem = dropSlot.currentItem;
                dropSlot.currentItem = null;
                dropSlot.currentItem = gameObject;
                GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            }
            else
            {
                originalSlot.currentItem = null;
                dropSlot.currentItem = gameObject;
                GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
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

        // 生成3D物体（通过itemData）
        if (itemData != null && itemData.prefab3D != null)
        {
            GameObject obj = Instantiate(itemData.prefab3D, dropPosition, Quaternion.identity);
            var questItem = obj.AddComponent<QuestItem3D>();
            // 这里假设 itemData 有 itemID 字段，或者你自己传递
            questItem.Init(itemData.itemName); // 或 itemData.id 或 objectiveID
            var spin = obj.GetComponent<GemSpin>();
            if (spin != null)
                spin.itemData = itemData;
        }
        else
        {
            Debug.LogError("itemData或itemData.prefab3D未设置！");
        }
        Destroy(gameObject);
        StockController.Instance.RebuildItemCounts();
    }
}
