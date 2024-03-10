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
    private bool musicOn = true;
    protected override void OnStart()
    {
    }
    public void Init()
    {
        MusicVolume = Config.MusicVolume;
        SoundVolume = Config.SoundVolume;
        MusicOn = Config.MusicOn;
        SoundOn = Config.SoundOn;
    }
    public bool MusicOn
    {
        get { return musicOn; }
        set
        {
            this.musicOn = value;
            //禁音
            this.MusicMute(!musicOn);
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
            this.SoundMute(!soundOn);
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
                if (musicOn) this.SetVolume("MusicVolume", musicVolume);
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
                if (soundOn) this.SetVolume("SoundVolume", soundVolume);
            }
        }
    }
    private void MusicMute(bool mute)
    {
        this.SetVolume("MusicVolume", mute ? 0 : musicVolume);
    }
    private void SoundMute(bool mute)
    {
        this.SetVolume("SoundVolume", mute ? 0 : soundVolume);
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

