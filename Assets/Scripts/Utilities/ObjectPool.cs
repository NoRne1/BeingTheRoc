using System.Collections.Generic;
using UnityEngine;

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
        GameObject newObj = Object.Instantiate(prefab, parentTransform); // 指定父节点
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

    public void ReturnObjectToPool(GameObject obj)
    {
        obj.SetActive(false);
    }

    public void ReturnAllObject()
    {
        foreach(var obj in pooledObjects)
        {
            obj.SetActive(false);
        }
    }
}

