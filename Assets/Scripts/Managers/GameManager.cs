
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

public enum PageType
{
    map = 0,
    town = 1,
    battle = 2,
    bar = 3,
    forge = 4,
    shop = 5,
    train = 6,
    walk = 7,
    restautant = 8,
}

public enum TimeInterval
{
    morning,
    afternoon,
    night
}

public class GameManager : MonoSingleton<GameManager>
{
    public List<GameObject> pages;
    public UICommonUI commonUI;
    public CanvasGroup taBlackPanel;
    public Dictionary<PageType, GameObject> pagesDic = new Dictionary<PageType, GameObject>();

    public BehaviorSubject<int> featherCoin = new BehaviorSubject<int>(1000);
    public BehaviorSubject<int> wheatCoin = new BehaviorSubject<int>(300);
    public BehaviorSubject<int> timeLeft = new BehaviorSubject<int>(92);
    public BehaviorSubject<TimeInterval> timeInterval = new BehaviorSubject<TimeInterval>(TimeInterval.morning);
    public PageType currentPageType = PageType.map;

    public Dictionary<string, NorneRelay<CharacterModel>> characterRelaysDic = new Dictionary<string, NorneRelay<CharacterModel>>();

    public RepositoryModel repository = new RepositoryModel();
    public TreasureManager treasureManager;
    public UITreasuresRect treasuresRect;
    // Start is called before the first frame update
    void Start()
    {
        treasureManager = new TreasureManager();
        treasuresRect.Setup(treasureManager.GetTreasuresList());
        treasureManager.treasuresUpdate.AsObservable()
            .TakeUntilDestroy(this).Subscribe(_ =>
            {
                treasuresRect.Setup(treasureManager.GetTreasuresList());
            });
        if (pages.Count == System.Enum.GetValues(typeof(PageType)).Length)
        {
            for (int i = 0; i < pages.Count; i++)
            {
                pagesDic.Add((PageType)i, pages[i]);
            }
        } else
        {
            UITip tip = UIManager.Instance.Show<UITip>();
            tip.UpdateGeneralTip("0001");
        }

        //todo
        NorneStore.Instance.RemoveAll();
        DataManager.Instance.DataLoaded.AsObservable().Take(2).TakeUntilDestroy(this).Subscribe(flag =>
        {
            if (flag)
            {
                // init characters
                CharacterDefine playerDefine = DataManager.Instance.Characters[GlobalAccess.CurrentCharacterId];
                CharacterModel mainCharacter = new CharacterModel(playerDefine);
                NorneStore.Instance.Update<CharacterModel>(mainCharacter, isFull: true);
                mainCharacter.InitInvokeSkill();
                characterRelaysDic.Add(mainCharacter.uuid, NorneStore.Instance.ObservableObject(mainCharacter));
                List<int> ids = GameUtil.Instance.GenerateUniqueRandomList(GlobalAccess.subCharacterStartIndex,
                    GlobalAccess.subCharacterStartIndex + GlobalAccess.subCharacterNum, 2);
                foreach (var id in ids)
                {
                    CharacterDefine define = DataManager.Instance.Characters[id];
                    CharacterModel model = new CharacterModel(define);
                    NorneStore.Instance.Update<CharacterModel>(model, isFull: true);
                    model.InitInvokeSkill();
                    characterRelaysDic.Add(model.uuid, NorneStore.Instance.ObservableObject(model));
                }
            }
        });

        timeLeft.AsObservable().DistinctUntilChanged().TakeUntilDestroy(this).Subscribe(time =>
        {
            if (time % 3 == 2){
                timeInterval.OnNext(TimeInterval.morning);
            } else if (time % 3 == 1){
                timeInterval.OnNext(TimeInterval.afternoon);
            } else if (time % 3 == 0){
                timeInterval.OnNext(TimeInterval.night);
            }
        });

        timeLeft.AsObservable().DistinctUntilChanged().TakeUntilDestroy(this).Subscribe(time =>
        {
            if (time<=0) 
            {
                StartCoroutine(GameOver());
            }
        });
    }

    private IEnumerator GameOver()
    {
        BlackBarManager.Instance.AddMessage("游戏失败");
        yield return new WaitForSeconds(1);
        SceneManager.Instance.LoadScene("start_game");
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Return) && !UIManager.Instance.HasActiveUIWindow())
        {
            UIManager.Instance.Show<UIGameConsole>();
        }
    }

    public void SwitchPage(PageType pageType, CoroutineAction action = null)
    {
        StartCoroutine(SwitchPageIEnumerator(pageType, action));
    }

    public delegate void CoroutineAction();
    IEnumerator SwitchPageIEnumerator(PageType pageType, CoroutineAction action)
    {
        //隐藏目前的page(黑幕渐显)
        foreach (KeyValuePair<PageType, GameObject> pair in pagesDic)
        {
            if (pair.Key != pageType && pair.Value.activeInHierarchy)
            {
                StartCoroutine(GameUtil.Instance.FadeIn(taBlackPanel, 0.3f));
                yield return new WaitForSeconds(0.45f);
                pair.Value.SetActive(false);
                break;
            }
        }

        currentPageType = pageType;

        switch (pageType)
        {
            case PageType.map:
                commonUI.setUIStyle(CommonUIStyle.map);
                break;
            case PageType.town:
                commonUI.setUIStyle(CommonUIStyle.town);
                UITownPage townPage = pagesDic[PageType.town].GetComponent<UITownPage>();
                townPage.SetActionPanels(MapManager.Instance.CurrentTownNode.townActions);
                break;
            case PageType.battle:
                commonUI.setUIStyle(CommonUIStyle.battle);
                break;
            case PageType.bar:
                commonUI.setUIStyle(CommonUIStyle.actionPage);
                break;
            case PageType.forge:
                commonUI.setUIStyle(CommonUIStyle.actionPage);
                break;
            case PageType.shop:
                commonUI.setUIStyle(CommonUIStyle.actionPage);
                break;
            case PageType.train:
                commonUI.setUIStyle(CommonUIStyle.actionPage);
                break;
            case PageType.walk:
                commonUI.setUIStyle(CommonUIStyle.actionPage);
                break;
            case PageType.restautant:
                commonUI.setUIStyle(CommonUIStyle.restautant);
                break;
        }

        foreach (KeyValuePair<PageType, GameObject> pair in pagesDic)
        {
            if (pair.Key == pageType)
            {
                pair.Value.SetActive(true);

                yield return null;
                action?.Invoke();

                StartCoroutine(GameUtil.Instance.FadeOut(taBlackPanel, 0.4f));
                break;
            }
        }
    }

    public void FeatherCoinChanged(int change)
    {
        if (featherCoin.Value + change < 0)
        {
            //错误请求(扣成负的了)
            UITip tip = UIManager.Instance.Show<UITip>();
            tip.UpdateGeneralTip("0003");
            return;
        }
        featherCoin.OnNext(featherCoin.Value + change);
    }

    public void WheatCoinChanged(int change)
    {
        if (wheatCoin.Value + change < 0)
        {
            //错误请求(扣成负的了)
            UITip tip = UIManager.Instance.Show<UITip>();
            tip.UpdateGeneralTip("0003");
            return;
        }
        wheatCoin.OnNext(wheatCoin.Value + change);
    }

    public void TimeChanged(int change)
    {
        timeLeft.OnNext(timeLeft.Value + change);
    }

    public void AddCharacter(CharacterModel cm)
    {
        NorneStore.Instance.Update<CharacterModel>(cm, true);
        characterRelaysDic.Add(cm.uuid, NorneStore.Instance.ObservableObject(cm));
    }

    public void RemoveCharacter(string uuid)
    {
        if (characterRelaysDic.ContainsKey(uuid))
        {
            characterRelaysDic.Remove(uuid);
        }
    }
}
