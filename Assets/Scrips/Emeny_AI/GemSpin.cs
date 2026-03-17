using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GemSpin : MonoBehaviour
{
    public ItemData itemData; // 只挂数据
    private StockController stockController;

    [Header("旋转设置")]
    public float rotateSpeed = 180f;

    [Header("高亮动画设置")]
    public float highlightSpeed = 2f;
    public float highlightIntensity = 1.5f;
    private Material mat;
    private float baseEmission;

    private bool canPickUp = false;
    private Transform player;

    void Start()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            mat = renderer.material;
            if (mat.HasProperty("_EmissionColor"))
                baseEmission = mat.GetColor("_EmissionColor").maxColorComponent;
        }
        stockController = FindFirstObjectByType<StockController>();
    }

    void Update()
    {
        transform.Rotate(0, 0, rotateSpeed * Time.deltaTime);

        if (mat != null && mat.HasProperty("_EmissionColor"))
        {
            float emission = baseEmission + Mathf.PingPong(Time.time * highlightSpeed, highlightIntensity);
            Color baseColor = mat.GetColor("_Color");
            mat.SetColor("_EmissionColor", baseColor * emission);
        }

        if (canPickUp && Input.GetKeyDown(KeyCode.E))
        {
            if (stockController != null && itemData != null)
            {
                stockController.AddItemToStock(itemData);
            }
            gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canPickUp = true;
            player = other.transform;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canPickUp = false;
            player = null;
        }
    }
}