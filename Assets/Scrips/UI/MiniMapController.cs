using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MiniMapController : MonoBehaviour
{
    public Transform player;
    public RectTransform mapRect;
    public RectTransform playerIcon;

    public Vector2 worldMin;
    public Vector2 worldMax;

    // 你要的限制区域
    public Vector2 minMapPos = new Vector2(-600, -230);
    public Vector2 maxMapPos = new Vector2(600, 230);

    void Update()
    {
        Vector3 playerPos = player.position;
        // 交换映射：世界Z映射到小地图X，世界X映射到小地图Y
        float normalizedX = Mathf.InverseLerp(worldMin.y, worldMax.y, playerPos.z); // Z → X
        float normalizedY = Mathf.InverseLerp(worldMin.x, worldMax.x, playerPos.x); // X → Y

        float iconX = Mathf.Lerp(minMapPos.x, maxMapPos.x, normalizedX);
        float iconY = Mathf.Lerp(minMapPos.y, maxMapPos.y, normalizedY);

        playerIcon.anchoredPosition = new Vector2(iconX, iconY);
    }
}