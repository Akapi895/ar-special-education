using System;
using System.Collections.Generic;
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
            ShowFeedback(FeedbackType.Correct, message ?? "Great job!", FeedbackIntensity.Medium);
        }

        /// <summary>
        /// Show incorrect feedback with default message.
        /// </summary>
        public void ShowIncorrect(string message = null)
        {
            ShowFeedback(FeedbackType.Incorrect, message ?? "Not quite. Try again!", FeedbackIntensity.Medium);
        }

        /// <summary>
        /// Show hint feedback.
        /// </summary>
        public void ShowHint(string message)
        {
            ShowFeedback(FeedbackType.Hint, message, FeedbackIntensity.Low);
        }

        /// <summary>
        /// Show success feedback.
        /// </summary>
        public void ShowSuccess(string message = null)
        {
            ShowFeedback(FeedbackType.Success, message ?? "Well done!", FeedbackIntensity.High);
        }

        /// <summary>
        /// Show failed feedback.
        /// </summary>
        public void ShowFailed(string message = null)
        {
            ShowFeedback(FeedbackType.Failed, message ?? "Good effort!", FeedbackIntensity.High);
        }

        /// <summary>
        /// Play a sound effect.
        /// TODO: Audio team to implement actual sound playback.
        /// </summary>
        public void PlaySound(string soundName)
        {
            if (string.IsNullOrEmpty(soundName))
            {
                return;
            }

            OnSoundEffectRequested?.Invoke(soundName);

            // TODO: Actually play the sound via audio service
            Debug.Log($"[FeedbackSystem] Playing sound: {soundName}");
        }

        /// <summary>
        /// Trigger a visual effect.
        /// TODO: VFX team to implement actual effect playback.
        /// </summary>
        public void PlayVisualEffect(string effectName, Vector3 position)
        {
            if (string.IsNullOrEmpty(effectName))
            {
                return;
            }

            OnVisualEffectRequested?.Invoke(effectName);

            // TODO: Actually play the effect via VFX service
            Debug.Log($"[FeedbackSystem] Playing VFX: {effectName} at {position}");
        }

        /// <summary>
        /// Trigger a visual effect at the center of the screen.
        /// </summary>
        public void PlayVisualEffect(string effectName)
        {
            PlayVisualEffect(effectName, Vector3.zero);
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
                "Great job!",
                "Well done!",
                "Excellent!",
                "Awesome!",
                "Fantastic!",
                "You're doing great!",
                "Keep it up!",
                "Brilliant!",
                "Perfect!",
                "Wonderful!"
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
                "Not quite. Try again!",
                "Almost there. Give it another shot!",
                "Don't give up!",
                "You can do this!",
                "Think carefully and try again!",
                "Good effort. Let's try once more!",
                "Not this time. Keep going!",
                "You're learning. Try again!"
            };

            return encouragements[UnityEngine.Random.Range(0, encouragements.Length)];
        }
    }
}
