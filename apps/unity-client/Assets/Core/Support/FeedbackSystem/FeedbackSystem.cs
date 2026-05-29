using System;
using System.Collections.Generic;
using Core.UI.Localization;
using UnityEngine;

namespace Core.Support.FeedbackSystem
{
    /// <summary>
    /// Service for managing feedback across learning activities.
    /// Handles sound effects, visual effects, and timing.
    /// </summary>
    public class FeedbackSystem
    {
        private FeedbackConfig config;
        private Queue<FeedbackData> feedbackQueue = new Queue<FeedbackData>();
        private bool isPlayingFeedback = false;

        // Events
        public event Action<FeedbackData> OnFeedbackTriggered;
        public event Action<string> OnSoundEffectRequested;
        public event Action<string> OnVisualEffectRequested;
        public event Action OnFeedbackComplete;

        /// <summary>
        /// Initialize with a feedback config.
        /// </summary>
        public void Initialize(FeedbackConfig feedbackConfig)
        {
            config = feedbackConfig;
        }

        /// <summary>
        /// Show feedback for a specific type with message.
        /// </summary>
        public void ShowFeedback(FeedbackType type, string message, FeedbackIntensity intensity = FeedbackIntensity.Medium)
        {
            FeedbackData data;

            if (config != null)
            {
                data = config.GetFeedbackData(type, message);
            }
            else
            {
                data = new FeedbackData(type, message, intensity);
            }

            data.Intensity = intensity;
            ApplyDefaultEffects(data);

            ShowFeedback(data);
        }

        /// <summary>
        /// Show feedback with full data.
        /// </summary>
        public void ShowFeedback(FeedbackData data)
        {
            feedbackQueue.Enqueue(data);
            ProcessQueue();
        }

        /// <summary>
        /// Show immediate feedback (skips queue, interrupts current).
        /// </summary>
        public void ShowImmediateFeedback(FeedbackData data)
        {
            isPlayingFeedback = false;
            feedbackQueue.Clear();
            ProcessFeedback(data);
        }

        /// <summary>
        /// Show correct feedback with default message.
        /// </summary>
        public void ShowCorrect(string message = null)
        {
            ShowFeedback(FeedbackType.Correct, message ?? SimpleLocalization.Get("feedback_correct"), FeedbackIntensity.Medium);
        }

        /// <summary>
        /// Show incorrect feedback with default message.
        /// </summary>
        public void ShowIncorrect(string message = null)
        {
            ShowFeedback(FeedbackType.Incorrect, message ?? SimpleLocalization.Get("feedback_incorrect"), FeedbackIntensity.Medium);
        }

        /// <summary>
        /// Show hint feedback.
        /// </summary>
        public void ShowHint(string message)
        {
            ShowFeedback(FeedbackType.Hint, message ?? SimpleLocalization.Get("feedback_hint"), FeedbackIntensity.Low);
        }

        /// <summary>
        /// Show success feedback.
        /// </summary>
        public void ShowSuccess(string message = null)
        {
            ShowFeedback(FeedbackType.Success, message ?? SimpleLocalization.Get("feedback_success"), FeedbackIntensity.High);
        }

        /// <summary>
        /// Show failed feedback.
        /// </summary>
        public void ShowFailed(string message = null)
        {
            ShowFeedback(FeedbackType.Failed, message ?? SimpleLocalization.Get("feedback_failed"), FeedbackIntensity.High);
        }

        /// <summary>
        /// Request a sound effect from the active audio proxy.
        /// </summary>
        public void PlaySound(string soundName)
        {
            if (string.IsNullOrEmpty(soundName))
            {
                return;
            }

            OnSoundEffectRequested?.Invoke(soundName);

            Debug.Log($"[FeedbackSystem] Playing sound: {soundName}");
        }

        /// <summary>
        /// Request a visual effect from the active feedback proxy.
        /// </summary>
        public void PlayVisualEffect(string effectName, Vector3 position)
        {
            if (string.IsNullOrEmpty(effectName))
            {
                return;
            }

            OnVisualEffectRequested?.Invoke(effectName);

            Debug.Log($"[FeedbackSystem] Playing VFX: {effectName} at {position}");
        }

        /// <summary>
        /// Trigger a visual effect at the center of the screen.
        /// </summary>
        public void PlayVisualEffect(string effectName)
        {
            PlayVisualEffect(effectName, Vector3.zero);
        }

        public void NotifyFeedbackTriggered(FeedbackData data)
        {
            OnFeedbackTriggered?.Invoke(data);
        }

        public void NotifyFeedbackComplete()
        {
            OnFeedbackComplete?.Invoke();
        }

        /// <summary>
        /// Clear all pending feedback.
        /// </summary>
        public void ClearQueue()
        {
            feedbackQueue.Clear();
            isPlayingFeedback = false;
        }

        /// <summary>
        /// Process the feedback queue.
        /// </summary>
        private void ProcessQueue()
        {
            if (isPlayingFeedback || feedbackQueue.Count == 0)
            {
                return;
            }

            FeedbackData data = feedbackQueue.Dequeue();
            ProcessFeedback(data);
        }

        /// <summary>
        /// Process a single feedback event.
        /// </summary>
        private void ProcessFeedback(FeedbackData data)
        {
            isPlayingFeedback = true;
            ApplyDefaultEffects(data);

            // Trigger the feedback
            OnFeedbackTriggered?.Invoke(data);

            // Play sound effect
            if (!string.IsNullOrEmpty(data.SoundEffect))
            {
                PlaySound(data.SoundEffect);
            }

            // Play visual effect
            if (!string.IsNullOrEmpty(data.VisualEffect))
            {
                PlayVisualEffect(data.VisualEffect);
            }

            // Schedule completion
            float duration = data.DisplayDuration > 0 ? data.DisplayDuration : 2f;

            // Use a timer or coroutine to complete feedback
            // For this non-MonoBehaviour class, we'll use a simple approach
            // The actual timing will be handled by the view or a MonoBehaviour proxy

            // Trigger completion after delay (would need a coroutine in MonoBehaviour)
            OnFeedbackComplete?.Invoke();
            isPlayingFeedback = false;

            // Process next item in queue
            ProcessQueue();
        }

        /// <summary>
        /// Get a random praise message for correct answers.
        /// </summary>
        public string GetRandomPraise()
        {
            string[] praises = new string[]
            {
                SimpleLocalization.Get("feedback_correct"),
                SimpleLocalization.Get("feedback_success"),
                "Gioi qua!",
                "Rat tot!",
                "Chinh xac!"
            };

            return praises[UnityEngine.Random.Range(0, praises.Length)];
        }

        /// <summary>
        /// Get a random encouragement message for incorrect answers.
        /// </summary>
        public string GetRandomEncouragement()
        {
            string[] encouragements = new string[]
            {
                SimpleLocalization.Get("feedback_incorrect"),
                "Gan dung roi, thu lai nhe!",
                "Con hay dem cham lai nhe.",
                "Con dang hoc rat tot, minh thu tiep nao."
            };

            return encouragements[UnityEngine.Random.Range(0, encouragements.Length)];
        }

        private static void ApplyDefaultEffects(FeedbackData data)
        {
            if (data == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(data.SoundEffect))
            {
                data.SoundEffect = data.Type switch
                {
                    FeedbackType.Correct => "sfx_correct",
                    FeedbackType.Incorrect => "sfx_incorrect",
                    FeedbackType.Hint => "sfx_hint",
                    FeedbackType.Success => "sfx_success",
                    FeedbackType.Failed => "sfx_failed",
                    _ => data.SoundEffect
                };
            }

            if (string.IsNullOrEmpty(data.VisualEffect))
            {
                data.VisualEffect = data.Type switch
                {
                    FeedbackType.Correct => "vfx_correct_sparkle",
                    FeedbackType.Incorrect => "vfx_wrong_gentle_shake",
                    FeedbackType.Success => "vfx_success_celebration",
                    FeedbackType.Boundary => "vfx_boundary_bump",
                    _ => data.VisualEffect
                };
            }
        }
    }
}
