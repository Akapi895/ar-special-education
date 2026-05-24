using System;
using UnityEngine;

namespace Core.Support.FeedbackSystem
{
    /// <summary>
    /// Data for a single feedback event.
    /// </summary>
    [Serializable]
    public class FeedbackData
    {
        /// <summary>
        /// Type of feedback.
        /// </summary>
        public FeedbackType Type;

        /// <summary>
        /// Message to display.
        /// </summary>
        public string Message;

        /// <summary>
        /// Intensity of the feedback.
        /// </summary>
        public FeedbackIntensity Intensity;

        /// <summary>
        /// Sound effect to play (audio clip name or path).
        /// TODO: Audio team to implement sound effect references.
        /// </summary>
        public string SoundEffect;

        /// <summary>
        /// Visual effect to play (particle system or animation name).
        /// TODO: VFX team to implement visual effect references.
        /// </summary>
        public string VisualEffect;

        /// <summary>
        /// Color for UI elements (text, panel backgrounds).
        /// </summary>
        public Color DisplayColor;

        /// <summary>
        /// Duration to display feedback (seconds).
        /// </summary>
        public float DisplayDuration;

        public FeedbackData()
        {
            DisplayDuration = 2f;
            DisplayColor = Color.white;
            Intensity = FeedbackIntensity.Medium;
        }

        public FeedbackData(FeedbackType type, string message, FeedbackIntensity intensity = FeedbackIntensity.Medium)
        {
            Type = type;
            Message = message;
            Intensity = intensity;
            DisplayDuration = 2f;
            DisplayColor = GetDefaultColor(type);
        }

        /// <summary>
        /// Get default color for a feedback type.
        /// </summary>
        private Color GetDefaultColor(FeedbackType type)
        {
            return type switch
            {
                FeedbackType.Correct => Color.green,
                FeedbackType.Success => new Color(0.2f, 0.8f, 0.2f),
                FeedbackType.Incorrect => Color.red,
                FeedbackType.Failed => new Color(0.8f, 0.2f, 0.2f),
                FeedbackType.Hint => new Color(0.2f, 0.6f, 1f),
                FeedbackType.Overshoot => new Color(1f, 0.6f, 0f),
                FeedbackType.Boundary => new Color(1f, 0.8f, 0f),
                FeedbackType.MaxJumps => new Color(0.8f, 0.4f, 0f),
                _ => Color.white
            };
        }
    }

    /// <summary>
    /// Configuration for feedback effects.
    /// </summary>
    [CreateAssetMenu(fileName = "SO_FeedbackConfig", menuName = "AR Learning/Feedback Config")]
    public class FeedbackConfig : ScriptableObject
    {
        [Header("Sound Effects")]
        [Tooltip("Sound to play for correct answers")]
        [SerializeField]
        private string correctSound = "SFX_Correct";

        [Tooltip("Sound to play for incorrect answers")]
        [SerializeField]
        private string incorrectSound = "SFX_Incorrect";

        [Tooltip("Sound to play for hints")]
        [SerializeField]
        private string hintSound = "SFX_Hint";

        [Tooltip("Sound to play for success/completion")]
        [SerializeField]
        private string successSound = "SFX_Success";

        [Tooltip("Sound to play for failure")]
        [SerializeField]
        private string failedSound = "SFX_Failed";

        [Header("Visual Effects")]
        [Tooltip("Particle effect for correct answers")]
        [SerializeField]
        private string correctVFX = "VFX_CorrectConfetti";

        [Tooltip("Particle effect for incorrect answers")]
        [SerializeField]
        private string incorrectVFX = "VFX_IncorrectShake";

        [Tooltip("Particle effect for success")]
        [SerializeField]
        private string successVFX = "VFX_SuccessFireworks";

        [Header("Display Settings")]
        [Tooltip("Default duration for feedback display (seconds)")]
        [SerializeField]
        private float defaultDuration = 2f;

        [Tooltip("Correct feedback color")]
        [SerializeField]
        private Color correctColor = Color.green;

        [Tooltip("Incorrect feedback color")]
        [SerializeField]
        private Color incorrectColor = Color.red;

        [Tooltip("Hint feedback color")]
        [SerializeField]
        private Color hintColor = new Color(0.2f, 0.6f, 1f);

        // Properties
        public string CorrectSound => correctSound;
        public string IncorrectSound => incorrectSound;
        public string HintSound => hintSound;
        public string SuccessSound => successSound;
        public string FailedSound => failedSound;
        public string CorrectVFX => correctVFX;
        public string IncorrectVFX => incorrectVFX;
        public string SuccessVFX => successVFX;
        public float DefaultDuration => defaultDuration;
        public Color CorrectColor => correctColor;
        public Color IncorrectColor => incorrectColor;
        public Color HintColor => hintColor;

        /// <summary>
        /// Get feedback data for a specific type.
        /// </summary>
        public FeedbackData GetFeedbackData(FeedbackType type, string message)
        {
            var data = new FeedbackData(type, message);

            // Set sound effect
            data.SoundEffect = type switch
            {
                FeedbackType.Correct => correctSound,
                FeedbackType.Incorrect => incorrectSound,
                FeedbackType.Hint => hintSound,
                FeedbackType.Success => successSound,
                FeedbackType.Failed => failedSound,
                _ => null
            };

            // Set visual effect
            data.VisualEffect = type switch
            {
                FeedbackType.Correct => correctVFX,
                FeedbackType.Incorrect => incorrectVFX,
                FeedbackType.Success => successVFX,
                _ => null
            };

            // Set color
            data.DisplayColor = type switch
            {
                FeedbackType.Correct => correctColor,
                FeedbackType.Incorrect => incorrectColor,
                FeedbackType.Hint => hintColor,
                _ => Color.white
            };

            // Set duration
            data.DisplayDuration = defaultDuration;

            return data;
        }
    }
}
