using Core.Learning.Models;
using Features.Activities.CompareQuantity;
using System;

namespace Features.Activities.CompareQuantity
{
    /// <summary>
    /// Represents an answer in the Compare Quantity activity.
    /// </summary>
    [Serializable]
    public class CompareQuantityAnswer : ActivityAnswer
    {
        /// <summary>
        /// The comparison answer the child selected.
        /// </summary>
        public ComparisonAnswer SelectedComparison;

        /// <summary>
        /// The left group count shown in the question.
        /// </summary>
        public int LeftGroupCount;

        /// <summary>
        /// The right group count shown in the question.
        /// </summary>
        public int RightGroupCount;

        public CompareQuantityAnswer()
        {
        }

        public CompareQuantityAnswer(ComparisonAnswer selectedComparison, int leftCount, int rightCount,
            float responseTimeSeconds, int attemptNumber, bool hintsUsed)
            : base(selectedComparison, responseTimeSeconds, attemptNumber, hintsUsed)
        {
            SelectedComparison = selectedComparison;
            LeftGroupCount = leftCount;
            RightGroupCount = rightCount;
        }

        /// <summary>
        /// Check if the selected comparison is correct based on the group counts.
        /// </summary>
        public bool IsCorrect()
        {
            ComparisonAnswer expected = CalculateExpectedAnswer();
            return SelectedComparison == expected;
        }

        /// <summary>
        /// Calculate what the correct answer should be.
        /// </summary>
        public ComparisonAnswer CalculateExpectedAnswer()
        {
            if (LeftGroupCount == RightGroupCount)
            {
                return ComparisonAnswer.Equal;
            }
            else if (LeftGroupCount > RightGroupCount)
            {
                return ComparisonAnswer.More;
            }
            else
            {
                return ComparisonAnswer.Fewer;
            }
        }

        /// <summary>
        /// Check if this is an equality question.
        /// </summary>
        public bool IsEqualityQuestion()
        {
            return LeftGroupCount == RightGroupCount;
        }
    }
}
