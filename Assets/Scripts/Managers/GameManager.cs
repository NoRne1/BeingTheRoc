using System.Collections;
using System.Collections.Generic;
using UniRx;
using Unity.Burst.Intrinsics;
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
    walk = 7
}

public class GameManager : MonoSingleton<GameManager>
{
    public List<GameObject> pages;
    public UICommonUI commonUI;
    public CanvasGroup taBlackPanel;
    public Dictionary<PageType, GameObject> pagesDic = new Dictionary<PageType, GameObject>();


    public BehaviorSubject<int> featherCoin = new BehaviorSubject<int>(0);
    public BehaviorSubject<int> timeLeft = new BehaviorSubject<int>(30);
    public PageType currentPageType = PageType.map;

    public List<int> characterIDs = new List<int>(GlobalAccess.teamOpacity);

    // Start is called before the first frame update
    void Start()
    {
        if (pages.Count == System.Enum.GetValues(typeof(PageType)).Length)
        {
            for (int i = 0; i < pages.Count; i++)
            {
                pagesDic.Add((PageType)i, pages[i]);
            }
        } else
        {
            UITip tip = UIManager.Instance.Show<UITip>();

            //todo
            tip.UpdateTip(DataManager.Instance.Language["go_next_town_tip"]);
        }

        //todo
        NorneStore.Instance.RemoveAll();
        DataManager.Instance.DataLoaded.AsObservable().Subscribe(flag =>
        {
            if (flag)
            {
                // init characters
                characterIDs.Add(GlobalAccess.CurrentCharacterId);
                CharacterDefine playerDefine = DataManager.Instance.Characters[GlobalAccess.CurrentCharacterId];
                CharacterModel mainCharacter = new CharacterModel(playerDefine);
                NorneStore.Instance.Update<CharacterModel>(mainCharacter, isFull: true);
                for (int i = 0; i < 2; i++)
                {
                    int id = DataManager.Instance.GetRandomSubCharacterID();
                    characterIDs.Add(id);
                    CharacterDefine define = DataManager.Instance.Characters[id];
                    CharacterModel model = new CharacterModel(define);
                    NorneStore.Instance.Update<CharacterModel>(model, isFull: true);
                }
            }
        });
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SwitchPage(PageType pageType)
    {
        StartCoroutine(SwitchPageIEnumerator(pageType));
    }

    IEnumerator SwitchPageIEnumerator(PageType pageType)
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
        
        switch (pageType)
        {
            case PageType.map:
                commonUI.gameObject.SetActive(false);
                break;
            case PageType.town:
                commonUI.gameObject.SetActive(true);
                commonUI.setLeftButtonStyle(true);
                commonUI.setPopButtonAutoHide(false);
                UITownPage townPage = pagesDic[PageType.town].GetComponent<UITownPage>();
                townPage.SetActionPanels(MapManager.Instance.CurrentTownNode.townActions);
                break;
            case PageType.battle:
                commonUI.gameObject.SetActive(true);
                commonUI.setLeftButtonStyle(false);
                commonUI.setPopButtonAutoHide(true);
                break;
            case PageType.bar:
                commonUI.gameObject.SetActive(true);
                commonUI.setLeftButtonStyle(false);
                commonUI.setPopButtonAutoHide(true);
                break;
            case PageType.forge:
                commonUI.gameObject.SetActive(true);
                commonUI.setLeftButtonStyle(false);
                commonUI.setPopButtonAutoHide(true);
                break;
            case PageType.shop:
                commonUI.gameObject.SetActive(true);
                commonUI.setLeftButtonStyle(false);
                commonUI.setPopButtonAutoHide(true);
                break;
            case PageType.train:
                commonUI.gameObject.SetActive(true);
                commonUI.setLeftButtonStyle(false);
                commonUI.setPopButtonAutoHide(true);
                break;
            case PageType.walk:
                commonUI.gameObject.SetActive(true);
                commonUI.setLeftButtonStyle(false);
                commonUI.setPopButtonAutoHide(true);
                break;
        }

        foreach (KeyValuePair<PageType, GameObject> pair in pagesDic)
        {
            if (pair.Key == pageType)
            {
                pair.Value.SetActive(true);
                StartCoroutine(GameUtil.Instance.FadeOut(taBlackPanel, 0.4f));
                break;
            }
        }
    }

    public void CoinChanged(int change)
    {
        featherCoin.OnNext(featherCoin.Value + change);
    }

    public void TimeChanged(int change)
    {
        timeLeft.OnNext(timeLeft.Value + change);
    }
}
