using Core.Learning.ActivityRunner;
using Core.Learning.Models;
using Core.Support.AudioManager;
using Core.Support.FeedbackSystem;
using Core.UI.Components;
using Core.UI.Layout;
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

        private Action<ActivityAnswer> onAnswerSelectedInterface;

        event Action<ActivityAnswer> IActivityView.OnAnswerSelected
        {
            add => onAnswerSelectedInterface += value;
            remove => onAnswerSelectedInterface -= value;
        }

        // Presenter reference
        private IActivityPresenter presenter;

        // State
        private ComparisonAnswer? currentSelectedAnswer;
        private bool activityFinished;

        private static readonly Vector2 RuntimeButtonSize = new Vector2(224f, 84f);
        private static readonly Vector2 RuntimeComparisonButtonSize = new Vector2(160f, 160f);
        private static readonly Vector2 RuntimeFeedbackPanelSize = new Vector2(790f, 128f);
        private static readonly Vector2 RuntimeFeedbackPanelCenter = new Vector2(0f, -150f);
        private const float RuntimeButtonGap = 34f;
        private const float RuntimeActionButtonBottomY = 52f;
        private const float RuntimeAnswerButtonBottomY = 150f;
        private const float RuntimeHintPanelBottomY = 220f;
        private const float RuntimeFeedbackPanelBottomY = 296f;
        private const string RuntimeHomeButtonLabel = "Trang ch\u1ee7";
        private const string RuntimeCompareQuestion = "B\u00ean tr\u00e1i  >  <  =  b\u00ean ph\u1ea3i?";
        private const string MoreSymbol = ">";
        private const string FewerSymbol = "<";
        private const string EqualSymbol = "=";
        private const int RuntimeTopNavButtonFontSize = 20;
        private const int AnswerSymbolFontSize = 62;

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

            UIKidFriendlyStyle.ApplyToTree(transform);
            NormalizeTopNavigationButtons();
            ConfigureFeedbackPanelForChildFocus();
            ApplyComparisonAnswerButtonVisuals();
            UIKidFriendlyStyle.ApplyReadableTextToScene(3, 24);
        }

        /// <summary>
        /// Builds minimal UI at runtime when prefabs are not assigned.
        /// </summary>
        public void BuildRuntimeUi(Canvas canvas)
        {
            var panel = CreateUiPanel(canvas.transform, "CompareQuantityPanel");

            CreateTopPanel(panel, "QuestionHeaderPanel", new Vector2(0f, -34f), new Vector2(720f, 84f), 0.56f);
            CreateTopPanel(panel, "GroupSideHeaderPanel", new Vector2(0f, -112f), new Vector2(640f, 46f), 0.42f);

            progressText = CreateTopText(panel, "Progress", "", 24, 16f, new Vector2(260f, 42f));
            progressText.GetComponent<RectTransform>().anchoredPosition = new Vector2(-500f, -16f);
            questionText = CreateTopText(panel, "QuestionText", RuntimeCompareQuestion, 32, 28f, new Vector2(700f, 74f));

            leftGroupCountText = CreateTopText(panel, "LeftGroupCount", "B\u00ean tr\u00e1i", 28, 104f, new Vector2(320f, 46f));
            leftGroupCountText.GetComponent<RectTransform>().anchoredPosition = new Vector2(-230f, -104f);
            rightGroupCountText = CreateTopText(panel, "RightGroupCount", "B\u00ean ph\u1ea3i", 28, 104f, new Vector2(320f, 46f));
            rightGroupCountText.GetComponent<RectTransform>().anchoredPosition = new Vector2(230f, -104f);

            feedbackPanel = CreateSubPanel(panel, "FeedbackPanel", new Vector2(0f, RuntimeFeedbackPanelBottomY), new Vector2(760f, 68f));
            feedbackText = CreatePanelText(feedbackPanel.transform, "FeedbackText", "", 26);
            ConfigureFeedbackPanelForChildFocus();
            feedbackPanel.SetActive(false);

            hintPanel = CreateSubPanel(panel, "HintPanel", new Vector2(0f, RuntimeHintPanelBottomY), new Vector2(760f, 64f));
            hintText = CreatePanelText(hintPanel.transform, "HintText", "", 24);
            hintPanel.SetActive(false);

            float answerStartX = -(RuntimeComparisonButtonSize.x + RuntimeButtonGap);
            moreButton = CreateButton(panel, "MoreButton", SimpleLocalization.Get("compare_more"), new Vector2(answerStartX, RuntimeAnswerButtonBottomY), () => OnAnswerButtonClicked(ComparisonAnswer.More), out moreButtonText);
            fewerButton = CreateButton(panel, "FewerButton", SimpleLocalization.Get("compare_fewer"), new Vector2(0f, RuntimeAnswerButtonBottomY), () => OnAnswerButtonClicked(ComparisonAnswer.Fewer), out fewerButtonText);
            equalButton = CreateButton(panel, "EqualButton", SimpleLocalization.Get("compare_equal"), new Vector2(RuntimeComparisonButtonSize.x + RuntimeButtonGap, RuntimeAnswerButtonBottomY), () => OnAnswerButtonClicked(ComparisonAnswer.Equal), out equalButtonText);

            float actionButtonOffset = (RuntimeButtonSize.x + RuntimeButtonGap) * 0.5f;
            hintButton = UIActivityNavButtons.CreateHintButton(panel, () => OnHintRequested?.Invoke());
            cancelButton = UIActivityNavButtons.CreateHomeButton(panel, OnCancelClicked);
            listenButton = UIActivityNavButtons.CreateListenButton(panel, OnListenClicked);
            nextRoundButton = CreateButton(panel, "NextButton", SimpleLocalization.Get("btn_next"), new Vector2(-actionButtonOffset, RuntimeActionButtonBottomY), OnNextRoundClicked, out _);
            progressButton = CreateButton(panel, "ProgressButton", SimpleLocalization.Get("btn_progress"), new Vector2(actionButtonOffset, RuntimeActionButtonBottomY), OnProgressClicked, out _);
            nextRoundButton.gameObject.SetActive(false);
            progressButton.gameObject.SetActive(false);
            NormalizeTopNavigationButtons();
            ApplyComparisonAnswerButtonVisuals();
            UIKidFriendlyStyle.ApplyReadableTextToScene(3, 24);
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
        public void ShowQuestion(int leftCount, int rightCount, bool isEquality, CompareQuantityQuestionType questionType = CompareQuantityQuestionType.Standard)
        {
            activityFinished = false;

            if (questionText != null)
            {
                if (questionType == CompareQuantityQuestionType.SymbolCompare)
                {
                    questionText.text = $"{leftCount} ? {rightCount}";
                }
                else
                {
                    questionText.text = RuntimeCompareQuestion;
                }
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

        public void RefreshAnswerButtonVisuals(string moreLabel, string fewerLabel, string equalLabel)
        {
            ApplyComparisonAnswerButtonVisuals();
        }

        /// <summary>
        /// Update progress display.
        /// </summary>
        public void UpdateProgress(int current, int total)
        {
            if (progressText != null)
            {
                progressText.text = $"Câu {current}/{total}";
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

            FeedbackServiceProxy.Instance?.ShowCorrect(message);
            SimpleAudioManager.EnsureExists().PlaySound("correct");

            DisableInput();
            SetAnswerButtonsActive(false);
            SetRunningActionButtonsActive(false);

            if (nextRoundButton != null)
            {
                nextRoundButton.gameObject.SetActive(true);
                CenterNavigationButton(nextRoundButton);
                UIKidFriendlyStyle.PlayFeedback(nextRoundButton, true);
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

            SimpleAudioManager.EnsureExists().PlaySound("incorrect");

            EnableAnswerButtons(true);
            UIKidFriendlyStyle.PlayFeedback(GetSelectedComparisonButton(), false);
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
            string message = $"Hoàn thành bài học!\n" +
                           $"Đúng: {(result.IsCorrect ? "Có" : "Chưa")}\n" +
                           $"Số lần thử: {result.TotalAttempts}\n" +
                           $"Gợi ý đã dùng: {result.HintsUsedCount}\n" +
                           $"Thời gian: {result.TimeSpentSeconds:F1} giây";

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
                CenterNavigationButton(nextRoundButton);
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
                               $"Số lần thử: {result.TotalAttempts}\n" +
                               $"Gợi ý đã dùng: {result.HintsUsedCount}";

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
                CenterNavigationButton(nextRoundButton);
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
            Text target = groupSide == "left" ? leftGroupCountText : rightGroupCountText;
            if (target != null)
            {
                target.color = highlight
                    ? new Color(1f, 0.9f, 0.2f, 1f)
                    : Color.white;
                if (highlight)
                {
                    var rect = target.GetComponent<RectTransform>();
                    if (rect != null)
                    {
                        rect.localScale = Vector3.one * 1.15f;
                    }
                }
                else
                {
                    var rect = target.GetComponent<RectTransform>();
                    if (rect != null)
                    {
                        rect.localScale = Vector3.one;
                    }
                }
            }
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
                ConfigureFeedbackPanelForChildFocus();
                Image image = feedbackPanel.GetComponent<Image>();
                if (image != null)
                {
                    bool positive = color.g >= color.r;
                    image.color = positive
                        ? new Color(0.06f, 0.42f, 0.2f, 0.9f)
                        : new Color(0.5f, 0.28f, 0.04f, 0.9f);
                }

                feedbackText.text = message;
                feedbackText.color = Color.white;
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
            ApplyComparisonAnswerButtonVisuals();
            OnAnswerSelected?.Invoke(answer);
            onAnswerSelectedInterface?.Invoke(new ActivityAnswer { AnswerData = (int)answer });
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
            LoadSceneIfAvailable("SC_MainMenu");
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
            if (cancelButton != null)
            {
                cancelButton.gameObject.SetActive(true);
                cancelButton.interactable = true;
            }
        }

        private void SetNavigationButtonsActive(bool active)
        {
            if (nextRoundButton != null)
            {
                nextRoundButton.gameObject.SetActive(active);
                if (active) CenterNavigationButton(nextRoundButton);
            }
            if (progressButton != null) progressButton.gameObject.SetActive(active);
        }

        /// <summary>
        /// Center the navigation button on screen.
        /// </summary>
        private void CenterNavigationButton(Button button)
        {
            if (button == null) return;
            RectTransform rect = button.GetComponent<RectTransform>();
            if (rect == null) return;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
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

        private void ApplyComparisonAnswerButtonVisuals()
        {
            ConfigureComparisonAnswerButton(
                moreButton,
                moreButtonText,
                MoreSymbol,
                KidButtonPurpose.ComparisonMore,
                currentSelectedAnswer == ComparisonAnswer.More);

            ConfigureComparisonAnswerButton(
                fewerButton,
                fewerButtonText,
                FewerSymbol,
                KidButtonPurpose.ComparisonFewer,
                currentSelectedAnswer == ComparisonAnswer.Fewer);

            ConfigureComparisonAnswerButton(
                equalButton,
                equalButtonText,
                EqualSymbol,
                KidButtonPurpose.ComparisonEqual,
                currentSelectedAnswer == ComparisonAnswer.Equal);
        }

        private static void ConfigureComparisonAnswerButton(
            Button button,
            Text labelText,
            string symbol,
            KidButtonPurpose purpose,
            bool selected)
        {
            if (button == null)
            {
                return;
            }

            RectTransform rect = button.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.sizeDelta = RuntimeComparisonButtonSize;
            }

            UIKidFriendlyStyle.Apply(button, purpose, symbol, AnswerSymbolFontSize, true);
            UIKidFriendlyStyle.SetSelected(button, selected, purpose);

            Text text = labelText != null ? labelText : button.GetComponentInChildren<Text>();
            if (text == null)
            {
                return;
            }

            text.text = symbol;
            text.fontSize = AnswerSymbolFontSize;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 42;
            text.resizeTextMaxSize = AnswerSymbolFontSize;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = new Color(0.16f, 0.12f, 0.08f, 1f);
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Truncate;
        }

        private Button GetSelectedComparisonButton()
        {
            return currentSelectedAnswer switch
            {
                ComparisonAnswer.More => moreButton,
                ComparisonAnswer.Fewer => fewerButton,
                ComparisonAnswer.Equal => equalButton,
                _ => null
            };
        }

        private void ConfigureFeedbackPanelForChildFocus()
        {
            if (feedbackPanel != null)
            {
                RectTransform rect = feedbackPanel.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.anchorMin = new Vector2(0.5f, 0.5f);
                    rect.anchorMax = new Vector2(0.5f, 0.5f);
                    rect.pivot = new Vector2(0.5f, 0.5f);
                    rect.sizeDelta = RuntimeFeedbackPanelSize;
                    rect.anchoredPosition = RuntimeFeedbackPanelCenter;
                }

                Image image = feedbackPanel.GetComponent<Image>();
                if (image != null)
                {
                    image.color = new Color(0.03f, 0.05f, 0.08f, 0.88f);
                }
            }

            if (feedbackText != null)
            {
                feedbackText.fontSize = 34;
                feedbackText.resizeTextForBestFit = true;
                feedbackText.resizeTextMinSize = 22;
                feedbackText.resizeTextMaxSize = 34;
                feedbackText.color = Color.white;
                feedbackText.alignment = TextAnchor.MiddleCenter;
            }
        }

        private static RectTransform CreateUiPanel(Transform parent, string name)
        {
            return UIActivityLayoutHelpers.CreateUiPanel(parent, name);
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
            text.font = UIKidFriendlyStyle.GetSharedFont();
            text.color = Color.white;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

        private static GameObject CreateSubPanel(Transform parent, string name, Vector2 anchoredPosition, Vector2 size)
        {
            return UIActivityLayoutHelpers.CreateSubPanel(parent, name, anchoredPosition, size);
        }

        private static Text CreatePanelText(Transform parent, string name, string content, int fontSize)
        {
            return UIActivityLayoutHelpers.CreatePanelText(parent, name, content, fontSize);
        }

        private static Button CreateButton(Transform parent, string name, string label, Vector2 anchoredPosition, UnityEngine.Events.UnityAction onClick, out Text labelText)
        {
            Button button = UIActivityLayoutHelpers.CreateButton(parent, name, label, anchoredPosition, onClick, RuntimeButtonSize);
            labelText = button.GetComponentInChildren<Text>();
            return button;
        }

        private void NormalizeTopNavigationButtons()
        {
            UIActivityNavButtons.ApplyStandardHomeButton(cancelButton);
        }

        private static void ConfigureTopRightNavigationButton(Button button, string label, Vector2 anchoredPosition, Vector2 size, KidButtonPurpose purpose)
        {
            if (button == null)
            {
                return;
            }

            RectTransform rect = button.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = new Vector2(1f, 1f);
                rect.anchorMax = new Vector2(1f, 1f);
                rect.pivot = new Vector2(1f, 1f);
                rect.sizeDelta = size;
                rect.anchoredPosition = anchoredPosition;
            }

            Text text = button.GetComponentInChildren<Text>();
            if (text != null)
            {
                text.text = label;
                text.fontSize = RuntimeTopNavButtonFontSize;
                text.resizeTextForBestFit = true;
                text.resizeTextMinSize = 12;
                text.resizeTextMaxSize = RuntimeTopNavButtonFontSize;
                text.alignment = TextAnchor.MiddleCenter;
            }

            UIKidFriendlyStyle.Apply(button, purpose, label, RuntimeTopNavButtonFontSize);
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
            UIKidFriendlyStyle.Apply(button, name, label, RuntimeTopNavButtonFontSize);
            return button;
        }

        private static Button CreateTopLeftButton(Transform parent, string name, string label, Vector2 anchoredPosition, Vector2 size, UnityEngine.Events.UnityAction onClick, out Text labelText)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;
            go.GetComponent<Image>().color = new Color(0.2f, 0.5f, 0.9f, 0.92f);

            var button = go.GetComponent<Button>();
            button.onClick.AddListener(onClick);

            labelText = CreateButtonLabel(go.transform, label);
            UIKidFriendlyStyle.Apply(button, name, label, RuntimeTopNavButtonFontSize);
            return button;
        }

        private static Text CreateButtonLabel(Transform parent, string label)
        {
            return UIActivityLayoutHelpers.CreateButtonLabel(parent, label);
        }

        private static void LoadSceneIfAvailable(string sceneName)
        {
            UIActivityLayoutHelpers.LoadSceneIfAvailable(sceneName);
        }
    }
}
