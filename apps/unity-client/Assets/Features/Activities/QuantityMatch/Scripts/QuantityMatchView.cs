using Core.Learning.ActivityRunner;
using Core.Learning.Models;
using Features.Activities.QuantityMatch;
using Project.App;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
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

        [SerializeField]
        private Button progressButton;

        [SerializeField]
        private Core.UI.Components.UIFeedbackOverlay feedbackOverlay;

        [Header("Number Input UI")]
        [SerializeField]
        private GameObject numberInputPanel;

        [SerializeField]
        private Text numberInputText;

        [SerializeField]
        private Button[] digitButtons;

        [SerializeField]
        private Button clearNumberButton;

        [SerializeField]
        private Button submitNumberButton;

        [Header("Navigation")]
        [SerializeField]
        private string activitySelectSceneName = "SC_ActivitySelect";

        [Header("Group Selection UI")]
        [SerializeField]
        private GameObject[] groupSelectionButtons;  // Optional: buttons for each group

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
        private bool activityFinished;
        private bool numberInputButtonsRegistered;

        private static readonly Vector2 RuntimeButtonSize = new Vector2(170f, 58f);
        private static readonly Vector2 RuntimeDigitButtonSize = new Vector2(120f, 80f);
        private static readonly Vector2 RuntimeNumberInputPanelSize = new Vector2(820f, 280f);
        private const float RuntimeButtonGap = 28f;
        private const float RuntimeDigitButtonGap = 12f;
        private const float RuntimeActionButtonBottomY = 52f;
        private const float RuntimeGroupButtonBottomY = 128f;
        private const float RuntimeHintPanelBottomY = 318f;
        private const float RuntimeFeedbackPanelBottomY = 394f;
        private const float RuntimeNumberInputPanelBottomY = 182f;
        private const int MaxNumberInputLength = 2;

        public bool HasUiReferences => targetNumberText != null;

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

            if (nextRoundButton != null)
            {
                nextRoundButton.onClick.AddListener(OnNextRoundClicked);
            }

            if (progressButton != null)
            {
                progressButton.onClick.AddListener(OnProgressClicked);
            }

            RegisterNumberInputButtons();
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

            // Fullscreen Background Gradient (soft blue to soft cream pastel)
            var bgGo = new GameObject("BackgroundGradient", typeof(RectTransform), typeof(Image));
            var bgRect = bgGo.GetComponent<RectTransform>();
            bgRect.SetParent(panel, false);
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            var bgImage = bgGo.GetComponent<Image>();
            bgImage.sprite = CreateVerticalGradientSprite(new Color(0.72f, 0.85f, 0.97f, 0.12f), new Color(1.0f, 0.96f, 0.90f, 0.12f));
            bgImage.raycastTarget = false;
            bgGo.transform.SetAsFirstSibling();

            targetNumberText = CreateTopText(panel, "TargetNumber", "Chọn nhóm có con vật", 36, 28f, new Vector2(920f, 72f));
            progressText = CreateTopLeftText(panel, "Progress", "", 24, new Vector2(40f, -40f), new Vector2(300f, 60f));

            feedbackPanel = CreateSubPanel(panel, "FeedbackPanel", new Vector2(0, RuntimeFeedbackPanelBottomY), true);
            feedbackText = CreatePanelText(feedbackPanel.transform, "FeedbackText", "", 26);
            feedbackPanel.SetActive(false);

            hintPanel = CreateSubPanel(panel, "HintPanel", new Vector2(0, RuntimeHintPanelBottomY), true);
            hintText = CreatePanelText(hintPanel.transform, "HintText", "", 22);
            hintPanel.SetActive(false);

            // Large center bottom Hint button (140x140px, gold, icon bulb)
            hintButton = CreateSizedBottomButton(panel, "HintButton", "💡", new Vector2(0f, RuntimeActionButtonBottomY), new Vector2(140f, 140f), () => OnHintRequested?.Invoke(), 72);
            
            // Small top-right Cancel button (100x100px, soft-red, icon cross) - enlarged for kids
            cancelButton = CreateSizedTopRightButton(panel, "CancelButton", "✕", new Vector2(-40f, -40f), new Vector2(100f, 100f), OnCancelClicked, 48);

            // Centered bottom buttons for transitioning (hidden by default)
            nextRoundButton = CreateSizedBottomButton(panel, "NextButton", "▶ Tiếp tục", new Vector2(0f, RuntimeActionButtonBottomY), new Vector2(240f, 100f), OnNextRoundClicked, 28);
            progressButton = CreateSizedBottomButton(panel, "ProgressButton", "Tiến độ", new Vector2(0f, RuntimeActionButtonBottomY), new Vector2(200f, 100f), OnProgressClicked, 24);
            nextRoundButton.gameObject.SetActive(false);
            progressButton.gameObject.SetActive(false);

            CreateNumberInputUi(panel);
            RegisterNumberInputButtons();
            SetNumberInputPanelActive(false);
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

            UpdateTargetNumber(targetNumber);
            HideFeedback();
            HideHint();
            if (currentUsesNumberInputMode)
            {
                SetRuntimeGroupButtonsActive(false);
                ResetNumberInput();
                SetNumberInputPanelActive(true);
            }
            else
            {
                SetRuntimeGroupButtonsActive(true);
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
                hintButton.gameObject.SetActive(true);
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
                    ? "Có bao nhiêu con vật?"
                    : $"Chọn nhóm có đúng {targetNumber} con";
            }

            // Play voice instruction for kids
            if (Core.Support.AudioManager.SimpleAudioManager.Instance != null)
            {
                if (currentUsesNumberInputMode)
                {
                    Core.Support.AudioManager.SimpleAudioManager.Instance.PlaySound("co_bao_nhieu_con_vat");
                }
                else
                {
                    Core.Support.AudioManager.SimpleAudioManager.Instance.PlaySound($"chon_{targetNumber}");
                }
            }
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

            if (presenter != null && presenter.HasMoreRounds())
            {
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
            if (feedbackOverlay != null)
            {
                feedbackOverlay.ShowIncorrect(message);
            }
            else
            {
                ShowFeedback(message, new Color(1.0f, 0.5f, 0f)); // orange
            }

            StartCoroutine(ShakeUiCoroutine());

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
            LoadSceneIfAvailable(activitySelectSceneName);
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
                NotifyGroupSelected(groupIndex, 0);  // Count will be filled by presenter
            }
        }

        private void EnsureRuntimeGroupButtons(int numberOfGroups)
        {
            // Stubbed out: Group buttons are removed to allow direct touch interaction with animal groups
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

            if (cancelButton != null)
            {
                cancelButton.gameObject.SetActive(active);
            }
        }

        private void RegisterNumberInputButtons()
        {
            if (numberInputButtonsRegistered)
            {
                return;
            }

            bool registeredAny = false;

            if (digitButtons != null)
            {
                for (int i = 0; i < digitButtons.Length; i++)
                {
                    int digit = i;
                    if (digitButtons[i] != null)
                    {
                        digitButtons[i].onClick.AddListener(() => AppendNumberDigit(digit));
                        registeredAny = true;
                    }
                }
            }

            if (clearNumberButton != null)
            {
                clearNumberButton.onClick.AddListener(ResetNumberInput);
                registeredAny = true;
            }

            if (submitNumberButton != null)
            {
                submitNumberButton.onClick.AddListener(SubmitNumberInput);
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
                numberInputText.text = string.IsNullOrEmpty(currentNumberInput) ? "_" : currentNumberInput;
            }
        }

        private void SetNumberInputPanelActive(bool active)
        {
            if (numberInputPanel != null)
            {
                numberInputPanel.SetActive(active);
            }

            SetNumberInputInteractable(active);
        }

        private void SetNumberInputInteractable(bool interactable)
        {
            if (digitButtons != null)
            {
                foreach (Button button in digitButtons)
                {
                    if (button != null)
                    {
                        button.interactable = interactable;
                    }
                }
            }

            if (clearNumberButton != null)
            {
                clearNumberButton.interactable = interactable;
            }

            if (submitNumberButton != null)
            {
                submitNumberButton.interactable = interactable;
            }
        }

        private void CreateNumberInputUi(Transform parent)
        {
            numberInputPanel = CreateSubPanel(parent, "NumberInputPanel", new Vector2(0f, RuntimeNumberInputPanelBottomY), true);
            RectTransform panelRect = numberInputPanel.GetComponent<RectTransform>();
            panelRect.sizeDelta = RuntimeNumberInputPanelSize;

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
                    $"DigitButton_{i}",
                    i.ToString(),
                    new Vector2(x, y),
                    RuntimeDigitButtonSize,
                    null,
                    36);
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

            rect.sizeDelta = new Vector2(720, 64);
            rect.anchoredPosition = anchoredPosition;
            var image = go.GetComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0.5f);
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
            image.color = new Color(1f, 1f, 1f, 0.95f);
            image.raycastTarget = false;

            var text = go.GetComponent<Text>();
            text.text = content;
            text.fontSize = fontSize;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 20;
            text.resizeTextMaxSize = fontSize;
            text.alignment = TextAnchor.MiddleCenter;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.color = new Color(0.08f, 0.1f, 0.12f, 1f);
            text.raycastTarget = false;
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
            text.fontSize = 24;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 16;
            text.resizeTextMaxSize = 24;
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
            if (string.IsNullOrEmpty(sceneName))
            {
                return;
            }

            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.LogWarning($"[QuantityMatchView] Scene '{sceneName}' is not available in Build Settings.");
                return;
            }

            SceneManager.LoadScene(sceneName);
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
            go.GetComponent<Image>().color = new Color(0.85f, 0.35f, 0.35f, 0.8f);

            var button = go.GetComponent<Button>();
            if (onClick != null)
            {
                button.onClick.AddListener(onClick);
            }

            Text text = CreateButtonLabel(go.transform, label);
            text.fontSize = fontSize;
            text.resizeTextMaxSize = fontSize;
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
