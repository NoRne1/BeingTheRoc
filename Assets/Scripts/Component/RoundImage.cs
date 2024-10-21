using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class RoundImage : MonoBehaviour
{
    public float factor = 0.2f; // 圆角为最小边长的乘法系数
    private Image image;

    void Awake()
    {
        image = GetComponent<Image>();

        // 为每个Image创建材质实例，而不是共享材质
        if (image.material != null)
        {
            image.material = Instantiate(image.material);
        }

        UpdateCornerRadius();
    }

    void Update()
    {
        UpdateCornerRadius();
    }

    // 根据当前设置更新圆角
    void UpdateCornerRadius()
    {
        // 比例模式，根据长宽计算圆角半径
        image.material.SetFloat("_Radius", factor);

        // 自动将Image的Sprite的纹理赋值给材质的_MainTex
        if (image.sprite != null && image.sprite.texture != null)
        {
            image.material.SetTexture("_MainTex", image.sprite.texture);
        }
    }
}

