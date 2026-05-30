using Core.Learning.ActivityRunner;
using Core.Learning.Models;
using Core.Support.AudioManager;
using Core.UI.Components;
using Core.UI.Layout;
using Core.UI.Localization;
using Project.App;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#if ENABLE_INPUT_SYSTEM
using UnityInputSystem = UnityEngine.InputSystem;
#endif

namespace Features.Activities.QuantityMatch
{
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
        private Button listenButton;

        [SerializeField]
        private Button cancelButton;

        [SerializeField]
        private Button nextRoundButton;

        [SerializeField]
        private Button progressButton;

        [SerializeField]
        private UIFeedbackOverlay feedbackOverlay;

        [Header("Number Input Mode")]
        [SerializeField]
        private GameObject numberInputPanel;

        [SerializeField]
        private Text numberInputText;

        [SerializeField]
        private Text numberInputPromptText;

        [SerializeField]
        private Button[] digitButtons;

        [SerializeField]
        private Button clearNumberButton;

        [SerializeField]
        private Button submitNumberButton;

        [SerializeField]
        private Button decrementNumberButton;

        [SerializeField]
        private Button incrementNumberButton;

        [Header("Navigation")]
        [SerializeField]
        private string homeSceneName = "SC_MainMenu";

        [Header("Group Selection UI")]
        [SerializeField]
        private GameObject[] groupSelectionButtons;

        // Events from IActivityView
        public event Action OnHintRequested;
        public event Action OnCancelRequested;

        event Action<ActivityAnswer> IActivityView.OnAnswerSelected
        {
            add { }
            remove { }
        }

        // Events specific to IQuantityMatchView
        public event Action<int, int> OnGroupSelected;
        public event Action<int> OnNumberAnswerSubmitted;

        // Presenter reference
        private IActivityPresenter presenter;

        // State
        private int currentTargetNumber;
        private int currentNumberOfGroups;
        private bool currentUsesNumberInputMode;
        private string currentNumberInput = string.Empty;

        private Button[] runtimeGroupButtons;
        private Transform runtimeUiRoot;
        private Canvas numberInputOverlayCanvas;
        private bool activityFinished;
        private bool numberInputButtonsRegistered;
        private bool numberInputControlsInteractable;
        private Button lastAnswerButton;
        private Text devKeyboardHintText;

        private static readonly Vector2 RuntimeButtonSize = new Vector2(170f, 58f);
        private static readonly Vector2 RuntimeDigitButtonSize = new Vector2(108f, 70f);
        private static readonly Vector2 RuntimeNumberInputPanelSize = new Vector2(900f, 330f);
        private const float RuntimeButtonGap = 28f;
        private const float RuntimeDigitButtonGap = 12f;
        private const float RuntimeActionButtonBottomY = 112f;
        private const float RuntimeGroupButtonBottomY = 238f;
        private const float RuntimeHintPanelBottomY = 374f;
        private const float RuntimeFeedbackPanelBottomY = 450f;
        private const float RuntimeNumberInputPanelBottomY = 190f;
        private const int MaxNumberInputLength = 2;
        private const int NumberChoiceMin = 1;
        private const int NumberChoiceMax = 10;
        private const string NumberQuestionTitle = "Con \u0111\u1ebfm \u0111\u01b0\u1ee3c bao nhi\u00eau con?";
        private const string NumberInputPrompt = "Ch\u1ecdn s\u1ed1 con v\u1eadt con \u0111\u1ebfm \u0111\u01b0\u1ee3c";
        private const string NumberInputEmptyValue = "?";
        private const string NumberInputClearLabel = "X\u00f3a";
        private const string NumberInputSubmitLabel = "Tr\u1ea3 l\u1eddi";
        private const string RuntimeHomeButtonLabel = "Trang ch\u1ee7";
        private const int RuntimeTopNavButtonFontSize = 20;
        private const string DEV_KEYBOARD_HINT = "Nhấn phím số để nhập nhanh";

        public bool HasUiReferences => targetNumberText != null;

        private void Awake()
        {
            CacheRuntimeUiRoot();

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

            if (progressButton != null)
            {
                progressButton.gameObject.SetActive(false);
            }

            if (numberInputPanel != null)
            {
                numberInputPanel.SetActive(false);
            }

            // Setup button listeners
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
            RegisterNumberInputButtons();
            UIKidFriendlyStyle.ApplyReadableTextToScene(3, 24);
        }

        private void ShowDevKeyboardHint()
        {
            if (devKeyboardHintText == null && runtimeUiRoot != null)
            {
                Transform existing = runtimeUiRoot.Find("DevKeyboardHint");
                if (existing != null)
                {
                    devKeyboardHintText = existing.GetComponent<Text>();
                }
                else
                {
                    var go = new GameObject("DevKeyboardHint", typeof(RectTransform), typeof(Text));
                    go.transform.SetParent(runtimeUiRoot);
                    devKeyboardHintText = go.GetComponent<Text>();
                    devKeyboardHintText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                    devKeyboardHintText.fontSize = 16;
                    devKeyboardHintText.color = new Color(0.2f, 0.8f, 0.2f, 1f);
                    devKeyboardHintText.alignment = TextAnchor.LowerRight;
                    devKeyboardHintText.raycastTarget = false;
                    devKeyboardHintText.resizeTextForBestFit = true;
                    devKeyboardHintText.resizeTextMinSize = 12;
                    devKeyboardHintText.resizeTextMaxSize = 18;
                    var rect = go.GetComponent<RectTransform>();
                    rect.anchorMin = new Vector2(0f, 0f);
                    rect.anchorMax = new Vector2(1f, 0f);
                    rect.pivot = new Vector2(0.5f, 0f);
                    rect.sizeDelta = new Vector2(-40f, 30f);
                    rect.anchoredPosition = new Vector2(0f, 10f);
                }
            }

            if (devKeyboardHintText != null)
            {
                devKeyboardHintText.text = DEV_KEYBOARD_HINT;
                devKeyboardHintText.gameObject.SetActive(true);
            }
        }

        private void HideDevKeyboardHint()
        {
            if (devKeyboardHintText != null)
            {
                devKeyboardHintText.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Builds minimal UI at runtime when prefabs are not assigned.
        /// </summary>
        public void BuildRuntimeUi(Canvas canvas)
        {
            var panel = CreateUiPanel(canvas.transform, "QuantityMatchPanel");
            runtimeUiRoot = panel;

            // Setup Camera background settings for a bright sky-blue feel in editor/standalone
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                mainCam.clearFlags = CameraClearFlags.SolidColor;
                mainCam.backgroundColor = new Color(0.65f, 0.88f, 0.98f, 1f); // bright sky-blue
            }

            CreateTopHeaderPanel(panel, "QuestionHeaderPanel", 24f, new Vector2(700f, 88f));
            targetNumberText = CreateTopText(panel, "TargetNumber", SimpleLocalization.Get("quantity_choose_group", "?"), 34, 30f, new Vector2(660f, 78f));
            progressText = CreateTopLeftText(panel, "Progress", "", 24, new Vector2(240f, -40f), new Vector2(300f, 60f));

            feedbackPanel = CreateSubPanel(panel, "FeedbackPanel", new Vector2(0, RuntimeFeedbackPanelBottomY), true);
            feedbackText = CreatePanelText(feedbackPanel.transform, "FeedbackText", "", 26);
            feedbackPanel.SetActive(false);

            hintPanel = CreateSubPanel(panel, "HintPanel", new Vector2(0, RuntimeHintPanelBottomY), true);
            hintText = CreatePanelText(hintPanel.transform, "HintText", "", 22);
            hintPanel.SetActive(false);

            // Hint button at top-left
            hintButton = UIActivityNavButtons.CreateHintButton(panel, () => OnHintRequested?.Invoke());

            // Navigation buttons using shared utility
            cancelButton = UIActivityNavButtons.CreateHomeButton(panel, OnCancelClicked);
            listenButton = UIActivityNavButtons.CreateListenButton(panel, OnListenClicked);

            // Centered bottom buttons for transitioning (hidden by default)
            nextRoundButton = CreateSizedBottomButton(panel, "NextButton", "▶ Tiếp tục", new Vector2(0f, RuntimeActionButtonBottomY), new Vector2(240f, 100f), OnNextRoundClicked, 28);
            progressButton = CreateSizedBottomButton(panel, "ProgressButton", "Tiến độ", new Vector2(0f, RuntimeActionButtonBottomY), new Vector2(200f, 100f), OnProgressClicked, 24);
            nextRoundButton.gameObject.SetActive(false);
            progressButton.gameObject.SetActive(false);

            CreateNumberInputUi(panel);
            RegisterNumberInputButtons();
            NormalizeTopNavigationButtons();
            SetNumberInputPanelActive(false);
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
        /// Show the question with target number and group count.
        /// </summary>
        public void ShowQuestion(int targetNumber, int numberOfGroups, bool useNumberInputMode = false)
        {
            currentTargetNumber = targetNumber;
            currentNumberOfGroups = numberOfGroups;
            currentUsesNumberInputMode = useNumberInputMode;
            activityFinished = false;
            EnsureRuntimeGroupButtons(numberOfGroups);

            UpdateTargetNumber(targetNumber);
            HideFeedback();
            HideHint();
            if (currentUsesNumberInputMode)
            {
                SetRuntimeGroupButtonsActive(false);
                EnsureFriendlyNumberInputUi();
                ResetNumberInput();
                SetNumberInputPanelActive(true);
            }
            else
            {
                SetRuntimeGroupButtonsActive(!UseSimulationCircleSelectionMode());
                SetNumberInputPanelActive(false);
            }

            if (nextRoundButton != null)
            {
                nextRoundButton.gameObject.SetActive(false);
            }

            if (progressButton != null)
            {
                progressButton.gameObject.SetActive(false);
            }

            if (hintButton != null)
            {
                hintButton.gameObject.SetActive(!currentUsesNumberInputMode);
            }

            if (listenButton != null)
            {
                listenButton.gameObject.SetActive(true);
            }

            if (cancelButton != null)
            {
                cancelButton.gameObject.SetActive(true);
            }

            EnableInput();
        }

        /// <summary>
        /// Update the displayed target number.
        /// </summary>
        public void UpdateTargetNumber(int targetNumber)
        {
            if (targetNumberText != null)
            {
                targetNumberText.text = currentUsesNumberInputMode
                    ? NumberQuestionTitle
                    : SimpleLocalization.Get("quantity_choose_group", targetNumber);
            }

            SimpleAudioManager.EnsureExists().PlayInstruction(currentUsesNumberInputMode
                ? "instruction_quantity_count"
                : "instruction_quantity_match");
            SimpleAudioManager.Instance.PlayNumber(targetNumber);
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
            ShowCorrectFeedback("Chính xác! Con giỏi quá!");
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
            SetRuntimeGroupButtonsActive(false);
            SetNumberInputPanelActive(false);
            SetRunningActionButtonsActive(false);
            if (nextRoundButton != null)
            {
                ShowContinueButton();
                UIKidFriendlyStyle.PlayFeedback(nextRoundButton, true);
            }
            else if (presenter != null && presenter.HasMoreRounds())
            {
                UIKidFriendlyStyle.PlayFeedback(lastAnswerButton, true);
                CancelInvoke(nameof(AutoContinueToNextRound));
                Invoke(nameof(AutoContinueToNextRound), 2.0f);
            }
            else
            {
                if (nextRoundButton != null)
                {
                    SetButtonLabel(nextRoundButton, "▶ Tiếp tục");
                    var rect = nextRoundButton.GetComponent<RectTransform>();
                    rect.anchoredPosition = new Vector2(0f, RuntimeActionButtonBottomY);
                    rect.sizeDelta = new Vector2(240f, 100f);
                    nextRoundButton.gameObject.SetActive(true);
                    UIKidFriendlyStyle.PlayFeedback(nextRoundButton, true);
                }
            }
        }

        private void AutoContinueToNextRound()
        {
            if (presenter != null && presenter.GetState() == ActivityState.Completed)
            {
                presenter.ContinueToNextRound();
            }
        }

        private void ShowContinueButton()
        {
            if (nextRoundButton == null)
            {
                return;
            }

            string label = presenter != null && presenter.HasMoreRounds()
                ? "Tiếp tục"
                : "Học tiếp";
            SetButtonLabel(nextRoundButton, label);
            var rect = nextRoundButton.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(0f, RuntimeActionButtonBottomY);
            rect.sizeDelta = new Vector2(240f, 100f);
            nextRoundButton.gameObject.SetActive(true);
        }

        /// <summary>
        /// Show incorrect feedback.
        /// </summary>
        public void ShowIncorrectFeedback()
        {
            ShowIncorrectFeedback("Chưa đúng rồi. Thử lại nhé!");
        }

        /// <summary>
        /// Show incorrect feedback with custom message.
        /// </summary>
        public void ShowIncorrectFeedback(string message)
        {
            DisableInput();

            if (feedbackOverlay != null)
            {
                feedbackOverlay.ShowIncorrect(message);
            }
            else
            {
                ShowFeedback(message, new Color(1.0f, 0.5f, 0f)); // orange
            }

            StartCoroutine(ShakeUiCoroutine());
            UIKidFriendlyStyle.PlayFeedback(lastAnswerButton, false);

            CancelInvoke(nameof(HideFeedbackAndRetry));
            Invoke(nameof(HideFeedbackAndRetry), 1.5f);
        }

        private void HideFeedbackAndRetry()
        {
            HideFeedback();
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
            activityFinished = true;
            DisableInput();
            SetRuntimeGroupButtonsInteractable(false);
            SetRuntimeGroupButtonsActive(false);
            SetNumberInputPanelActive(false);
            SetRunningActionButtonsActive(false);

            if (feedbackOverlay != null)
            {
                feedbackOverlay.ShowSuccess("Hoàn thành! Con làm tốt lắm!");
            }
            else
            {
                ShowFeedback("Hoàn thành! Con làm tốt lắm!", Color.green);
            }

            if (nextRoundButton != null)
            {
                SetButtonLabel(nextRoundButton, "▶ Học tiếp");
                var rect = nextRoundButton.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(0f, RuntimeActionButtonBottomY);
                rect.sizeDelta = new Vector2(240f, 100f);
                var text = nextRoundButton.GetComponentInChildren<Text>();
                if (text != null) text.fontSize = 28;

                nextRoundButton.gameObject.SetActive(true);
            }

            if (progressButton != null)
            {
                progressButton.gameObject.SetActive(false); // progress button removed per specs
            }
        }

        /// <summary>
        /// Show activity failure message.
        /// </summary>
        public void ShowActivityFailed(string message, ActivityResult result)
        {
            activityFinished = true;
            DisableInput();
            SetRuntimeGroupButtonsInteractable(false);
            SetRuntimeGroupButtonsActive(false);
            SetNumberInputPanelActive(false);
            SetRunningActionButtonsActive(false);

            if (feedbackOverlay != null)
            {
                feedbackOverlay.ShowIncorrect("Cố lên nhé! Thử lại sau nhé!");
            }
            else
            {
                ShowFeedback("Cố lên nhé! Thử lại sau nhé!", Color.red);
            }

            if (nextRoundButton != null)
            {
                SetButtonLabel(nextRoundButton, "▶ Học tiếp");
                var rect = nextRoundButton.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(0f, RuntimeActionButtonBottomY);
                rect.sizeDelta = new Vector2(240f, 100f);
                var text = nextRoundButton.GetComponentInChildren<Text>();
                if (text != null) text.fontSize = 28;

                nextRoundButton.gameObject.SetActive(true);
            }

            if (progressButton != null)
            {
                progressButton.gameObject.SetActive(false); // progress button removed per specs
            }
        }

        /// <summary>
        /// Highlight a specific group.
        /// </summary>
        public void HighlightGroup(int groupIndex, bool highlight)
        {
            // Group highlight is applied by the AR interaction service when objects are registered.
            Debug.Log($"[QuantityMatchView] Highlight group {groupIndex}: {highlight}");
        }

        public void ShowCountingFeedback(int groupIndex, int countedSoFar, int groupObjectCount)
        {
            int displayGroup = groupIndex + 1;
            string message = groupObjectCount > 0
                ? $"Nhom {displayGroup}: dem {countedSoFar}/{groupObjectCount}"
                : $"Nhom {displayGroup}: dem {countedSoFar}";

            ShowFeedback(message, new Color(0.1f, 0.85f, 1f));
            CancelInvoke(nameof(HideFeedback));
            Invoke(nameof(HideFeedback), 1.0f);
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

            SetRuntimeGroupButtonsInteractable(true);
            SetNumberInputInteractable(true);
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

            SetRuntimeGroupButtonsInteractable(false);
            SetNumberInputInteractable(false);
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
            Debug.Log("[QuantityMatchView] Next round / Finish clicked");

            if (activityFinished || presenter?.GetState() == ActivityState.Failed)
            {
                if (!ActivityFlowNavigator.LoadNextActivity("QuantityMatch"))
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

                if (!hasMoreRounds && !ActivityFlowNavigator.LoadNextActivity("QuantityMatch"))
                {
                    ActivityFlowNavigator.LoadProgressDashboard();
                }
            }
        }

        private void OnProgressClicked()
        {
            Debug.Log("[QuantityMatchView] Progress clicked");
            ActivityFlowNavigator.LoadProgressDashboard();
        }

        private void OnCancelClicked()
        {
            OnCancelRequested?.Invoke();
            LoadSceneIfAvailable(homeSceneName);
        }

        private void OnListenClicked()
        {
            SimpleAudioManager.EnsureExists().ReplayLastInstruction();
            SimpleAudioManager.Instance.PlayNumber(currentTargetNumber);
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
            if (!currentUsesNumberInputMode && groupIndex >= 0 && groupIndex < currentNumberOfGroups)
            {
                lastAnswerButton = runtimeGroupButtons != null && groupIndex < runtimeGroupButtons.Length
                    ? runtimeGroupButtons[groupIndex]
                    : null;
                NotifyGroupSelected(groupIndex, 0);  // Count will be filled by presenter
            }
        }

        private void CacheRuntimeUiRoot()
        {
            if (runtimeUiRoot != null)
            {
                return;
            }

            if (targetNumberText != null && targetNumberText.transform.parent != null)
            {
                runtimeUiRoot = targetNumberText.transform.parent;
                return;
            }

            if (numberInputPanel != null && numberInputPanel.transform.parent != null)
            {
                runtimeUiRoot = numberInputPanel.transform.parent;
                return;
            }

            Canvas canvas = GetComponentInChildren<Canvas>(true);
            if (canvas != null)
            {
                runtimeUiRoot = canvas.transform;
            }
        }

        private Transform ResolveRuntimeUiRoot()
        {
            CacheRuntimeUiRoot();
            return runtimeUiRoot;
        }

        private void EnsureRuntimeGroupButtons(int numberOfGroups)
        {
            if (runtimeUiRoot == null || numberOfGroups <= 0)
            {
                return;
            }

            if (runtimeGroupButtons != null && runtimeGroupButtons.Length == numberOfGroups)
            {
                return;
            }

            if (runtimeGroupButtons != null)
            {
                foreach (Button button in runtimeGroupButtons)
                {
                    if (button != null)
                    {
                        Destroy(button.gameObject);
                    }
                }
            }

            runtimeGroupButtons = new Button[numberOfGroups];
            float rowWidth = RuntimeButtonSize.x * numberOfGroups + RuntimeButtonGap * (numberOfGroups - 1);
            float startX = -rowWidth * 0.5f + RuntimeButtonSize.x * 0.5f;

            for (int i = 0; i < numberOfGroups; i++)
            {
                int groupIndex = i;
                float x = startX + i * (RuntimeButtonSize.x + RuntimeButtonGap);
                Button button = CreateSizedBottomButton(
                    runtimeUiRoot,
                    $"GroupButton_{i + 1}",
                    $"Nhom {i + 1}",
                    new Vector2(x, RuntimeGroupButtonBottomY),
                    RuntimeButtonSize,
                    () => OnGroupButtonClicked(groupIndex),
                    22);

                button.gameObject.SetActive(false);
                runtimeGroupButtons[i] = button;
            }
        }

        private void SetRuntimeGroupButtonsActive(bool active)
        {
            if (groupSelectionButtons != null)
            {
                foreach (GameObject groupButton in groupSelectionButtons)
                {
                    if (groupButton != null)
                    {
                        groupButton.SetActive(active);
                    }
                }
            }

            if (runtimeGroupButtons == null)
            {
                return;
            }

            foreach (Button button in runtimeGroupButtons)
            {
                if (button != null)
                {
                    button.gameObject.SetActive(active);
                }
            }
        }

        private void SetRuntimeGroupButtonsInteractable(bool interactable)
        {
            if (groupSelectionButtons != null)
            {
                foreach (GameObject groupButton in groupSelectionButtons)
                {
                    Button button = groupButton != null ? groupButton.GetComponent<Button>() : null;
                    if (button != null)
                    {
                        button.interactable = interactable;
                    }
                }
            }

            if (runtimeGroupButtons == null)
            {
                return;
            }

            foreach (Button button in runtimeGroupButtons)
            {
                if (button != null)
                {
                    button.interactable = interactable;
                }
            }
        }

        private void SetRunningActionButtonsActive(bool active)
        {
            if (hintButton != null)
            {
                hintButton.gameObject.SetActive(active);
            }

            if (listenButton != null)
            {
                listenButton.gameObject.SetActive(active);
            }

            if (cancelButton != null)
            {
                cancelButton.gameObject.SetActive(true);
                cancelButton.interactable = true;
            }
        }

        private static bool UseSimulationCircleSelectionMode()
        {
            return Application.isEditor && !Application.isMobilePlatform;
        }

        private void RegisterNumberInputButtons()
        {
            bool registeredAny = false;

            if (digitButtons != null)
            {
                for (int i = 0; i < digitButtons.Length; i++)
                {
                    int answer = NumberChoiceMin + i;
                    if (answer > NumberChoiceMax)
                    {
                        continue;
                    }

                    if (digitButtons[i] != null)
                    {
                        digitButtons[i].onClick.RemoveAllListeners();
                        digitButtons[i].onClick.AddListener(() => ChooseNumberAnswer(answer));
                        registeredAny = true;
                    }
                }
            }

            if (clearNumberButton != null)
            {
                clearNumberButton.onClick.RemoveAllListeners();
                clearNumberButton.onClick.AddListener(ResetNumberInput);
                registeredAny = true;
            }

            if (submitNumberButton != null)
            {
                submitNumberButton.onClick.RemoveAllListeners();
                submitNumberButton.onClick.AddListener(SubmitNumberInput);
                registeredAny = true;
            }

            if (decrementNumberButton != null)
            {
                decrementNumberButton.onClick.RemoveAllListeners();
                decrementNumberButton.onClick.AddListener(() => AdjustNumberInput(-1));
                registeredAny = true;
            }

            if (incrementNumberButton != null)
            {
                incrementNumberButton.onClick.RemoveAllListeners();
                incrementNumberButton.onClick.AddListener(() => AdjustNumberInput(1));
                registeredAny = true;
            }

            numberInputButtonsRegistered = registeredAny;
        }

        private void AppendNumberDigit(int digit)
        {
            if (!currentUsesNumberInputMode || digit < 0 || digit > 9)
            {
                return;
            }

            if (currentNumberInput.Length >= MaxNumberInputLength)
            {
                return;
            }

            if (currentNumberInput == "0")
            {
                currentNumberInput = digit.ToString();
            }
            else
            {
                currentNumberInput += digit.ToString();
            }

            UpdateNumberInputText();
        }

        private void ChooseNumberAnswer(int answer)
        {
            if (!currentUsesNumberInputMode)
            {
                return;
            }

            int buttonIndex = answer - NumberChoiceMin;
            lastAnswerButton = digitButtons != null && buttonIndex >= 0 && buttonIndex < digitButtons.Length
                ? digitButtons[buttonIndex]
                : null;

            currentNumberInput = Mathf.Clamp(answer, NumberChoiceMin, NumberChoiceMax).ToString();
            UpdateNumberInputText();
            SubmitNumberInput();
        }

        private void AdjustNumberInput(int delta)
        {
            if (!currentUsesNumberInputMode || delta == 0)
            {
                return;
            }

            int value = 0;
            if (!string.IsNullOrEmpty(currentNumberInput))
            {
                int.TryParse(currentNumberInput, out value);
            }

            value = Mathf.Clamp(value + delta, NumberChoiceMin, NumberChoiceMax);
            currentNumberInput = value.ToString();
            UpdateNumberInputText();
        }

        private void ResetNumberInput()
        {
            currentNumberInput = string.Empty;
            UpdateNumberInputText();
        }

        private void SubmitNumberInput()
        {
            if (!currentUsesNumberInputMode || string.IsNullOrEmpty(currentNumberInput))
            {
                return;
            }

            if (int.TryParse(currentNumberInput, out int answer))
            {
                OnNumberAnswerSubmitted?.Invoke(answer);
            }
        }

        private void UpdateNumberInputText()
        {
            if (numberInputText != null)
            {
                numberInputText.text = string.IsNullOrEmpty(currentNumberInput) ? NumberInputEmptyValue : currentNumberInput;
            }

            UpdateNumberInputActionButtonStates();
        }

        private void SetNumberInputPanelActive(bool active)
        {
            if (active && currentUsesNumberInputMode)
            {
                EnsureFriendlyNumberInputUi();
            }

            if (numberInputOverlayCanvas != null)
            {
                bool panelUsesOverlay = numberInputPanel != null
                    && numberInputPanel.transform.IsChildOf(numberInputOverlayCanvas.transform);
                numberInputOverlayCanvas.gameObject.SetActive(active && panelUsesOverlay);
            }

            if (numberInputPanel != null)
            {
                numberInputPanel.SetActive(active);
                if (active)
                {
                    numberInputPanel.transform.SetAsLastSibling();
                }
            }

            SetNumberInputInteractable(active);

            if (active)
            {
                Canvas.ForceUpdateCanvases();
            }
        }

        private void SetNumberInputInteractable(bool interactable)
        {
            numberInputControlsInteractable = interactable;

            if (digitButtons != null)
            {
                foreach (Button button in digitButtons)
                {
                    if (button != null)
                    {
                        button.gameObject.SetActive(currentUsesNumberInputMode || interactable);
                        button.enabled = true;
                        button.interactable = interactable;
                        Image image = button.GetComponent<Image>();
                        if (image != null)
                        {
                            image.raycastTarget = true;
                        }
                    }
                }
            }

            if (decrementNumberButton != null)
            {
                decrementNumberButton.gameObject.SetActive(false);
                decrementNumberButton.enabled = true;
                decrementNumberButton.interactable = interactable;
            }

            if (incrementNumberButton != null)
            {
                incrementNumberButton.gameObject.SetActive(false);
                incrementNumberButton.enabled = true;
                incrementNumberButton.interactable = interactable;
            }

            UpdateNumberInputActionButtonStates();
        }

        private void UpdateNumberInputActionButtonStates()
        {
            bool hasValue = !string.IsNullOrEmpty(currentNumberInput);

            if (clearNumberButton != null)
            {
                clearNumberButton.gameObject.SetActive(hasValue);
                clearNumberButton.interactable = numberInputControlsInteractable && hasValue;
            }

            if (submitNumberButton != null)
            {
                submitNumberButton.gameObject.SetActive(hasValue);
                submitNumberButton.interactable = numberInputControlsInteractable && hasValue;
            }
        }

        private void CreateNumberInputUi(Transform parent)
        {
            numberInputPanel = CreateSubPanel(parent, "NumberInputPanel", new Vector2(0f, RuntimeNumberInputPanelBottomY), true);
            RectTransform panelRect = numberInputPanel.GetComponent<RectTransform>();
            panelRect.sizeDelta = RuntimeNumberInputPanelSize;
            ConfigureFriendlyNumberInputPanel();

            numberInputText = CreateNumberInputText(numberInputPanel.transform, "AnswerText", "_", 36, new Vector2(0f, 95f), new Vector2(240f, 60f));
            clearNumberButton = CreateSizedButton(numberInputPanel.transform, "ClearNumberButton", "⌫", new Vector2(-220f, 95f), new Vector2(100f, 60f), null, 24);
            submitNumberButton = CreateSizedButton(numberInputPanel.transform, "SubmitNumberButton", "OK", new Vector2(220f, 95f), new Vector2(100f, 60f), null, 24);

            digitButtons = new Button[10];
            float rowWidth = RuntimeDigitButtonSize.x * 5f + RuntimeDigitButtonGap * 4f;
            float startX = -rowWidth * 0.5f + RuntimeDigitButtonSize.x * 0.5f;

            for (int i = 0; i < digitButtons.Length; i++)
            {
                int row = i < 5 ? 0 : 1;
                int col = i % 5;
                float x = startX + col * (RuntimeDigitButtonSize.x + RuntimeDigitButtonGap);
                float y = row == 0 ? 10f : -80f;
                digitButtons[i] = CreateSizedButton(
                    numberInputPanel.transform,
                    $"DigitButton_{NumberChoiceMin + i}",
                    (NumberChoiceMin + i).ToString(),
                    new Vector2(x, y),
                    RuntimeDigitButtonSize,
                    null,
                    36);
            }

            UIKidFriendlyStyle.ApplyDropZone(numberInputPanel);
        }

        private void EnsureFriendlyNumberInputUi()
        {
            Transform parent = ResolveNumberInputOverlayParent();
            bool needsRuntimePanel = numberInputPanel == null
                || numberInputPanel.transform.parent != parent
                || numberInputPanel.name != "RuntimeNumberKeypadPanel";

            if (needsRuntimePanel)
            {
                if (numberInputPanel != null && numberInputPanel.name != "RuntimeNumberKeypadPanel")
                {
                    numberInputPanel.SetActive(false);
                }

                Transform existingPanel = parent != null ? parent.Find("RuntimeNumberKeypadPanel") : null;
                numberInputPanel = existingPanel != null
                    ? existingPanel.gameObject
                    : CreateSubPanel(parent, "RuntimeNumberKeypadPanel", new Vector2(0f, RuntimeNumberInputPanelBottomY), true);
            }

            numberInputPanel.SetActive(true);
            ConfigureFriendlyNumberInputPanel();
            CreateFriendlyNumberInputControls();
            RegisterNumberInputButtons();
            UpdateNumberInputText();
            numberInputPanel.transform.SetAsLastSibling();
        }

        private Transform ResolveNumberInputOverlayParent()
        {
            Transform root = ResolveRuntimeUiRoot();
            if (root != null)
            {
                if (numberInputOverlayCanvas != null)
                {
                    numberInputOverlayCanvas.gameObject.SetActive(false);
                }

                return root;
            }

            if (numberInputOverlayCanvas == null)
            {
                Transform existingCanvas = transform.Find("QuantityMatchNumberInputCanvas");
                numberInputOverlayCanvas = existingCanvas != null
                    ? existingCanvas.GetComponent<Canvas>()
                    : null;
            }

            if (numberInputOverlayCanvas == null)
            {
                numberInputOverlayCanvas = CreateStandaloneRuntimeCanvas(transform, "QuantityMatchNumberInputCanvas", 1000);
            }

            numberInputOverlayCanvas.gameObject.SetActive(true);

            Transform existingPanel = numberInputOverlayCanvas.transform.Find("NumberInputOverlayRoot");
            if (existingPanel != null)
            {
                existingPanel.gameObject.SetActive(true);
                return existingPanel;
            }

            RectTransform overlayRoot = CreateUiPanel(numberInputOverlayCanvas.transform, "NumberInputOverlayRoot");
            overlayRoot.gameObject.SetActive(true);
            return overlayRoot;
        }

        private void ConfigureFriendlyNumberInputPanel()
        {
            if (numberInputPanel == null)
            {
                return;
            }

            RectTransform panelRect = numberInputPanel.GetComponent<RectTransform>();
            if (panelRect != null)
            {
                panelRect.anchorMin = new Vector2(0.5f, 0f);
                panelRect.anchorMax = new Vector2(0.5f, 0f);
                panelRect.pivot = new Vector2(0.5f, 0.5f);
                panelRect.sizeDelta = RuntimeNumberInputPanelSize;
                panelRect.anchoredPosition = new Vector2(0f, RuntimeNumberInputPanelBottomY);
            }

            Image image = numberInputPanel.GetComponent<Image>();
            if (image != null)
            {
                image.color = Color.clear;
                image.raycastTarget = false;
            }

            UIKidFriendlyStyle.ApplyDropZone(numberInputPanel);
        }

        private void CreateFriendlyNumberInputControls()
        {
            if (numberInputPanel == null)
            {
                return;
            }

            Transform panel = numberInputPanel.transform;
            numberInputPromptText = EnsurePanelChildText(panel, numberInputPromptText, "NumberInputPrompt", NumberInputPrompt, 30, new Vector2(0f, 112f), new Vector2(800f, 48f), new Color(0.16f, 0.12f, 0.08f, 1f));
            numberInputText = EnsureNumberInputDisplay(panel, numberInputText, "AnswerText", NumberInputEmptyValue, 52, new Vector2(0f, 66f), new Vector2(210f, 70f));
            numberInputText.gameObject.SetActive(false);

            decrementNumberButton = EnsureNumberInputButton(panel, decrementNumberButton, "DecreaseNumberButton", "-", new Vector2(-180f, 66f), new Vector2(82f, 70f), 40, new Color(0.2f, 0.5f, 0.9f, 1f), Color.white);
            incrementNumberButton = EnsureNumberInputButton(panel, incrementNumberButton, "IncreaseNumberButton", "+", new Vector2(180f, 66f), new Vector2(82f, 70f), 40, new Color(0.2f, 0.5f, 0.9f, 1f), Color.white);
            decrementNumberButton.gameObject.SetActive(false);
            incrementNumberButton.gameObject.SetActive(false);

            if (digitButtons == null || digitButtons.Length != 10)
            {
                digitButtons = new Button[10];
            }

            float rowWidth = RuntimeDigitButtonSize.x * 5f + RuntimeDigitButtonGap * 4f;
            float startX = -rowWidth * 0.5f + RuntimeDigitButtonSize.x * 0.5f;
            for (int i = 0; i < digitButtons.Length; i++)
            {
                int answer = NumberChoiceMin + i;
                int row = i < 5 ? 0 : 1;
                int col = i % 5;
                float x = startX + col * (RuntimeDigitButtonSize.x + RuntimeDigitButtonGap);
                float y = row == 0 ? 36f : -54f;
                digitButtons[i] = EnsureNumberInputButton(
                    panel,
                    digitButtons[i],
                    $"DigitButton_{answer}",
                    answer.ToString(),
                    new Vector2(x, y),
                    RuntimeDigitButtonSize,
                    34,
                    new Color(0.96f, 0.98f, 1f, 1f),
                    new Color(0.05f, 0.08f, 0.12f, 1f));
            }

            clearNumberButton = EnsureNumberInputButton(panel, clearNumberButton, "ClearNumberButton", NumberInputClearLabel, new Vector2(-155f, -154f), new Vector2(210f, 66f), 26, new Color(0.45f, 0.5f, 0.56f, 1f), Color.white);
            submitNumberButton = EnsureNumberInputButton(panel, submitNumberButton, "SubmitNumberButton", NumberInputSubmitLabel, new Vector2(155f, -154f), new Vector2(260f, 70f), 28, new Color(0.16f, 0.65f, 0.34f, 1f), Color.white);
            clearNumberButton.gameObject.SetActive(false);
            submitNumberButton.gameObject.SetActive(false);
            UIKidFriendlyStyle.ApplyDropZone(numberInputPanel);
        }

        private static Text EnsurePanelChildText(Transform parent, Text current, string name, string content, int fontSize, Vector2 anchoredPosition, Vector2 size, Color color)
        {
            Text text = current;
            if (text == null)
            {
                Transform existing = parent.Find(name);
                text = existing != null ? existing.GetComponent<Text>() : null;
            }

            if (text == null)
            {
                var go = new GameObject(name, typeof(RectTransform), typeof(Text));
                text = go.GetComponent<Text>();
            }

            text.gameObject.name = name;
            text.gameObject.SetActive(true);
            text.enabled = true;
            ConfigureCenteredRect(text.GetComponent<RectTransform>(), parent, anchoredPosition, size);
            text.text = content;
            text.fontSize = fontSize;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 18;
            text.resizeTextMaxSize = fontSize;
            text.alignment = TextAnchor.MiddleCenter;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.color = color;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

        private static Text EnsureNumberInputDisplay(Transform parent, Text current, string name, string content, int fontSize, Vector2 anchoredPosition, Vector2 size)
        {
            Text text = current;
            if (text == null)
            {
                Transform existing = parent.Find(name);
                text = existing != null ? existing.GetComponent<Text>() : null;
            }

            if (text == null)
            {
                var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Text));
                text = go.GetComponent<Text>();
            }

            text.gameObject.name = name;
            text.gameObject.SetActive(true);
            text.enabled = true;
            ConfigureCenteredRect(text.GetComponent<RectTransform>(), parent, anchoredPosition, size);
            Image image = text.GetComponent<Image>();
            if (image == null)
            {
                image = text.gameObject.AddComponent<Image>();
            }

            image.color = new Color(1f, 1f, 1f, 0.98f);
            image.raycastTarget = false;

            text.text = content;
            text.fontSize = fontSize;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 30;
            text.resizeTextMaxSize = fontSize;
            text.alignment = TextAnchor.MiddleCenter;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.color = new Color(0.05f, 0.08f, 0.12f, 1f);
            text.raycastTarget = false;
            return text;
        }

        private static Button EnsureNumberInputButton(Transform parent, Button current, string name, string label, Vector2 anchoredPosition, Vector2 size, int fontSize, Color backgroundColor, Color textColor)
        {
            Button button = current;
            if (button == null)
            {
                Transform existing = parent.Find(name);
                button = existing != null ? existing.GetComponent<Button>() : null;
            }

            if (button == null)
            {
                var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
                button = go.GetComponent<Button>();
            }

            button.gameObject.name = name;
            button.gameObject.SetActive(true);
            button.enabled = true;
            ConfigureCenteredRect(button.GetComponent<RectTransform>(), parent, anchoredPosition, size);

            Image image = button.GetComponent<Image>();
            if (image == null)
            {
                image = button.gameObject.AddComponent<Image>();
            }

            image.color = backgroundColor;
            image.raycastTarget = true;
            button.targetGraphic = image;
            button.interactable = true;

            Text labelText = button.GetComponentInChildren<Text>();
            if (labelText == null)
            {
                labelText = CreateButtonLabel(button.transform, label);
            }

            labelText.gameObject.SetActive(true);
            labelText.enabled = true;
            labelText.gameObject.name = "Label";
            RectTransform labelRect = labelText.GetComponent<RectTransform>();
            if (labelRect != null)
            {
                labelRect.SetParent(button.transform, false);
                labelRect.anchorMin = Vector2.zero;
                labelRect.anchorMax = Vector2.one;
                labelRect.offsetMin = new Vector2(8f, 4f);
                labelRect.offsetMax = new Vector2(-8f, -4f);
            }

            labelText.text = label;
            labelText.fontSize = fontSize;
            labelText.resizeTextForBestFit = true;
            labelText.resizeTextMinSize = 16;
            labelText.resizeTextMaxSize = fontSize;
            labelText.color = textColor;
            labelText.alignment = TextAnchor.MiddleCenter;
            labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            labelText.raycastTarget = false;
            UIKidFriendlyStyle.Apply(button, name, label, fontSize, name.Contains("Digit"));
            return button;
        }

        private static void ConfigureCenteredRect(RectTransform rect, Transform parent, Vector2 anchoredPosition, Vector2 size)
        {
            if (rect == null)
            {
                return;
            }

            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;
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

        private static Canvas CreateStandaloneRuntimeCanvas(Transform parent, string name = "QuantityMatchCanvas", int sortingOrder = 0)
        {
            var canvasGo = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(parent, false);

            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = sortingOrder != 0;
            canvas.sortingOrder = sortingOrder;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            }

            return canvas;
        }

        private static GameObject CreateSubPanel(Transform parent, string name, Vector2 anchoredPosition, bool anchorToBottom = false)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            if (anchorToBottom)
            {
                rect.anchorMin = new Vector2(0.5f, 0f);
                rect.anchorMax = new Vector2(0.5f, 0f);
            }

            rect.sizeDelta = new Vector2(660, 58);
            rect.anchoredPosition = anchoredPosition;
            var image = go.GetComponent<Image>();
            image.color = new Color(0.02f, 0.03f, 0.04f, 0.32f);
            image.raycastTarget = false;
            return go;
        }

        private static Text CreateText(Transform parent, string name, string content, int fontSize, Vector2 anchoredPosition)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.sizeDelta = new Vector2(600, 100);
            rect.anchoredPosition = anchoredPosition;

            var text = go.GetComponent<Text>();
            text.text = content;
            text.fontSize = fontSize;
            text.alignment = TextAnchor.MiddleCenter;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.color = Color.white;
            text.raycastTarget = false;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 18;
            text.resizeTextMaxSize = fontSize;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        private static Text CreateTopText(Transform parent, string name, string content, int fontSize, float topOffset, Vector2 size)
        {
            Text text = CreateText(parent, name, content, fontSize, Vector2.zero);
            var rect = text.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = size;
            rect.anchoredPosition = new Vector2(0f, -topOffset);
            return text;
        }

        private static GameObject CreateTopHeaderPanel(Transform parent, string name, float topOffset, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = size;
            rect.anchoredPosition = new Vector2(0f, -topOffset);

            var image = go.GetComponent<Image>();
            image.color = new Color(0.03f, 0.05f, 0.08f, 0.58f);
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
            rect.offsetMin = new Vector2(18f, 6f);
            rect.offsetMax = new Vector2(-18f, -6f);

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
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

        private static Text CreateNumberInputText(Transform parent, string name, string content, int fontSize, Vector2 anchoredPosition, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Text));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;

            var image = go.GetComponent<Image>();
            if (image != null)
            {
                image.color = new Color(1f, 1f, 1f, 0.95f);
                image.raycastTarget = false;
            }

            var text = go.GetComponent<Text>();
            if (text != null)
            {
                text.text = content;
                text.fontSize = fontSize;
                text.resizeTextForBestFit = true;
                text.resizeTextMinSize = 20;
                text.resizeTextMaxSize = fontSize;
                text.alignment = TextAnchor.MiddleCenter;
                text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                text.color = new Color(0.08f, 0.1f, 0.12f, 1f);
                text.raycastTarget = false;
            }
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
            UIKidFriendlyStyle.Apply(button, name, label, 24);
            return button;
        }

        private static Button CreateSizedButton(Transform parent, string name, string label, Vector2 anchoredPosition,
            Vector2 size, UnityEngine.Events.UnityAction onClick, int fontSize)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;
            go.GetComponent<Image>().color = new Color(0.2f, 0.5f, 0.9f, 1f);

            var button = go.GetComponent<Button>();
            if (onClick != null)
            {
                button.onClick.AddListener(onClick);
            }

            Text text = CreateButtonLabel(go.transform, label);
            text.fontSize = fontSize;
            text.resizeTextMaxSize = fontSize;
            UIKidFriendlyStyle.Apply(button, name, label, fontSize, size.x <= size.y + 8f);
            return button;
        }

        private static Text CreateButtonLabel(Transform parent, string label)
        {
            return UIActivityLayoutHelpers.CreateButtonLabel(parent, label);
        }

        private static void SetButtonLabel(Button button, string label)
        {
            UIActivityLayoutHelpers.SetButtonLabel(button, label);
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
            UIActivityLayoutHelpers.LoadSceneIfAvailable(sceneName);
        }

        private Vector3 originalUiRootPos;
        private bool isShaking;

        private System.Collections.IEnumerator ShakeUiCoroutine()
        {
            if (runtimeUiRoot == null || isShaking) yield break;
            isShaking = true;
            originalUiRootPos = runtimeUiRoot.localPosition;

            float duration = 0.4f;
            float elapsed = 0f;
            float magnitude = 10f; // ±10px

            while (elapsed < duration)
            {
                float x = UnityEngine.Random.Range(-1f, 1f) * magnitude;
                float y = UnityEngine.Random.Range(-1f, 1f) * magnitude;
                runtimeUiRoot.localPosition = originalUiRootPos + new Vector3(x, y, 0f);
                elapsed += Time.deltaTime;
                yield return null;
            }

            runtimeUiRoot.localPosition = originalUiRootPos;
            isShaking = false;
        }

        private static Sprite CreateVerticalGradientSprite(Color topColor, Color bottomColor)
        {
            Texture2D texture = new Texture2D(2, 128, TextureFormat.RGBA32, false);
            for (int y = 0; y < 128; y++)
            {
                float t = (float)y / 127f;
                Color color = Color.Lerp(bottomColor, topColor, t);
                texture.SetPixel(0, y, color);
                texture.SetPixel(1, y, color);
            }
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, 2, 128), new Vector2(0.5f, 0.5f));
        }

        private void NormalizeTopNavigationButtons()
        {
            UIActivityNavButtons.ApplyStandardHomeButton(cancelButton);
        }

        private static void ConfigureTopRightNavigationButton(Button button, string label, Vector2 anchoredPosition, Vector2 size, Color color)
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

            Image image = button.GetComponent<Image>();
            if (image != null)
            {
                image.color = color;
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

            UIKidFriendlyStyle.Apply(button, button.name, label, RuntimeTopNavButtonFontSize);
        }

        private static Button CreateSizedTopRightButton(Transform parent, string name, string label, Vector2 anchoredPosition, Vector2 size, UnityEngine.Events.UnityAction onClick, int fontSize)
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
                : new Color(0.85f, 0.35f, 0.35f, 0.85f);

            var button = go.GetComponent<Button>();
            if (onClick != null)
            {
                button.onClick.AddListener(onClick);
            }

            Text text = CreateButtonLabel(go.transform, label);
            text.fontSize = fontSize;
            text.resizeTextMaxSize = fontSize;
            UIKidFriendlyStyle.Apply(button, name, label, fontSize);
            return button;
        }

        private static Button CreateSizedTopLeftButton(Transform parent, string name, string label, Vector2 anchoredPosition, Vector2 size, UnityEngine.Events.UnityAction onClick, int fontSize)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;
            go.GetComponent<Image>().color = new Color(0.98f, 0.8f, 0.2f, 1.0f); // Bright yellow

            var button = go.GetComponent<Button>();
            if (onClick != null)
            {
                button.onClick.AddListener(onClick);
            }

            Text text = CreateButtonLabel(go.transform, label);
            text.fontSize = fontSize;
            text.resizeTextMaxSize = fontSize;
            UIKidFriendlyStyle.Apply(button, name, label, fontSize);
            return button;
        }

        private static Button CreateSizedBottomButton(Transform parent, string name, string label, Vector2 anchoredPosition, Vector2 size, UnityEngine.Events.UnityAction onClick, int fontSize)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;
            go.GetComponent<Image>().color = new Color(0.98f, 0.8f, 0.2f, 1.0f); // Bright yellow

            var button = go.GetComponent<Button>();
            if (onClick != null)
            {
                button.onClick.AddListener(onClick);
            }

            Text text = CreateButtonLabel(go.transform, label);
            text.fontSize = fontSize;
            text.resizeTextMaxSize = fontSize;
            UIKidFriendlyStyle.Apply(button, name, label, fontSize, size.x <= size.y + 8f);
            return button;
        }
        private static Text CreateTopLeftText(Transform parent, string name, string content, int fontSize, Vector2 anchoredPosition, Vector2 size)
        {
            Text text = CreateText(parent, name, content, fontSize, Vector2.zero);
            var rect = text.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;
            text.alignment = TextAnchor.MiddleLeft;
            return text;
        }
    }
}
