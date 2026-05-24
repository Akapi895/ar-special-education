using Core.Learning.ActivityRunner;
using Core.Learning.Models;
using Features.Activities.NumberLineJump;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Features.Activities.NumberLineJump
{
    /// <summary>
    /// MonoBehaviour implementation of Number Line Jump view.
    /// Handles UI display and user input for the Number Line Jump activity.
    /// </summary>
    public class NumberLineJumpView : MonoBehaviour, INumberLineJumpView
    {
        [Header("UI References - Question Info")]
        [SerializeField]
        private Text startNumberText;

        [SerializeField]
        private Text targetNumberText;

        [SerializeField]
        private Text progressText;

        [SerializeField]
        private Text currentPositionText;

        [Header("UI References - Equation")]
        [SerializeField]
        private Text equationText;

        [SerializeField]
        private GameObject equationPanel;

        [Header("UI References - Feedback")]
        [SerializeField]
        private Text feedbackText;

        [SerializeField]
        private GameObject feedbackPanel;

        [SerializeField]
        private Text hintText;

        [SerializeField]
        private GameObject hintPanel;

        [Header("Jump Controls")]
        [SerializeField]
        private Button leftJumpButton;

        [SerializeField]
        private Button rightJumpButton;

        [SerializeField]
        private Button confirmButton;

        [SerializeField]
        private Button resetButton;

        [Header("Other Controls")]
        [SerializeField]
        private Button hintButton;

        [SerializeField]
        private Button cancelButton;

        [SerializeField]
        private Button nextRoundButton;

        // Events from IActivityView
        public event Action OnHintRequested;
        public event Action OnCancelRequested;

        event Action<ActivityAnswer> IActivityView.OnAnswerSelected
        {
            add { }
            remove { }
        }

        // Events specific to INumberLineJumpView
        public event Action<JumpStepDirection> OnJumpRequested;
        public event Action OnConfirmRequested;
        public event Action OnResetRequested;
        public event Action<int> OnTileTapped;

        // Presenter reference
        private IActivityPresenter presenter;

        // State
        private JumpDirection currentAllowedDirection;
        private int currentMinNumber;
        private int currentMaxNumber;
        private int currentPos;

        private void Awake()
        {
            // Hide panels initially
            if (feedbackPanel != null)
            {
                feedbackPanel.SetActive(false);
            }

            if (hintPanel != null)
            {
                hintPanel.SetActive(false);
            }

            if (nextRoundButton != null)
            {
                nextRoundButton.gameObject.SetActive(false);
            }

            // Setup button listeners
            if (leftJumpButton != null)
            {
                leftJumpButton.onClick.AddListener(() => OnJumpRequested?.Invoke(JumpStepDirection.Left));
            }

            if (rightJumpButton != null)
            {
                rightJumpButton.onClick.AddListener(() => OnJumpRequested?.Invoke(JumpStepDirection.Right));
            }

            if (confirmButton != null)
            {
                confirmButton.onClick.AddListener(() => OnConfirmRequested?.Invoke());
            }

            if (resetButton != null)
            {
                resetButton.onClick.AddListener(() => OnResetRequested?.Invoke());
            }

            if (hintButton != null)
            {
                hintButton.onClick.AddListener(() => OnHintRequested?.Invoke());
            }

            if (cancelButton != null)
            {
                cancelButton.onClick.AddListener(() => OnCancelRequested?.Invoke());
            }

            if (nextRoundButton != null)
            {
                nextRoundButton.onClick.AddListener(OnNextRoundClicked);
            }
        }

        /// <summary>
        /// Initialize the view with presenter callbacks.
        /// </summary>
        public void Initialize(IActivityPresenter activityPresenter)
        {
            presenter = activityPresenter;
        }

        /// <summary>
        /// Show the activity UI.
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Hide the activity UI.
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Show the question with all relevant data.
        /// </summary>
        public void ShowQuestion(int startNumber, int targetNumber, int minNumber, int maxNumber, JumpDirection allowedDirection)
        {
            currentAllowedDirection = allowedDirection;
            currentMinNumber = minNumber;
            currentMaxNumber = maxNumber;
            currentPos = startNumber;

            // Update displays
            if (startNumberText != null)
            {
                startNumberText.text = $"Start: {startNumber}";
            }

            if (targetNumberText != null)
            {
                targetNumberText.text = $"Target: {targetNumber}";
            }

            UpdateCurrentPosition(startNumber);

            // Hide feedback and hints
            HideFeedback();
            HideHint();

            // Show equation panel
            if (equationPanel != null)
            {
                equationPanel.SetActive(true);
            }

            // Enable input
            SetJumpInputEnabled(true);

            // Enable hint button
            if (hintButton != null)
            {
                hintButton.interactable = true;
            }

            // Update button states
            UpdateJumpButtonsState(allowedDirection, startNumber, minNumber, maxNumber);
        }

        /// <summary>
        /// Update the equation display.
        /// </summary>
        public void UpdateEquation(string equation)
        {
            if (equationText != null)
            {
                equationText.text = equation;
            }
        }

        /// <summary>
        /// Update the current position display.
        /// </summary>
        public void UpdateCurrentPosition(int position)
        {
            currentPos = position;

            if (currentPositionText != null)
            {
                currentPositionText.text = $"Position: {position}";
            }

            // Update button states based on new position
            UpdateJumpButtonsState(currentAllowedDirection, position, currentMinNumber, currentMaxNumber);
        }

        /// <summary>
        /// Update progress display.
        /// </summary>
        public void UpdateProgress(int current, int total)
        {
            if (progressText != null)
            {
                progressText.text = $"Question {current} of {total}";
            }
        }

        /// <summary>
        /// Show correct feedback with equation.
        /// </summary>
        public void ShowCorrectFeedback()
        {
            ShowCorrectFeedback("Great job!", "");
        }

        /// <summary>
        /// Show correct feedback with equation.
        /// </summary>
        public void ShowCorrectFeedback(string message, string finalEquation)
        {
            string fullMessage = string.IsNullOrEmpty(finalEquation) ? message : $"{message}\n{finalEquation}";
            ShowFeedback(fullMessage, Color.green);
            DisableInput();
        }

        /// <summary>
        /// Show incorrect feedback with equation.
        /// </summary>
        public void ShowIncorrectFeedback()
        {
            ShowIncorrectFeedback("Not quite. Try again!", "");
        }

        /// <summary>
        /// Show incorrect feedback with equation.
        /// </summary>
        public void ShowIncorrectFeedback(string message, string attemptedEquation)
        {
            string fullMessage = string.IsNullOrEmpty(attemptedEquation) ? message : $"{message}\n{attemptedEquation}";
            ShowFeedback(fullMessage, Color.red);
            EnableInput();
        }

        /// <summary>
        /// Show overshoot feedback.
        /// </summary>
        public void ShowOvershootFeedback(int currentPosition, int targetPosition)
        {
            ShowFeedback($"You went too far! You're at {currentPosition}, but the target was {targetPosition}.", Color.red);
        }

        /// <summary>
        /// Show boundary hit feedback.
        /// </summary>
        public void ShowBoundaryHit(int currentPosition)
        {
            ShowFeedback($"You can't go further from {currentPosition}. You've reached the edge!", Color.yellow);

            // TODO: Play bump animation
            Debug.Log("[NumberLineJumpView] Play bump animation at boundary");
        }

        /// <summary>
        /// Show max jumps exceeded feedback.
        /// </summary>
        public void ShowMaxJumpsExceeded()
        {
            ShowFeedback("You've used too many jumps! Press Reset to try again.", Color.red);
            DisableInput();
        }

        /// <summary>
        /// Show max jumps warning.
        /// </summary>
        public void ShowMaxJumpsWarning(int remainingJumps)
        {
            // Show as a temporary notification
            Debug.Log($"[NumberLineJumpView] Max jumps warning: {remainingJumps} remaining");

            // Could display as a toast or warning panel
            // For now, log it
        }

        /// <summary>
        /// Show that a direction is not allowed.
        /// </summary>
        public void ShowDirectionNotAllowed(JumpStepDirection direction)
        {
            string directionText = direction == JumpStepDirection.Right ? "right" : "left";
            Debug.Log($"[NumberLineJumpView] Cannot jump {directionText} - not allowed for this question");

            // Could show a brief visual indicator
        }

        /// <summary>
        /// Show a hint to the user.
        /// </summary>
        public void ShowHint(ActivityHint hint)
        {
            if (hintPanel != null && hintText != null)
            {
                hintText.text = hint.HintText;
                hintPanel.SetActive(true);

                // Auto-hide after 8 seconds (longer for this activity)
                CancelInvoke(nameof(HideHint));
                Invoke(nameof(HideHint), 8f);
            }
        }

        /// <summary>
        /// Hide the hint panel.
        /// </summary>
        public void HideHint()
        {
            if (hintPanel != null)
            {
                hintPanel.SetActive(false);
            }
        }

        /// <summary>
        /// Show activity completion summary.
        /// </summary>
        public void ShowActivityComplete(ActivityResult result)
        {
            string message = $"Activity Complete!\n" +
                           $"Correct: {result.IsCorrect}\n" +
                           $"Attempts: {result.TotalAttempts}\n" +
                           $"Hints Used: {result.HintsUsedCount}\n" +
                           $"Time: {result.TimeSpentSeconds:F1} seconds";

            ShowFeedback(message, Color.green);

            if (nextRoundButton != null)
            {
                nextRoundButton.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// Show activity failure message.
        /// </summary>
        public void ShowActivityFailed(string message, ActivityResult result)
        {
            string fullMessage = $"{message}\n" +
                               $"Attempts: {result.TotalAttempts}\n" +
                               $"Hints Used: {result.HintsUsedCount}";

            ShowFeedback(fullMessage, Color.red);

            if (nextRoundButton != null)
            {
                nextRoundButton.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// Highlight a specific number tile.
        /// </summary>
        public void HighlightTile(int number, bool highlight)
        {
            // TODO: Implement visual highlighting of AR tiles
            Debug.Log($"[NumberLineJumpView] Highlight tile {number}: {highlight}");
        }

        /// <summary>
        /// Enable or disable user input (IActivityView).
        /// </summary>
        public void SetInputEnabled(bool enabled)
        {
            SetJumpInputEnabled(enabled);

            if (hintButton != null)
            {
                hintButton.interactable = enabled;
            }

            if (cancelButton != null)
            {
                cancelButton.interactable = enabled;
            }
        }

        /// <summary>
        /// Enable or disable jump input.
        /// </summary>
        public void SetJumpInputEnabled(bool enabled)
        {
            if (enabled)
            {
                EnableJumpButtons();
            }
            else
            {
                DisableJumpButtons();
            }
        }

        /// <summary>
        /// Update the state of jump buttons based on allowed direction and position.
        /// </summary>
        public void UpdateJumpButtonsState(JumpDirection allowedDirection, int currentPosition, int minNumber, int maxNumber)
        {
            if (leftJumpButton != null)
            {
                bool canGoLeft = allowedDirection == JumpDirection.LeftOnly || allowedDirection == JumpDirection.Both;
                bool notAtMin = currentPosition > minNumber;
                leftJumpButton.interactable = canGoLeft && notAtMin;
            }

            if (rightJumpButton != null)
            {
                bool canGoRight = allowedDirection == JumpDirection.RightOnly || allowedDirection == JumpDirection.Both;
                bool notAtMax = currentPosition < maxNumber;
                rightJumpButton.interactable = canGoRight && notAtMax;
            }

            // Enable confirm if not at start position
            if (confirmButton != null)
            {
                // confirmButton is enabled when player has moved from start
            }
        }

        /// <summary>
        /// Enable jump buttons.
        /// </summary>
        private void EnableJumpButtons()
        {
            UpdateJumpButtonsState(currentAllowedDirection, currentPos, currentMinNumber, currentMaxNumber);

            if (confirmButton != null)
            {
                confirmButton.interactable = true;
            }

            if (resetButton != null)
            {
                resetButton.interactable = true;
            }
        }

        /// <summary>
        /// Disable jump buttons.
        /// </summary>
        private void DisableJumpButtons()
        {
            if (leftJumpButton != null)
            {
                leftJumpButton.interactable = false;
            }

            if (rightJumpButton != null)
            {
                rightJumpButton.interactable = false;
            }

            if (confirmButton != null)
            {
                confirmButton.interactable = false;
            }

            if (resetButton != null)
            {
                resetButton.interactable = false;
            }
        }

        /// <summary>
        /// Enable all user input.
        /// </summary>
        private void EnableInput()
        {
            EnableJumpButtons();

            if (hintButton != null)
            {
                hintButton.interactable = true;
            }
        }

        /// <summary>
        /// Disable all user input.
        /// </summary>
        private void DisableInput()
        {
            DisableJumpButtons();

            if (hintButton != null)
            {
                hintButton.interactable = false;
            }
        }

        /// <summary>
        /// Show feedback panel with message.
        /// </summary>
        private void ShowFeedback(string message, Color color)
        {
            if (feedbackPanel != null && feedbackText != null)
            {
                feedbackText.text = message;
                feedbackText.color = color;
                feedbackPanel.SetActive(true);
            }
        }

        /// <summary>
        /// Hide feedback panel.
        /// </summary>
        private void HideFeedback()
        {
            if (feedbackPanel != null)
            {
                feedbackPanel.SetActive(false);
            }
        }

        /// <summary>
        /// Handle next round button click.
        /// </summary>
        private void OnNextRoundClicked()
        {
            Debug.Log("[NumberLineJumpView] Next round / Finish clicked");

            if (nextRoundButton != null)
            {
                nextRoundButton.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Called when a tile is tapped (from AR interaction).
        /// </summary>
        public void NotifyTileTapped(int tileNumber)
        {
            OnTileTapped?.Invoke(tileNumber);
        }
    }
}
