using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDirectionRing : MonoBehaviour
{
    [Header("圆环参数")]
    public float ringRadius = 2.5f; // 圆环半径
    public GameObject indicatorPrefab; // 指示器预制体
    public Transform player; // 玩家Transform

    [Header("敌人列表")]
    public List<Transform> enemies = new List<Transform>();

    private Dictionary<Transform, GameObject> indicators = new Dictionary<Transform, GameObject>();

    void Update()
    {
        Debug.Log("Enemies count: " + enemies.Count);
        foreach (Transform enemy in enemies)
        {
            if (enemy == null) continue;
            Debug.Log("Enemy: " + enemy.name + " Pos: " + enemy.position);

            // 计算敌人相对玩家的方向
            Vector3 dir = (enemy.position - player.position).normalized;
            dir.y = 0; // 只考虑水平面

            // 计算圆环上的位置
            Vector3 indicatorPos = player.position + dir * ringRadius + Vector3.up * 1.5f; // 1.5f为圆环高度

            // 没有指示器就生成
            if (!indicators.ContainsKey(enemy))
            {
                GameObject indicator = Instantiate(indicatorPrefab, indicatorPos, Quaternion.identity, this.transform);
                indicators[enemy] = indicator;
            }

            // 更新指示器位置
            indicators[enemy].transform.position = indicatorPos;
            Debug.Log("Indicator position: " + indicators[enemy].transform.position);

            // 让指示器朝向玩家摄像机
            if (Camera.main != null)
            {
                // 让箭头指向敌人方向
                indicators[enemy].transform.forward = (enemy.position - player.position).normalized;
            }
        }

        // 清理已消失的敌人
        List<Transform> toRemove = new List<Transform>();
        foreach (var kvp in indicators)
        {
            Debug.Log("Indicator for: " + kvp.Key.name + " Pos: " + kvp.Value.transform.position);
            if (!enemies.Contains(kvp.Key) || kvp.Key == null)
            {
                Destroy(kvp.Value);
                toRemove.Add(kvp.Key);
            }
        }
        foreach (var t in toRemove) indicators.Remove(t);
    }
}