using System;

namespace Core.Support.FeedbackSystem
{
    /// <summary>
    /// Types of feedback that can be displayed.
    /// </summary>
    [Serializable]
    public enum FeedbackType
    {
        /// <summary>
        /// Correct answer feedback.
        /// </summary>
        Correct,

        /// <summary>
        /// Incorrect answer feedback.
        /// </summary>
        Incorrect,

        /// <summary>
        /// Hint shown.
        /// </summary>
        Hint,

        /// <summary>
        /// Overshoot warning (Number Line).
        /// </summary>
        Overshoot,

        /// <summary>
        /// Boundary hit (Number Line).
        /// </summary>
        Boundary,

        /// <summary>
        /// Max jumps exceeded (Number Line).
        /// </summary>
        MaxJumps,

        /// <summary>
        /// Activity completed successfully.
        /// </summary>
        Success,

        /// <summary>
        /// Activity failed.
        /// </summary>
        Failed,

        /// <summary>
        /// Generic info/notification.
        /// </summary>
        Info
    }

    /// <summary>
    /// Intensity levels for feedback effects.
    /// </summary>
    [Serializable]
    public enum FeedbackIntensity
    {
        /// <summary>
        /// Subtle feedback (small mistake, gentle reminder).
        /// </summary>
        Low,

        /// <summary>
        /// Normal feedback (standard interaction).
        /// </summary>
        Medium,

        /// <summary>
        /// Strong feedback (major success or failure).
        /// </summary>
        High
    }
}
