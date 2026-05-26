using Core.Learning.ActivityRunner;
using Core.Learning.Models;
using Core.Support.AudioManager;
using Core.UI.Localization;
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
        private Button listenButton;

        [SerializeField]
        private Button cancelButton;

        [SerializeField]
        private Button nextRoundButton;

        [SerializeField]
        private Button progressButton;

        [SerializeField]
        private Core.UI.Components.UIFeedbackOverlay feedbackOverlay;

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

        private static readonly Vector2 RuntimeButtonSize = new Vector2(190f, 66f);
        private const float RuntimeButtonGap = 22f;
        private const float RuntimeActionButtonBottomY = 52f;
        private const float RuntimeAnswerButtonBottomY = 128f;
        private const float RuntimeHintPanelBottomY = 220f;
        private const float RuntimeFeedbackPanelBottomY = 296f;

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

            if (listenButton != null)
            {
                listenButton.onClick.AddListener(OnListenClicked);
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

            CreateTopPanel(panel, "QuestionHeaderPanel", new Vector2(0f, -34f), new Vector2(1040f, 84f), 0.56f);
            CreateTopPanel(panel, "GroupSideHeaderPanel", new Vector2(0f, -112f), new Vector2(720f, 46f), 0.42f);

            progressText = CreateTopText(panel, "Progress", "", 22, 16f, new Vector2(260f, 40f));
            progressText.GetComponent<RectTransform>().anchoredPosition = new Vector2(-500f, -16f);
            questionText = CreateTopText(panel, "QuestionText", SimpleLocalization.Get("compare_question"), 30, 28f, new Vector2(980f, 70f));

            leftGroupCountText = CreateTopText(panel, "LeftGroupCount", "B\u00ean tr\u00e1i", 24, 104f, new Vector2(300f, 42f));
            leftGroupCountText.GetComponent<RectTransform>().anchoredPosition = new Vector2(-230f, -104f);
            rightGroupCountText = CreateTopText(panel, "RightGroupCount", "B\u00ean ph\u1ea3i", 24, 104f, new Vector2(300f, 42f));
            rightGroupCountText.GetComponent<RectTransform>().anchoredPosition = new Vector2(230f, -104f);

            feedbackPanel = CreateSubPanel(panel, "FeedbackPanel", new Vector2(0f, RuntimeFeedbackPanelBottomY), new Vector2(760f, 68f));
            feedbackText = CreatePanelText(feedbackPanel.transform, "FeedbackText", "", 22);
            feedbackPanel.SetActive(false);

            hintPanel = CreateSubPanel(panel, "HintPanel", new Vector2(0f, RuntimeHintPanelBottomY), new Vector2(760f, 64f));
            hintText = CreatePanelText(hintPanel.transform, "HintText", "", 20);
            hintPanel.SetActive(false);

            float answerStartX = -(RuntimeButtonSize.x + RuntimeButtonGap);
            moreButton = CreateButton(panel, "MoreButton", SimpleLocalization.Get("compare_more"), new Vector2(answerStartX, RuntimeAnswerButtonBottomY), () => OnAnswerButtonClicked(ComparisonAnswer.More), out moreButtonText);
            fewerButton = CreateButton(panel, "FewerButton", SimpleLocalization.Get("compare_fewer"), new Vector2(0f, RuntimeAnswerButtonBottomY), () => OnAnswerButtonClicked(ComparisonAnswer.Fewer), out fewerButtonText);
            equalButton = CreateButton(panel, "EqualButton", SimpleLocalization.Get("compare_equal"), new Vector2(RuntimeButtonSize.x + RuntimeButtonGap, RuntimeAnswerButtonBottomY), () => OnAnswerButtonClicked(ComparisonAnswer.Equal), out equalButtonText);

            float actionButtonOffset = (RuntimeButtonSize.x + RuntimeButtonGap) * 0.5f;
            hintButton = CreateButton(panel, "HintButton", SimpleLocalization.Get("btn_hint"), new Vector2(-actionButtonOffset, RuntimeActionButtonBottomY), () => OnHintRequested?.Invoke(), out _);
            cancelButton = CreateButton(panel, "CancelButton", SimpleLocalization.Get("btn_home"), new Vector2(actionButtonOffset, RuntimeActionButtonBottomY), OnCancelClicked, out _);
            listenButton = CreateTopRightButton(panel, "ListenButton", SimpleLocalization.Get("btn_listen"), new Vector2(-36f, -28f), new Vector2(150f, 68f), OnListenClicked, out _);
            nextRoundButton = CreateButton(panel, "NextButton", SimpleLocalization.Get("btn_next"), new Vector2(-actionButtonOffset, RuntimeActionButtonBottomY), OnNextRoundClicked, out _);
            progressButton = CreateButton(panel, "ProgressButton", SimpleLocalization.Get("btn_progress"), new Vector2(actionButtonOffset, RuntimeActionButtonBottomY), OnProgressClicked, out _);
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
                questionText.text = SimpleLocalization.Get("compare_question");
            }

            // Note: We don't show the actual counts to the child - they need to count!
            // But we may use this internally or show "?"

            if (leftGroupCountText != null)
            {
                leftGroupCountText.text = "B\u00ean tr\u00e1i";
            }

            if (rightGroupCountText != null)
            {
                rightGroupCountText.text = "B\u00ean ph\u1ea3i";
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

            if (listenButton != null)
            {
                listenButton.gameObject.SetActive(true);
            }

            if (cancelButton != null)
            {
                cancelButton.gameObject.SetActive(true);
            }

            SimpleAudioManager.EnsureExists().PlayInstruction("instruction_compare_quantity");

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
                moreButtonText.text = NormalizeComparisonButtonLabel(moreLabel, "B\u00ean tr\u00e1i nhi\u1ec1u h\u01a1n");
            }

            if (fewerButtonText != null)
            {
                fewerButtonText.text = NormalizeComparisonButtonLabel(fewerLabel, "B\u00ean tr\u00e1i \u00edt h\u01a1n");
            }

            if (equalButtonText != null)
            {
                equalButtonText.text = NormalizeComparisonButtonLabel(equalLabel, "B\u1eb1ng nhau");
            }
        }

        /// <summary>
        /// Update progress display.
        /// </summary>
        public void UpdateProgress(int current, int total)
        {
            if (progressText != null)
            {
                progressText.text = $"Cau {current}/{total}";
            }
        }

        /// <summary>
        /// Show correct feedback.
        /// </summary>
        public void ShowCorrectFeedback()
        {
            ShowCorrectFeedback(SimpleLocalization.Get("feedback_correct"));
        }

        /// <summary>
        /// Show correct feedback with custom message.
        /// </summary>
        public void ShowCorrectFeedback(string message)
        {
            if (feedbackOverlay != null)
            {
                feedbackOverlay.ShowCorrect(message);
            }
            else
            {
                ShowFeedback(message, Color.green);
            }
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
            ShowIncorrectFeedback(SimpleLocalization.Get("feedback_incorrect"));
        }

        /// <summary>
        /// Show incorrect feedback with custom message.
        /// </summary>
        public void ShowIncorrectFeedback(string message)
        {
            if (feedbackOverlay != null)
            {
                feedbackOverlay.ShowIncorrect(message);
            }
            else
            {
                ShowFeedback(message, Color.red);
            }
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

            activityFinished = true;
            DisableInput();
            SetAnswerButtonsActive(false);
            SetRunningActionButtonsActive(false);

            if (feedbackOverlay != null)
            {
                feedbackOverlay.ShowSuccess(SimpleLocalization.Get("feedback_success"));
            }
            else
            {
                ShowFeedback(message, Color.green);
            }

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

            activityFinished = true;
            DisableInput();
            SetAnswerButtonsActive(false);
            SetRunningActionButtonsActive(false);

            if (feedbackOverlay != null)
            {
                feedbackOverlay.ShowIncorrect(message);
            }
            else
            {
                ShowFeedback(fullMessage, Color.red);
            }

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
            // Group highlight is applied by the AR interaction service when objects are registered.
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
        public void HideFeedback()
        {
            if (feedbackOverlay != null)
            {
                feedbackOverlay.Hide();
            }
            else if (feedbackPanel != null)
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

        private void OnListenClicked()
        {
            SimpleAudioManager.EnsureExists().ReplayLastInstruction();
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
            if (listenButton != null) listenButton.gameObject.SetActive(active);
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

        private static string NormalizeComparisonButtonLabel(string configuredLabel, string vietnameseFallback)
        {
            if (string.IsNullOrWhiteSpace(configuredLabel))
            {
                return vietnameseFallback;
            }

            string normalized = configuredLabel.Trim().ToLowerInvariant();
            return normalized switch
            {
                "more" => vietnameseFallback,
                "fewer" => vietnameseFallback,
                "less" => vietnameseFallback,
                "equal" => vietnameseFallback,
                _ => configuredLabel
            };
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

        private static GameObject CreateTopPanel(Transform parent, string name, Vector2 anchoredPosition, Vector2 size, float alpha)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;

            var image = go.GetComponent<Image>();
            image.color = new Color(0.03f, 0.05f, 0.08f, alpha);
            image.raycastTarget = false;
            return go;
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

        private static Button CreateTopRightButton(Transform parent, string name, string label, Vector2 anchoredPosition, Vector2 size, UnityEngine.Events.UnityAction onClick, out Text labelText)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;
            go.GetComponent<Image>().color = new Color(0.2f, 0.5f, 0.9f, 0.92f);

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
