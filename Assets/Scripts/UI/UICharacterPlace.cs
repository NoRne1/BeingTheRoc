using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UICharacterPlace : MonoBehaviour
{
    public float floatHeight = 10.0f; // 浮动的高度
    public float floatSpeed = 1.0f;  // 浮动的速度

    private Vector3 startPos;         // 初始位置

    
    public Image characterIcon;
    public Image placeIcon;
    // Start is called before the first frame update
    void Start()
    {
        // 记录初始位置
        startPos = placeIcon.transform.position;
    }

    void Update()
    {
        // 计算垂直方向的浮动偏移量
        float offsetY = Mathf.Sin(Time.time * floatSpeed) * floatHeight;

        // 更新物体的位置
        placeIcon.transform.position = startPos + new Vector3(0, offsetY, 0);
    }
}
