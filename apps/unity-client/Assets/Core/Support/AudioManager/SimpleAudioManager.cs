using UnityEngine;

namespace Core.Support.AudioManager
{
    public class SimpleAudioManager : MonoBehaviour
    {
        public static SimpleAudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource bgmSource;

        [Header("Library")]
        [SerializeField] private AudioClipLibrary clipLibrary;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                
                // Add sources if missing
                if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();
                if (bgmSource == null)
                {
                    bgmSource = gameObject.AddComponent<AudioSource>();
                    bgmSource.loop = true;
                }

                // Load preferences
                float savedVolume = PlayerPrefs.GetFloat("Volume", 1f);
                SetVolume(savedVolume);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void PlaySound(string soundName)
        {
            if (clipLibrary == null)
            {
                #if UNITY_EDITOR
                Debug.Log($"[SimpleAudioManager] No clip library assigned. SFX requested: {soundName}");
                #endif
                return;
            }

            AudioClip clip = clipLibrary.GetClip(soundName);
            if (clip != null && sfxSource != null)
            {
                sfxSource.PlayOneShot(clip);
            }
            else
            {
                #if UNITY_EDITOR
                Debug.Log($"[SimpleAudioManager] Sound '{soundName}' not found or audio source is missing.");
                #endif
            }
        }

        public void SetVolume(float volume)
        {
            volume = Mathf.Clamp01(volume);
            if (sfxSource != null) sfxSource.volume = volume;
            if (bgmSource != null) bgmSource.volume = volume * 0.5f; // BGM is usually quieter
            
            PlayerPrefs.SetFloat("Volume", volume);
            PlayerPrefs.Save();
        }

        public float GetVolume()
        {
            return sfxSource != null ? sfxSource.volume : 1f;
        }

        public void ToggleMute(bool isMuted)
        {
            if (sfxSource != null) sfxSource.mute = isMuted;
            if (bgmSource != null) bgmSource.mute = isMuted;
        }
    }
}
