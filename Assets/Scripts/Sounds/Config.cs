using UnityEngine;

namespace Assets.Scripts.Sound
{
    class Config
    {
        public static bool MusicOn
        {
            get
            {
                return PlayerPrefs.GetInt("Music", 1) == 1;
            }
            set
            {
                PlayerPrefs.SetInt("Music", value ? 1 : 0);
            }
        }
        public static bool SoundOn
        {
            get
            {
                return PlayerPrefs.GetInt("Sound", 1) == 1;
            }
            set
            {
                PlayerPrefs.SetInt("Sound", value ? 1 : 0);
            }
        }
        public static int MusicVolume
        {
            get
            {
                return PlayerPrefs.GetInt("MusicVolume", 100);
            }
            set
            {
                PlayerPrefs.SetInt("MusicVolume", value);
            }
        }
        public static int SoundVolume
        {
            get
            {
                return PlayerPrefs.GetInt("SoundVolume", 100);
            }
            set
            {
                PlayerPrefs.SetInt("SoundVolume", value);
            }
        }

        ~Config()
        {
            PlayerPrefs.Save();
        }
    }
}
