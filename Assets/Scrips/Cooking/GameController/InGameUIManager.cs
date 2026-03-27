using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; 
using System.Collections.Generic;
using TMPro;

public class InGameUIManager : MonoBehaviour
{
    [SerializeField] private GameObject menuPanel; 
    [SerializeField] private GameObject player;    

    [Header("Main UI Group")]
    [SerializeField] private GameObject mainUIGroup; // [新增] 把平时的常驻UI（如金币等）作为子物体放进去

    [Header("Cooking Focus UI")]
    [SerializeField] private GameObject cookingFocusPanel; // [新增] 特写镜头专用UI面板

    [Header("Kitchen / Storage UI")]
    [SerializeField] private GameObject kitchenPanel; 
    [SerializeField] private Button[] slotButtons;    
    
    // [修改 1] 删除了 currentAppliance 引用，因为厨具现在使用3D物理点击，不再需要UI列表
    private StorageContainer currentContainer; 

    [Header("Build UI")]
    [SerializeField] private GameObject buildModeBottomBar; 
    public Building buildingSystem;

    [Header("Economy UI")]
    [SerializeField] private TMP_Text moneyText; 
    private int currentMoney = 70; 

    public bool IsMenuOpen => menuPanel.activeSelf;

    void Start()
    {
        menuPanel.SetActive(false);
        if (kitchenPanel != null) kitchenPanel.SetActive(false);
        if (buildModeBottomBar != null) buildModeBottomBar.SetActive(false);
        
        // [新增] 初始化状态
        if (cookingFocusPanel != null) cookingFocusPanel.SetActive(false);
        if (mainUIGroup != null) mainUIGroup.SetActive(true);
        
        UpdateMoneyUI();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (buildingSystem != null && buildingSystem.isBuildMode) return;
            ToggleMenu();
        }

        if (buildModeBottomBar != null && buildingSystem != null)
        {
            if (buildModeBottomBar.activeSelf != buildingSystem.isBuildMode)
            {
                buildModeBottomBar.SetActive(buildingSystem.isBuildMode);
            }
        }
    }

    public void ToggleMenu()
    {
        menuPanel.SetActive(!menuPanel.activeSelf);
        if (menuPanel.activeSelf) CloseAllContainerUI();
    }

    // [新增] 切换专注做菜的UI
    public void ToggleCookingFocusUI(bool isFocus)
    {
        if (cookingFocusPanel != null) cookingFocusPanel.SetActive(isFocus);
        if (mainUIGroup != null) mainUIGroup.SetActive(!isFocus);
        
        if (isFocus)
        {
            CloseAllContainerUI(); // 确保箱子等UI都关掉
            if (buildModeBottomBar != null) buildModeBottomBar.SetActive(false);
        }
    }

    public void SaveProgress() { if (player != null) SaveSystem.SaveGame(player.transform.position); }
    public void ExitToTitle() { Time.timeScale = 1; SceneManager.LoadScene("UI_main_menu"); }

    // [修改 1] 删除了 ToggleKitchenUI 和 OpenKitchenUI，完全剥离厨具的UI相关逻辑

    public void ToggleStorageUI(StorageContainer container)
    {
        if (kitchenPanel.activeSelf && currentContainer == container)
        {
            CloseAllContainerUI();
        }
        else
        {
            OpenStorageUI(container);
        }
    }

    public void OpenStorageUI(StorageContainer container)
    {
        currentContainer = container;
        kitchenPanel.SetActive(true);
        RefreshContentUI(container.storedItems);
    }

    public StorageContainer GetCurrentContainer() => currentContainer;

    private void CloseAllContainerUI()
    {
        kitchenPanel.SetActive(false);
        currentContainer = null;
    }

    private void RefreshContentUI(List<GameObject> items)
    {
        for (int i = 0; i < slotButtons.Length; i++)
        {
            if (i < items.Count)
            {
                slotButtons[i].gameObject.SetActive(true);
                slotButtons[i].GetComponentInChildren<TMPro.TMP_Text>().text = items[i].name;
                
                int index = i; 
                slotButtons[i].onClick.RemoveAllListeners();
                
                slotButtons[i].onClick.AddListener(() => OnClickSlot(index));
            }
            else
            {
                slotButtons[i].gameObject.SetActive(false);
            }
        }
    }

    private void OnClickSlot(int index)
    {
        // [修改 1] 移除了对 KitchenAppliance 的判断，只保留 StorageContainer 逻辑
        if (currentContainer != null) 
        {
            currentContainer.RemoveItem(index);
            RefreshContentUI(currentContainer.storedItems);
            // （可选）当箱子空了自动关闭 UI
            if (currentContainer.storedItems.Count == 0) CloseAllContainerUI();
        }
    }

    public void OnBuildButtonClicked()
    {
        buildingSystem.ToggleBuildMode();
        if (buildModeBottomBar != null) buildModeBottomBar.SetActive(buildingSystem.isBuildMode);
    }

    public void AddMoney(int amount)
    {
        currentMoney += amount;
        UpdateMoneyUI();
    }

    private void UpdateMoneyUI()
    {
        if (moneyText != null) moneyText.text = $"{currentMoney}";
    }
}
