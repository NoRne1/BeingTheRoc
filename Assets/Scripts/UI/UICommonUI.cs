using System;
using UnityEngine;
using UnityEngine.UI;

public enum CommonUIStyle
{
    map = 0,
    town = 1,
    actionPage = 2,
    battle = 3,
    restaurant = 4,
    bar = 5,
    game = 6,
}

public class UICommonUI : MonoBehaviour
{
    public Button backButton;
    public Button menuButton;
    public Button helpButton;
    public UIWeatherPanel weatherPanel;
    public GameObject timeLeftGroup;
    public GameObject rightButtonGroup;
    public PopObjComponent uIPopButtonGroup;
    public UITreasuresRect treasuresRect;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void setUIStyle(CommonUIStyle style)
    {
        switch(style)
        {
            case CommonUIStyle.map:
                gameObject.SetActive(true);
                setLeftButtonStyle(LeftButtonStyle.none);
                hideRightButtonGroup(true);
                hideTimeLeftGroup(false);
                weatherPanel.gameObject.SetActive(false);
                uIPopButtonGroup.gameObject.SetActive(false);
                treasuresRect.gameObject.SetActive(false);
                break;
            case CommonUIStyle.town:
                gameObject.SetActive(true);
                setLeftButtonStyle(LeftButtonStyle.menu);
                hideRightButtonGroup(false);
                hideTimeLeftGroup(false);
                weatherPanel.gameObject.SetActive(false);
                setBottomPopButtonAutoHide(false);
                uIPopButtonGroup.gameObject.SetActive(true);
                setLeftRectAutoHide(true);
                treasuresRect.gameObject.SetActive(true);
                break;
            case CommonUIStyle.actionPage:
                gameObject.SetActive(true);
                setLeftButtonStyle(LeftButtonStyle.back);
                setBottomPopButtonAutoHide(true);
                hideRightButtonGroup(false);
                hideTimeLeftGroup(false);
                weatherPanel.gameObject.SetActive(false);
                uIPopButtonGroup.gameObject.SetActive(true);
                setLeftRectAutoHide(true);
                treasuresRect.gameObject.SetActive(true);
                break;
            case CommonUIStyle.battle:
                gameObject.SetActive(true);
                setLeftButtonStyle(LeftButtonStyle.menu);
                setBottomPopButtonAutoHide(true);
                hideRightButtonGroup(true);
                hideTimeLeftGroup(false);
                weatherPanel.gameObject.SetActive(true);
                uIPopButtonGroup.gameObject.SetActive(true);
                setLeftRectAutoHide(true);
                treasuresRect.gameObject.SetActive(true);
                break;
            case CommonUIStyle.restaurant:
                gameObject.SetActive(true);
                setLeftButtonStyle(LeftButtonStyle.back | LeftButtonStyle.help);
                setBottomPopButtonAutoHide(false);
                hideRightButtonGroup(false);
                hideTimeLeftGroup(false);
                weatherPanel.gameObject.SetActive(false);
                uIPopButtonGroup.gameObject.SetActive(true);
                setLeftRectAutoHide(true);
                treasuresRect.gameObject.SetActive(true);
                break;
            case CommonUIStyle.bar:
                gameObject.SetActive(true);
                setLeftButtonStyle(LeftButtonStyle.back | LeftButtonStyle.help);
                setBottomPopButtonAutoHide(true);
                hideRightButtonGroup(false);
                hideTimeLeftGroup(false);
                weatherPanel.gameObject.SetActive(false);
                uIPopButtonGroup.gameObject.SetActive(true);
                setLeftRectAutoHide(true);
                treasuresRect.gameObject.SetActive(true);
                break;
            case CommonUIStyle.game:
                gameObject.SetActive(true);
                setLeftButtonStyle(LeftButtonStyle.back | LeftButtonStyle.help);
                setBottomPopButtonAutoHide(true);
                hideRightButtonGroup(false);
                hideTimeLeftGroup(false);
                weatherPanel.gameObject.SetActive(false);
                uIPopButtonGroup.gameObject.SetActive(true);
                setLeftRectAutoHide(true);
                treasuresRect.gameObject.SetActive(true);
                break;
        }
        
    }

    [Flags]
    public enum LeftButtonStyle 
    {
        none = 0,
        back = 1,
        menu = 2,
        help = 4,

    }
    public void setLeftButtonStyle(LeftButtonStyle style)
    {
        backButton.gameObject.SetActive(false);
        menuButton.gameObject.SetActive(false);
        helpButton.gameObject.SetActive(false);
        if (((int)style & (int)LeftButtonStyle.back) != 0)
        {
            backButton.gameObject.SetActive(true);
        }
        if (((int)style & (int)LeftButtonStyle.menu) != 0)
        {
            menuButton.gameObject.SetActive(true);
        }
        if (((int)style & (int)LeftButtonStyle.help) != 0)
        {
            helpButton.gameObject.SetActive(true);
        }
    }

    public void onBackButtonClicked()
    {
        GameManager.Instance.SwitchPage(PageType.town);
    }

    public void setLeftRectAutoHide(bool autoHide)
    {
        treasuresRect.GetComponent<PopObjComponent>().AutoHidden = autoHide;
    }

    public void setBottomPopButtonAutoHide(bool autoHide)
    {
        uIPopButtonGroup.AutoHidden = autoHide;
    }

    public void nextDay()
    {
        switch (GameManager.Instance.currentPageType)
        {
            case PageType.town:
            case PageType.bar:
            case PageType.forge:
            case PageType.shop:
            case PageType.train:
            case PageType.game:
            case PageType.restaurant:
                GameManager.Instance.TimeChanged(-1, false);
                break;
            case PageType.battle:
            case PageType.map:
                break;
        }
    }

    public void backToMapPage()
    {
        GameManager.Instance.SwitchPage(PageType.map);
    }

    public void invokeTeamWindow()
    {
        var teamWindow = UIManager.Instance.Show<UITeamWindow>();
        if (GameManager.Instance.currentPageType == PageType.battle)
        {
            teamWindow.BattleInit();
        } else
        {
            teamWindow.NormalInit();
        }
    }

    public void hideRightButtonGroup(bool flag)
    {
        rightButtonGroup.SetActive(!flag);
    }

    public void hideTimeLeftGroup(bool flag)
    {
        timeLeftGroup.SetActive(!flag);
    }

    public void OpenOptionsMenu()
    {
        UIManager.Instance.Show<UIOptionsWindow>();
    }
}