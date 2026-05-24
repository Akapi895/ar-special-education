using System;

namespace Features.Activities.NumberLineJump
{
    /// <summary>
    /// Direction of jumps for the Number Line Jump activity.
    /// </summary>
    [Serializable]
    public enum JumpDirection
    {
        /// <summary>
        /// Only right jumps (addition).
        /// Character moves to higher numbers.
        /// </summary>
        RightOnly,

        /// <summary>
        /// Only left jumps (subtraction).
        /// Character moves to lower numbers.
        /// </summary>
        LeftOnly,

        /// <summary>
        /// Both directions allowed.
        /// Character can move left or right.
        /// </summary>
        Both
    }

    /// <summary>
    /// Direction of a single jump.
    /// </summary>
    [Serializable]
    public enum JumpStepDirection
    {
        /// <summary>
        /// Jump to the right (increase number).
        /// </summary>
        Right,

        /// <summary>
        /// Jump to the left (decrease number).
        /// </summary>
        Left
    }
}
