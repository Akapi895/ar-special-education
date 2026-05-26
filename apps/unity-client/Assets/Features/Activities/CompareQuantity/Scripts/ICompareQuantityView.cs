using Core.Learning.ActivityRunner;
using Core.Learning.Models;
using Features.Activities.CompareQuantity;
using System;

namespace Features.Activities.CompareQuantity
{
    /// <summary>
    /// View interface for Compare Quantity activity.
    /// Separates UI concerns from presenter logic.
    /// </summary>
    public interface ICompareQuantityView : IActivityView
    {
        /// <summary>
        /// Event fired when an answer is selected.
        /// </summary>
        new event Action<ComparisonAnswer> OnAnswerSelected;

        /// <summary>
        /// Show the question with group counts.
        /// </summary>
        /// <param name="leftCount">Number of objects in left group.</param>
        /// <param name="rightCount">Number of objects in right group.</param>
        /// <param name="isEquality">Whether this is an equality question.</param>
        void ShowQuestion(int leftCount, int rightCount, bool isEquality);

        /// <summary>
        /// Show correct feedback with custom message.
        /// </summary>
        void ShowCorrectFeedback(string message);

        /// <summary>
        /// Show incorrect feedback with custom message.
        /// </summary>
        void ShowIncorrectFeedback(string message);

        /// <summary>
        /// Show activity completion summary.
        /// </summary>
        void ShowActivityComplete(ActivityResult result);

        /// <summary>
        /// Show activity failure message.
        /// </summary>
        void ShowActivityFailed(string message, ActivityResult result);

        /// <summary>
        /// Update the answer button labels.
        /// </summary>
        void UpdateButtonLabels(string moreLabel, string fewerLabel, string equalLabel);

        /// <summary>
        /// Highlight a group (left or right).
        /// </summary>
        void HighlightGroup(string groupSide, bool highlight);
    }
}
