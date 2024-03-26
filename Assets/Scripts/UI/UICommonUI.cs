using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UICommonUI : MonoBehaviour
{
    public Button backButton;
    public Button menuButton;
    public UIPopButtonGroup uIPopButtonGroup;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

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
}