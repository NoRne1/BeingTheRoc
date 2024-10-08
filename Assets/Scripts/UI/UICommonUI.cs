using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public enum CommonUIStyle
{
    map = 0,
    town = 1,
    actionPage = 2,
    battle = 3,
}

public class UICommonUI : MonoBehaviour
{
    public Button backButton;
    public Button menuButton;
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
                setLeftButtonStyle(LeftButtonStyle.back);
                setBottomPopButtonAutoHide(true);
                hideRightButtonGroup(true);
                hideTimeLeftGroup(false);
                weatherPanel.gameObject.SetActive(true);
                uIPopButtonGroup.gameObject.SetActive(true);
                setLeftRectAutoHide(true);
                treasuresRect.gameObject.SetActive(true);
                break;
        }
        
    }

    public enum LeftButtonStyle 
    {
        none = 0,
        menu = 1,
        back = 2,

    }
    public void setLeftButtonStyle(LeftButtonStyle style)
    {
        switch (style) 
        {
            case LeftButtonStyle.none:
                backButton.gameObject.SetActive(false);
                menuButton.gameObject.SetActive(false);
                break;
            case LeftButtonStyle.menu:
                backButton.gameObject.SetActive(false);
                menuButton.gameObject.SetActive(true);
                break;
            case LeftButtonStyle.back:
                backButton.gameObject.SetActive(true);
                menuButton.gameObject.SetActive(false);
                break;
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
            case PageType.walk:
                GameManager.Instance.TimeChanged(-1);
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
}