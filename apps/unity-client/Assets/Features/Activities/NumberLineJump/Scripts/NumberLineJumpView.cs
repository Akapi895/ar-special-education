using Core.Learning.ActivityRunner;
using Core.Learning.Models;
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

        private static readonly Vector2 RuntimeButtonSize = new Vector2(150f, 56f);
        private static readonly Vector2 RuntimeEdgeJumpButtonSize = new Vector2(104f, 180f);
        private const float RuntimeButtonGap = 16f;
        private const float RuntimeActionButtonBottomY = 52f;
        private const float RuntimeJumpButtonBottomY = 124f;
        private const float RuntimeEdgeJumpButtonX = 18f;
        private const float RuntimeEdgeJumpButtonY = -18f;
        private const float RuntimeHintPanelBottomY = 194f;
        private const float RuntimeEquationPanelBottomY = 262f;
        private const float RuntimeFeedbackPanelBottomY = 334f;

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
                SetButtonLabel(leftJumpButton, "<");
            }

            if (rightJumpButton != null)
            {
                rightJumpButton.onClick.AddListener(() => OnJumpRequested?.Invoke(JumpStepDirection.Right));
                SetButtonLabel(rightJumpButton, ">");
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
            var panel = CreateUiPanel(canvas.transform, "NumberLineJumpPanel");
            runtimeUiRoot = panel;

            progressText = CreateTopText(panel, "Progress", "", 24, 24f, new Vector2(620f, 40f));
            targetNumberText = CreateTopText(panel, "TargetNumber", "Jump to the target number", 34, 68f, new Vector2(820f, 58f));
            startNumberText = CreateTopText(panel, "StartNumber", "", 22, 124f, new Vector2(360f, 38f));
            startNumberText.GetComponent<RectTransform>().anchoredPosition = new Vector2(-205f, -124f);
            currentPositionText = CreateTopText(panel, "CurrentPosition", "", 22, 124f, new Vector2(360f, 38f));
            currentPositionText.GetComponent<RectTransform>().anchoredPosition = new Vector2(205f, -124f);

            feedbackPanel = CreateSubPanel(panel, "FeedbackPanel", new Vector2(0f, RuntimeFeedbackPanelBottomY), new Vector2(760f, 72f));
            feedbackText = CreatePanelText(feedbackPanel.transform, "FeedbackText", "", 22);
            feedbackPanel.SetActive(false);

            equationPanel = CreateSubPanel(panel, "EquationPanel", new Vector2(0f, RuntimeEquationPanelBottomY), new Vector2(560f, 54f));
            equationText = CreatePanelText(equationPanel.transform, "EquationText", "", 24);

            hintPanel = CreateSubPanel(panel, "HintPanel", new Vector2(0f, RuntimeHintPanelBottomY), new Vector2(760f, 64f));
            hintText = CreatePanelText(hintPanel.transform, "HintText", "", 20);
            hintPanel.SetActive(false);

            leftJumpButton = CreateEdgeJumpButton(panel, "LeftJumpButton", "<", true, () => OnJumpRequested?.Invoke(JumpStepDirection.Left));
            rightJumpButton = CreateEdgeJumpButton(panel, "RightJumpButton", ">", false, () => OnJumpRequested?.Invoke(JumpStepDirection.Right));

            float jumpActionOffset = (RuntimeButtonSize.x + RuntimeButtonGap) * 0.5f;
            confirmButton = CreateButton(panel, "ConfirmButton", "Confirm", new Vector2(-jumpActionOffset, RuntimeJumpButtonBottomY), () => OnConfirmRequested?.Invoke());
            resetButton = CreateButton(panel, "ResetButton", "Reset", new Vector2(jumpActionOffset, RuntimeJumpButtonBottomY), () => OnResetRequested?.Invoke());

            float actionButtonOffset = (RuntimeButtonSize.x + RuntimeButtonGap) * 0.5f;
            hintButton = CreateButton(panel, "HintButton", "Hint", new Vector2(-actionButtonOffset, RuntimeActionButtonBottomY), () => OnHintRequested?.Invoke());
            cancelButton = CreateButton(panel, "CancelButton", "Cancel", new Vector2(actionButtonOffset, RuntimeActionButtonBottomY), OnCancelClicked);
            nextRoundButton = CreateButton(panel, "NextButton", "Next", new Vector2(-actionButtonOffset, RuntimeActionButtonBottomY), OnNextRoundClicked);
            progressButton = CreateButton(panel, "ProgressButton", "Progress", new Vector2(actionButtonOffset, RuntimeActionButtonBottomY), OnProgressClicked);
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
                startNumberText.text = currentUsesEquationPromptMode ? $"Start on {startNumber}" : $"Start: {startNumber}";
            }

            if (targetNumberText != null)
            {
                targetNumberText.text = currentUsesEquationPromptMode
                    ? currentEquationPrompt
                    : $"Jump from {startNumber} to {targetNumber}";
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

            if (cancelButton != null)
            {
                cancelButton.gameObject.SetActive(true);
            }

            SetJumpControlsActive(true);
            LayoutEdgeJumpButtons();
            SetNavigationButtonsActive(false);

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
                SetButtonLabel(nextRoundButton, GetNextButtonLabel("NumberLineJump"));
                nextRoundButton.gameObject.SetActive(true);
            }
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
            if (feedbackOverlay != null)
            {
                feedbackOverlay.ShowIncorrect(fullMessage);
            }
            else
            {
                ShowFeedback(fullMessage, Color.red);
            }
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

            activityFinished = true;
            DisableInput();
            SetJumpControlsActive(false);
            SetRunningActionButtonsActive(false);

            if (feedbackOverlay != null)
            {
                feedbackOverlay.ShowSuccess("Activity Complete! Progress saved.");
            }
            else
            {
                ShowFeedback(message, Color.green);
            }

            if (nextRoundButton != null)
            {
                SetButtonLabel(nextRoundButton, GetNextButtonLabel("NumberLineJump"));
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
                               $"Attempts: {result.TotalAttempts}\n" +
                               $"Hints Used: {result.HintsUsedCount}";

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
                SetButtonLabel(nextRoundButton, GetNextButtonLabel("NumberLineJump"));
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
            LoadSceneIfAvailable("SC_ActivitySelect");
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
            if (cancelButton != null) cancelButton.gameObject.SetActive(active);
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

            SetButtonLabel(button, isLeft ? "<" : ">");
            Text label = button.GetComponentInChildren<Text>();
            if (label != null)
            {
                label.fontSize = 58;
                label.resizeTextMaxSize = 58;
            }
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
            text.fontSize = 58;
            text.resizeTextMaxSize = 58;
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
                return "Next";
            }

            return ActivityFlowNavigator.TryGetNextActivityId(activityId, out _) ? "Next Activity" : "Finish";
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
