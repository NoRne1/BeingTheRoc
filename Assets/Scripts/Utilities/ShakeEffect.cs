using UnityEngine;

public class ShakeEffect : MonoBehaviour
{
    public float shakeIntensity = 0.1f; // 抖动强度
    public float shakeDuration = 0.5f; // 抖动持续时间

    private Vector3 originalPosition; // 原始位置
    private float shakeTimer = 0f; // 抖动计时器

    void Start()
    {
        originalPosition = transform.position; // 记录初始位置
    }

    void Update()
    {
        if (shakeTimer > 0)
        {
            // 生成随机偏移量
            Vector3 randomOffset = Random.insideUnitSphere * shakeIntensity;

            // 应用偏移量到位置
            transform.position = originalPosition + randomOffset;

            // 减少计时器
            shakeTimer -= Time.deltaTime;
        }
        else
        {
            // 抖动结束，恢复到原始位置
            transform.position = originalPosition;
        }
    }

    public void TriggerShake()
    {
        // 开始抖动，重置计时器
        shakeTimer = shakeDuration;
    }
}
