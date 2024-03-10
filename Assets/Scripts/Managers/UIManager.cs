using Assets.Scripts.Sound;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIManager: Singleton<UIManager>
{
    //UI信息
    class UIElement
    {
        //资源路径
        public string Resource;
        //是否缓存
        public bool Cache;
        //资源实例
        public GameObject Instance;
    }
    //维护一个资源字典
    private Dictionary<Type, UIElement> UIResources = new Dictionary<Type, UIElement>();

    public UIManager()
    {
        this.UIResources.Add(typeof(UISoundSetting), new UIElement() { Resource = "UI/UISoundSetting", Cache = false });
    }

    ~UIManager()
    {

    }

    //显示UI
    public T Show<T>()
    {
        //播放声音
        SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_Win_Open);
        //UI类型
        Type type = typeof(T);
        if (this.UIResources.ContainsKey(type))
        {
            //UI资源包含该类型,则获取该资源
            UIElement info = this.UIResources[type];
            if (info.Instance != null)
            {
                //资源实例已经存在,则设置为显示状态
                info.Instance.SetActive(true);
            }
            else
            {
                //资源实例不存在,则创建一个新的实例
                //从资源路径加载资源
                UnityEngine.Object prefab = Resources.Load(info.Resource);
                if (prefab == null)
                {
                    //资源为空
                    return default(T);
                }
                //创建资源实例
                info.Instance = (GameObject)GameObject.Instantiate(prefab);
            }
            //返回资源脚本
            return info.Instance.GetComponent<T>();
        }
        else
        {
            //UI资源不包含该类型
            return default(T);
        }
    }

    //关闭UI
    public void Close(Type type)
    {
        SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_Win_Close);
        if (this.UIResources.ContainsKey(type))
        {
            //存在资源就获取信息
            UIElement info = this.UIResources[type];
            if (info.Cache)
            {
                //UI是缓存的,设置不可见
                info.Instance.SetActive(false);
            }
            else
            {
                //UI是不缓存的,销毁实例
                GameObject.Destroy(info.Instance);
                info.Instance = null;
            }
        }
    }
    internal void Close<T>()
    {
        this.Close(typeof(T));
    }
}
