using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class UIBarPage : MonoBehaviour
{
    public CollectCharacterLayer collectCharacterLayer;
    public Button collectCharacterButton;
    public TextMeshProUGUI collectCharacterText;
    private IDisposable collectCharacterTextDisposable;
    // Start is called before the first frame update
    void Start()
    {
        collectCharacterText.text = GameUtil.Instance.GetDisplayString("召集");
        collectCharacterLayer.collectButtonSubject.AsObservable().Subscribe(info=>{
            if (GameManager.Instance.otherProperty.currentCollectPlanInfo.Value == null)
            {
                StartCollectCharacter(info);
            } else {
                //当前有正在执行的召集任务
                Debug.LogError("UIBarPage collectButtonSubject 意外点击，当前有正在执行的召集任务");
            }
        }).AddTo(this);
        GameManager.Instance.otherProperty.currentCollectPlanInfo.DistinctUntilChanged().AsObservable()
            .CombineLatest(GameManager.Instance.otherProperty.collectCharacterTimer.DistinctUntilChanged().AsObservable(), (info, timer)=>(info, timer))
            .Subscribe(para => {
            if (para.info == null)
            {
                collectCharacterTextDisposable.IfNotNull(dispose => { dispose.Dispose(); });
                collectCharacterText.text = GameUtil.Instance.GetDisplayString("召集");
                collectCharacterButton.enabled = true;
            } else if (para.info != null && para.timer == 0)
            {
                //召集任务到期了
                collectCharacterTextDisposable.IfNotNull(dispose => { dispose.Dispose(); });
                collectCharacterText.text = GameUtil.Instance.GetDisplayString("召集完成!");
                collectCharacterButton.enabled = true;
            } else if (para.info != null && para.timer > 0)
            {
                //当前有召集任务，也没到期
                string[] displayStrings = { "召集中", "召集中.", "召集中..", "召集中..." };
                collectCharacterTextDisposable.IfNotNull(dispose => { dispose.Dispose(); });
                // 使用 Observable.Interval 创建一个每隔一段时间发射一次的可观察序列
                collectCharacterTextDisposable = Observable.Interval(System.TimeSpan.FromSeconds(1f))
                    .Select(index => displayStrings[index % displayStrings.Length]) // 根据当前索引选择字符串
                    .Subscribe(text => collectCharacterText.text = GameUtil.Instance.GetDisplayString(text)) // 更新文本
                    .AddTo(this); // 确保在对象销毁时取消订阅
                collectCharacterButton.enabled = false;
            }
            // else {
                // Debug.LogError("UIBarPage unknown state");
            // }
        }).AddTo(this);
        collectCharacterButton.OnClickAsObservable().Subscribe(_=>
        {
            if (GameManager.Instance.otherProperty.currentCollectPlanInfo.Value != null && GameManager.Instance.otherProperty.collectCharacterTimer.Value == 0)
            {
                Debug.Log(GameManager.Instance.otherProperty.currentCollectPlanInfo.Value.title + "被执行啦！！！！！！！");
                ProcessCollectCharacter(GameManager.Instance.otherProperty.currentCollectPlanInfo.Value);
            } else {
                OpenCollectCharacterLayer();
            }
        }).AddTo(this);
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartCollectCharacter(CollectCharacterInfo info)
    {
        GameManager.Instance.otherProperty.currentCollectPlanInfo.OnNext(info);
        GameManager.Instance.otherProperty.collectCharacterTimer.OnNext(info.waitTime * 3);
        CloseCollectCharacterLayer();
    }
    public void ProcessCollectCharacter(CollectCharacterInfo info)
    {
        GameManager.Instance.otherProperty.currentCollectPlanInfo.OnNext(null);
        GameManager.Instance.otherProperty.collectCharacterTimer.OnNext(-1);
    }

    public void OpenCollectCharacterLayer()
    {
        collectCharacterLayer.gameObject.SetActive(true);
        StartCoroutine(collectCharacterLayer.Show());
    }

    public void CloseCollectCharacterLayer()
    {
        StartCoroutine(collectCharacterLayer.Close());
    }
}
