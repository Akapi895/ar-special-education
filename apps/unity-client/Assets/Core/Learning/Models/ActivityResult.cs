using System;

namespace Core.Learning.Models
{
    /// <summary>
    /// Represents the result of an activity completion.
    /// </summary>
    [Serializable]
    public class ActivityResult
    {
        /// <summary>
        /// Unique identifier for the activity type (e.g., "QuantityMatch", "CompareQuantity").
        /// </summary>
        public string ActivityId;

        /// <summary>
        /// Session ID to group results from the same play session.
        /// </summary>
        public string SessionId;

        /// <summary>
        /// The level or question number within the activity.
        /// </summary>
        public int LevelNumber;

        /// <summary>
        /// Difficulty level of this activity/level.
        /// </summary>
        public DifficultyLevel DifficultyLevel;

        /// <summary>
        /// Whether the final answer was correct.
        /// </summary>
        public bool IsCorrect;

        /// <summary>
        /// Total number of attempts made.
        /// </summary>
        public int TotalAttempts;

        /// <summary>
        /// Number of hints used during the activity.
        /// </summary>
        public int HintsUsedCount;

        /// <summary>
        /// Total time spent on the activity (in seconds).
        /// </summary>
        public float TimeSpentSeconds;

        /// <summary>
        /// Timestamp when the activity was started.
        /// </summary>
        public DateTime StartTime;

        /// <summary>
        /// Timestamp when the activity was completed.
        /// </summary>
        public DateTime EndTime;

        /// <summary>
        /// ISO 8601 string representation of StartTime for JSON serialization.
        /// JsonUtility doesn't serialize DateTime directly.
        /// </summary>
        [SerializeField]
        private string startTimeString;

        /// <summary>
        /// ISO 8601 string representation of EndTime for JSON serialization.
        /// JsonUtility doesn't serialize DateTime directly.
        /// </summary>
        [SerializeField]
        private string endTimeString;

        /// <summary>
        /// Public access to the serialized start time string.
        /// </summary>
        public string StartTimeString
        {
            get => startTimeString;
            set
            {
                startTimeString = value;
                if (DateTime.TryParse(value, null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime dt))
                {
                    StartTime = dt;
                }
            }
        }

        /// <summary>
        /// Public access to the serialized end time string.
        /// </summary>
        public string EndTimeString
        {
            get => endTimeString;
            set
            {
                endTimeString = value;
                if (DateTime.TryParse(value, null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime dt))
                {
                    EndTime = dt;
                }
            }
        }

        /// <summary>
        /// The type of error made (if incorrect).
        /// Useful for learning analytics.
        /// </summary>
        public ErrorType? ErrorType;

        /// <summary>
        /// Optional additional data for specific activities.
        /// </summary>
        public string AdditionalData;

        public ActivityResult()
        {
        }

        public ActivityResult(string activityId, string sessionId, int levelNumber, DifficultyLevel difficultyLevel)
        {
            ActivityId = activityId;
            SessionId = sessionId;
            LevelNumber = levelNumber;
            DifficultyLevel = difficultyLevel;
            StartTime = DateTime.UtcNow;
        }

        /// <summary>
        /// Marks the activity as completed with the given result.
        /// </summary>
        public void Complete(bool isCorrect, ErrorType? errorType = null)
        {
            IsCorrect = isCorrect;
            ErrorType = errorType;
            EndTime = DateTime.UtcNow;
            TimeSpentSeconds = (float)(EndTime - StartTime).TotalSeconds;
        }

        /// <summary>
        /// Increments the attempt counter.
        /// </summary>
        public void IncrementAttempts()
        {
            TotalAttempts++;
        }

        /// <summary>
        /// Increments the hint usage counter.
        /// </summary>
        public void IncrementHintsUsed()
        {
            HintsUsedCount++;
        }
    }

    /// <summary>
    /// Difficulty levels for activities.
    /// </summary>
    [Serializable]
    public enum DifficultyLevel
    {
        Easy = 1,
        Medium = 2,
        Hard = 3
    }

    /// <summary>
    /// Types of errors that can occur during an activity.
    /// </summary>
    [Serializable]
    public enum ErrorType
    {
        /// <summary>
        /// Selected wrong quantity.
        /// </summary>
        WrongQuantity,

        /// <summary>
        /// Selected wrong comparison (more/less/equal).
        /// </summary>
        WrongComparison,

        /// <summary>
        /// Jumped in wrong direction.
        /// </summary>
        WrongDirection,

        /// <summary>
        /// Wrong number of jumps.
        /// </summary>
        WrongJumpCount,

        /// <summary>
        /// Ran out of time (if timed activity).
        /// </summary>
        Timeout,

        /// <summary>
        /// Generic/other error.
        /// </summary>
        Other
    }
}
