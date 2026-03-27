using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using TMPro; // ���� TextMeshPro

public class BuildMenuUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Settings")]
    public float collapsedHeight = 40f;
    public float expandedHeight = 240f;
    public float animationSpeed = 10f;
    [SerializeField] private BuildMenuUI buildMenuUI;
    [Header("References")]
    public RectTransform containerRect;
    public Building buildingSystem;
    public Transform contentGrid;
    public GameObject itemButtonPrefab;


    [System.Serializable]
    public class FurnitureData
    {
        public string name;
        public Sprite icon;
        public GameObject prefab;
        public int quantity = 5; 
        [HideInInspector] public GameObject buttonInstance; 
        [HideInInspector] public TMP_Text countText;        
    }
    public List<FurnitureData> furnitureList;

    private float targetHeight;

    void Start()
    {
        targetHeight = collapsedHeight;
        if (containerRect == null) containerRect = GetComponent<RectTransform>();
        
        GenerateButtons();
        SetContentGridActive(false);
    }

    void Update()
    {
        Vector2 size = containerRect.sizeDelta;
        if (Mathf.Abs(size.y - targetHeight) > 0.1f)
        {
            float newHeight = Mathf.Lerp(size.y, targetHeight, Time.deltaTime * animationSpeed);
            containerRect.sizeDelta = new Vector2(size.x, newHeight);
        }

        if (Input.GetKeyDown(KeyCode.S) && buildingSystem != null && buildingSystem.isBuildMode)
        {

             if (buildingSystem.selectedObject != null) RetrieveFurniture(buildingSystem.selectedObject);
        }
    }
    public void SetContentGridActive(bool active)
    {
        if (contentGrid != null)
            contentGrid.gameObject.SetActive(active);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        targetHeight = expandedHeight;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetHeight = collapsedHeight;
    }

    private void GenerateButtons()
    {
        foreach (Transform child in contentGrid)
        {
            Destroy(child.gameObject);
        }

        foreach (var data in furnitureList)
        {
            GameObject btnObj = Instantiate(itemButtonPrefab, contentGrid);
            data.buttonInstance = btnObj;

            Image img = btnObj.transform.Find("Icon").GetComponent<Image>();
            if(img != null) img.sprite = data.icon;

            TMP_Text txt = btnObj.GetComponentInChildren<TMP_Text>();
            if (txt == null)
            {
                GameObject textObj = new GameObject("CountText");
                textObj.transform.SetParent(btnObj.transform, false);
                txt = textObj.AddComponent<TextMeshProUGUI>();
                
                // �򵥵����ϽǶ�λ
                RectTransform rt = textObj.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(1, 1);
                rt.anchorMax = new Vector2(1, 1);
                rt.pivot = new Vector2(1, 1);
                rt.anchoredPosition = new Vector2(-5, -5);
                txt.fontSize = 20;
                txt.color = Color.white;
            }
            data.countText = txt;

            Button btn = btnObj.GetComponent<Button>();
            var currentData = data; 
            btn.onClick.AddListener(() => OnFurnitureClicked(currentData));

            RefreshButtonState(data);
        }
    }

    private void OnFurnitureClicked(FurnitureData data)
    {
        if (buildingSystem != null && data.quantity > 0)
        {
            buildingSystem.SelectFurnitureFromUI(data.prefab);
            
            data.quantity--;
            RefreshButtonState(data);
        }
    }


    private void RefreshButtonState(FurnitureData data)
    {
        if (data.buttonInstance != null)
        {
            if (data.quantity > 0)
            {
                data.buttonInstance.SetActive(true);
                if (data.countText != null) data.countText.text = data.quantity.ToString();
            }
            else
            {
                data.buttonInstance.SetActive(false); 
            }
        }
    }

 
    public void RetrieveFurniture(GameObject worldObject)
    {
        if (worldObject == null) return;


        string cleanName = worldObject.name.Replace("(Clone)", "").Trim();

        FurnitureData targetData = null;
        foreach (var data in furnitureList)
        {
            if (data.prefab.name == cleanName)
            {
                targetData = data;
                break;
            }
        }

        if (targetData != null)
        {
            targetData.quantity++;
            RefreshButtonState(targetData);

            Destroy(worldObject);

        }
    }
}