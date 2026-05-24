using UnityEngine;
using System;
using System.Collections;

namespace Core.Support.FeedbackSystem
{
    /// <summary>
    /// MonoBehaviour proxy for the FeedbackSystem.
    /// Attach to a GameObject in the scene to enable feedback functionality.
    /// Provides a singleton instance for easy access across activities.
    /// </summary>
    public class FeedbackServiceProxy : MonoBehaviour
    {
        private static FeedbackServiceProxy instance;
        private static FeedbackSystem feedbackSystem;

        [Header("Configuration")]
        [Tooltip("Feedback config asset to use")]
        [SerializeField]
        private FeedbackConfig feedbackConfig;

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
        /// TODO: Audio team to implement actual sound playback.
        /// </summary>
        private void HandleSoundRequested(string soundName)
        {
            // TODO: Play actual sound via audio manager
            Debug.Log($"[FeedbackServiceProxy] Sound requested: {soundName}");
        }

        /// <summary>
        /// Handle visual effect requests.
        /// TODO: VFX team to implement actual effect playback.
        /// </summary>
        private void HandleVisualEffectRequested(string effectName)
        {
            // TODO: Play actual VFX via VFX manager
            Debug.Log($"[FeedbackServiceProxy] VFX requested: {effectName}");
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
