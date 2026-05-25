using Core.Learning.ActivityRunner;
using Core.Learning.Models;
using Features.Activities.CompareQuantity;
using Project.App;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
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
        private Text questionText;

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

        [SerializeField]
        private Button progressButton;

        public event Action OnHintRequested;
        public event Action OnCancelRequested;

        // Events specific to ICompareQuantityView
        public event Action<ComparisonAnswer> OnAnswerSelected;

        event Action<ActivityAnswer> IActivityView.OnAnswerSelected
        {
            add { }
            remove { }
        }

        // Presenter reference
        private IActivityPresenter presenter;

        // State
        private ComparisonAnswer? currentSelectedAnswer;
        private bool activityFinished;

        private static readonly Vector2 RuntimeButtonSize = new Vector2(170f, 56f);
        private const float RuntimeButtonGap = 18f;
        private const float RuntimeActionButtonBottomY = 52f;
        private const float RuntimeAnswerButtonBottomY = 126f;
        private const float RuntimeHintPanelBottomY = 196f;
        private const float RuntimeFeedbackPanelBottomY = 272f;

        public bool HasUiReferences => progressText != null;

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

            if (progressButton != null)
            {
                progressButton.gameObject.SetActive(false);
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
                cancelButton.onClick.AddListener(OnCancelClicked);
            }

            if (nextRoundButton != null)
            {
                nextRoundButton.onClick.AddListener(OnNextRoundClicked);
            }

            if (progressButton != null)
            {
                progressButton.onClick.AddListener(OnProgressClicked);
            }
        }

        /// <summary>
        /// Builds minimal UI at runtime when prefabs are not assigned.
        /// </summary>
        public void BuildRuntimeUi(Canvas canvas)
        {
            var panel = CreateUiPanel(canvas.transform, "CompareQuantityPanel");

            progressText = CreateTopText(panel, "Progress", "", 24, 24f, new Vector2(620f, 40f));
            questionText = CreateTopText(panel, "QuestionText", "Does the left group have more, fewer, or equal balls?", 32, 70f, new Vector2(920f, 64f));

            leftGroupCountText = CreateTopText(panel, "LeftGroupCount", "Left: ?", 24, 132f, new Vector2(320f, 44f));
            leftGroupCountText.GetComponent<RectTransform>().anchoredPosition = new Vector2(-235f, -132f);
            rightGroupCountText = CreateTopText(panel, "RightGroupCount", "Right: ?", 24, 132f, new Vector2(320f, 44f));
            rightGroupCountText.GetComponent<RectTransform>().anchoredPosition = new Vector2(235f, -132f);

            feedbackPanel = CreateSubPanel(panel, "FeedbackPanel", new Vector2(0f, RuntimeFeedbackPanelBottomY), new Vector2(760f, 68f));
            feedbackText = CreatePanelText(feedbackPanel.transform, "FeedbackText", "", 22);
            feedbackPanel.SetActive(false);

            hintPanel = CreateSubPanel(panel, "HintPanel", new Vector2(0f, RuntimeHintPanelBottomY), new Vector2(760f, 64f));
            hintText = CreatePanelText(hintPanel.transform, "HintText", "", 20);
            hintPanel.SetActive(false);

            float answerStartX = -(RuntimeButtonSize.x + RuntimeButtonGap);
            moreButton = CreateButton(panel, "MoreButton", "Left More", new Vector2(answerStartX, RuntimeAnswerButtonBottomY), () => OnAnswerButtonClicked(ComparisonAnswer.More), out moreButtonText);
            fewerButton = CreateButton(panel, "FewerButton", "Left Fewer", new Vector2(0f, RuntimeAnswerButtonBottomY), () => OnAnswerButtonClicked(ComparisonAnswer.Fewer), out fewerButtonText);
            equalButton = CreateButton(panel, "EqualButton", "Equal", new Vector2(RuntimeButtonSize.x + RuntimeButtonGap, RuntimeAnswerButtonBottomY), () => OnAnswerButtonClicked(ComparisonAnswer.Equal), out equalButtonText);

            float actionButtonOffset = (RuntimeButtonSize.x + RuntimeButtonGap) * 0.5f;
            hintButton = CreateButton(panel, "HintButton", "Hint", new Vector2(-actionButtonOffset, RuntimeActionButtonBottomY), () => OnHintRequested?.Invoke(), out _);
            cancelButton = CreateButton(panel, "CancelButton", "Cancel", new Vector2(actionButtonOffset, RuntimeActionButtonBottomY), OnCancelClicked, out _);
            nextRoundButton = CreateButton(panel, "NextButton", "Next", new Vector2(-actionButtonOffset, RuntimeActionButtonBottomY), OnNextRoundClicked, out _);
            progressButton = CreateButton(panel, "ProgressButton", "Progress", new Vector2(actionButtonOffset, RuntimeActionButtonBottomY), OnProgressClicked, out _);
            nextRoundButton.gameObject.SetActive(false);
            progressButton.gameObject.SetActive(false);
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
            activityFinished = false;

            if (questionText != null)
            {
                questionText.text = "Does the left group have more, fewer, or equal balls than the right group?";
            }

            // Note: We don't show the actual counts to the child - they need to count!
            // But we may use this internally or show "?"

            if (leftGroupCountText != null)
            {
                leftGroupCountText.text = "Left: ?";  // Child counts the objects
            }

            if (rightGroupCountText != null)
            {
                rightGroupCountText.text = "Right: ?";  // Child counts the objects
            }

            HideFeedback();
            HideHint();

            // Enable answer buttons
            EnableAnswerButtons(true);

            // Enable hint button
            if (hintButton != null)
            {
                hintButton.gameObject.SetActive(true);
                hintButton.interactable = true;
            }

            if (cancelButton != null)
            {
                cancelButton.gameObject.SetActive(true);
            }

            SetAnswerButtonsActive(true);
            SetNavigationButtonsActive(false);

            currentSelectedAnswer = null;
        }

        /// <summary>
        /// Update the answer button labels from config.
        /// </summary>
        public void UpdateButtonLabels(string moreLabel, string fewerLabel, string equalLabel)
        {
            if (moreButtonText != null)
            {
                moreButtonText.text = FormatComparisonLabel("Left", moreLabel);
            }

            if (fewerButtonText != null)
            {
                fewerButtonText.text = FormatComparisonLabel("Left", fewerLabel);
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
            SetAnswerButtonsActive(false);
            SetRunningActionButtonsActive(false);

            if (nextRoundButton != null)
            {
                nextRoundButton.gameObject.SetActive(true);
            }
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
            activityFinished = true;
            DisableInput();
            SetAnswerButtonsActive(false);
            SetRunningActionButtonsActive(false);

            if (nextRoundButton != null)
            {
                nextRoundButton.gameObject.SetActive(ActivityFlowNavigator.TryGetNextActivityId("CompareQuantity", out _));
            }

            if (progressButton != null)
            {
                progressButton.gameObject.SetActive(true);
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
            activityFinished = true;
            DisableInput();
            SetAnswerButtonsActive(false);
            SetRunningActionButtonsActive(false);

            if (nextRoundButton != null)
            {
                nextRoundButton.gameObject.SetActive(ActivityFlowNavigator.TryGetNextActivityId("CompareQuantity", out _));
            }

            if (progressButton != null)
            {
                progressButton.gameObject.SetActive(true);
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

            if (activityFinished || presenter?.GetState() == ActivityState.Failed)
            {
                if (!ActivityFlowNavigator.LoadNextActivity("CompareQuantity"))
                {
                    ActivityFlowNavigator.LoadProgressDashboard();
                }
                return;
            }

            if (nextRoundButton != null)
            {
                nextRoundButton.gameObject.SetActive(false);
            }

            if (presenter?.GetState() == ActivityState.Completed)
            {
                bool hasMoreRounds = presenter.HasMoreRounds();
                presenter.ContinueToNextRound();

                if (!hasMoreRounds && !ActivityFlowNavigator.LoadNextActivity("CompareQuantity"))
                {
                    ActivityFlowNavigator.LoadProgressDashboard();
                }
            }
        }

        private void OnProgressClicked()
        {
            Debug.Log("[CompareQuantityView] Progress clicked");
            ActivityFlowNavigator.LoadProgressDashboard();
        }

        private void OnCancelClicked()
        {
            OnCancelRequested?.Invoke();
            LoadSceneIfAvailable("SC_ActivitySelect");
        }

        private void SetAnswerButtonsActive(bool active)
        {
            if (moreButton != null) moreButton.gameObject.SetActive(active);
            if (fewerButton != null) fewerButton.gameObject.SetActive(active);
            if (equalButton != null) equalButton.gameObject.SetActive(active);
        }

        private void SetRunningActionButtonsActive(bool active)
        {
            if (hintButton != null) hintButton.gameObject.SetActive(active);
            if (cancelButton != null) cancelButton.gameObject.SetActive(active);
        }

        private void SetNavigationButtonsActive(bool active)
        {
            if (nextRoundButton != null) nextRoundButton.gameObject.SetActive(active);
            if (progressButton != null) progressButton.gameObject.SetActive(active);
        }

        private static string FormatComparisonLabel(string subject, string label)
        {
            if (string.IsNullOrWhiteSpace(label))
            {
                return subject;
            }

            if (label.StartsWith(subject, StringComparison.OrdinalIgnoreCase))
            {
                return label;
            }

            return $"{subject} {label}";
        }

        private static RectTransform CreateUiPanel(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return rect;
        }

        private static Text CreateTopText(Transform parent, string name, string content, int fontSize, float topOffset, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = size;
            rect.anchoredPosition = new Vector2(0f, -topOffset);

            var text = go.GetComponent<Text>();
            text.text = content;
            text.fontSize = fontSize;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 16;
            text.resizeTextMaxSize = fontSize;
            text.alignment = TextAnchor.MiddleCenter;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.color = Color.white;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        private static GameObject CreateSubPanel(Transform parent, string name, Vector2 anchoredPosition, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;

            var image = go.GetComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0.55f);
            image.raycastTarget = false;
            return go;
        }

        private static Text CreatePanelText(Transform parent, string name, string content, int fontSize)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(16f, 6f);
            rect.offsetMax = new Vector2(-16f, -6f);

            var text = go.GetComponent<Text>();
            text.text = content;
            text.fontSize = fontSize;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 14;
            text.resizeTextMaxSize = fontSize;
            text.alignment = TextAnchor.MiddleCenter;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.color = Color.white;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

        private static Button CreateButton(Transform parent, string name, string label, Vector2 anchoredPosition, UnityEngine.Events.UnityAction onClick, out Text labelText)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.sizeDelta = RuntimeButtonSize;
            rect.anchoredPosition = anchoredPosition;
            go.GetComponent<Image>().color = new Color(0.2f, 0.5f, 0.9f, 1f);

            var button = go.GetComponent<Button>();
            button.onClick.AddListener(onClick);

            labelText = CreateButtonLabel(go.transform, label);
            return button;
        }

        private static Text CreateButtonLabel(Transform parent, string label)
        {
            var go = new GameObject("Label", typeof(RectTransform), typeof(Text));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(8f, 4f);
            rect.offsetMax = new Vector2(-8f, -4f);

            var text = go.GetComponent<Text>();
            text.text = label;
            text.fontSize = 22;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 14;
            text.resizeTextMaxSize = 22;
            text.alignment = TextAnchor.MiddleCenter;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.color = Color.white;
            text.raycastTarget = false;
            return text;
        }

        private static void LoadSceneIfAvailable(string sceneName)
        {
            if (Application.CanStreamedLevelBeLoaded(sceneName))
            {
                SceneManager.LoadScene(sceneName);
            }
            else
            {
                Debug.LogWarning($"[CompareQuantityView] Scene '{sceneName}' is not available in Build Settings.");
            }
        }
    }
}
