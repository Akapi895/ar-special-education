using Core.Learning.Models;
using UnityEngine;
using System.Collections.Generic;

namespace Core.Learning.ActivityRunner
{
    /// <summary>
    /// Base ScriptableObject configuration for activities.
    /// All activity-specific configs should inherit from this.
    /// </summary>
    [CreateAssetMenu(fileName = "SO_ActivityConfig", menuName = "AR Learning/Activity Config")]
    public class ActivityConfig : ScriptableObject
    {
        [Header("Activity Identification")]
        [Tooltip("Unique identifier for this activity type.")]
        [SerializeField]
        private string activityId;

        [Tooltip("Display name for this activity.")]
        [SerializeField]
        private string displayName;

        [Tooltip("Description of what this activity teaches.")]
        [SerializeField]
        private string description;

        [Header("Difficulty Settings")]
        [Tooltip("Difficulty level of this activity.")]
        [SerializeField]
        protected DifficultyLevel difficultyLevel = DifficultyLevel.Easy;

        [Tooltip("Maximum number of hints allowed per question.")]
        [SerializeField]
        protected int maxHintsPerQuestion = 3;

        [Tooltip("Maximum number of attempts allowed per question.")]
        [SerializeField]
        protected int maxAttemptsPerQuestion = 3;

        [Tooltip("Time limit per question in seconds (0 = no limit).")]
        [SerializeField]
        protected float timeLimitSeconds = 0f;

        [Header("Questions / Rounds")]
        [Tooltip("Number of questions/rounds in this activity.")]
        [SerializeField]
        protected int numberOfRounds = 5;

        [Header("Hints")]
        [Tooltip("Default hints for this activity.")]
        [SerializeField]
        protected List<ActivityHint> defaultHints = new List<ActivityHint>();

        // Properties
        public string ActivityId => activityId;
        public string DisplayName => displayName;
        public string Description => description;
        public DifficultyLevel Difficulty => difficultyLevel;
        public int MaxHintsPerQuestion => maxHintsPerQuestion;
        public int MaxAttemptsPerQuestion => maxAttemptsPerQuestion;
        public float TimeLimitSeconds => timeLimitSeconds;
        public int NumberOfRounds => numberOfRounds;
        public List<ActivityHint> DefaultHints => defaultHints;

        /// <summary>
        /// Validate the configuration.
        /// Override in derived classes for activity-specific validation.
        /// </summary>
        public virtual bool IsValid()
        {
            return !string.IsNullOrEmpty(activityId)
                && !string.IsNullOrEmpty(displayName)
                && numberOfRounds > 0
                && maxAttemptsPerQuestion > 0;
        }

        /// <summary>
        /// Get hints for a specific level/question.
        /// Override in derived classes to provide level-specific hints.
        /// </summary>
        public virtual List<ActivityHint> GetHintsForLevel(int levelNumber)
        {
            return defaultHints;
        }
    }
}
