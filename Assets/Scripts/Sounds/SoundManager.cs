using System.Threading;
using Assets.Scripts.Sound;
using UnityEngine;
using UnityEngine.Audio;


/// <summary>
/// 声音管理
/// </summary>
public class SoundManager : MonoSingleton<SoundManager>
{
    public AudioMixer audioMixer;
    public AudioSource musicAudioSource;
    public AudioSource soundAudioSource;
    const string MusicPath = "Music/";
    const string SoundPath = "Sound/";
    
    protected override void OnStart()
    {
    }
    public void Init()
    {
        musicOn = Config.MusicOn;
        soundOn = Config.SoundOn;
        musicVolume = Config.MusicVolume;
        soundVolume = Config.SoundVolume;
    }

    private bool musicOn = true;
    public bool MusicOn
    {
        get { return musicOn; }
        set
        {
            this.musicOn = value;
            //禁音
            this.SetMusicOn(musicOn);
        }
    }

    private bool soundOn = true;
    public bool SoundOn
    {
        get { return soundOn; }
        set
        {
            this.soundOn = value;
            //禁音
            this.SetSoundOn(soundOn);
        }
    }

    private int musicVolume;
    public int MusicVolume
    {
        get { return musicVolume; }
        set
        {
            if (musicVolume != value)
            {
                musicVolume = value;
                this.SetMusicVolume(musicVolume);
            }
        }
    }

    private int soundVolume;
    public int SoundVolume
    {
        get { return soundVolume; }
        set
        {
            if (soundVolume != value)
            {
                soundVolume = value;
                this.SetSoundVolume(soundVolume);
            }
        }
    }
    private void SetMusicOn(bool on)
    {
        this.SetVolume("MusicVolume", !on ? 0 : musicVolume);
        Config.MusicOn = on;
    }
    private void SetSoundOn(bool on)
    {
        this.SetVolume("SoundVolume", !on ? 0 : soundVolume);
        Config.SoundOn = on;
    }

    private void SetMusicVolume(int vol)
    {
        this.SetVolume("MusicVolume", vol);
        Config.MusicVolume = vol;
        PlayTestSound();
    }

    private void SetSoundVolume(int vol)
    {
        this.SetVolume("SoundVolume", vol);
        Config.SoundVolume = vol;
        PlayTestSound();
    }

    private void SetVolume(string name, int value)
    {
        //100对应0,即音源正常音量
        float volume = value * 0.5f - 50f;
        this.audioMixer.SetFloat(name, volume);
    }

    public void PlayMusic(string name)
    {
        AudioClip clip = Resloader.Load<AudioClip>(MusicPath + name);
        if (clip == null)
        {
            Debug.LogWarningFormat("PlayMusic:{0} not existed", name);
            return;
        }
        if (musicAudioSource.isPlaying)
        {
            musicAudioSource.Stop();
        }
        musicAudioSource.clip = clip;
        //循环播放
        musicAudioSource.Play();
    }

    float lastPlay = 0;
    //每当调整完滑块,播放一次声音
    private void PlayTestSound()
    {
        if (Time.realtimeSinceStartup - lastPlay > 0.1)
        {
            lastPlay = Time.realtimeSinceStartup;
            this.PlaySound(SoundDefine.SFX_UI_Click);
        }
    }

    public void PlaySound(string name)
    {
        AudioClip clip = Resloader.Load<AudioClip>(SoundPath + name);
        if (clip == null)
        {
            Debug.LogWarningFormat("PlaySound:{0} not existed", name);
            return;
        }
        if (soundAudioSource.isPlaying)
        { 
            soundAudioSource.Stop();
        }
        //播放一次
        soundAudioSource.PlayOneShot(clip);
    }
}

