using Core.Learning.ActivityRunner;
using Core.Learning.Models;
using Features.Activities.QuantityMatch;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Features.Activities.QuantityMatch
{
    /// <summary>
    /// MonoBehaviour implementation of Quantity Match view.
    /// Handles UI display and user input for the Quantity Match activity.
    /// </summary>
    public class QuantityMatchView : MonoBehaviour, IQuantityMatchView
    {
        [Header("UI References")]
        [SerializeField]
        private Text targetNumberText;

        [SerializeField]
        private Text progressText;

        [SerializeField]
        private Text feedbackText;

        [SerializeField]
        private GameObject feedbackPanel;

        [SerializeField]
        private Text hintText;

        [SerializeField]
        private GameObject hintPanel;

        [SerializeField]
        private Button hintButton;

        [SerializeField]
        private Button cancelButton;

        [SerializeField]
        private Button nextRoundButton;

        [Header("Group Selection UI")]
        [SerializeField]
        private GameObject[] groupSelectionButtons;  // Optional: buttons for each group

        // Events from IActivityView
        public event Action<ActivityAnswer> OnAnswerSelected;
        public event Action OnHintRequested;
        public event Action OnCancelRequested;

        // Events specific to IQuantityMatchView
        public event Action<int, int> OnGroupSelected;

        // Presenter reference
        private IActivityPresenter presenter;

        // State
        private int currentTargetNumber;
        private int currentNumberOfGroups;

        private void Awake()
        {
            // Hide feedback panels initially
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
        /// Show the question with target number and group count.
        /// </summary>
        public void ShowQuestion(int targetNumber, int numberOfGroups)
        {
            currentTargetNumber = targetNumber;
            currentNumberOfGroups = numberOfGroups;

            UpdateTargetNumber(targetNumber);
            HideFeedback();
            HideHint();

            // Enable hint button
            if (hintButton != null)
            {
                hintButton.interactable = true;
            }
        }

        /// <summary>
        /// Update the displayed target number.
        /// </summary>
        public void UpdateTargetNumber(int targetNumber)
        {
            if (targetNumberText != null)
            {
                targetNumberText.text = targetNumber.ToString();
            }
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
        /// Show correct feedback.
        /// </summary>
        public void ShowCorrectFeedback()
        {
            ShowCorrectFeedback("Great job! You found the right group!");
        }

        /// <summary>
        /// Show correct feedback with custom message.
        /// </summary>
        public void ShowCorrectFeedback(string message)
        {
            ShowFeedback(message, Color.green);
            DisableInput();
        }

        /// <summary>
        /// Show incorrect feedback.
        /// </summary>
        public void ShowIncorrectFeedback()
        {
            ShowIncorrectFeedback("Not quite. Let's try again!");
        }

        /// <summary>
        /// Show incorrect feedback with custom message.
        /// </summary>
        public void ShowIncorrectFeedback(string message)
        {
            ShowFeedback(message, Color.red);
            EnableInput();
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

                // Auto-hide after 5 seconds
                CancelInvoke(nameof(HideHint));
                Invoke(nameof(HideHint), 5f);
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

            // Show next button or complete button
            if (nextRoundButton != null)
            {
                nextRoundButton.gameObject.SetActive(true);
                // TODO: Change to "Finish" or return to menu
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
        /// Highlight a specific group.
        /// </summary>
        public void HighlightGroup(int groupIndex, bool highlight)
        {
            // TODO: Implement visual highlighting of AR groups
            // This would involve communicating with AR service to highlight
            Debug.Log($"[QuantityMatchView] Highlight group {groupIndex}: {highlight}");
        }

        /// <summary>
        /// Enable or disable user input.
        /// </summary>
        public void SetInputEnabled(bool enabled)
        {
            if (enabled)
            {
                EnableInput();
            }
            else
            {
                DisableInput();
            }
        }

        /// <summary>
        /// Enable all user input.
        /// </summary>
        private void EnableInput()
        {
            // Enable hint button
            if (hintButton != null)
            {
                hintButton.interactable = true;
            }

            // Enable group interaction via AR service
            // TODO: Notify AR interaction service
        }

        /// <summary>
        /// Disable all user input.
        /// </summary>
        private void DisableInput()
        {
            // Disable hint button
            if (hintButton != null)
            {
                hintButton.interactable = false;
            }

            // Disable group interaction via AR service
            // TODO: Notify AR interaction service
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
            // This is called when activity is complete
            // TODO: Return to menu or load next activity
            Debug.Log("[QuantityMatchView] Next round / Finish clicked");

            if (nextRoundButton != null)
            {
                nextRoundButton.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Called when a group is selected (from AR interaction or UI button).
        /// This is typically called by the AR interaction service.
        /// </summary>
        public void NotifyGroupSelected(int groupIndex, int objectCount)
        {
            OnGroupSelected?.Invoke(groupIndex, objectCount);
        }

        /// <summary>
        /// Called when UI button for a group is clicked.
        /// </summary>
        public void OnGroupButtonClicked(int groupIndex)
        {
            if (groupIndex >= 0 && groupIndex < currentNumberOfGroups)
            {
                NotifyGroupSelected(groupIndex, 0);  // Count will be filled by presenter
            }
        }
    }
}
