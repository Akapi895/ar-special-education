using System;

namespace Core.Learning.Models
{
    /// <summary>
    /// Base class for activity answers.
    /// Specific activities can extend this with additional data.
    /// </summary>
    [Serializable]
    public class ActivityAnswer
    {
        /// <summary>
        /// The answer data (can be simple value or complex object).
        /// Activities should override or use this appropriately.
        /// </summary>
        public object AnswerData;

        /// <summary>
        /// Timestamp when the answer was submitted.
        /// </summary>
        public DateTime Timestamp;

        /// <summary>
        /// Time elapsed since the question was presented (in seconds).
        /// </summary>
        public float ResponseTimeSeconds;

        /// <summary>
        /// Number of attempts made before this answer.
        /// </summary>
        public int AttemptNumber;

        /// <summary>
        /// Whether hints were used before this answer.
        /// </summary>
        public bool HintsUsed;

        public ActivityAnswer()
        {
            Timestamp = DateTime.UtcNow;
        }

        public ActivityAnswer(object answerData, float responseTimeSeconds, int attemptNumber, bool hintsUsed)
        {
            AnswerData = answerData;
            ResponseTimeSeconds = responseTimeSeconds;
            AttemptNumber = attemptNumber;
            HintsUsed = hintsUsed;
            Timestamp = DateTime.UtcNow;
        }
    }
}
