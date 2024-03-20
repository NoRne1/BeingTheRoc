using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PageType
{
    map = 0,
    town = 1,
    battle = 2
}

public class GameManager : MonoSingleton<GameManager>
{
    public GameObject mapPage;
    public GameObject townPage;
    public GameObject battlePage;
    public Dictionary<PageType, GameObject> pagesDic = new Dictionary<PageType, GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        pagesDic.Add(PageType.map, mapPage);
        pagesDic.Add(PageType.town, townPage);
        pagesDic.Add(PageType.battle, battlePage);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SwitchPage(PageType pageType)
    {
        foreach (KeyValuePair<PageType, GameObject> pair in pagesDic)
        {
            if (pair.Key == pageType)
            {
                pair.Value.SetActive(true);
            }
            else
            {
                pair.Value.SetActive(false);
            }
        }

        switch (pageType)
        {
            case PageType.map:
                break;
            case PageType.town:
                UITownPage townPage = pagesDic[PageType.town].GetComponent<UITownPage>();
                townPage.SetActionPanels(MapManager.Instance.CurrentTownNode.townActions);
                break;
            case PageType.battle:
                break;
        }
    }
}
