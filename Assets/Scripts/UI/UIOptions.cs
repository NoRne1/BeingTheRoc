using System;
using System.Collections.Generic;
using Assets.Scripts.Sound;
using TMPro;
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

    public TMP_Dropdown languageSelect;

    public TextMeshProUGUI music_title;
    public TextMeshProUGUI sound_title;
    public TextMeshProUGUI language_title;

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

        //Dropdown init
        List<string> dropOptions = new List<string> {};
        foreach (KeyValuePair<int, Dictionary<String, String>> item in DataManager.Instance.LanguagesDic)
        {
            dropOptions.Add(item.Value["Name"]);
        }
        languageSelect.ClearOptions();
        languageSelect.AddOptions(dropOptions);
        languageSelect.value = Config.Language;
        languageSelect.onValueChanged.AddListener(delegate {
            DropdownValueChanged(languageSelect);
        });

        music_title.text = DataManager.Instance.Language["music"];
        sound_title.text = DataManager.Instance.Language["sound"];
        language_title.text = DataManager.Instance.Language["language"];
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

    void DropdownValueChanged(TMP_Dropdown change)
    {
        Config.Language = change.value;
    }
}

