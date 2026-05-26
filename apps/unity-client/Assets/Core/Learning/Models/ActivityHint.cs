using System;

namespace Core.Learning.Models
{
    /// <summary>
    /// Represents a hint that can be shown to the child.
    /// </summary>
    [Serializable]
    public class ActivityHint
    {
        /// <summary>
        /// Unique identifier for this hint.
        /// </summary>
        public string HintId;

        /// <summary>
        /// The hint text to display (child-friendly language).
        /// </summary>
        public string HintText;

        /// <summary>
        /// Hint level (1 = gentle nudge, higher = more explicit).
        /// Higher levels should be shown after lower levels fail.
        /// </summary>
        public int Level;

        /// <summary>
        /// Optional: AR-related hint (e.g., "Try moving closer").
        /// </summary>
        public string ARHint;

        public ActivityHint()
        {
        }

        public ActivityHint(string hintId, string hintText, int level = 1, string arHint = null)
        {
            HintId = hintId;
            HintText = hintText;
            Level = level;
            ARHint = arHint;
        }
    }
}
