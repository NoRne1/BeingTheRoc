using Assets.Scripts.Sound;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;

public enum CanvasType
{
    ui = 0,
    tooltip = 1
}

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
        this.UIResources.Add(typeof(UIOptions), new UIElement() { Resource = "Prefabs/UIOptions", Cache = false });
        this.UIResources.Add(typeof(UIDescHint), new UIElement() { Resource = "Prefabs/UIDescHint", Cache = false });
        this.UIResources.Add(typeof(UITip), new UIElement() { Resource = "Prefabs/UITip", Cache = false });
        this.UIResources.Add(typeof(UITeamWindow), new UIElement() { Resource = "Prefabs/UITeamWindow", Cache = false });
        this.UIResources.Add(typeof(UIStoreItemHint), new UIElement() { Resource = "Prefabs/UIStoreItemHint", Cache = false });
        this.UIResources.Add(typeof(UISkillHint), new UIElement() { Resource = "Prefabs/UISkillHint", Cache = false });
        this.UIResources.Add(typeof(UISkillSelect), new UIElement() { Resource = "Prefabs/UISkillSelect", Cache = false });
        this.UIResources.Add(typeof(UIGameConsole), new UIElement() { Resource = "Prefabs/UIGameConsole", Cache = false });
        this.UIResources.Add(typeof(UIConfirmWindow), new UIElement() { Resource = "Prefabs/UIConfirmWindow", Cache = false });
        this.UIResources.Add(typeof(UICharacterHint), new UIElement() { Resource = "Prefabs/UICharacterHint", Cache = false });
        this.UIResources.Add(typeof(UITownHint), new UIElement() { Resource = "Prefabs/UITownHint", Cache = false });
        this.UIResources.Add(typeof(UIEventPage), new UIElement() { Resource = "Prefabs/UIEventPage", Cache = false });
        this.UIResources.Add(typeof(UIOptionsWindow), new UIElement() { Resource = "Prefabs/UIOptionsWindow", Cache = false });
    }

    ~UIManager()
    {

    }

    public T Show<T>(CanvasType canvasType = CanvasType.ui)
    {
        //播放声音
        //SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_Win_Open);
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
                Transform canvas = null;
                switch (canvasType)
                {
                    case CanvasType.ui:
                        canvas = GameObject.FindGameObjectWithTag("UICanvas").transform;
                        break;
                    case CanvasType.tooltip:
                        canvas = GameObject.FindGameObjectWithTag("ToolTipCanvas").transform;
                        break;
                }
                //创建资源实例
                info.Instance = (GameObject)GameObject.Instantiate(prefab, canvas);
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
    // public void Close(Type type)
    // {
    //     if (this.UIResources.ContainsKey(type))
    //     {
    //         //存在资源就获取信息
    //         UIElement info = this.UIResources[type];
    //         if (info.Cache)
    //         {
    //             //UI是缓存的,设置不可见
    //             info.Instance.SetActive(false);
    //         }
    //         else
    //         {
    //             //UI是不缓存的,销毁实例
    //             GameObject.Destroy(info.Instance);
    //             info.Instance = null;
    //         }
    //     }
    // }

    internal void Close<T>(bool playSound = true)
    {
        this.Close(typeof(T), playSound);
    }

    public T GetUIResource<T>() where T: class
    {
        Type type = typeof(T);
        if (this.UIResources.ContainsKey(type))
        {
            //UI资源包含该类型,则获取该资源
            UIElement info = this.UIResources[type];
            if (info.Instance != null)
            {
                return info.Instance.GetComponent<T>();
            } else
            {
                return null;
            }
        } else
        {
            return null;
        }
    }

    public bool HasActiveUIWindow()
    {
        foreach (var kvp in UIResources)
        {
            UIElement element = kvp.Value;
            if (element.Instance != null && element.Instance.activeInHierarchy)
            {
                return true;
            }
        }
        return false;
    }

    public void Close(Type type, bool playSound = true)
    {
        if (this.UIResources.ContainsKey(type))
        {
            // 存在资源就获取信息
            UIElement info = this.UIResources[type];
            if (playSound)
            {
                SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_Click);
            }
            // 启动协程延迟处理
            SceneManager.Instance.StartCoroutine(DelayedClose(info, type));
        }
    }

    private IEnumerator DelayedClose(UIElement info, Type type)
    {
        // 先延迟一帧，确保 HasActiveUIWindow() 返回 true
        yield return null;

        // 视觉效果：隐藏或销毁 UI
        if (info.Cache)
        {
            info.Instance.SetActive(false); // 隐藏 UI
        }
        else
        {
            GameObject.Destroy(info.Instance); // 销毁 UI 实例
            info.Instance = null;
        }
    }
}
