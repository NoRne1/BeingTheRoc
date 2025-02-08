using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    public List<HintComponent> chairItems;
    private CollectCharacterInfo info = new CollectCharacterInfo(0,GameUtil.Instance.GetDisplayString("neutral_collect"),0.03f,0.08f,0.14f,0.75f,1,0);
    private IDisposable collectCharacterTextDisposable;

    private int dayLeft = -1;

    // Start is called before the first frame update
    void Start()
    {
        collectCharacterText.text = GameUtil.Instance.GetDisplayString("collect");
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
                collectCharacterText.text = GameUtil.Instance.GetDisplayString("collect");
                collectCharacterButton.enabled = true;
            } else if (para.info != null && para.timer == 0)
            {
                //召集任务到期了
                collectCharacterTextDisposable.IfNotNull(dispose => { dispose.Dispose(); });
                collectCharacterText.text = GameUtil.Instance.GetDisplayString("collect_done");
                collectCharacterButton.enabled = true;
            } else if (para.info != null && para.timer > 0)
            {
                //当前有召集任务，也没到期
                string[] displayStrings = { GameUtil.Instance.GetDisplayString("collecting"), GameUtil.Instance.GetDisplayString("collecting")+".", 
                    GameUtil.Instance.GetDisplayString("collecting") + "..", GameUtil.Instance.GetDisplayString("collecting") + "..." };
                collectCharacterTextDisposable.IfNotNull(dispose => { dispose.Dispose(); });
                // 使用 Observable.Interval 创建一个每隔一段时间发射一次的可观察序列
                collectCharacterTextDisposable = Observable.Interval(System.TimeSpan.FromSeconds(1f))
                    .Select(index => displayStrings[index % displayStrings.Length]) // 根据当前索引选择字符串
                    .Subscribe(text => collectCharacterText.text = text) // 更新文本
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
        foreach (var item in chairItems)
        {
            item.GetComponent<Button>().OnClickAsObservable().Subscribe(_=>{
                ClickChairItem(item);
            }).AddTo(this);
        }
        GameManager.Instance.timeLeft.DistinctUntilChanged().Subscribe(timeLeft=>{
            RefreshChairItems(timeLeft);
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
            tip.UpdateTip("collect_no_money");
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

    public void RefreshChairItems(int timeLeft)
    {
        if (timeLeft % 3 == 0)
        {
            
            var resultList = DataManager.Instance.GetRandomLevelDefine<CharacterDefine>(DataManager.Instance.levelCharacters,info.greenRate, info.blueRate, info.redRate, chairItems.Count, true);
            foreach(var index in Enumerable.Range(0, chairItems.Count))
            {    
                if (resultList[index].Item1) 
                {
                    //随机到角色了
                    chairItems[index].Setup(new CharacterModel(resultList[index].Item2));
                    chairItems[index].gameObject.SetActive(true);
                }
                else {
                    chairItems[index].gameObject.SetActive(false);
                }
            }
        } else 
        {
            foreach(var item in chairItems)
            {
                item.gameObject.SetActive(false);
            }
        }
    }

    private void ClickChairItem(HintComponent hint)
    {
        if (hint.Character != null)
        {
            if (GameManager.Instance.characterRelaysDic.Count + 1 <= GlobalAccess.teamOpacity)
            {
                var price = hint.hintObject.GetComponent<UICharacterHint>().price;
                if (price <= GameManager.Instance.featherCoin.Value)
                {
                    GameManager.Instance.FeatherCoinChanged(-price);
                    GameManager.Instance.AddCharacter(hint.Character);
                    hint.gameObject.SetActive(false);
                    UIManager.Instance.Close<UICharacterHint>();
                } else {
                    //钱不够买
                    UITip tip = UIManager.Instance.Show<UITip>();
                    //todo
                    tip.UpdateTip("buy_character_no_money");
                }
            } else {
                UITip tip = UIManager.Instance.Show<UITip>();
                tip.UpdateTip("buy_character_beyond_limit");
            }
        } else 
        {
            Debug.LogError("UIBarPage ClickChairItem HintComponent.Character cannot be null");
        }
    }
}
