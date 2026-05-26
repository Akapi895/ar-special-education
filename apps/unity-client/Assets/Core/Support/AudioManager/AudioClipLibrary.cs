using UnityEngine;
using System.Collections.Generic;

namespace Core.Support.AudioManager
{
    [CreateAssetMenu(fileName = "AudioClipLibrary", menuName = "Audio/Audio Clip Library", order = 1)]
    public class AudioClipLibrary : ScriptableObject
    {
        [System.Serializable]
        public class SoundEntry
        {
            public string soundName;
            public AudioClip clip;
        }

        [Header("Sound Library")]
        [SerializeField] private List<SoundEntry> sounds = new List<SoundEntry>();

        private Dictionary<string, AudioClip> soundDict;

        private void OnEnable()
        {
            InitializeDictionary();
        }

        public void InitializeDictionary()
        {
            soundDict = new Dictionary<string, AudioClip>();
            foreach (var entry in sounds)
            {
                if (entry != null && !string.IsNullOrEmpty(entry.soundName) && entry.clip != null)
                {
                    soundDict[entry.soundName.ToLower()] = entry.clip;
                }
            }
        }

        public AudioClip GetClip(string soundName)
        {
            if (soundDict == null)
            {
                InitializeDictionary();
            }

            if (string.IsNullOrEmpty(soundName)) return null;

            string key = soundName.ToLower();
            if (soundDict.TryGetValue(key, out var clip))
            {
                return clip;
            }

            return null;
        }
    }
}
