using Core.Learning.ActivityRunner;
using Core.Learning.Models;
using Features.Activities.NumberLineJump;
using System;

namespace Features.Activities.NumberLineJump
{
    /// <summary>
    /// View interface for Number Line Jump activity.
    /// Separates UI concerns from presenter logic.
    /// </summary>
    public interface INumberLineJumpView : IActivityView
    {
        /// <summary>
        /// Event fired when a jump is requested.
        /// </summary>
        event Action<JumpStepDirection> OnJumpRequested;

        /// <summary>
        /// Event fired when player confirms their final position.
        /// </summary>
        event Action OnConfirmRequested;

        /// <summary>
        /// Event fired when player requests a reset.
        /// </summary>
        event Action OnResetRequested;

        /// <summary>
        /// Event fired when a number tile is tapped.
        /// </summary>
        event Action<int> OnTileTapped;

        /// <summary>
        /// Show the question with all relevant data.
        /// </summary>
        void ShowQuestion(int startNumber, int targetNumber, int minNumber, int maxNumber,
            JumpDirection allowedDirection, bool useEquationPromptMode = false, string equationPrompt = null);

        /// <summary>
        /// Update the equation display.
        /// </summary>
        void UpdateEquation(string equation);

        /// <summary>
        /// Update the current position display.
        /// </summary>
        void UpdateCurrentPosition(int position);

        /// <summary>
        /// Show correct feedback with equation.
        /// </summary>
        void ShowCorrectFeedback(string message, string finalEquation);

        /// <summary>
        /// Show incorrect feedback with equation.
        /// </summary>
        void ShowIncorrectFeedback(string message, string attemptedEquation);

        /// <summary>
        /// Show overshoot feedback.
        /// </summary>
        void ShowOvershootFeedback(int currentPosition, int targetPosition);

        /// <summary>
        /// Show boundary hit feedback.
        /// </summary>
        void ShowBoundaryHit(int currentPosition);

        /// <summary>
        /// Show max jumps exceeded feedback.
        /// </summary>
        void ShowMaxJumpsExceeded();

        /// <summary>
        /// Show max jumps warning.
        /// </summary>
        void ShowMaxJumpsWarning(int remainingJumps);

        /// <summary>
        /// Show that a direction is not allowed.
        /// </summary>
        void ShowDirectionNotAllowed(JumpStepDirection direction);

        /// <summary>
        /// Enable or disable jump input.
        /// </summary>
        void SetJumpInputEnabled(bool enabled);

        /// <summary>
        /// Update the state of jump buttons based on allowed direction.
        /// </summary>
        void UpdateJumpButtonsState(JumpDirection allowedDirection, int currentPosition, int minNumber, int maxNumber);

        /// <summary>
        /// Show activity completion summary.
        /// </summary>
        void ShowActivityComplete(ActivityResult result);

        /// <summary>
        /// Show activity failure message.
        /// </summary>
        void ShowActivityFailed(string message, ActivityResult result);

        /// <summary>
        /// Highlight a specific number tile.
        /// </summary>
        void HighlightTile(int number, bool highlight);
    }
}
