using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

public class ObjectPool
{
    private GameObject prefab;
    private Transform parentTransform; // 父节点 Transform
    private List<GameObject> pooledObjects = new List<GameObject>();

    public ObjectPool(GameObject prefab, int initialSize, Transform parentTransform)
    {
        this.prefab = prefab;
        this.parentTransform = parentTransform;

        // 预先创建初始数量的对象
        for (int i = 0; i < initialSize; i++)
        {
            CreateObject();
        }
    }

    private GameObject CreateObject()
    {
        GameObject newObj = GameObject.Instantiate(prefab, parentTransform); // 指定父节点
        newObj.SetActive(false);
        pooledObjects.Add(newObj);
        return newObj;
    }

    public GameObject GetObjectFromPool()
    {
        // 查找未激活的对象并返回
        foreach (GameObject obj in pooledObjects)
        {
            if (!obj.activeSelf)
            {
                obj.SetActive(true);
                return obj;
            }
        }

        // 如果没有未激活的对象，创建一个新对象并返回
        GameObject newObj = CreateObject();
        newObj.SetActive(true);
        return newObj;
    }

    public GameObject FindObject(Func<GameObject, bool> predicate)
    {
        // 使用 lambda 表达式在对象池中查找符合条件的对象
        foreach (GameObject obj in pooledObjects)
        {
            if (obj.activeSelf && predicate(obj))
            {
                return obj;
            }
        }
        return null;
    }

    public void ReturnObjectToPool(GameObject obj)
    {
        obj.SetActive(false);
    }

    public bool ReturnOneActiveObject()
    {
        // 查找未激活的对象并返回
        foreach (GameObject obj in pooledObjects)
        {
            if (obj.activeSelf)
            {
                ReturnObjectToPool(obj);
                return true;
            }
        }
        return false;
    }

    public void ReturnAllObject()
    {
        foreach(var obj in pooledObjects)
        {
            obj.SetActive(false);
        }
    }
}

public class ButtonObjectPool
{
    private GameObject buttonPrefab;
    private Transform parentTransform;
    private List<GameObject> pooledObjects = new List<GameObject>();
    private Action<Button> onClickAction; // 点击事件的回调方法

    public ButtonObjectPool(GameObject buttonPrefab, int initialSize, Transform parentTransform, Action<Button> onClickAction)
    {
        this.buttonPrefab = buttonPrefab;
        this.parentTransform = parentTransform;
        this.onClickAction = onClickAction;

        // 预先创建初始数量的按钮并设置点击事件
        for (int i = 0; i < initialSize; i++)
        {
            CreateObject();
        }
    }

    ~ButtonObjectPool()
    {
        foreach (GameObject obj in pooledObjects)
        {
            obj.GetComponent<Button>().onClick.RemoveAllListeners();
        }
    }

    private GameObject CreateObject()
    {
        GameObject newButtonObject = GameObject.Instantiate(buttonPrefab, parentTransform);
        newButtonObject.SetActive(false);
        Button newButton = newButtonObject.GetComponent<Button>();

        // 添加按钮点击事件监听器
        newButton.onClick.AddListener(() => OnButtonClick(newButton));

        pooledObjects.Add(newButtonObject);
        return newButtonObject;
    }

    public GameObject GetObjectFromPool()
    {
        // 查找未激活的对象并返回
        foreach (GameObject obj in pooledObjects)
        {
            if (!obj.activeSelf)
            {
                obj.SetActive(true);
                return obj;
            }
        }

        // 如果没有未激活的对象，创建一个新对象并返回
        GameObject newObj = CreateObject();
        newObj.SetActive(true);
        return newObj;
    }

    public void ReturnObjectToPool(GameObject obj)
    {
        obj.SetActive(false);
    }

    public void ReturnAllObject()
    {
        foreach (var obj in pooledObjects)
        {
            obj.SetActive(false);
        }
    }

    private void OnButtonClick(Button clickedButton)
    {
        Debug.Log("Button clicked!");

        // 调用传入的 lambda 表达式
        onClickAction?.Invoke(clickedButton);
    }
}
