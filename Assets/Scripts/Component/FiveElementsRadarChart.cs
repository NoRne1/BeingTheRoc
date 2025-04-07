using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class FiveElementsRadarChart : Graphic
{
    [Header("Chart Settings")]
    public float chartSize = 100f;
    public Color backgroundColor = new Color(1f, 0.925f, 0.757f); // #ffecc1
    public Color lineColorInner = new Color(0f, 0f, 0f, 0.8f);
    public Color lineColorOuter = new Color(0f, 0f, 0f, 1f);
    public float outerLineWidth = 3f;
    public float innerLineWidth = 1f;

    
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        DrawBackground(vh);
        DrawGrid(vh);
        DrawCenterLines(vh);
        // DrawDataPolygon(vh);
    }

    private void DrawBackground(VertexHelper vh)
    {
        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = backgroundColor;
        
        Vector2 center = Vector2.zero;
        Vector2[] vertices = CalculateVertices(chartSize);
        
        for (int i = 0; i < 5; i++)
        {
            int next = (i + 1) % 5;
            vh.AddVert(center, vertex.color, Vector2.zero);
            vh.AddVert(vertices[i], vertex.color, Vector2.zero);
            vh.AddVert(vertices[next], vertex.color, Vector2.zero);
            
            int baseIndex = i * 3;
            vh.AddTriangle(baseIndex, baseIndex + 1, baseIndex + 2);
        }
    }

    private void DrawGrid(VertexHelper vh)
    {
        // 绘制5层五边形网格 (对应值2,4,6,8,10)
        for (int i = 1; i <= 5; i++)
        {
            float radius = chartSize * (i / 5f);
            bool isOuter = (i == 5);
            DrawPentagon(vh, radius, isOuter ? outerLineWidth : innerLineWidth, isOuter ? lineColorOuter : lineColorInner);
        }
    }

    private void DrawCenterLines(VertexHelper vh)
    {
        Vector2[] outerVertices = CalculateVertices(chartSize);
        Vector2 center = Vector2.zero;
        
        foreach (Vector2 vertex in outerVertices)
        {
            DrawLine(vh, center, vertex, lineColorInner, innerLineWidth);
        }
    }

    // private void DrawDataPolygon(VertexHelper vh)
    // {
    //     if (elementsData == null) return;
        
    //     Vector2[] vertices = new Vector2[5];
    //     FiveElementsType[] types = {
    //         FiveElementsType.Metal,
    //         FiveElementsType.Wood,
    //         FiveElementsType.Water,
    //         FiveElementsType.Fire,
    //         FiveElementsType.Earth
    //     };
        
    //     // 计算数据顶点
    //     for (int i = 0; i < 5; i++)
    //     {
    //         if (elementsData.baseFiveElements.TryGetValue(types[i], out int value))
    //         {
    //             float radius = chartSize * (value / 10f);
    //             float angle = i * 72f;
    //             vertices[i] = new Vector2(
    //                 Mathf.Sin(angle * Mathf.Deg2Rad) * radius,
    //                 Mathf.Cos(angle * Mathf.Deg2Rad) * radius
    //             );
    //         }
    //     }
        
    //     // 绘制数据多边形
    //     for (int i = 0; i < 5; i++)
    //     {
    //         int next = (i + 1) % 5;
    //         DrawLine(vh, vertices[i], vertices[next], Color.blue, 2f); // 数据线用蓝色
    //     }
    // }

    private Vector2[] CalculateVertices(float radius)
    {
        Vector2[] vertices = new Vector2[5];
        for (int i = 0; i < 5; i++)
        {
            float angle = i * 72f; // 每个顶点间隔72度
            vertices[i] = new Vector2(
                Mathf.Sin(angle * Mathf.Deg2Rad) * radius,
                Mathf.Cos(angle * Mathf.Deg2Rad) * radius
            );
        }
        return vertices;
    }

    private void DrawPentagon(VertexHelper vh, float radius, float width, Color color)
    {
        Vector2[] vertices = CalculateVertices(radius);
        for (int i = 0; i < 5; i++)
        {
            int next = (i + 1) % 5;
            DrawLine(vh, vertices[i], vertices[next], color, width);
        }
    }

    private void DrawLine(VertexHelper vh, Vector2 start, Vector2 end, Color color, float width)
    {
        Vector2 dir = (end - start).normalized;
        Vector2 perpendicular = new Vector2(-dir.y, dir.x) * width / 2f;
        
        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = color;
        
        vh.AddVert(start - perpendicular, vertex.color, Vector2.zero);
        vh.AddVert(start + perpendicular, vertex.color, Vector2.zero);
        vh.AddVert(end + perpendicular, vertex.color, Vector2.zero);
        vh.AddVert(end - perpendicular, vertex.color, Vector2.zero);
        
        int baseIndex = vh.currentVertCount - 4;
        vh.AddTriangle(baseIndex, baseIndex + 1, baseIndex + 2);
        vh.AddTriangle(baseIndex, baseIndex + 2, baseIndex + 3);
    }
}