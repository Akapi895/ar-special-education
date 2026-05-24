using UnityEngine;
using Core.Learning.Models;

namespace Core.Support.HintSystem
{
    /// <summary>
    /// MonoBehaviour proxy for the HintSystem.
    /// Attach to a GameObject in the scene to enable hint functionality.
    /// Provides a singleton instance for easy access across activities.
    /// </summary>
    public class HintServiceProxy : MonoBehaviour
    {
        private static HintServiceProxy instance;
        private static HintSystem hintSystem;

        [Header("Settings")]
        [Tooltip("Maximum hints allowed per question (default)")]
        [SerializeField]
        private int defaultMaxHints = 3;

        [Tooltip("How long to show hint text (seconds)")]
        [SerializeField]
        private float hintDisplayDuration = 5f;

        [Tooltip("Minimum time between hints (seconds)")]
        [SerializeField]
        private float hintCooldown = 10f;

        /// <summary>
        /// Get the singleton HintSystem instance.
        /// </summary>
        public static HintSystem Instance
        {
            get
            {
                if (hintSystem == null)
                {
                    hintSystem = new HintSystem();
                    if (instance != null)
                    {
                        hintSystem.SetDefaultMaxHints(instance.defaultMaxHints);
                        hintSystem.SetHintDisplayDuration(instance.hintDisplayDuration);
                        hintSystem.SetHintCooldown(instance.hintCooldown);
                    }
                }
                return hintSystem;
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

            // Initialize hint system with settings
            if (hintSystem == null)
            {
                hintSystem = new HintSystem();
            }

            hintSystem.SetDefaultMaxHints(defaultMaxHints);
            hintSystem.SetHintDisplayDuration(hintDisplayDuration);
            hintSystem.SetHintCooldown(hintCooldown);
        }

        /// <summary>
        /// Reset hints for a specific question.
        /// Convenience method for activities to call.
        /// </summary>
        public void ResetQuestionHints(string activityId, int questionNumber)
        {
            Instance.ResetHints(activityId, questionNumber);
        }

        /// <summary>
        /// Reset all hints for an activity.
        /// </summary>
        public void ResetActivityHints(string activityId)
        {
            Instance.ResetActivityHints(activityId);
        }
    }
}
