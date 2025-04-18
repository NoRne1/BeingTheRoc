using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Sound;
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
    game = 6,
    restaurant = 7,
}

public enum TimeInterval
{
    morning,
    afternoon,
    night
}

public class GameOtherProperty
{
    //currentCollectPlanIndex.value != null表示当前有召集任务，collectCharacterTimer.value == 0表示任务可交付
    public BehaviorSubject<CollectCharacterInfo> currentCollectPlanInfo = new BehaviorSubject<CollectCharacterInfo>(null);
    public BehaviorSubject<int> collectCharacterTimer = new BehaviorSubject<int>(-1);

    //currentMergeTaskInfo.value != null表示当前有召集任务，mergeTaskTimer.value == 0表示任务可交付
    public BehaviorSubject<MergeEquipInfo> currentMergeTaskInfo = new BehaviorSubject<MergeEquipInfo>(null);
    public BehaviorSubject<int> mergeTaskTimer = new BehaviorSubject<int>(-1);
}

public class GameManager : MonoSingleton<GameManager>
{
    public List<GameObject> pages;
    public UICommonUI commonUI;
    public CanvasGroup taBlackPanel;
    public Dictionary<PageType, GameObject> pagesDic = new Dictionary<PageType, GameObject>();

    public BehaviorSubject<int> featherCoin = new BehaviorSubject<int>(2000);
    public BehaviorSubject<int> wheatCoin = new BehaviorSubject<int>(300);
    public BehaviorSubject<int> timeLeft = new BehaviorSubject<int>(92);
    public BehaviorSubject<TimeInterval> timeInterval = new BehaviorSubject<TimeInterval>(TimeInterval.morning);
    public Subject<PageType> switchPageSubject = new Subject<PageType>();
    public PageType currentPageType = PageType.map;

    public Dictionary<string, NorneRelay<CharacterModel>> characterRelaysDic = new Dictionary<string, NorneRelay<CharacterModel>>();

    public RepositoryModel repository = new RepositoryModel();
    public TreasureManager treasureManager;
    public UITreasuresRect treasuresRect;
    public GameOtherProperty otherProperty = new GameOtherProperty();

    public WeatherDefine currentWeather;
    //0表示刚触发过事件，所以初始设置为999，表示当前可以触发事件
    public int eventNotInvokeTime = 999;
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
                GameManager.Instance.timeLeft.Select(timeleft=>(int)(timeleft / 3)).DistinctUntilChanged().Subscribe(_=>{
                    //新一天
                    RefreshWeather();
                }).AddTo(this);
            }
        });
        // 时段变化
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
        // 游戏结束判断
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
        BlackBarManager.Instance.AddMessage(GameUtil.Instance.GetDisplayString("game_over"));
        yield return new WaitForSeconds(1);
        SceneManager.Instance.LoadScene("start_game");
        SoundManager.Instance.PlayMusic(SoundDefine.Music_Main_Menu);
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
        switchPageSubject.OnNext(pageType);
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
                SoundManager.Instance.PlayMusic(SoundDefine.Music_Map_Page);
                break;
            case PageType.town:
                commonUI.setUIStyle(CommonUIStyle.town);
                UITownPage townPage = pagesDic[PageType.town].GetComponent<UITownPage>();
                townPage.SetActionPanels(MapManager.Instance.CurrentTownNode.model.townActions);
                SoundManager.Instance.PlayMusic(SoundDefine.Music_Town_Page);
                break;
            case PageType.battle:
                commonUI.setUIStyle(CommonUIStyle.battle);
                SoundManager.Instance.PlayMusic(SoundDefine.Music_Normal_Battle);
                break;
            case PageType.bar:
                commonUI.setUIStyle(CommonUIStyle.bar);
                SoundManager.Instance.PlayMusic(SoundDefine.Music_Bar_Page);
                break;
            case PageType.forge:
                commonUI.setUIStyle(CommonUIStyle.actionPage);
                SoundManager.Instance.PlayMusic(SoundDefine.Music_Forge_Page);
                break;
            case PageType.shop:
                commonUI.setUIStyle(CommonUIStyle.actionPage);
                SoundManager.Instance.PlayMusic(SoundDefine.Music_Shop_Page);
                break;
            case PageType.game:
                commonUI.setUIStyle(CommonUIStyle.game);
                SoundManager.Instance.PlayMusic(SoundDefine.Music_Game_Page);
                break;
            case PageType.restaurant:
                commonUI.setUIStyle(CommonUIStyle.restaurant);
                SoundManager.Instance.PlayMusic(SoundDefine.Music_Restaurant_Page);
                break;
            default:
                commonUI.setUIStyle(CommonUIStyle.actionPage);
                SoundManager.Instance.StopMusic();
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
            tip.UpdateGeneralTip("0004");
            return;
        }
        wheatCoin.OnNext(wheatCoin.Value + change);
    }

    //只有主动点击时钟，跨越时段才可能触发事件
    public void TimeChanged(int change, bool isPassive)
    {
        timeLeft.OnNext(timeLeft.Value + change);
        if (otherProperty.currentCollectPlanInfo.Value != null && otherProperty.collectCharacterTimer.Value > 0)
        {
            otherProperty.collectCharacterTimer.OnNext(Math.Max(0, otherProperty.collectCharacterTimer.Value + change));
        }
        if (otherProperty.currentMergeTaskInfo.Value != null && otherProperty.mergeTaskTimer.Value > 0)
        {
            otherProperty.mergeTaskTimer.OnNext(Math.Max(0, otherProperty.mergeTaskTimer.Value + change));
        }
        eventNotInvokeTime -= change;
        if(!isPassive && eventNotInvokeTime >= GlobalAccess.eventInvokeInterval)
        {
            //是主动跨跃时段且距离上一次触发时间满足间歇时间
            if (GameUtil.Instance.GetRandomRate(GlobalAccess.eventInvokeRate))
            {
                var page = UIManager.Instance.Show<UIEventPage>();
                page.Setup(DataManager.Instance.eventsDefines[0]);
                eventNotInvokeTime = 0;
            }
        }
        HungryChange(change);
    }

    // 玩家饥饿度变化
    public void HungryChange(int timeChange)
    {
        if(timeChange < 0) {
            // 区别战斗内外
            if(BattleManager.Instance.isInBattle)
            {
                var itemIDs = BattleManager.Instance.battleItemManager.playerItemIDs
                    .Concat(BattleManager.Instance.battleItemManager.enemyItemIDs).ToList();
                foreach (var index in Enumerable.Range(0, itemIDs.Count))
                {
                    var battleItem = GlobalAccess.GetBattleItem(itemIDs[index]);
                    int wheatConsume = timeChange * battleItem.attributes.hungryConsume;
                    battleItem.HungryChange(wheatConsume);
                }
            } else 
            {
                var characterRelays = characterRelaysDic.Values.ToList();
                foreach (var index in Enumerable.Range(0, characterRelays.Count))
                {
                    var cm = characterRelays[index].Value;
                    int wheatConsume = timeChange * cm.attributes.hungryConsume;
                    cm.HungryChange(wheatConsume);
                }
            }
        }
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
        NorneStore.Instance.Remove(new CharacterModel(uuid));

        //游戏结束判断
        if(characterRelaysDic.Count == 0)
        {
            StartCoroutine(GameOver());
        }
    }

    public void CharacterHPChange(string uuid, int change)
    {
        var cm = GlobalAccess.GetCharacterModel(uuid);
        cm.attributes.currentHP = Mathf.Max(Mathf.Min(cm.attributes.MaxHP, cm.attributes.currentHP + change), 0);
        if (cm.attributes.currentHP <= 0)
        {
            RemoveCharacter(uuid);
        }
        GlobalAccess.SaveCharacterModel(cm, false);
    }

    public void RefreshWeather()
    {
        currentWeather = DataManager.Instance.weatherDefines.Values.ToList().RandomItem();
        commonUI.weatherPanel.Setup(currentWeather);
        if (BattleManager.Instance.isInBattle)
        {
            BattleManager.Instance.battleItemManager.RefreshWeatherEffect();
        }
    }
}
