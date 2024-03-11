using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Sound;
using Unity.VisualScripting;
using UnityEngine;

public class UIStartGame : MonoBehaviour
{

    public Animator ani;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void startNewGame()
    {
        this.ani.SetTrigger("startGame");
        SoundManager.Instance.PlayMusic(SoundDefine.Music_Select);
    }

    public void backToMainMenu()
    {
        this.ani.SetTrigger("backToMainMenu");
        SoundManager.Instance.PlayMusic(SoundDefine.Music_Login);
    }

    public void options()
    {
        UIManager.Instance.Show<UIOptions>();
    }

    public void quit()
    {
        Application.Quit();
    }
}
