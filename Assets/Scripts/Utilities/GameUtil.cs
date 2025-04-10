﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using UniRx;
using UnityEngine;


public class GameUtil : Singleton<GameUtil>
{
    private System.Random _random = new System.Random();
    public bool GetRandomRate(float rate)
    {
        return UnityEngine.Random.Range(0, 100.0f) < rate;
    }

    public bool GetRandomRate_affected(float rate)
    {
        return UnityEngine.Random.Range(0, 100.0f) < rate;
    }

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

    public bool InScreen(Transform a)
    {
        // 尝试通过Renderer获取边界
        Renderer renderer = a.GetComponent<Renderer>();
        if (renderer != null)
        {
            // 如果有Renderer，使用Renderer.bounds
            return IsWithinScreenBounds(renderer.bounds);
        }

        // 如果没有Renderer，尝试使用Collider获取边界
        Collider collider = a.GetComponent<Collider>();
        if (collider != null)
        {
            // 如果有Collider，使用Collider.bounds
            return IsWithinScreenBounds(collider.bounds);
        }

        // 如果没有Renderer和Collider，尝试使用RectTransform获取边界
        RectTransform rectTransform = a.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            // 如果有RectTransform，使用sizeDelta和localPosition计算边界
            return IsWithinScreenBounds(rectTransform);
        }

        // 如果没有Renderer、Collider和RectTransform，返回false
        return false;
    }

    // 辅助方法：判断物体的Bounds是否在屏幕内
    private bool IsWithinScreenBounds(Bounds bounds)
    {
        Vector3[] objectCorners = new Vector3[4];
        objectCorners[0] = Camera.main.WorldToScreenPoint(bounds.min); // 左下角
        objectCorners[1] = Camera.main.WorldToScreenPoint(new Vector3(bounds.min.x, bounds.max.y, bounds.min.z)); // 左上角
        objectCorners[2] = Camera.main.WorldToScreenPoint(bounds.max); // 右上角
        objectCorners[3] = Camera.main.WorldToScreenPoint(new Vector3(bounds.max.x, bounds.min.y, bounds.min.z)); // 右下角

        // 判断所有角是否都在屏幕内
        foreach (var corner in objectCorners)
        {
            if (Screen.safeArea.Contains(corner))
            {
                return true;
            }
        }

        return false;
    }

    // 使用RectTransform来计算边界
    private bool IsWithinScreenBounds(RectTransform rectTransform)
    {
        // 获取UI元素的四个角
        Vector3[] objectCorners = new Vector3[4];
        rectTransform.GetWorldCorners(objectCorners);

        // 判断所有角是否都在屏幕内
        foreach (var corner in objectCorners)
        {
            if (Screen.safeArea.Contains(Camera.main.WorldToScreenPoint(corner)))
            {
                return true;
            }
        }

        return false;
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

    //世界坐标移动一个屏幕坐标的位置
    public Vector3 GetMovedWorldPosition(Vector3 worldPosition, Vector3 movePostion) 
    {
        return Camera.main.ScreenToWorldPoint(Camera.main.WorldToScreenPoint(worldPosition) + movePostion);
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

    //判断是否是主角
    public bool IsMainCharacter(int id)
    {
        return id >= 0 && id < GlobalAccess.mainCharacterNum;
    }

    //[begin,end)
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

        // 偏移量变量
        float offsetX = 0f;
        float offsetY = 0f;

        // 四个角落的可用空间和超出屏幕的面积
        float topLeftOverflow = 0f;
        float topRightOverflow = 0f;
        float bottomLeftOverflow = 0f;
        float bottomRightOverflow = 0f;

        // 判断物体是否能够完全放在四个角落之一，并计算超出屏幕的面积
        bool canFitTopLeft = screenPosition.x - prefabSize.x >= 0 && screenPosition.y + prefabSize.y <= screenHeight;
        bool canFitTopRight = screenPosition.x + prefabSize.x <= screenWidth && screenPosition.y + prefabSize.y <= screenHeight;
        bool canFitBottomLeft = screenPosition.x - prefabSize.x >= 0 && screenPosition.y - prefabSize.y >= 0;
        bool canFitBottomRight = screenPosition.x + prefabSize.x <= screenWidth && screenPosition.y - prefabSize.y >= 0;

        if (canFitBottomRight)
        {
            offsetX = prefabSize.x / 2;
            offsetY = -prefabSize.y / 2;
            return new Vector2(offsetX, offsetY);
        }
        else if (canFitTopRight)
        {
            offsetX = prefabSize.x / 2;
            offsetY = prefabSize.y / 2;
            return new Vector2(offsetX, offsetY);
        }
        else if (canFitBottomLeft)
        {
            offsetX = -prefabSize.x / 2;
            offsetY = -prefabSize.y / 2;
            return new Vector2(offsetX, offsetY);
        }
        else if (canFitTopLeft)
        {
            offsetX = -prefabSize.x / 2;
            offsetY = prefabSize.y / 2;
            return new Vector2(offsetX, offsetY);
        }

        // 计算每个角落超出屏幕的面积
        topLeftOverflow = Mathf.Max(0, prefabSize.x - screenPosition.x) * prefabSize.y + Mathf.Max(0, prefabSize.y + screenPosition.y - screenHeight) * prefabSize.x;
        topRightOverflow = Mathf.Max(0, prefabSize.x + screenPosition.x - screenWidth) * prefabSize.y + Mathf.Max(0, prefabSize.y + screenPosition.y - screenHeight) * prefabSize.x;
        bottomLeftOverflow = Mathf.Max(0, prefabSize.x - screenPosition.x) * prefabSize.y + Mathf.Max(0, prefabSize.y - screenPosition.y) * prefabSize.x;
        bottomRightOverflow = Mathf.Max(0, prefabSize.x + screenPosition.x - screenWidth) * prefabSize.y + Mathf.Max(0, prefabSize.y - screenPosition.y) * prefabSize.x;

        // 找到最小超出面积的角落
        float minOverflow = Mathf.Min(topLeftOverflow, topRightOverflow, bottomLeftOverflow, bottomRightOverflow);
 
        if (minOverflow == bottomRightOverflow)
        {
            offsetX = prefabSize.x / 2;
            offsetY = -prefabSize.y / 2;
        }
        else if (minOverflow == topRightOverflow)
        {
            offsetX = prefabSize.x / 2;
            offsetY = prefabSize.y / 2;
        }
        else if (minOverflow == bottomLeftOverflow)
        {
            offsetX = -prefabSize.x / 2;
            offsetY = -prefabSize.y / 2;
        }
        else if (minOverflow == topLeftOverflow)
        {
            offsetX = -prefabSize.x / 2;
            offsetY = prefabSize.y / 2;
        }

        return new Vector2(offsetX, offsetY);
    }

    //todo muti language
    public string GetDirectDisplayString(string text)
    {
        return text;
    }

    public string GetDisplayString(string text)
    {
        // return text;
        return DataManager.Instance.Language[text].ReplaceNewLines();
    }

    //既可以获取攻击范围（传入攻击者坐标），也可以获取可攻击目标的所有位置（传入目标位置）
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

    // public T DeepCopy<T>(T obj)
    // {
    //     if (obj == null)
    //     {
    //         return default(T);
    //     }
    //     JsonSerializerSettings settings = new JsonSerializerSettings();
    //     //后面发现这俩可以处理。但是由于dic的key存储时会被转换为字符串，然后再反序列就回不来了，所以要单独处理
    //     // settings.Converters.Add(new Vector2Converter());
    //     // settings.Converters.Add(new Vector2IntConverter());
    //     settings.Converters.Add(new Vec2DictionaryConverter());
        
    //     string json = JsonConvert.SerializeObject(obj, settings);
    //     var result = JsonConvert.DeserializeObject<T>(json, settings);
    //     return result;
    // }

    public T DeepCopy<T>(T obj)
    {
        if (obj == null)
        {
            return default(T);
        }

        // 使用 HashSet 来避免重复拷贝相同的对象（避免死循环）
        var visitedObjects = new HashSet<object>();

        return DeepCopyInternal(obj, visitedObjects);
    }

   private T DeepCopyInternal<T>(T obj, HashSet<object> visitedObjects)
    {
        if (obj == null)
            return default(T);

        // 如果已经处理过该对象，直接返回该对象，避免递归
        if (visitedObjects.Contains(obj))
        {
            return obj;
        }

        visitedObjects.Add(obj);

        return (T)DeepCopyInternalCore(obj, typeof(T), visitedObjects);
    }

    private object DeepCopyInternalCore(object obj, Type type, HashSet<object> visitedObjects)
    {
        if (obj == null)
            return null;

        // 如果已经处理过该对象，直接返回该对象，避免递归
        if (visitedObjects.Contains(obj))
        {
            return obj;
        }

        visitedObjects.Add(obj);

        // 如果是值类型或字符串，直接返回
        if (type.IsValueType || obj is string)
        {
            return obj;
        }

        // 处理字典类型
        if (obj is IDictionary dict)
        {
            var newDict = (IDictionary)Activator.CreateInstance(type);

            foreach (var key in dict.Keys)
            {
                var newKey = DeepCopyInternalCore(key, key.GetType(), visitedObjects);
                var newValue = DeepCopyInternalCore(dict[key], dict[key].GetType(), visitedObjects);
                newDict.Add(newKey, newValue);
            }

            return newDict;
        }

        // 处理集合类型
        if (obj is IEnumerable list)
        {
            var newList = Activator.CreateInstance(type) as IList;

            foreach (var item in list)
            {
                var newItem = DeepCopyInternalCore(item, item.GetType(), visitedObjects);
                newList.Add(newItem);
            }

            return newList;
        }

        // 创建目标类型的实例
        var newObject = Activator.CreateInstance(type);

        // 遍历字段
        foreach (var field in type.GetFields())
        {
            if (field.FieldType.IsClass && field.FieldType != typeof(string))
            {
                var fieldValue = field.GetValue(obj);
                var newFieldValue = DeepCopyInternalCore(fieldValue, field.FieldType, visitedObjects);
                field.SetValue(newObject, newFieldValue);
            }
            else
            {
                field.SetValue(newObject, field.GetValue(obj));
            }
        }

        // 遍历属性
        foreach (var property in type.GetProperties())
        {
            if (property.CanWrite && property.PropertyType.IsClass && property.PropertyType != typeof(string))
            {
                var propertyValue = property.GetValue(obj);
                var newPropertyValue = DeepCopyInternalCore(propertyValue, property.PropertyType, visitedObjects);
                property.SetValue(newObject, newPropertyValue);
            }
            else
            {
                property.SetValue(newObject, property.GetValue(obj));
            }
        }

        return newObject;
    }

    public Effect BattleEffectToItemUseEffect(BattleEffect battleEffect)
    {
        switch (battleEffect)
        {
            case BattleEffect.DashToTarget:
                return new Effect()
                {
                    effectType = EffectType.battleEffect,
                    invokeType = EffectInvokeType.useInstant,
                    invokeTime = 1,
                    methodName = "DashToTarget",
                    value = 0
                };
            case BattleEffect.Backward:
                return new Effect()
                {
                    effectType = EffectType.battleEffect,
                    invokeType = EffectInvokeType.useInstant,
                    invokeTime = 1,
                    methodName = "Backward",
                    value = 1
                };
            case BattleEffect.Knockback:
                return new Effect()
                {
                    effectType = EffectType.battleEffect,
                    invokeType = EffectInvokeType.useInstant,
                    invokeTime = 1,
                    methodName = "Knockback",
                    value = 1
                };
            case BattleEffect.ReturnEnergy:
                return new Effect()
                {
                    effectType = EffectType.battleEffect,
                    invokeType = EffectInvokeType.useInstant,
                    invokeTime = 1,
                    methodName = "ReturnEnergy"
                };
            default:
                Debug.LogError("Unknown battle effect type.");
                return null;
        }
    }

    public int GetTrulyFloatFactor(int factor)
    {
        return UnityEngine.Random.Range(-factor, factor);
    }

    public FiveElements GetFiveElements()
    {
        return new FiveElements(
            _random.Next(0, 6),
            _random.Next(0, 6),
            _random.Next(0, 6),
            _random.Next(0, 6),
            _random.Next(0, 6)
        );
    }
}