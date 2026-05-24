using Core.Learning.Models;
using System;

namespace Features.Activities.QuantityMatch
{
    /// <summary>
    /// Represents an answer in the Quantity Match activity.
    /// </summary>
    [Serializable]
    public class QuantityMatchAnswer : ActivityAnswer
    {
        /// <summary>
        /// The group index the child selected (0-based).
        /// </summary>
        public int SelectedGroupIndex;

        /// <summary>
        /// The number of objects in the selected group.
        /// </summary>
        public int SelectedGroupCount;

        /// <summary>
        /// The target number for this question.
        /// </summary>
        public int TargetNumber;

        public QuantityMatchAnswer()
        {
        }

        public QuantityMatchAnswer(int selectedGroupIndex, int selectedGroupCount, int targetNumber,
            float responseTimeSeconds, int attemptNumber, bool hintsUsed)
            : base(selectedGroupIndex, responseTimeSeconds, attemptNumber, hintsUsed)
        {
            SelectedGroupIndex = selectedGroupIndex;
            SelectedGroupCount = selectedGroupCount;
            TargetNumber = targetNumber;
        }

        /// <summary>
        /// Check if the selected group matches the target number.
        /// </summary>
        public bool IsMatchingTarget()
        {
            return SelectedGroupCount == TargetNumber;
        }
    }
}
