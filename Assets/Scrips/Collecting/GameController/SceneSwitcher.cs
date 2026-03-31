using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    public string targetSceneName; 

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            LoadSceneByName(targetSceneName);
        }
    }

    public void LoadSceneByName(string sceneName)
    {
        // 切换前手动保存
        if (BackpackUIController.Instance != null)
            BackpackUIController.Instance.SaveBackpack();
        if (StockController.Instance != null)
            StockController.Instance.SaveStock();

        SceneManager.LoadScene(sceneName);   
    }
    // 或者用 Build Settings 中的索引切换（从 0 开始）
    public void LoadSceneByIndex(int buildIndex)
    {
        SceneManager.LoadScene(buildIndex);
    }
    public void LoadSceneAsync(string sceneName)
    {
        StartCoroutine(LoadSceneCoroutine(sceneName));
    }

    private IEnumerator LoadSceneCoroutine(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        
        // 可选：显示进度条
        while (!asyncLoad.isDone)
        {
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f); // 进度 0~1
            Debug.Log("加载进度: " + (progress * 100) + "%");
            // 这里可以更新 UI 进度条
            yield return null;
        }
    }
}

