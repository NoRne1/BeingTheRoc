using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UniRx;
using System.Linq;
using System;
using Unity.Mathematics;

public class CollectResultLayer : MonoBehaviour
{
    public CanvasGroup selfMask;
    public Transform resultButtons;
    private List<GameObject> resultButtonsList = new List<GameObject>();
    private List<UICollectCharacterButton> characterButtons = new List<UICollectCharacterButton>();
    private List<UICollectItemButton> collectItemButtons = new List<UICollectItemButton>();

    public GameObject characterButtonPrefab;
    public GameObject collectItemButtonPrefab;

    public Button acceptButton;
    public Button rejectButton;
    // Start is called before the first frame update
    private void Start()
    {
        acceptButton.OnClickAsObservable().Subscribe(_=> {
            StartCoroutine(AcceptAll());
        }).AddTo(this);
        rejectButton.OnClickAsObservable().Subscribe(_=> {
            StartCoroutine(Close());
        }).AddTo(this);
    }

    public GeneralLevel RefreshItem(CollectCharacterInfo info)
    {
        int maxLevel = -1;
        resultButtonsList.Clear();
        characterButtons.Clear();
        collectItemButtons.Clear();
        GameUtil.Instance.DetachChildren(resultButtons);

        
        var resultList = DataManager.Instance.GetRandomLevelDefine<CharacterDefine>(DataManager.Instance.levelCharacters, info.greenRate, info.blueRate, info.redRate, 5, true);
        foreach(var result in resultList)
        {    
            if (result.Item1) 
            {
                //随机到角色了
                var characterButtonObject = Instantiate(characterButtonPrefab, resultButtons);
                var characterButton = characterButtonObject.GetComponent<UICollectCharacterButton>();
                resultButtonsList.Add(characterButtonObject);
                characterButtons.Add(characterButton);
                characterButton.Setup(new CharacterModel(result.Item2));
                characterButton.GetComponent<Button>().OnClickAsObservable().Subscribe(_=>{
                    AcceptCharacter(characterButton);
                }).AddTo(characterButton);

                maxLevel = math.max((int)result.Item2.Level, maxLevel);
            } else 
            {
                //随机的是垃圾
                var collectItemObject = Instantiate(collectItemButtonPrefab, resultButtons);
                var collectItem = collectItemObject.GetComponent<UICollectItemButton>();
                resultButtonsList.Add(collectItemObject);
                collectItemButtons.Add(collectItem);
                
                var collectItemDefine = DataManager.Instance.collectItemDefines[UnityEngine.Random.Range(0, DataManager.Instance.collectItemDefines.Count)];
                collectItem.Setup(new CollectItemModel(collectItemDefine));
                collectItem.GetComponent<Button>().OnClickAsObservable().Subscribe(_=>{
                    AcceptCollectItem(collectItem);
                }).AddTo(collectItem);
            }
        }
        return (GeneralLevel)maxLevel;
    }
    
    private void AcceptCharacter(UICollectCharacterButton characterButton)
    {
        if (GameManager.Instance.characterRelaysDic.Count + 1 <= GlobalAccess.teamOpacity)
        {
            GameManager.Instance.AddCharacter(characterButton.model);
            characterButton.transform.DOMove(acceptButton.transform.position, 0.5f).SetEase(Ease.InQuad);
            characterButton.transform.DOScaleX(0, 0.5f).SetEase(Ease.InQuad);
            characterButton.transform.DOScaleY(0, 0.5f).SetEase(Ease.InQuad).OnComplete(()=>{
                resultButtonsList.Remove(characterButton.gameObject);
                characterButtons.Remove(characterButton);
                Destroy(characterButton.gameObject);
            });
        } else 
        {
            UITip tip = UIManager.Instance.Show<UITip>();
            tip.UpdateTip("accept_character_beyond_limit");
        }
    }

    private void AcceptCollectItem(UICollectItemButton collectItem)
    {
        switch(collectItem.model.type)
        {
            case CollectItemModelType.feather:
                GameManager.Instance.FeatherCoinChanged(collectItem.model.num);
                break;
            case CollectItemModelType.wheat:
                GameManager.Instance.WheatCoinChanged(collectItem.model.num);
                break;
            default:
                Debug.LogWarning("CollectResultLayer AcceptAll Unknown collectItem type");
                break;
        }
        collectItem.transform.DOMove(acceptButton.transform.position, 0.5f).SetEase(Ease.InQuad);
        collectItem.transform.DOScaleX(0, 0.5f).SetEase(Ease.InQuad);
        collectItem.transform.DOScaleY(0, 0.5f).SetEase(Ease.InQuad).OnComplete(()=>{
            resultButtonsList.Remove(collectItem.gameObject);
            collectItemButtons.Remove(collectItem);
            Destroy(collectItem.gameObject);
        });
    }

    public IEnumerator AcceptAll()
    {
        bool errorFlag = false;
        //处理collectItemButtons
        int featherNum = 0;
        int wheatNum = 0;
        foreach(var collectItem in collectItemButtons)
        {
            switch(collectItem.model.type)
            {
                case CollectItemModelType.feather:
                    featherNum += collectItem.model.num;
                    break;
                case CollectItemModelType.wheat:
                    wheatNum += collectItem.model.num;
                    break;
                default:
                    Debug.LogWarning("CollectResultLayer AcceptAll Unknown collectItem type");
                    break;
            }
            collectItem.transform.DOMove(acceptButton.transform.position, 0.5f).SetEase(Ease.InQuad);
            collectItem.transform.DOScaleX(0, 0.5f).SetEase(Ease.InQuad);
            collectItem.transform.DOScaleY(0, 0.5f).SetEase(Ease.InQuad).OnComplete(()=>{
                resultButtonsList.Remove(collectItem.gameObject);
                Destroy(collectItem.gameObject);
            });
        }
        collectItemButtons.Clear();

        //处理characterButtons
        if (GameManager.Instance.characterRelaysDic.Count + characterButtons.Count <= GlobalAccess.teamOpacity)
        {
            foreach(var characterButton in characterButtons)
            {
                GameManager.Instance.AddCharacter(characterButton.model);
                characterButton.transform.DOMove(acceptButton.transform.position, 0.5f).SetEase(Ease.InQuad);
                characterButton.transform.DOScaleX(0, 0.5f).SetEase(Ease.InQuad);
                characterButton.transform.DOScaleY(0, 0.5f).SetEase(Ease.InQuad).OnComplete(()=>{
                    resultButtonsList.Remove(characterButton.gameObject);
                    Destroy(characterButton.gameObject);
                });
            }
            characterButtons.Clear();
        } else {
            errorFlag = true;
        }
        yield return new WaitForSeconds(0.5f);
        if (featherNum > 0){GameManager.Instance.FeatherCoinChanged(featherNum);}
        if (wheatNum > 0){GameManager.Instance.WheatCoinChanged(wheatNum);}
        
        if (!errorFlag)
        {
            //没错误，直接关闭
            StartCoroutine(Close());
        } else {
            UITip tip = UIManager.Instance.Show<UITip>();
            tip.UpdateTip("accept_character_beyond_limit_plural");
        }
    }

    public IEnumerator Show()
    {
        StartCoroutine(GameUtil.Instance.FadeIn(selfMask, 0.3f));
        foreach (var button in resultButtonsList)
        {
            button.transform.localScale = new Vector3(0, 0, 1);
        }
        yield return new WaitForSeconds(0.3f);

        foreach (var button in resultButtonsList)
        {
            // 使用 DOTween 动画 y 轴从  到 1，持续时间为 1 秒
            button.transform.DOScaleX(1, 0.5f).SetEase(Ease.OutQuad);
            button.transform.DOScaleY(1, 0.5f).SetEase(Ease.OutQuad);
            yield return new WaitForSeconds(0.5f);
        }
    }
    
    public IEnumerator Close()
    {
        StartCoroutine(GameUtil.Instance.FadeOut(selfMask, 0.15f));
        yield return new WaitForSeconds(0.15f);
    }
}
