using Core.Learning.ActivityRunner;
using Core.Learning.Models;
using System;

namespace Features.Activities.QuantityMatch
{
    /// <summary>
    /// View interface for Quantity Match activity.
    /// Separates UI concerns from presenter logic.
    /// </summary>
    public interface IQuantityMatchView : IActivityView
    {
        /// <summary>
        /// Event fired when a group is selected.
        /// </summary>
        event Action<int, int> OnGroupSelected;  // groupIndex, objectCount

        /// <summary>
        /// Event fired when a typed number answer is submitted.
        /// </summary>
        event Action<int> OnNumberAnswerSubmitted;

        /// <summary>
        /// Show the question with target number and group count.
        /// </summary>
        void ShowQuestion(int targetNumber, int numberOfGroups, bool useNumberInputMode = false);

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
        /// Highlight a specific group.
        /// </summary>
        void HighlightGroup(int groupIndex, bool highlight);

        /// <summary>
        /// Update the displayed target number.
        /// </summary>
        void UpdateTargetNumber(int targetNumber);
    }
}
