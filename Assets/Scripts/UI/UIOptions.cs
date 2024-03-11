using Assets.Scripts.Sound;
using Assets.Scripts.UI;
using UnityEngine;
using UnityEngine.UI;


class UIOptions : UIWindow
{
    public Slider musicSlider;
    public Slider soundSlider;
    public Toggle musicToggle;
    public Toggle soundToggle;
    public Image musicToggleImage;
    public Image soundToggleImage;
    public Sprite musicOnSprite;
    public Sprite musicMuteSprite;

    public bool MusicOn
    {
        get { return SoundManager.Instance.MusicOn; }
        set
        {
            SoundManager.Instance.MusicOn = value;
            this.setMusicToggle(value);
        }
    }
    public bool SoundOn
    {
        get { return SoundManager.Instance.SoundOn; }
        set
        {
            SoundManager.Instance.SoundOn = value;
            this.setSoundToggle(value);
        }
    }

    public float MusicVolume
    {
        get { return (float)SoundManager.Instance.MusicVolume; }
        set
        {
            SoundManager.Instance.MusicVolume = (int)value;
        }
    }
    public float SoundVolume
    {
        get { return (float)SoundManager.Instance.SoundVolume; }
        set
        {
            SoundManager.Instance.SoundVolume = (int)value;
        }
    }

    void Start()
    {
        this.musicToggle.isOn = Config.MusicOn;
        musicToggleImage.overrideSprite = Config.MusicOn ? musicOnSprite : musicMuteSprite;      
        this.soundToggle.isOn = Config.SoundOn;
        soundToggleImage.overrideSprite = Config.SoundOn ? musicOnSprite : musicMuteSprite;
        this.musicSlider.value = Config.MusicVolume;
        this.soundSlider.value = Config.SoundVolume;
    }

    public override void OnCloseClick()
    {
        SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_Click);
        PlayerPrefs.Save();
        base.OnCloseClick();
    }

    private void setMusicToggle(bool on)
    {
        //切换图片外观(打开关闭)
        musicToggleImage.overrideSprite = on ? musicOnSprite : musicMuteSprite;
        //播放按钮点击音效
        SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_Click);
    }

    private void setSoundToggle(bool on)
    {
        //切换图片外观(打开关闭)
        soundToggleImage.overrideSprite = on ? musicOnSprite : musicMuteSprite;
        //播放按钮点击音效
        SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_Click);
    }

    public void CloseSelf()
    {
        this.Close();
    }
}

