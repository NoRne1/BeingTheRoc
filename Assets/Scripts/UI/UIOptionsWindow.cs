using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using Assets.Scripts.Sound;

public class UIOptionsWindow : UIWindow
{
    public Button closeButton;
    public Button optionButton;
    public Button backMenuButton;

    public void Options()
    {
        UIManager.Instance.Show<UIOptions>();
        OnCloseClick(false);
    }

    public void BackToMainMenu()
    {
        SceneManager.Instance.LoadScene("start_game");
        SoundManager.Instance.PlayMusic(SoundDefine.Music_Main_Menu);
    }
}