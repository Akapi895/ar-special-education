using Core.Learning.ActivityRunner;
using Core.Learning.Models;
using Features.Activities.CompareQuantity;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Features.Activities.CompareQuantity
{
    /// <summary>
    /// MonoBehaviour implementation of Compare Quantity view.
    /// Handles UI display and user input for the Compare Quantity activity.
    /// </summary>
    public class CompareQuantityView : MonoBehaviour, ICompareQuantityView
    {
        [Header("UI References")]
        [SerializeField]
        private Text leftGroupCountText;

        [SerializeField]
        private Text rightGroupCountText;

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

        [Header("Answer Buttons")]
        [SerializeField]
        private Button moreButton;

        [SerializeField]
        private Text moreButtonText;

        [SerializeField]
        private Button fewerButton;

        [SerializeField]
        private Text fewerButtonText;

        [SerializeField]
        private Button equalButton;

        [SerializeField]
        private Text equalButtonText;

        [Header("Other Buttons")]
        [SerializeField]
        private Button hintButton;

        [SerializeField]
        private Button cancelButton;

        [SerializeField]
        private Button nextRoundButton;

        // Events from IActivityView
        public event Action<ActivityAnswer> OnAnswerSelected;
        public event Action OnHintRequested;
        public event Action OnCancelRequested;

        // Events specific to ICompareQuantityView
        public event Action<ComparisonAnswer> OnAnswerSelected;

        // Presenter reference
        private IActivityPresenter presenter;

        // State
        private ComparisonAnswer? currentSelectedAnswer;

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
            if (moreButton != null)
            {
                moreButton.onClick.AddListener(() => OnAnswerButtonClicked(ComparisonAnswer.More));
            }

            if (fewerButton != null)
            {
                fewerButton.onClick.AddListener(() => OnAnswerButtonClicked(ComparisonAnswer.Fewer));
            }

            if (equalButton != null)
            {
                equalButton.onClick.AddListener(() => OnAnswerButtonClicked(ComparisonAnswer.Equal));
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
        /// Show the question with group counts.
        /// </summary>
        public void ShowQuestion(int leftCount, int rightCount, bool isEquality)
        {
            // Note: We don't show the actual counts to the child - they need to count!
            // But we may use this internally or show "?"

            if (leftGroupCountText != null)
            {
                leftGroupCountText.text = "?";  // Child counts the objects
            }

            if (rightGroupCountText != null)
            {
                rightGroupCountText.text = "?";  // Child counts the objects
            }

            HideFeedback();
            HideHint();

            // Enable answer buttons
            EnableAnswerButtons(true);

            // Enable hint button
            if (hintButton != null)
            {
                hintButton.interactable = true;
            }

            currentSelectedAnswer = null;
        }

        /// <summary>
        /// Update the answer button labels from config.
        /// </summary>
        public void UpdateButtonLabels(string moreLabel, string fewerLabel, string equalLabel)
        {
            if (moreButtonText != null)
            {
                moreButtonText.text = moreLabel;
            }

            if (fewerButtonText != null)
            {
                fewerButtonText.text = fewerLabel;
            }

            if (equalButtonText != null)
            {
                equalButtonText.text = equalLabel;
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
            ShowCorrectFeedback("Great job!");
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
            ShowIncorrectFeedback("Not quite. Try again!");
        }

        /// <summary>
        /// Show incorrect feedback with custom message.
        /// </summary>
        public void ShowIncorrectFeedback(string message)
        {
            ShowFeedback(message, Color.red);
            EnableAnswerButtons(true);  // Allow retry
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

                // Auto-hide after 6 seconds (slightly longer for comparison)
                CancelInvoke(nameof(HideHint));
                Invoke(nameof(HideHint), 6f);
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
        /// Highlight a group (left or right).
        /// </summary>
        public void HighlightGroup(string groupSide, bool highlight)
        {
            // TODO: Implement visual highlighting of AR groups
            // This would involve communicating with AR service to highlight
            Debug.Log($"[CompareQuantityView] Highlight {groupSide} group: {highlight}");
        }

        /// <summary>
        /// Enable or disable user input.
        /// </summary>
        public void SetInputEnabled(bool enabled)
        {
            EnableAnswerButtons(enabled);
        }

        /// <summary>
        /// Enable or disable answer buttons.
        /// </summary>
        private void EnableAnswerButtons(bool enabled)
        {
            if (moreButton != null)
            {
                moreButton.interactable = enabled;
            }

            if (fewerButton != null)
            {
                fewerButton.interactable = enabled;
            }

            if (equalButton != null)
            {
                equalButton.interactable = enabled;
            }
        }

        /// <summary>
        /// Disable all user input.
        /// </summary>
        private void DisableInput()
        {
            EnableAnswerButtons(false);

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
        /// Handle answer button click.
        /// </summary>
        private void OnAnswerButtonClicked(ComparisonAnswer answer)
        {
            currentSelectedAnswer = answer;
            OnAnswerSelected?.Invoke(answer);
        }

        /// <summary>
        /// Handle next round button click.
        /// </summary>
        private void OnNextRoundClicked()
        {
            Debug.Log("[CompareQuantityView] Next round / Finish clicked");

            if (nextRoundButton != null)
            {
                nextRoundButton.gameObject.SetActive(false);
            }
        }
    }
}
