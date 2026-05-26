using UnityEngine;
using System;
using System.Collections;
using Core.Support.AudioManager;

namespace Core.Support.FeedbackSystem
{
    /// <summary>
    /// MonoBehaviour proxy for the FeedbackSystem.
    /// Attach to a GameObject in the scene to enable feedback functionality.
    /// Provides a singleton instance for easy access across activities.
    /// Incorporates SimpleAudioManager and visual particle prefabs.
    /// </summary>
    public class FeedbackServiceProxy : MonoBehaviour
    {
        private static FeedbackServiceProxy instance;
        private static FeedbackSystem feedbackSystem;

        [Header("Configuration")]
        [Tooltip("Feedback config asset to use")]
        [SerializeField]
        private FeedbackConfig feedbackConfig;

        [Header("Feedback Particles Prefabs (Optional)")]
        [SerializeField] private GameObject correctVfxPrefab;
        [SerializeField] private GameObject incorrectVfxPrefab;
        [SerializeField] private GameObject successVfxPrefab;
        [SerializeField] private float vfxDestroyDelay = 3f;

        /// <summary>
        /// Get the singleton FeedbackSystem instance.
        /// </summary>
        public static FeedbackSystem Instance
        {
            get
            {
                EnsureFeedbackSystemInitialized();
                return feedbackSystem;
            }
        }

        /// <summary>
        /// Initialize the feedback service without creating scene lifecycle objects.
        /// </summary>
        public static void Initialize()
        {
            EnsureFeedbackSystemInitialized();
        }

        private static void EnsureFeedbackSystemInitialized()
        {
            if (feedbackSystem == null)
            {
                feedbackSystem = new FeedbackSystem();
            }

            if (instance != null && instance.feedbackConfig != null)
            {
                feedbackSystem.Initialize(instance.feedbackConfig);
            }
        }

        private void Awake()
        {
            // Singleton pattern
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            // Initialize feedback system
            EnsureFeedbackSystemInitialized();

            // Subscribe to events
            feedbackSystem.OnSoundEffectRequested += HandleSoundRequested;
            feedbackSystem.OnVisualEffectRequested += HandleVisualEffectRequested;
        }

        /// <summary>
        /// Handle sound effect requests.
        /// Plays SFX via SimpleAudioManager.
        /// </summary>
        private void HandleSoundRequested(string soundName)
        {
            if (SimpleAudioManager.Instance != null)
            {
                SimpleAudioManager.Instance.PlaySound(soundName);
            }
            else
            {
                Debug.Log($"[FeedbackServiceProxy] SimpleAudioManager not found. Sound requested: {soundName}");
            }
        }

        /// <summary>
        /// Handle visual effect requests.
        /// Spawns particle prefabs.
        /// </summary>
        private void HandleVisualEffectRequested(string effectName)
        {
            GameObject prefabToSpawn = null;
            string lowerName = effectName.ToLower();

            if (lowerName.Contains("correct") || lowerName.Contains("praise") || lowerName.Contains("win"))
            {
                prefabToSpawn = correctVfxPrefab;
            }
            else if (lowerName.Contains("incorrect") || lowerName.Contains("fail") || lowerName.Contains("encouragement"))
            {
                prefabToSpawn = incorrectVfxPrefab;
            }
            else if (lowerName.Contains("success") || lowerName.Contains("complete") || lowerName.Contains("congrats"))
            {
                prefabToSpawn = successVfxPrefab;
            }

            if (prefabToSpawn != null)
            {
                try
                {
                    // Spawn at center of camera or at (0, 0, 0)
                    Camera cam = Camera.main;
                    Vector3 spawnPos = Vector3.zero;
                    Quaternion spawnRot = Quaternion.identity;

                    if (cam != null)
                    {
                        // Spawn 2 meters in front of main camera so it's visible in AR/3D space
                        spawnPos = cam.transform.position + cam.transform.forward * 2f;
                        spawnRot = cam.transform.rotation;
                    }

                    GameObject vfxInstance = Instantiate(prefabToSpawn, spawnPos, spawnRot);
                    Destroy(vfxInstance, vfxDestroyDelay);

                    #if UNITY_EDITOR
                    Debug.Log($"[FeedbackServiceProxy] Spawned VFX: {effectName}");
                    #endif
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[FeedbackServiceProxy] Failed to spawn VFX '{effectName}': {ex.Message}");
                }
            }
            else
            {
                Debug.Log($"[FeedbackServiceProxy] VFX requested: {effectName} (no prefab bound)");
            }
        }

        /// <summary>
        /// Show feedback with coroutine-based timing.
        /// </summary>
        public void ShowFeedbackWithTiming(FeedbackData data)
        {
            StartCoroutine(ShowFeedbackCoroutine(data));
        }

        private IEnumerator ShowFeedbackCoroutine(FeedbackData data)
        {
            // Trigger the feedback
            Instance.NotifyFeedbackTriggered(data);

            // Play sound effect
            if (!string.IsNullOrEmpty(data.SoundEffect))
            {
                Instance.PlaySound(data.SoundEffect);
            }

            // Wait for duration
            yield return new WaitForSeconds(data.DisplayDuration > 0 ? data.DisplayDuration : 2f);

            // Complete
            Instance.NotifyFeedbackComplete();
        }

        private void OnDestroy()
        {
            if (feedbackSystem != null)
            {
                feedbackSystem.OnSoundEffectRequested -= HandleSoundRequested;
                feedbackSystem.OnVisualEffectRequested -= HandleVisualEffectRequested;
            }
        }

        /// <summary>
        /// Convenience method for showing correct feedback.
        /// </summary>
        public void ShowCorrect(string message = null)
        {
            Instance.ShowCorrect(message ?? Instance.GetRandomPraise());
        }

        /// <summary>
        /// Convenience method for showing incorrect feedback.
        /// </summary>
        public void ShowIncorrect(string message = null)
        {
            Instance.ShowIncorrect(message ?? Instance.GetRandomEncouragement());
        }

        /// <summary>
        /// Convenience method for showing hint feedback.
        /// </summary>
        public void ShowHint(string message)
        {
            Instance.ShowHint(message);
        }

        /// <summary>
        /// Convenience method for showing success feedback.
        /// </summary>
        public void ShowSuccess(string message = null)
        {
            Instance.ShowSuccess(message ?? "Activity Complete!");
        }
    }
}
