using Assets.Scripts.Sound;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UniRx;

public abstract class UIWindow: MonoBehaviour
{
    //委托
    // public delegate void CloseHandler(UIWindow sender,WindowResult result);
    //事件
    // public event CloseHandler OnClose;
    public Subject<(UIWindow, WindowResult)> OnCloseSubject = new Subject<(UIWindow, WindowResult)>();
    //UI类型
    public virtual Type Type { get { return this.GetType(); } }
    //根节点
    public GameObject Root;
    //结果枚举
    public enum WindowResult
    {
        None = 0,
        Yes,
        No
    }
    //关闭UI
    public void Close(WindowResult result = WindowResult.None)
    {
        //SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_Win_Close);
        UIManager.Instance.Close(this.Type);
        OnCloseSubject.OnNext((this, result));
        OnCloseSubject.OnCompleted();
    }
    public virtual void OnCloseClick()
    {
        this.Close();
    }
    public virtual void OnYesClick()
    {
        this.Close(WindowResult.Yes);
    }
    public virtual void OnNoClick()
    {
        this.Close(WindowResult.No);
    }
}
