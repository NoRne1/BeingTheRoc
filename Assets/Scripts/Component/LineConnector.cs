using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;
using System;

public class LineConnector : MonoBehaviour
{
    public GameObject linePrefab;  // 连接线的 prefab
    private List<GameObject> lineConnections = new List<GameObject>();  // 存储多个连接线
    private List<LineConnection> connections = new List<LineConnection>(); // 存储连接的点对

    public bool isConnected => connections.Count > 0;  // 是否有任何连接

    [System.Serializable]
    public class LineConnection
    {
        public Transform pointA;  // 连接的点A
        public Transform pointB;  // 连接的点B
        public GameObject line1;  // 连接点A的linePrefab
        public GameObject line2;  // 连接点B的linePrefab
    }

    void Start()
    {
        // 初始化时不连接
        lineConnections.Clear();
        connections.Clear();
    }

    void Update()
    {
        // 如果有连接，更新所有连接线的位置和旋转
        if (isConnected)
        {
            UpdateAllLinePositions();
        }
    }

    // 开始连接：创建并显示连接线
    public void AddConnection(Transform pointA, Transform pointB)
    {
        // 如果已经有相同的连接，跳过
        foreach (var conn in connections)
        {
            if ((conn.pointA == pointA && conn.pointB == pointB) || (conn.pointA == pointB && conn.pointB == pointA))
            {
                Debug.LogWarning("这两个点已经连接！");
                return;
            }
        }

        // 创建linePrefab并保存连接数据
        GameObject newLine1 = Instantiate(linePrefab, transform);
        GameObject newLine2 = Instantiate(linePrefab, transform);

        LineConnection newConnection = new LineConnection
        {
            pointA = pointA,
            pointB = pointB,
            line1 = newLine1,
            line2 = newLine2
        };

        connections.Add(newConnection);
        UpdateLinePositions(newConnection);  // 立即更新这组连接线的位置
    }

    // 关闭连接：禁用指定的连接线
    public void StopConnection(Transform pointA, Transform pointB)
    {
        // 查找并销毁连接线
        for (int i = 0; i < connections.Count; i++)
        {
            var conn = connections[i];
            if ((conn.pointA == pointA && conn.pointB == pointB) || (conn.pointA == pointB && conn.pointB == pointA))
            {
                Destroy(conn.line1);
                Destroy(conn.line2);
                connections.RemoveAt(i);
                Debug.Log("连接已关闭");
                return;
            }
        }

        Debug.LogWarning("没有找到匹配的连接！");
    }

    // 关闭所有连接：禁用所有连接线
    public void StopAllConnections()
    {
        foreach (var conn in connections)
        {
            Destroy(conn.line1);
            Destroy(conn.line2);
        }
        connections.Clear();
        Debug.Log("所有连接已关闭");
    }

    // 更新所有连接线的位置和旋转
    void UpdateAllLinePositions()
    {
        foreach (var conn in connections)
        {
            UpdateLinePositions(conn);
        }
    }

    // 更新单个连接线的位置和旋转
    void UpdateLinePositions(LineConnection conn)
    {
        if (conn == null || conn.line1 == null || conn.line2 == null || conn.pointA == null || conn.pointB == null)
        return;
    
    // 将A点和B点的位置转换到屏幕空间
    Vector3 pointAPos = Camera.main.WorldToScreenPoint(conn.pointA.position);
    Vector3 pointBPos = Camera.main.WorldToScreenPoint(conn.pointB.position);

    // 计算两点间的方向和距离
    Vector3 dirVector3 = pointBPos - pointAPos; // A到B的方向
    float distance = dirVector3.magnitude;
    float sideSpacing = MathF.Min(60f, 0.1f * distance);
    // 计算 spacing 对应的长度比例
    float totalLength = distance - sideSpacing * 2; // 总长度，减去两条线之间的间隔
    var spacing = MathF.Max(0f, MathF.Min(0.9f, 0.000004f * distance)) * distance;
    var a = (totalLength - spacing)/2;
    float lineLength = Mathf.Max(5f, Mathf.Min(200f, a)); // 线条长度限制

    // 更新line1和line2的位置
    Vector3 line1Pos = pointAPos + dirVector3.normalized * (lineLength / 2f + sideSpacing); // line1接近点A
    Vector3 line2Pos = pointBPos - dirVector3.normalized * (lineLength / 2f + sideSpacing); // line2接近点B

    conn.line1.transform.position = Camera.main.ScreenToWorldPoint(line1Pos);
    conn.line2.transform.position = Camera.main.ScreenToWorldPoint(line2Pos);

    // 设置line1和line2的尺寸，根据线条的长度来调整
    conn.line1.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(lineLength, 10f);
    conn.line2.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(lineLength, 10f);

    // 计算旋转角度
    float angle = Vector3.Angle(Vector3.right, dirVector3); // 计算角度，水平向右为0度
    if (dirVector3.y < 0)
    {
        angle = -angle; // 如果是负方向，需要调整为负角度
    }

    // 设置line1和line2的旋转，使其与AB的方向一致
    conn.line1.transform.rotation = Quaternion.Euler(0, 0, angle);
    conn.line2.transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}