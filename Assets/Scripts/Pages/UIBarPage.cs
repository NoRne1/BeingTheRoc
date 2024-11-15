using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class UIBarPage : MonoBehaviour
{
    public CollectCharacterLayer collectCharacterLayer;
    public CollectResultLayer collectResultLayer;
    public Button collectCharacterButton;
    public Image collectCharacterButtonLight;
    public List<Sprite> levelLightSprites;
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
        if (info.price > GameManager.Instance.featherCoin.Value)
        {
            //钱不够买
            UITip tip = UIManager.Instance.Show<UITip>();
            //todo
            tip.UpdateGeneralTip("钱不够买，todo！");
        } else {
            GameManager.Instance.otherProperty.currentCollectPlanInfo.OnNext(info);
            GameManager.Instance.otherProperty.collectCharacterTimer.OnNext(info.waitTime * 3);
            GameManager.Instance.FeatherCoinChanged(-info.price);
        }
        CloseCollectCharacterLayer();
    }
    public void ProcessCollectCharacter(CollectCharacterInfo info)
    {
        GameManager.Instance.otherProperty.currentCollectPlanInfo.OnNext(null);
        GameManager.Instance.otherProperty.collectCharacterTimer.OnNext(-1);
        
        collectResultLayer.gameObject.SetActive(true);
        var maxLevel = collectResultLayer.RefreshItem(info);

        //处理maxLevel闪光效果
        switch (maxLevel)
        {
            case GeneralLevel.none:
                collectCharacterButtonLight.overrideSprite = levelLightSprites[levelLightSprites.Count - 1];
                break;
            case GeneralLevel.green:
            case GeneralLevel.blue:
            case GeneralLevel.red:
                collectCharacterButtonLight.overrideSprite = levelLightSprites[(int)maxLevel];
                break;
        }
        // 透明度从 0 到 1
        collectCharacterButtonLight.DOFade(1, 0.5f).SetEase(Ease.InOutQuad).OnComplete(() =>
        {
            collectCharacterButtonLight.DOFade(0, 0.3f).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                collectCharacterButtonLight.color = new Color(collectCharacterButtonLight.color.r, 
                                                            collectCharacterButtonLight.color.g, 
                                                            collectCharacterButtonLight.color.b, 
                                                            0); 
                StartCoroutine(collectResultLayer.Show());
            });
        }); // 透明度从 1 到 0
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
