using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    public bool InChessBoard(Vector2 vect)
    {
        return vect.x >= 0 && vect.x <= 7 && vect.y >= 0 && vect.y <= 7;
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

    //仅判断行动力的范围（显示可行动范围）
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

    public IEnumerator BlinkObject(GameObject obj, float blinkDuration, int blinkCount = 2)
    {
        for (int i = 0; i < blinkCount; i++)
        {
            // 根据初始状态决定先隐藏还是先显示
            if (obj.activeSelf)
                obj.SetActive(false);
            else
                obj.SetActive(true);
            yield return new WaitForSeconds(blinkDuration);

            if (obj.activeSelf)
                obj.SetActive(false);
            else
                obj.SetActive(true);
            yield return new WaitForSeconds(blinkDuration);
        }
    }

    public RaycastHit2D? RaycastAndFindFirstHit(Vector2 position, System.Func<RaycastHit2D, bool> condition)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(position, Vector2.zero);

        // 遍历射线碰撞到的每个对象
        foreach (RaycastHit2D hit in hits)
        {
            // 调用传入的条件 lambda 表达式，判断是否满足条件
            if (condition(hit))
            {
                return hit; // 返回第一个符合条件的 RaycastHit2D 对象
            }
        }

        return null; // 如果未找到符合条件的对象，返回 null
    }

    // 根据物体位置和大小计算合适的偏移量
    public Vector2 CalculateOffset(Vector2 screenPosition, Vector2 prefabSize)
    {
        // 获取屏幕的宽度和高度
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        // 获取相机的视野范围
        float cameraOrthographicSize = Camera.main.orthographicSize;
        float cameraAspect = Camera.main.aspect;

        // 计算相机的宽度和高度
        float cameraWidth = cameraOrthographicSize * 2 * cameraAspect;
        float cameraHeight = cameraOrthographicSize * 2;

        // 计算偏移量
        float offsetX = 0f;
        float offsetY = 0f;

        // 如果物体越过了屏幕边界，则调整偏移量使其保持在屏幕内
        if (screenPosition.x + prefabSize.x <= screenWidth)
        {
            offsetX = prefabSize.x / 2;
        }
        else
        {
            offsetX = -prefabSize.x / 2;
        }

        if (screenPosition.y - prefabSize.y >= 0)
        {
            offsetY = -prefabSize.y / 2;
        }
        else
        {
            offsetY = prefabSize.y / 2;
        }

        return new Vector2(offsetX, offsetY);
    }

    public string GetDisplayString(string text)
    {
        return text;
    }

    public List<Vector2> GetTargetRangeList(Vector2 vect, TargetRange targetRange)
    {
        List<Vector2> result;
        bool flag = true;
        switch (targetRange)
        {
            case TargetRange.range_1:
                result = new List<Vector2>()
                {
                    new Vector2(0, 1),
                    new Vector2(0, -1),
                    new Vector2(1, 0),
                    new Vector2(-1, 0),
                };
                break;
            case TargetRange.range_2:
                result = new List<Vector2>()
                {
                    new Vector2(0, 1),
                    new Vector2(0, -1),
                    new Vector2(1, 0),
                    new Vector2(-1, 0),
                    new Vector2(-2, 0),
                    new Vector2(2, 0),
                    new Vector2(0, 2),
                    new Vector2(0, -2),
                    new Vector2(1, 1),
                    new Vector2(1, -1),
                    new Vector2(-1, 1),
                    new Vector2(-1, -1),
                };
                break;
            case TargetRange.range_3:
                result = new List<Vector2>()
                {
                    new Vector2(0, 1),
                    new Vector2(0, -1),
                    new Vector2(1, 0),
                    new Vector2(-1, 0),
                    new Vector2(-2, 0),
                    new Vector2(2, 0),
                    new Vector2(0, 2),
                    new Vector2(0, -2),
                    new Vector2(-3, 0),
                    new Vector2(3, 0),
                    new Vector2(0, 3),
                    new Vector2(0, -3),
                    new Vector2(1, 1),
                    new Vector2(1, -1),
                    new Vector2(-1, 1),
                    new Vector2(-1, -1),
                    new Vector2(2, 1),
                    new Vector2(2, -1),
                    new Vector2(-2, 1),
                    new Vector2(-2, -1),
                    new Vector2(1, 2),
                    new Vector2(1, -2),
                    new Vector2(-1, 2),
                    new Vector2(-1, -2),
                };
                break;
            case TargetRange.archer:
                result = new List<Vector2>()
                {
                    new Vector2(-2, 0),
                    new Vector2(2, 0),
                    new Vector2(0, 2),
                    new Vector2(0, -2),
                    new Vector2(1, 1),
                    new Vector2(1, -1),
                    new Vector2(-1, 1),
                    new Vector2(-1, -1),
                };
                break;
            case TargetRange.archer_long:
                result = new List<Vector2>()
                {
                    new Vector2(-3, 0),
                    new Vector2(3, 0),
                    new Vector2(0, 3),
                    new Vector2(0, -3),
                    new Vector2(2, 1),
                    new Vector2(2, -1),
                    new Vector2(-2, 1),
                    new Vector2(-2, -1),
                    new Vector2(1, 2),
                    new Vector2(1, -2),
                    new Vector2(-1, 2),
                    new Vector2(-1, -2),
                };
                break;
            case TargetRange.around_8:
                result = new List<Vector2>()
                {
                    new Vector2(0, 1),
                    new Vector2(0, -1),
                    new Vector2(1, 0),
                    new Vector2(-1, 0),
                    new Vector2(1, 1),
                    new Vector2(1, -1),
                    new Vector2(-1, 1),
                    new Vector2(-1, -1),
                };
                break;
            case TargetRange.line:
                int boardSize = 8;
                List<Vector2> tempList = new List<Vector2>();
                // 获取同行的坐标
                for (int x = 0; x < boardSize; x++)
                {
                    if (x != vect.x) // 排除传入坐标本身
                    {
                        tempList.Add(new Vector2(x, vect.y));
                    }
                }

                // 获取同列的坐标
                for (int y = 0; y < boardSize; y++)
                {
                    if (y != vect.y && y != vect.y) // 排除传入坐标本身
                    {
                        tempList.Add(new Vector2(vect.x, y));
                    }
                }
                result = tempList;
                flag = false;
                break;
            case TargetRange.none:
            default:
                result = new List<Vector2>();
                break;
        }
        if (flag)
        {
            return result.Select(temp => { return temp + vect; })
                .Where(temp => { return GameUtil.Instance.InChessBoard(temp); }).ToList();
        }
        else
        {
            return result;
        }
    }

    public int CalculateStrengthAngryTexture(int healthPercentage)
    {
        // 确保输入在0到100之间
        healthPercentage = Math.Max(0, Math.Min(100, healthPercentage));

        // 二次插值的力量值
        double a = 0.004;
        double b = 0.2;
        double strength = a * Math.Pow((100 - healthPercentage), 2) + b * (100 - healthPercentage);

        // 确保输出在0到60之间
        strength = Math.Max(0, Math.Min(60, strength));

        return (int)strength;
    }
}