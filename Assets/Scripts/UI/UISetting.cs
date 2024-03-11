using Assets.Scripts.Sound;
using Assets.Scripts.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


class UISetting : UIWindow
{
	public void OnClickBackCharacterSelect()
    {
        //加载角色选择场景
        //SceneManager.Instance.LoadScene("CharSelect");
        //播放对应音乐
        SoundManager.Instance.PlayMusic(SoundDefine.Music_Select);
        //发送游戏离开消息
        //UserService.Instance.SendGameLeave();
        //回到角色选择场景之后,会存在两个摄像机都有audio listener的情况
        //所以需要禁用掉当前的
        //MainPlayerCamera.Instance.gameObject.SetActive(false);
    }
    public void OnClickSoundSetting()
    {
        UIManager.Instance.Show<UIOptions>();
        this.Close();
    }
    public void OnClickExitGame()
    {
        Application.Quit();
    }
}

