using Core.Learning.ActivityRunner;
using Core.Learning.Models;
using Core.Support.AudioManager;
using Core.UI.Components;
using Core.UI.Localization;
using Features.Activities.NumberLineJump;
using Project.App;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
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
        private Button listenButton;

        [SerializeField]
        private Button cancelButton;

        [SerializeField]
        private Button nextRoundButton;

        [SerializeField]
        private Button progressButton;

        [SerializeField]
        private Core.UI.Components.UIFeedbackOverlay feedbackOverlay;

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
        private bool currentUsesEquationPromptMode;
        private string currentEquationPrompt;
        private Transform runtimeUiRoot;
        private bool activityFinished;

        private static readonly Vector2 RuntimeButtonSize = new Vector2(180f, 72f);
        private static readonly Vector2 RuntimeEdgeJumpButtonSize = new Vector2(142f, 218f);
        private const float RuntimeButtonGap = 16f;
        private const float RuntimeActionButtonBottomY = 52f;
        private const float RuntimeJumpButtonBottomY = 124f;
        private const float RuntimeEdgeJumpButtonX = 26f;
        private const float RuntimeEdgeJumpButtonY = -12f;
        private const float RuntimeHintPanelBottomY = 194f;
        private const float RuntimeEquationPanelBottomY = 262f;
        private const float RuntimeFeedbackPanelBottomY = 334f;
        private const string RuntimeHomeButtonLabel = "Trang ch\u1ee7";
        private const string LeftJumpArrow = "←";
        private const string RightJumpArrow = "→";
        private const int RuntimeTopNavButtonFontSize = 20;

        public bool HasUiReferences => startNumberText != null;

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
            if (leftJumpButton != null)
            {
                leftJumpButton.onClick.AddListener(() => OnJumpRequested?.Invoke(JumpStepDirection.Left));
                SetButtonLabel(leftJumpButton, LeftJumpArrow);
            }

            if (rightJumpButton != null)
            {
                rightJumpButton.onClick.AddListener(() => OnJumpRequested?.Invoke(JumpStepDirection.Right));
                SetButtonLabel(rightJumpButton, RightJumpArrow);
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
            LayoutEdgeJumpButtons();
            UIKidFriendlyStyle.ApplyReadableTextToScene(3, 24);
        }

        /// <summary>
        /// Builds minimal UI at runtime when prefabs are not assigned.
        /// </summary>
        public void BuildRuntimeUi(Canvas canvas)
        {
            var panel = CreateUiPanel(canvas.transform, "NumberLineJumpPanel");
            runtimeUiRoot = panel;

            progressText = CreateTopText(panel, "Progress", "", 26, 24f, new Vector2(640f, 44f));
            targetNumberText = CreateTopText(panel, "TargetNumber", SimpleLocalization.Get("instruction_number_line"), 36, 68f, new Vector2(730f, 64f));
            startNumberText = CreateTopText(panel, "StartNumber", "", 26, 124f, new Vector2(380f, 44f));
            startNumberText.GetComponent<RectTransform>().anchoredPosition = new Vector2(-205f, -124f);
            currentPositionText = CreateTopText(panel, "CurrentPosition", "", 26, 124f, new Vector2(380f, 44f));
            currentPositionText.GetComponent<RectTransform>().anchoredPosition = new Vector2(205f, -124f);

            feedbackPanel = CreateSubPanel(panel, "FeedbackPanel", new Vector2(0f, RuntimeFeedbackPanelBottomY), new Vector2(760f, 72f));
            feedbackText = CreatePanelText(feedbackPanel.transform, "FeedbackText", "", 26);
            feedbackPanel.SetActive(false);

            equationPanel = CreateSubPanel(panel, "EquationPanel", new Vector2(0f, RuntimeEquationPanelBottomY), new Vector2(560f, 54f));
            equationText = CreatePanelText(equationPanel.transform, "EquationText", "", 28);

            hintPanel = CreateSubPanel(panel, "HintPanel", new Vector2(0f, RuntimeHintPanelBottomY), new Vector2(760f, 64f));
            hintText = CreatePanelText(hintPanel.transform, "HintText", "", 24);
            hintPanel.SetActive(false);

            leftJumpButton = CreateEdgeJumpButton(panel, "LeftJumpButton", LeftJumpArrow, true, () => OnJumpRequested?.Invoke(JumpStepDirection.Left));
            rightJumpButton = CreateEdgeJumpButton(panel, "RightJumpButton", RightJumpArrow, false, () => OnJumpRequested?.Invoke(JumpStepDirection.Right));

            float jumpActionOffset = (RuntimeButtonSize.x + RuntimeButtonGap) * 0.5f;
            confirmButton = CreateButton(panel, "ConfirmButton", SimpleLocalization.Get("btn_confirm"), new Vector2(-jumpActionOffset, RuntimeJumpButtonBottomY), () => OnConfirmRequested?.Invoke());
            resetButton = CreateButton(panel, "ResetButton", SimpleLocalization.Get("btn_reset"), new Vector2(jumpActionOffset, RuntimeJumpButtonBottomY), () => OnResetRequested?.Invoke());

            float actionButtonOffset = (RuntimeButtonSize.x + RuntimeButtonGap) * 0.5f;
            hintButton = UIActivityNavButtons.CreateHintButton(panel, () => OnHintRequested?.Invoke());
            cancelButton = UIActivityNavButtons.CreateHomeButton(panel, OnCancelClicked);
            listenButton = UIActivityNavButtons.CreateListenButton(panel, OnListenClicked);
            nextRoundButton = CreateButton(panel, "NextButton", SimpleLocalization.Get("btn_next"), new Vector2(-actionButtonOffset, RuntimeActionButtonBottomY), OnNextRoundClicked);
            progressButton = CreateButton(panel, "ProgressButton", SimpleLocalization.Get("btn_progress"), new Vector2(actionButtonOffset, RuntimeActionButtonBottomY), OnProgressClicked);
            nextRoundButton.gameObject.SetActive(false);
            progressButton.gameObject.SetActive(false);
            NormalizeTopNavigationButtons();
            LayoutEdgeJumpButtons();
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
        /// Show the question with all relevant data.
        /// </summary>
        public void ShowQuestion(int startNumber, int targetNumber, int minNumber, int maxNumber,
            JumpDirection allowedDirection, bool useEquationPromptMode = false, string equationPrompt = null)
        {
            currentAllowedDirection = allowedDirection;
            currentMinNumber = minNumber;
            currentMaxNumber = maxNumber;
            currentPos = startNumber;
            currentUsesEquationPromptMode = useEquationPromptMode;
            currentEquationPrompt = equationPrompt;
            activityFinished = false;

            // Update displays
            if (startNumberText != null)
            {
                startNumberText.text = currentUsesEquationPromptMode ? $"Bắt đầu ở {startNumber}" : $"Bắt đầu: {startNumber}";
            }

            if (targetNumberText != null)
            {
                targetNumberText.text = currentUsesEquationPromptMode
                    ? currentEquationPrompt
                    : SimpleLocalization.Get("numberline_question", startNumber, targetNumber);
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

            if (currentUsesEquationPromptMode && equationText != null)
            {
                equationText.text = currentEquationPrompt;
            }

            // Enable input
            SetJumpInputEnabled(true);

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

            SetJumpControlsActive(true);
            LayoutEdgeJumpButtons();
            SetNavigationButtonsActive(false);

            // Update button states
            UpdateJumpButtonsState(allowedDirection, startNumber, minNumber, maxNumber);

            SimpleAudioManager.EnsureExists().PlayInstruction("instruction_number_line");
            SimpleAudioManager.Instance.PlayNumber(targetNumber);
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
                currentPositionText.text = $"Đang ở: {position}";
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
                progressText.text = $"Câu {current}/{total}";
            }
        }

        /// <summary>
        /// Show correct feedback with equation.
        /// </summary>
        public void ShowCorrectFeedback()
        {
            ShowCorrectFeedback(SimpleLocalization.Get("feedback_correct"), "");
        }

        /// <summary>
        /// Show correct feedback with equation.
        /// </summary>
        public void ShowCorrectFeedback(string message, string finalEquation)
        {
            string fullMessage = string.IsNullOrEmpty(finalEquation) ? message : $"{message}\n{finalEquation}";
            if (feedbackOverlay != null)
            {
                feedbackOverlay.ShowCorrect(fullMessage);
            }
            else
            {
                ShowFeedback(fullMessage, Color.green);
            }
            DisableInput();
            SetJumpControlsActive(false);
            SetRunningActionButtonsActive(false);

            if (nextRoundButton != null)
            {
                string nextLabel = GetNextButtonLabel("NumberLineJump");
                SetButtonLabel(nextRoundButton, nextLabel);
                UIKidFriendlyStyle.Apply(nextRoundButton, KidButtonPurpose.Primary, nextLabel, 28);
                nextRoundButton.gameObject.SetActive(true);
                UIKidFriendlyStyle.PlayFeedback(nextRoundButton, true);
            }
        }

        /// <summary>
        /// Show incorrect feedback with equation.
        /// </summary>
        public void ShowIncorrectFeedback()
        {
            ShowIncorrectFeedback(SimpleLocalization.Get("feedback_incorrect"), "");
        }

        /// <summary>
        /// Show incorrect feedback with equation.
        /// </summary>
        public void ShowIncorrectFeedback(string message, string attemptedEquation)
        {
            string fullMessage = string.IsNullOrEmpty(attemptedEquation) ? message : $"{message}\n{attemptedEquation}";
            if (feedbackOverlay != null)
            {
                feedbackOverlay.ShowIncorrect(fullMessage);
            }
            else
            {
                ShowFeedback(fullMessage, Color.red);
            }
            EnableInput();
            UIKidFriendlyStyle.PlayFeedback(confirmButton, false);
        }

        /// <summary>
        /// Show overshoot feedback.
        /// </summary>
        public void ShowOvershootFeedback(int currentPosition, int targetPosition)
        {
            ShowFeedback(SimpleLocalization.Get("numberline_overshoot", currentPosition, targetPosition), Color.red);
        }

        /// <summary>
        /// Show boundary hit feedback.
        /// </summary>
        public void ShowBoundaryHit(int currentPosition)
        {
            ShowFeedback($"Đã đến cạnh trục số tại {currentPosition}.", Color.yellow);
            Debug.Log("[NumberLineJumpView] Play bump animation at boundary");

            if (currentPosition <= currentMinNumber)
            {
                UIKidFriendlyStyle.PlayFeedback(leftJumpButton, false);
            }
            else if (currentPosition >= currentMaxNumber)
            {
                UIKidFriendlyStyle.PlayFeedback(rightJumpButton, false);
            }
        }

        /// <summary>
        /// Show max jumps exceeded feedback.
        /// </summary>
        public void ShowMaxJumpsExceeded()
        {
            ShowFeedback("Con đã nhảy quá nhiều bước. Bấm Làm lại để thử tiếp.", Color.red);
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
            string directionText = direction == JumpStepDirection.Right ? "phải" : "trái";
            Debug.Log($"[NumberLineJumpView] Cannot jump {directionText} - not allowed for this question");

            UIKidFriendlyStyle.PlayFeedback(
                direction == JumpStepDirection.Right ? rightJumpButton : leftJumpButton,
                false);
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
            string message = $"Hoàn thành bài học!\n" +
                           $"Đúng: {(result.IsCorrect ? "Có" : "Chưa")}\n" +
                           $"Số lần thử: {result.TotalAttempts}\n" +
                           $"Gợi ý đã dùng: {result.HintsUsedCount}\n" +
                           $"Thời gian: {result.TimeSpentSeconds:F1} giây";

            activityFinished = true;
            DisableInput();
            SetJumpControlsActive(false);
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
                string nextLabel = GetNextButtonLabel("NumberLineJump");
                SetButtonLabel(nextRoundButton, nextLabel);
                UIKidFriendlyStyle.Apply(nextRoundButton, KidButtonPurpose.Primary, nextLabel, 28);
                nextRoundButton.gameObject.SetActive(ActivityFlowNavigator.TryGetNextActivityId("NumberLineJump", out _));
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
            SetJumpControlsActive(false);
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
                string nextLabel = GetNextButtonLabel("NumberLineJump");
                SetButtonLabel(nextRoundButton, nextLabel);
                UIKidFriendlyStyle.Apply(nextRoundButton, KidButtonPurpose.Primary, nextLabel, 28);
                nextRoundButton.gameObject.SetActive(ActivityFlowNavigator.TryGetNextActivityId("NumberLineJump", out _));
            }

            if (progressButton != null)
            {
                progressButton.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// Highlight a specific number tile.
        /// </summary>
        public void HighlightTile(int number, bool highlight)
        {
            // Tile highlight is applied by the AR interaction service when objects are registered.
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
                cancelButton.interactable = true;
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
                bool notAtMin = currentPosition > minNumber;
                leftJumpButton.interactable = notAtMin;
            }

            if (rightJumpButton != null)
            {
                bool notAtMax = currentPosition < maxNumber;
                rightJumpButton.interactable = notAtMax;
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
        /// Handle next round button click.
        /// </summary>
        private void OnNextRoundClicked()
        {
            Debug.Log("[NumberLineJumpView] Next round / Finish clicked");

            if (activityFinished || presenter?.GetState() == ActivityState.Failed)
            {
                if (!ActivityFlowNavigator.LoadNextActivity("NumberLineJump"))
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

                if (!hasMoreRounds && !ActivityFlowNavigator.LoadNextActivity("NumberLineJump"))
                {
                    ActivityFlowNavigator.LoadProgressDashboard();
                }
            }
        }

        private void OnProgressClicked()
        {
            Debug.Log("[NumberLineJumpView] Progress clicked");
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

        /// <summary>
        /// Called when a tile is tapped (from AR interaction).
        /// </summary>
        public void NotifyTileTapped(int tileNumber)
        {
            OnTileTapped?.Invoke(tileNumber);
        }

        private void SetJumpControlsActive(bool active)
        {
            if (leftJumpButton != null) leftJumpButton.gameObject.SetActive(active);
            if (rightJumpButton != null) rightJumpButton.gameObject.SetActive(active);
            if (confirmButton != null) confirmButton.gameObject.SetActive(active);
            if (resetButton != null) resetButton.gameObject.SetActive(active);
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
            if (nextRoundButton != null) nextRoundButton.gameObject.SetActive(active);
            if (progressButton != null) progressButton.gameObject.SetActive(active);
        }

        private void LayoutEdgeJumpButtons()
        {
            LayoutEdgeJumpButton(leftJumpButton, true);
            LayoutEdgeJumpButton(rightJumpButton, false);
        }

        private static void LayoutEdgeJumpButton(Button button, bool isLeft)
        {
            if (button == null)
            {
                return;
            }

            RectTransform rect = button.GetComponent<RectTransform>();
            if (rect == null)
            {
                return;
            }

            rect.anchorMin = new Vector2(isLeft ? 0f : 1f, 0.5f);
            rect.anchorMax = rect.anchorMin;
            rect.pivot = new Vector2(isLeft ? 0f : 1f, 0.5f);
            rect.sizeDelta = RuntimeEdgeJumpButtonSize;
            rect.anchoredPosition = new Vector2(isLeft ? RuntimeEdgeJumpButtonX : -RuntimeEdgeJumpButtonX, RuntimeEdgeJumpButtonY);

            string arrow = isLeft ? LeftJumpArrow : RightJumpArrow;
            SetButtonLabel(button, arrow);
            Text label = button.GetComponentInChildren<Text>();
            if (label != null)
            {
                label.fontSize = 92;
                label.resizeTextMinSize = 58;
                label.resizeTextMaxSize = 92;
                label.fontStyle = FontStyle.Bold;
            }

            UIKidFriendlyStyle.Apply(
                button,
                isLeft ? KidButtonPurpose.ComparisonFewer : KidButtonPurpose.ComparisonMore,
                arrow,
                92);
            EmphasizeEdgeArrow(button);
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

        private static Button CreateButton(Transform parent, string name, string label, Vector2 anchoredPosition, UnityEngine.Events.UnityAction onClick)
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

            CreateButtonLabel(go.transform, label);
            UIKidFriendlyStyle.Apply(button, name, label, 26);
            return button;
        }

        private void NormalizeTopNavigationButtons()
        {
            // Buttons are already configured correctly by UIActivityNavButtons
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

        private static Button CreateTopRightButton(Transform parent, string name, string label, Vector2 anchoredPosition, Vector2 size, UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;
            go.GetComponent<Image>().color = name.Contains("Listen")
                ? new Color(0.2f, 0.5f, 0.9f, 0.9f)
                : new Color(0.85f, 0.35f, 0.35f, 0.9f);

            var button = go.GetComponent<Button>();
            button.onClick.AddListener(onClick);

            Text text = CreateButtonLabel(go.transform, label);
            text.fontSize = RuntimeTopNavButtonFontSize;
            text.resizeTextMaxSize = RuntimeTopNavButtonFontSize;
            UIKidFriendlyStyle.Apply(button, name, label, RuntimeTopNavButtonFontSize);
            return button;
        }

        private static Button CreateTopLeftButton(Transform parent, string name, string label, Vector2 anchoredPosition, Vector2 size, UnityEngine.Events.UnityAction onClick)
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

            Text text = CreateButtonLabel(go.transform, label);
            text.fontSize = RuntimeTopNavButtonFontSize;
            text.resizeTextMaxSize = RuntimeTopNavButtonFontSize;
            UIKidFriendlyStyle.Apply(button, name, label, RuntimeTopNavButtonFontSize);
            return button;
        }

        private static Button CreateEdgeJumpButton(Transform parent, string name, string label, bool isLeft, UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(isLeft ? 0f : 1f, 0.5f);
            rect.anchorMax = rect.anchorMin;
            rect.pivot = new Vector2(isLeft ? 0f : 1f, 0.5f);
            rect.sizeDelta = RuntimeEdgeJumpButtonSize;
            rect.anchoredPosition = new Vector2(isLeft ? RuntimeEdgeJumpButtonX : -RuntimeEdgeJumpButtonX, RuntimeEdgeJumpButtonY);
            go.GetComponent<Image>().color = new Color(0.08f, 0.32f, 0.72f, 0.88f);

            var button = go.GetComponent<Button>();
            button.onClick.AddListener(onClick);

            Text text = CreateButtonLabel(go.transform, label);
            text.fontSize = 92;
            text.resizeTextMinSize = 58;
            text.resizeTextMaxSize = 92;
            text.fontStyle = FontStyle.Bold;
            UIKidFriendlyStyle.Apply(
                button,
                isLeft ? KidButtonPurpose.ComparisonFewer : KidButtonPurpose.ComparisonMore,
                label,
                92);
            EmphasizeEdgeArrow(button);
            return button;
        }

        private static void EmphasizeEdgeArrow(Button button)
        {
            Text text = button != null ? button.GetComponentInChildren<Text>(true) : null;
            if (text == null)
            {
                return;
            }

            text.fontSize = 92;
            text.resizeTextMinSize = 58;
            text.resizeTextMaxSize = 92;
            text.fontStyle = FontStyle.Bold;
            text.color = new Color(0.12f, 0.08f, 0.04f, 1f);

            Outline outline = text.GetComponent<Outline>();
            if (outline == null)
            {
                outline = text.gameObject.AddComponent<Outline>();
            }

            outline.effectColor = new Color(1f, 1f, 1f, 0.85f);
            outline.effectDistance = new Vector2(3f, -3f);
            outline.useGraphicAlpha = true;

            Shadow shadow = text.GetComponent<Shadow>();
            if (shadow == null)
            {
                shadow = text.gameObject.AddComponent<Shadow>();
            }

            shadow.effectColor = new Color(0.2f, 0.14f, 0.05f, 0.28f);
            shadow.effectDistance = new Vector2(0f, -5f);
            shadow.useGraphicAlpha = true;
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

        private static void SetButtonLabel(Button button, string label)
        {
            if (button == null)
            {
                return;
            }

            Text text = button.GetComponentInChildren<Text>();
            if (text != null)
            {
                text.text = label;
            }
        }

        private string GetNextButtonLabel(string activityId)
        {
            if (presenter != null && presenter.HasMoreRounds())
            {
                return SimpleLocalization.Get("btn_next");
            }

            return ActivityFlowNavigator.TryGetNextActivityId(activityId, out _) ? "Bài tiếp" : "Hoàn thành";
        }

        private static void LoadSceneIfAvailable(string sceneName)
        {
            if (Application.CanStreamedLevelBeLoaded(sceneName))
            {
                SceneManager.LoadScene(sceneName);
            }
            else
            {
                Debug.LogWarning($"[NumberLineJumpView] Scene '{sceneName}' is not available in Build Settings.");
            }
        }
    }
}
