using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;

public class FiveElementsPropertyDisplay : MonoBehaviour 
{
    [Header("References")]
    [SerializeField] private FiveElements elementsData;
    [SerializeField] private Material radarMaterial;
    [SerializeField] private float chartSize = 100f;
    
    [Header("Visual Settings")]
    [SerializeField] private Color[] elementColors = new Color[5];
    [SerializeField] private TextMeshProUGUI[] elementLabels;

    private CanvasRenderer radarRenderer;
    private Vector2[] vertices = new Vector2[6]; // 5顶点+闭合点

    void Start()
    {
        radarRenderer = GetComponent<CanvasRenderer>();
    }

    public void UpdateElements(FiveElements newData)
    {
        elementsData = newData;
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        // 更新雷达图
        UpdateRadarMesh();
        
        // 更新标签
        UpdateLabels();
    }

    private void UpdateRadarMesh()
    {
        float angleIncrement = 360f / 5;
        float currentAngle = 0f; // 从上方开始（Metal）

        for (int i = 0; i < 5; i++)
        {
            FiveElementsType type = (FiveElementsType)i;
            if (elementsData.baseFiveElements.TryGetValue(type, out int value))
            {
                float normalizedValue = value / 10f;
                float x = Mathf.Sin(currentAngle * Mathf.Deg2Rad) * chartSize * normalizedValue;
                float y = Mathf.Cos(currentAngle * Mathf.Deg2Rad) * chartSize * normalizedValue;
                vertices[i] = new Vector2(x, y);
            }
            currentAngle += angleIncrement;
        }
        vertices[5] = vertices[0]; // 闭合多边形

        // 创建网格
        Mesh mesh = new Mesh();
        mesh.vertices = System.Array.ConvertAll(vertices, v => new Vector3(v.x, v.y, 0));
        
        // 三角形索引（扇形分割）
        int[] triangles = new int[15]; // 5个三角形×3顶点
        for (int i = 0; i < 5; i++)
        {
            triangles[i*3] = 5;       // 中心点
            triangles[i*3+1] = i;     // 当前顶点
            triangles[i*3+2] = (i+1)%5;// 下一顶点
        }
        
        mesh.triangles = triangles;
        radarRenderer.SetMesh(mesh);
        radarRenderer.SetMaterial(radarMaterial, null);
    }

    private void UpdateLabels()
    {
        for (int i = 0; i < 5; i++)
        {
            FiveElementsType type = (FiveElementsType)i;
            if (elementsData.baseFiveElements.TryGetValue(type, out int value))
            {
                elementLabels[i].text = $"{value}";
                elementLabels[i].color = elementColors[i];
                
                // 定位标签到顶点位置
                Vector2 pos = vertices[i] * 1.2f; // 外扩20%
                elementLabels[i].rectTransform.anchoredPosition = pos;
            }
        }
    }
}
