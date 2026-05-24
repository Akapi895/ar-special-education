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

        // Presenter reference
        private IActivityPresenter presenter;

        // State
        private int currentTargetNumber;
        private int currentNumberOfGroups;

        private Button[] runtimeGroupButtons;
        private Transform runtimeUiRoot;
        private bool activityFinished;

        private static readonly Vector2 RuntimeButtonSize = new Vector2(170f, 58f);
        private const float RuntimeButtonGap = 28f;
        private const float RuntimeActionButtonBottomY = 52f;
        private const float RuntimeGroupButtonBottomY = 128f;
        private const float RuntimeHintPanelBottomY = 202f;
        private const float RuntimeFeedbackPanelBottomY = 280f;

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
        }

        /// <summary>
        /// Builds minimal UI at runtime when prefabs are not assigned.
        /// </summary>
        public void BuildRuntimeUi(Canvas canvas)
        {
            var panel = CreateUiPanel(canvas.transform, "QuantityMatchPanel");
            runtimeUiRoot = panel;

            targetNumberText = CreateTopText(panel, "TargetNumber", "Choose the matching group", 36, 28f, new Vector2(920f, 72f));
            progressText = CreateTopText(panel, "Progress", "", 24, 98f, new Vector2(720f, 42f));

            feedbackPanel = CreateSubPanel(panel, "FeedbackPanel", new Vector2(0, RuntimeFeedbackPanelBottomY), true);
            feedbackText = CreatePanelText(feedbackPanel.transform, "FeedbackText", "", 26);
            feedbackPanel.SetActive(false);

            hintPanel = CreateSubPanel(panel, "HintPanel", new Vector2(0, RuntimeHintPanelBottomY), true);
            hintText = CreatePanelText(hintPanel.transform, "HintText", "", 22);
            hintPanel.SetActive(false);

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
        /// Show the question with target number and group count.
        /// </summary>
        public void ShowQuestion(int targetNumber, int numberOfGroups)
        {
            currentTargetNumber = targetNumber;
            currentNumberOfGroups = numberOfGroups;
            activityFinished = false;

            UpdateTargetNumber(targetNumber);
            HideFeedback();
            HideHint();
            EnsureRuntimeGroupButtons(numberOfGroups);
            SetRuntimeGroupButtonsActive(true);

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
                targetNumberText.text = $"Choose the group with exactly {targetNumber} balls";
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
            activityFinished = true;
            DisableInput();
            SetRuntimeGroupButtonsInteractable(false);
            SetRuntimeGroupButtonsActive(false);
            SetRunningActionButtonsActive(false);
            ShowFeedback("Activity complete! Progress saved.", Color.green);

            if (nextRoundButton != null)
            {
                SetButtonLabel(nextRoundButton, "Next");
                nextRoundButton.gameObject.SetActive(ActivityFlowNavigator.TryGetNextActivityId("QuantityMatch", out _));
            }

            if (progressButton != null)
            {
                SetButtonLabel(progressButton, "Progress");
                progressButton.gameObject.SetActive(true);
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
            SetRunningActionButtonsActive(false);
            ShowFeedback(message, Color.red);

            if (nextRoundButton != null)
            {
                SetButtonLabel(nextRoundButton, "Next");
                nextRoundButton.gameObject.SetActive(ActivityFlowNavigator.TryGetNextActivityId("QuantityMatch", out _));
            }

            if (progressButton != null)
            {
                SetButtonLabel(progressButton, "Progress");
                progressButton.gameObject.SetActive(true);
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
            Debug.Log("[QuantityMatchView] Next round / Finish clicked");

            if (activityFinished || presenter?.GetState() == ActivityState.Completed || presenter?.GetState() == ActivityState.Failed)
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
            if (groupIndex >= 0 && groupIndex < currentNumberOfGroups)
            {
                NotifyGroupSelected(groupIndex, 0);  // Count will be filled by presenter
            }
        }

        private void EnsureRuntimeGroupButtons(int numberOfGroups)
        {
            if (groupSelectionButtons != null && groupSelectionButtons.Length > 0)
            {
                return;
            }

            if (runtimeGroupButtons != null)
            {
                foreach (Button button in runtimeGroupButtons)
                {
                    if (button != null)
                    {
                        button.gameObject.SetActive(false);
                        Destroy(button.gameObject);
                    }
                }
            }

            runtimeGroupButtons = new Button[numberOfGroups];
            float buttonSpacing = RuntimeButtonSize.x + RuntimeButtonGap;
            float startX = -(numberOfGroups - 1) * buttonSpacing * 0.5f;

            for (int i = 0; i < numberOfGroups; i++)
            {
                int groupIndex = i;
                runtimeGroupButtons[i] = CreateButton(
                    runtimeUiRoot != null ? runtimeUiRoot : transform,
                    $"GroupButton_{i}",
                    $"Group {i + 1}",
                    new Vector2(startX + i * buttonSpacing, RuntimeGroupButtonBottomY),
                    () => OnGroupButtonClicked(groupIndex));
            }
        }

        private void SetRuntimeGroupButtonsActive(bool active)
        {
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
    }
}
