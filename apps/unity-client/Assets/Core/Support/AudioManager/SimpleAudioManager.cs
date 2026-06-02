using System.Collections.Generic;
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

        private const string VolumeKey = "UserPrefs.Volume";
        private const string LegacyVolumeKey = "Volume";
        private const string AudioEnabledKey = "UserPrefs.AudioEnabled";
        private const int FallbackSampleRate = 22050;
        private const float FallbackVolume = 0.22f;

        private string lastInstructionSoundName;
        private readonly Dictionary<string, AudioClip> temporaryFallbackClips = new Dictionary<string, AudioClip>();

        public static SimpleAudioManager EnsureExists()
        {
            Debug.Log("[SimpleAudioManager] EnsureExists called.");
            if (Instance != null)
            {
                return Instance;
            }

            GameObject existing = GameObject.Find("SimpleAudioManager");
            if (existing != null && existing.TryGetComponent(out SimpleAudioManager manager))
            {
                return manager;
            }

            GameObject audioGo = new GameObject("SimpleAudioManager");
            Debug.Log("[SimpleAudioManager] Creating new SimpleAudioManager instance.");
            return audioGo.AddComponent<SimpleAudioManager>();
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                EnsureAudioSources();

                ConfigureAudioSource(sfxSource);
                ConfigureAudioSource(bgmSource);

                float savedVolume = PlayerPrefs.GetFloat(VolumeKey, PlayerPrefs.GetFloat(LegacyVolumeKey, 1f));
                SetVolume(savedVolume);
                SetAudioEnabled(PlayerPrefs.GetInt(AudioEnabledKey, 1) == 1);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public bool HasClip(string soundName)
        {
            if (clipLibrary != null && clipLibrary.GetClip(soundName) != null)
                return true;
            return GetTemporaryFallbackClip(soundName) != null;
        }

        public void PlaySound(string soundName)
        {
            if (string.IsNullOrEmpty(soundName))
            {
                Debug.LogWarning("[SimpleAudioManager] PlaySound called with null/empty soundName.");
                return;
            }
            if (!IsAudioEnabled())
            {
                return;
            }

            EnsureAudioSources();
            if (sfxSource == null)
            {
                return;
            }

            AudioClip clip = clipLibrary != null ? clipLibrary.GetClip(soundName) : null;
            if (clip == null)
            {
                clip = GetTemporaryFallbackClip(soundName);
            }

            if (clip == null)
            {
                Debug.LogWarning($"[SimpleAudioManager] Audio clip '{soundName}' not found. Using fallback beep.");
            }
            else
            {
                Debug.Log($"[SimpleAudioManager] Playing '{soundName}' (clip: {clip.name}, length: {clip.length:F2}s, sources ready: {sfxSource != null})");
            }

            if (clip != null)
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

        public void PlayInstruction(string soundName)
        {
            Debug.Log($"[SimpleAudioManager] PlayInstruction called: '{soundName}'");
            lastInstructionSoundName = soundName;
            PlaySound(soundName);
        }

        public void ReplayLastInstruction()
        {
            if (!string.IsNullOrEmpty(lastInstructionSoundName))
            {
                PlaySound(lastInstructionSoundName);
            }
        }

        public void PlayNumber(int number)
        {
            if (number < 0 || number > 10)
            {
                return;
            }

            PlaySound($"number_{number}");
        }

        public bool HasNumberAudio(int number)
        {
            return number >= 0 && number <= 10;
        }

        public void SetVolume(float volume)
        {
            volume = Mathf.Clamp01(volume);
            if (sfxSource != null) sfxSource.volume = volume;
            if (bgmSource != null) bgmSource.volume = volume * 0.5f; // BGM is usually quieter
            
            PlayerPrefs.SetFloat(VolumeKey, volume);
            PlayerPrefs.SetFloat(LegacyVolumeKey, volume);
            PlayerPrefs.Save();
        }

        public float GetVolume()
        {
            return sfxSource != null ? sfxSource.volume : 1f;
        }

        public void ToggleMute(bool isMuted)
        {
            SetAudioEnabled(!isMuted);
        }

        public void SetAudioEnabled(bool enabled)
        {
            PlayerPrefs.SetInt(AudioEnabledKey, enabled ? 1 : 0);
            PlayerPrefs.Save();

            if (sfxSource != null) sfxSource.mute = !enabled;
            if (bgmSource != null) bgmSource.mute = !enabled;
        }

        public bool IsAudioEnabled()
        {
            return PlayerPrefs.GetInt(AudioEnabledKey, 1) == 1;
        }

        private void EnsureAudioSources()
        {
            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
                ConfigureAudioSource(sfxSource);
            }

            if (bgmSource == null)
            {
                bgmSource = gameObject.AddComponent<AudioSource>();
                bgmSource.playOnAwake = false;
                bgmSource.loop = true;
                ConfigureAudioSource(bgmSource);
            }
        }

        private static void ConfigureAudioSource(AudioSource source)
        {
            if (source == null) return;
            source.spatialBlend = 0f; // Force 2D sound
        }

        private AudioClip GetTemporaryFallbackClip(string soundName)
        {
            if (string.IsNullOrEmpty(soundName))
            {
                return null;
            }

            string key = soundName.ToLowerInvariant();
            if (temporaryFallbackClips.TryGetValue(key, out AudioClip cachedClip))
            {
                return cachedClip;
            }

            AudioClip clip = key.StartsWith("number_") && TryParseNumberSound(key, out int number)
                ? CreateNumberFallbackClip(key, number)
                : CreateInstructionFallbackClip(key);

            temporaryFallbackClips[key] = clip;
            return clip;
        }

        private static bool TryParseNumberSound(string soundName, out int number)
        {
            number = 0;
            const string prefix = "number_";
            if (!soundName.StartsWith(prefix))
            {
                return false;
            }

            string numberPart = soundName.Substring(prefix.Length);
            return int.TryParse(numberPart, out number) && number >= 0 && number <= 10;
        }

        private static AudioClip CreateNumberFallbackClip(string clipName, int number)
        {
            int beepCount = Mathf.Clamp(number, 1, 10);
            float baseFrequency = number == 0 ? 330f : 520f;
            return CreateBeepSequenceClip(clipName, beepCount, baseFrequency, 0.085f, 0.04f);
        }

        private static AudioClip CreateInstructionFallbackClip(string clipName)
        {
            if (clipName.Contains("quantity"))
            {
                return CreateMelodyClip(clipName, new[] { 523.25f, 659.25f, 783.99f }, 0.13f, 0.035f);
            }

            return CreateBeepSequenceClip(clipName, 2, 440f, 0.11f, 0.05f);
        }

        private static AudioClip CreateMelodyClip(string clipName, float[] frequencies, float toneSeconds, float gapSeconds)
        {
            int toneSamples = Mathf.Max(1, Mathf.RoundToInt(toneSeconds * FallbackSampleRate));
            int gapSamples = Mathf.Max(0, Mathf.RoundToInt(gapSeconds * FallbackSampleRate));
            int totalSamples = frequencies.Length * toneSamples + Mathf.Max(0, frequencies.Length - 1) * gapSamples;
            float[] data = new float[totalSamples];

            int writeIndex = 0;
            for (int i = 0; i < frequencies.Length; i++)
            {
                WriteTone(data, writeIndex, toneSamples, frequencies[i]);
                writeIndex += toneSamples + gapSamples;
            }

            AudioClip clip = AudioClip.Create(clipName, totalSamples, 1, FallbackSampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip CreateBeepSequenceClip(string clipName, int beepCount, float frequency, float beepSeconds, float gapSeconds)
        {
            int toneSamples = Mathf.Max(1, Mathf.RoundToInt(beepSeconds * FallbackSampleRate));
            int gapSamples = Mathf.Max(0, Mathf.RoundToInt(gapSeconds * FallbackSampleRate));
            int totalSamples = beepCount * toneSamples + Mathf.Max(0, beepCount - 1) * gapSamples;
            float[] data = new float[totalSamples];

            int writeIndex = 0;
            for (int i = 0; i < beepCount; i++)
            {
                WriteTone(data, writeIndex, toneSamples, frequency + i * 18f);
                writeIndex += toneSamples + gapSamples;
            }

            AudioClip clip = AudioClip.Create(clipName, totalSamples, 1, FallbackSampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static void WriteTone(float[] data, int startSample, int sampleCount, float frequency)
        {
            int fadeSamples = Mathf.Max(1, Mathf.RoundToInt(0.006f * FallbackSampleRate));
            for (int i = 0; i < sampleCount && startSample + i < data.Length; i++)
            {
                float t = (float)i / FallbackSampleRate;
                float fadeIn = Mathf.Clamp01((float)i / fadeSamples);
                float fadeOut = Mathf.Clamp01((float)(sampleCount - i - 1) / fadeSamples);
                float envelope = Mathf.Min(fadeIn, fadeOut);
                data[startSample + i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * FallbackVolume * envelope;
            }
        }
    }
}
