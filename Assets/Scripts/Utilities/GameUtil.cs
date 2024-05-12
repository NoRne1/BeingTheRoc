using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UniRx;
using UnityEngine;


public class GameUtil : Singleton<GameUtil>
{
    public void DetachChildren(Transform nodeFather) {
        foreach (Transform child in nodeFather)
        {
            // 销毁子节点
            UnityEngine.Object.Destroy(child.gameObject);
        }
        // 清空子节点列表
        nodeFather.DetachChildren();
    }

    public string GenerateUniqueId()
    {
        return Guid.NewGuid().ToString();
    }

    public bool InScreen(Vector3 position)
    {
        return Screen.safeArea.Contains(Camera.main.WorldToScreenPoint(position));
    }

    public bool IsPointInsideGameObject(GameObject gameObject, Vector3 screenPoint)
    {
        Vector2 localPoint;
        var rectTransform = gameObject.GetComponent<RectTransform>();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, Camera.main, out localPoint);
        // 检查点是否在范围内
        if (rectTransform.rect.Contains(localPoint))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public int CalcDamage(float power, float agile, float attack, float armor)
    {
        if (UnityEngine.Random.Range(0f, 100f) < agile)
        {
            return 0;
        } else
        {
            return (int)Mathf.Floor(power * (1f + attack / 100f) * (1 - (armor * 0.05f / (1f + armor * 0.05f))));
        }
    }

    public IEnumerator FadeIn(CanvasGroup canvasGroup, float transitionDuration)
    {
        canvasGroup.alpha = 0;
        canvasGroup.gameObject.SetActive(true);
        float elapsedTime = 0;
        while (elapsedTime < transitionDuration)
        {
            canvasGroup.alpha = Mathf.Lerp(0, 1, (elapsedTime / transitionDuration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = 1;
    }

    public IEnumerator FadeOut(CanvasGroup canvasGroup, float transitionDuration)
    {
        canvasGroup.gameObject.SetActive(true);
        canvasGroup.alpha = 1;
        float elapsedTime = 0;
        while (elapsedTime < transitionDuration)
        {
            canvasGroup.alpha = Mathf.Lerp(1, 0, (elapsedTime / transitionDuration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = 0;
        canvasGroup.gameObject.SetActive(false);
    }

    public Color hexToColor(string hex, float alpha = 1f)
    {
        hex = hex.Replace("#", ""); // 去除十六进制字符串中的 "#" 符号
        byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber); // 解析红色分量
        byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber); // 解析绿色分量
        byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber); // 解析蓝色分量
        byte a = (byte)(alpha * 255f); // 默认不透明

        if (hex.Length == 8)
        {
            a = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber); // 解析透明度分量
        }

        return new Color32(r, g, b, a);
    }

    public T CreateObjectFromDictionary<T>(Dictionary<string, object> dict) where T : new()
    {
        T obj = new T();
        foreach (var kvp in dict)
        {
            PropertyInfo property = typeof(T).GetProperty(kvp.Key);
            if (property != null && property.CanWrite)
            {
                property.SetValue(obj, kvp.Value);
            }
        }
        return obj;
    }

    public IEnumerator RotateCoroutine(Transform transform, Quaternion startRotation, Quaternion endRotation, float duration, BehaviorSubject<bool> isRotating)
    {
        isRotating.OnNext(true);
         // 绕 Z 轴逆时针旋转 90 度
        float startTime = Time.time;

        while (Time.time - startTime < duration)
        {
            float t = (Time.time - startTime) / duration;
            if (transform != null)
            {
                transform.rotation = Quaternion.Lerp(startRotation, endRotation, t);
            } else
            {
                break;
            }
            yield return null;
        }
        if (transform != null)
        {
            // 确保结束时确切地达到目标旋转状态
            transform.rotation = endRotation;
        }
        isRotating.OnNext(false);
    }

    public bool CanMoveTo(Vector2 source, Vector2 dest, int mobility)
    {
        float temp = Mathf.Abs(source.x - dest.x) + Mathf.Abs(source.y - dest.y);
        return temp != 0 && temp <= mobility;
    }

    public List<int> GenerateUniqueRandomList(int begin, int end, int num)
    {
        if (num > (end - begin) || begin > end)
        {
            throw new ArgumentException("Invalid input parameters.");
        }

        List<int> result = new List<int>();
        List<int> allPossibleValues = new List<int>();

        // 构建包含所有可能随机数的列表
        for (int i = begin; i < end; i++)
        {
            allPossibleValues.Add(i);
        }

        System.Random random = new System.Random();

        // 使用洗牌算法获取不重复的随机数列表
        for (int i = 0; i < num; i++)
        {
            int index = random.Next(0, allPossibleValues.Count);
            result.Add(allPossibleValues[index]);
            allPossibleValues.RemoveAt(index);
        }

        return result;
    }
}