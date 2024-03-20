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
    
    protected override void OnStart()
    {
    }
    public void Init()
    {
        MusicOn = Config.MusicOn;
        SoundOn = Config.SoundOn;
        MusicVolume = Config.MusicVolume;
        SoundVolume = Config.SoundVolume;
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
        this.SetVolume("MusicVolume", !on ? -100 : musicVolume);
        Config.MusicOn = on;
    }
    private void SetSoundOn(bool on)
    {
        this.SetVolume("SoundVolume", !on ? -100 : soundVolume);
        Config.SoundOn = on;
    }

    private void SetMusicVolume(int vol)
    {
        if (Config.MusicOn)
        {
            this.SetVolume("MusicVolume", vol);
        }
        if(Config.MusicVolume != vol) //避免init时播放testSound
        {
            Config.MusicVolume = vol;
            PlayTestSound();
        }

        
    }

    private void SetSoundVolume(int vol)
    {
        if(Config.SoundOn)
        {
            this.SetVolume("SoundVolume", vol);
        }
        if (Config.SoundVolume != vol) //避免init时播放testSound
        {
            Config.SoundVolume = vol;
            PlayTestSound();
        }
    }

    private void SetVolume(string name, int value)
    {
        //100对应0,即音源正常音量
        float volume = value * 0.5f - 50f;
        this.audioMixer.SetFloat(name, volume);
    }

    public void PlayMusic(string name)
    {
        AudioClip clip = Resloader.Load<AudioClip>(ConstValue.musicPath + name);
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
        AudioClip clip = Resloader.Load<AudioClip>(ConstValue.soundPath + name);
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

