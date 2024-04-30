using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
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
    public UIPopButtonGroup uIPopButtonGroup;
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
                //setLeftButtonStyle(false);
                //setPopButtonAutoHide(true);
                //hideRightButtonGroup(true);
                //hideTimeLeftGroup(true);
                //weatherPanel.gameObject.SetActive(false);
                //uIPopButtonGroup.gameObject.SetActive(false);
                break;
            case CommonUIStyle.town:
                gameObject.SetActive(true);
                setLeftButtonStyle(true);
                setPopButtonAutoHide(false);
                hideRightButtonGroup(false);
                hideTimeLeftGroup(false);
                weatherPanel.gameObject.SetActive(false);
                uIPopButtonGroup.gameObject.SetActive(true);
                break;
            case CommonUIStyle.actionPage:
                gameObject.SetActive(true);
                setLeftButtonStyle(false);
                setPopButtonAutoHide(true);
                hideRightButtonGroup(false);
                hideTimeLeftGroup(false);
                weatherPanel.gameObject.SetActive(false);
                uIPopButtonGroup.gameObject.SetActive(true);
                break;
            case CommonUIStyle.battle:
                gameObject.SetActive(true);
                setLeftButtonStyle(false);
                setPopButtonAutoHide(true);
                hideRightButtonGroup(true);
                hideTimeLeftGroup(false);
                weatherPanel.gameObject.SetActive(true);
                uIPopButtonGroup.gameObject.SetActive(true);
                break;
        }
        
    }

    public void setLeftButtonStyle(bool menuOrBack)
    {
        backButton.gameObject.SetActive(!menuOrBack);
        menuButton.gameObject.SetActive(menuOrBack);
    }

    public void onBackButtonClicked()
    {
        GameManager.Instance.SwitchPage(PageType.town);
    }

    public void setPopButtonAutoHide(bool autoHide)
    {
        uIPopButtonGroup.AutoHidden = autoHide;
    }

    public void nextDay()
    {
        GameManager.Instance.TimeChanged(-1);
    }

    public void backToMapPage()
    {
        GameManager.Instance.SwitchPage(PageType.map);
    }

    public void invokeTeamWindow()
    {
        UIManager.Instance.Show<UITeamWindow>();
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