using UnityEngine;

namespace Core.Data
{
    public static class UserPreferences
    {
        private const string VolumeKey = "UserPrefs.Volume";
        private const string FontScaleKey = "UserPrefs.FontScale";
        private const string AnimationsEnabledKey = "UserPrefs.AnimationsEnabled";
        private const string HighContrastModeKey = "UserPrefs.HighContrastMode";

        public static float Volume
        {
            get => PlayerPrefs.GetFloat(VolumeKey, 1f);
            set
            {
                PlayerPrefs.SetFloat(VolumeKey, value);
                PlayerPrefs.Save();
                
                // Dynamically update AudioManager if present
                var audioManager = GameObject.Find("SimpleAudioManager");
                if (audioManager != null)
                {
                    audioManager.SendMessage("SetVolume", value, SendMessageOptions.DontRequireReceiver);
                }
            }
        }

        public static float FontScale
        {
            get => PlayerPrefs.GetFloat(FontScaleKey, 1f); // 1.0f is medium, 0.8f small, 1.3f large
            set
            {
                PlayerPrefs.SetFloat(FontScaleKey, value);
                PlayerPrefs.Save();
            }
        }

        public static bool AnimationsEnabled
        {
            get => PlayerPrefs.GetInt(AnimationsEnabledKey, 1) == 1;
            set
            {
                PlayerPrefs.SetInt(AnimationsEnabledKey, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        public static bool HighContrastMode
        {
            get => PlayerPrefs.GetInt(HighContrastModeKey, 0) == 1;
            set
            {
                PlayerPrefs.SetInt(HighContrastModeKey, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }
    }
}
