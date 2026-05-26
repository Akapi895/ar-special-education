using Core.Learning.Models;
using System;

namespace Core.Learning.ActivityRunner
{
    /// <summary>
    /// Base interface for activity views.
    /// Views are responsible for UI display and user input only.
    /// All logic should be in the Presenter.
    /// </summary>
    public interface IActivityView
    {
        /// <summary>
        /// Event fired when user submits an answer.
        /// </summary>
        event Action<ActivityAnswer> OnAnswerSelected;

        /// <summary>
        /// Event fired when user requests a hint.
        /// </summary>
        event Action OnHintRequested;

        /// <summary>
        /// Event fired when user cancels the activity.
        /// </summary>
        event Action OnCancelRequested;

        /// <summary>
        /// Initialize the view with presenter callbacks.
        /// </summary>
        void Initialize(IActivityPresenter presenter);

        /// <summary>
        /// Show the activity UI.
        /// </summary>
        void Show();

        /// <summary>
        /// Hide the activity UI.
        /// </summary>
        void Hide();

        /// <summary>
        /// Display feedback for correct answer.
        /// </summary>
        void ShowCorrectFeedback();

        /// <summary>
        /// Display feedback for incorrect answer.
        /// </summary>
        void ShowIncorrectFeedback();

        /// <summary>
        /// Display a hint to the user.
        /// </summary>
        void ShowHint(ActivityHint hint);

        /// <summary>
        /// Update the progress display (e.g., "Question 2 of 5").
        /// </summary>
        void UpdateProgress(int current, int total);

        /// <summary>
        /// Enable or disable user input.
        /// </summary>
        void SetInputEnabled(bool enabled);
    }

    /// <summary>
    /// Interface for activity presenters.
    /// Separates the presenter logic from the view.
    /// </summary>
    public interface IActivityPresenter
    {
        /// <summary>
        /// Get the current activity state.
        /// </summary>
        ActivityState GetState();

        /// <summary>
        /// Submit an answer for checking.
        /// </summary>
        void SubmitAnswer(ActivityAnswer answer);

        /// <summary>
        /// Request a hint.
        /// </summary>
        void RequestHint();

        /// <summary>
        /// Continue after a completed round.
        /// </summary>
        void ContinueToNextRound();

        /// <summary>
        /// Check whether the completed round is followed by another round in the same activity.
        /// </summary>
        bool HasMoreRounds();

        /// <summary>
        /// Cancel the current activity.
        /// </summary>
        void Cancel();
    }
}
