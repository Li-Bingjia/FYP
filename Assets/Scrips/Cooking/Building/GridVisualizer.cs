using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridVisualizer : MonoBehaviour
{
    public int gridWidth = 10;
    public int gridHeight = 10;
    public float cellSize = 2.0f;
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
    public void UpdateGridToCamera(Camera cam)
    {
        Plane ground = new Plane(Vector3.up, Vector3.zero);
        Vector3[] corners = new Vector3[4];
        for (int i = 0; i < 4; i++)
        {
            Ray ray = cam.ViewportPointToRay(new Vector3(i % 2, i / 2, 0));
            if (ground.Raycast(ray, out float enter) && enter > 0)
                corners[i] = ray.GetPoint(enter);
            else
                // 没有交点时，取摄像机前方很远的地面点
                corners[i] = cam.transform.position + cam.transform.forward * 100f;
                corners[i].y = 0;
        }
        // 后续计算同前
        float minX = Mathf.Min(corners[0].x, corners[1].x, corners[2].x, corners[3].x);
        float maxX = Mathf.Max(corners[0].x, corners[1].x, corners[2].x, corners[3].x);
        float minZ = Mathf.Min(corners[0].z, corners[1].z, corners[2].z, corners[3].z);
        float maxZ = Mathf.Max(corners[0].z, corners[1].z, corners[2].z, corners[3].z);

        gridOrigin = new Vector3(minX, 0, minZ);
        gridWidth = Mathf.CeilToInt((maxX - minX) / cellSize);
        gridHeight = Mathf.CeilToInt((maxZ - minZ) / cellSize);

        RedrawGrid();
    }
}