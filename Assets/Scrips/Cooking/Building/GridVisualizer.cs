using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridVisualizer : MonoBehaviour
{
    public int gridWidth = 10;
    public int gridHeight = 10;
    public float cellSize = 0.5f;
    public Vector3 gridOrigin = new Vector3(-2.5f, 0, -2.5f);
    public Color lineColor = Color.white;

    void Start()
    {
        RedrawGrid();
    }

    public void RedrawGrid()
    {
        // 清理旧线
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        DrawGrid();
    }

    void DrawGrid()
    {
        for (int x = 0; x <= gridWidth; x++)
        {
            Vector3 start = gridOrigin + new Vector3(x * cellSize, 0.05f, 0);
            Vector3 end = gridOrigin + new Vector3(x * cellSize, 0.05f, gridHeight * cellSize);
            DrawLine(start, end);
        }
        for (int z = 0; z <= gridHeight; z++)
        {
            Vector3 start = gridOrigin + new Vector3(0, 0.05f, z * cellSize);
            Vector3 end = gridOrigin + new Vector3(gridWidth * cellSize, 0.05f, z * cellSize);
            DrawLine(start, end);
        }
    }

    void DrawLine(Vector3 start, Vector3 end)
    {
        GameObject lineObj = new GameObject("GridLine");
        lineObj.transform.parent = this.transform;
        var lr = lineObj.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        lr.startWidth = 0.02f;
        lr.endWidth = 0.02f;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = lineColor;
        lr.endColor = lineColor;
        lr.useWorldSpace = true;
    }
}