using Assets.Scripts.Sound;
using Assets.Scripts.UI;
using UnityEngine;
using UnityEngine.UI;


class UISoundSetting : UIWindow
{
    public Image musicOff;
    public Image soundOff;
    public Toggle musicToggle;
    public Toggle soundToggle;
    public Slider musicSlider;
    public Slider soundSlider;
	void Start()
    {
        this.musicToggle.isOn = Config.MusicOn;
        this.soundToggle.isOn = Config.SoundOn;
        this.musicSlider.value = Config.MusicVolume;
        this.soundSlider.value = Config.SoundVolume;
    }
    public override void OnCloseClick()
    {
        SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_Click);
        PlayerPrefs.Save();
        base.OnCloseClick();
    }
    public void MusicToggle(bool on)
    {
        //切换图片外观(打开关闭)
        musicOff.enabled = !on;
        //改变声音配置(开关)
        Config.MusicOn = on;
        //播放按钮点击音效
        SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_Click);
    }
    public void SoundToggle(bool on)
    {
        soundOff.enabled = !on;
        Config.SoundOn = on;
        SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_Click);
    }
    public void MusicVolume(float vol)
    {
        Config.MusicVolume = (int)vol;
        //PlaySound();
    }
    public void SoundVolume(float vol)
    {
        Config.SoundVolume = (int)vol;
        PlaySound();
    }
    float lastPlay = 0;
    //每当调整完滑块,播放一次声音
    public void PlaySound()
    {
        if (Time.realtimeSinceStartup - lastPlay > 0.1)
        {
            lastPlay = Time.realtimeSinceStartup;
            SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_Click);
        }
    }
}

