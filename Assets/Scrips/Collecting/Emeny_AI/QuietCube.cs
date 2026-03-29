using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuietCube : VisionObstacle
{
    [SerializeField] bool onlyActiveInQuiet = true;

    // 供外部调用，切换生效状态
    public void SetActiveByQuiet(bool isQuiet)
    {
        if (onlyActiveInQuiet)
        {
            solid = isQuiet;
            Debug.Log($"QuietCube solid set to {solid} (isQuiet={isQuiet})");
        }
        else
        {
            // 如果不是只在Quiet时生效，可以根据其它逻辑设置solid
        }
    }
}