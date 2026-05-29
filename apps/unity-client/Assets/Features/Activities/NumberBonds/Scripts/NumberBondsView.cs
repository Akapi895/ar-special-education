using Core.Learning.ActivityRunner;
using Core.Learning.Models;
using Core.Support.AudioManager;
using Core.UI.Components;
using Core.UI.Localization;
using Project.App;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Features.Activities.NumberBonds
{
    public partial class NumberBondsView : MonoBehaviour, INumberBondsView
    {
        [SerializeField] private Text instructionText;
        [SerializeField] private Text expressionText;
        [SerializeField] private Text progressText;
        [SerializeField] private Text feedbackText;
        [SerializeField] private GameObject feedbackPanel;
        [SerializeField] private Text hintText;
        [SerializeField] private GameObject hintPanel;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button hintButton;
        [SerializeField] private Button listenButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private Button nextRoundButton;
        [SerializeField] private Button progressButton;
        [SerializeField] private UIFeedbackOverlay feedbackOverlay;

        private readonly Dictionary<BondZone, NumberBondZoneView> zoneViews = new Dictionary<BondZone, NumberBondZoneView>();
        private readonly Dictionary<BondZone, List<NumberBondObjectView>> zoneObjects = new Dictionary<BondZone, List<NumberBondObjectView>>();
        private readonly Dictionary<string, NumberBondObjectView> objectViews = new Dictionary<string, NumberBondObjectView>();

        private IActivityPresenter presenter;
        private IARPlacementService placementService;
        private IARInteractionService interactionService;
        private NumberBondsConfig config;
        private NumberBondsQuestion currentQuestion;
        private NumberBondDragAdapter dragAdapter;
        private Transform contentRoot;
        private GameObject zoneRoot;
        private GameObject lineRoot;
        private Material lineMaterial;
        private bool activityFinished;
        private int objectSequence;

        private static readonly Vector2 RuntimeButtonSize = new Vector2(190f, 72f);
        private static readonly Vector2 RuntimeFeedbackPanelSize = new Vector2(800f, 104f);
        private const float RuntimeActionButtonBottomY = 52f;
        private const float RuntimeConfirmBottomY = 132f;
        private const float RuntimeHintPanelBottomY = 214f;
        private const float RuntimeFeedbackPanelBottomY = 292f;
        private const float ButtonGap = 28f;

        public bool HasUiReferences => progressText != null;

        public event Action OnHintRequested;
        public event Action OnCancelRequested;
        public event Action<NumberBondMoveRequest> OnObjectMoveRequested;
        public event Action OnConfirmRequested;

        event Action<ActivityAnswer> IActivityView.OnAnswerSelected
        {
            add { }
            remove { }
        }

        private void Awake()
        {
            InitializeZoneLists();
            WireButtonListeners();
            SetPanelActive(feedbackPanel, false);
            SetPanelActive(hintPanel, false);
            SetButtonActive(nextRoundButton, false);
            SetButtonActive(progressButton, false);
            UIKidFriendlyStyle.ApplyToTree(transform);
            UIKidFriendlyStyle.ApplyReadableTextToScene(3, 24);
        }

        public void Initialize(IActivityPresenter activityPresenter)
        {
            presenter = activityPresenter;
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void SetupRound(NumberBondsQuestion question, NumberBondRoundState state,
            IARPlacementService placementService, IARInteractionService interactionService, NumberBondsConfig config)
        {
            currentQuestion = question;
            this.placementService = placementService;
            this.interactionService = interactionService;
            this.config = config;
            activityFinished = false;
            objectSequence = 0;

            ClearSpawnedObjects();
            InitializeZoneLists();
            contentRoot = placementService?.LearningAreaContentRoot;
            CreateZones(state);
            CreateConnectionLines();
            SpawnInitialObjects(state);
            ConfigureDragAdapter();
            UpdateZoneCounts(state);
            UpdateExpression(NumberBondExpressionBinder.Format(question, state));
            ShowQuestionText(question);
            SetConfirmEnabled(false);
            SetInputEnabled(true);
            SetRunningControlsActive(true);
            SetButtonActive(nextRoundButton, false);
            SetButtonActive(progressButton, false);
            HideFeedback();
            HideHint();

            SimpleAudioManager.EnsureExists().PlayInstruction("instruction_number_bonds");
            SimpleAudioManager.Instance.PlayNumber(question.WholeTarget);
        }

        public void CommitObjectMove(string objectId, BondZone fromZone, BondZone toZone)
        {
            if (!objectViews.TryGetValue(objectId, out NumberBondObjectView objectView))
            {
                return;
            }

            zoneObjects[fromZone].Remove(objectView);
            zoneObjects[toZone].Add(objectView);
            objectView.SetZone(toZone);
            RefreshObjectSlots(fromZone);
            RefreshObjectSlots(toZone);
        }

        public void RejectObjectMove(string objectId, NumberBondValidationResult reason)
        {
            if (objectViews.TryGetValue(objectId, out NumberBondObjectView objectView))
            {
                objectView.ReturnToLastStablePosition();
                RefreshObjectSlots(objectView.CurrentZone);
            }

            if (reason == NumberBondValidationResult.LockedZoneModified)
            {
                ShowFeedback(config != null ? config.GetFeedback(reason, null) : string.Empty, Color.red);
            }
        }

        public void UpdateZoneCounts(NumberBondRoundState state)
        {
            SetZoneCount(BondZone.Whole, state?.WholeCount ?? 0);
            SetZoneCount(BondZone.PartA, state?.PartACount ?? 0);
            SetZoneCount(BondZone.PartB, state?.PartBCount ?? 0);
        }

        public void UpdateExpression(string expression)
        {
            if (expressionText != null)
            {
                expressionText.text = expression;
            }
        }

        public void SetConfirmEnabled(bool enabled)
        {
            if (confirmButton != null)
            {
                confirmButton.interactable = enabled;
            }
        }

        public void ShowCorrectFeedback()
        {
            ShowCorrectFeedback(SimpleLocalization.Get("feedback_correct"));
        }

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

            SetInputEnabled(false);
            SetRunningControlsActive(false);
            SetButtonActive(confirmButton, false);
            SetNextButtonForCurrentState();
        }

        public void ShowIncorrectFeedback()
        {
            ShowIncorrectFeedback(SimpleLocalization.Get("feedback_incorrect"));
        }

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

            SetInputEnabled(true);
            UIKidFriendlyStyle.PlayFeedback(confirmButton, false);
        }

        public void ShowHint(ActivityHint hint)
        {
            if (hintPanel != null && hintText != null)
            {
                hintText.text = hint.HintText;
                hintPanel.SetActive(true);
                CancelInvoke(nameof(HideHint));
                Invoke(nameof(HideHint), 6f);
            }
        }

        public void HideHint()
        {
            SetPanelActive(hintPanel, false);
        }

        public void UpdateProgress(int current, int total)
        {
            if (progressText != null)
            {
                progressText.text = $"C\u00e2u {current}/{total}";
            }
        }

        public void SetInputEnabled(bool enabled)
        {
            if (dragAdapter != null)
            {
                dragAdapter.SetInputEnabled(enabled);
            }

            if (hintButton != null)
            {
                hintButton.interactable = enabled;
            }
        }

        public void ShowActivityComplete(ActivityResult result)
        {
            activityFinished = true;
            SetInputEnabled(false);
            SetRunningControlsActive(false);
            SetButtonActive(confirmButton, false);

            string message = SimpleLocalization.Get("feedback_success");
            if (feedbackOverlay != null)
            {
                feedbackOverlay.ShowSuccess(message);
            }
            else
            {
                ShowFeedback(message, Color.green);
            }

            SetButtonActive(nextRoundButton, ActivityFlowNavigator.TryGetNextActivityId("NumberBonds", out _));
            SetButtonActive(progressButton, true);
        }

        public void ShowActivityFailed(string message, ActivityResult result)
        {
            activityFinished = true;
            SetInputEnabled(false);
            SetRunningControlsActive(false);
            SetButtonActive(confirmButton, false);
            if (feedbackOverlay != null)
            {
                feedbackOverlay.ShowIncorrect(message);
            }
            else
            {
                ShowFeedback(message, Color.red);
            }
            SetButtonActive(nextRoundButton, ActivityFlowNavigator.TryGetNextActivityId("NumberBonds", out _));
            SetButtonActive(progressButton, true);
        }

        public void ClearSpawnedObjects()
        {
            foreach (NumberBondObjectView objectView in objectViews.Values)
            {
                if (objectView != null)
                {
                    interactionService?.UnregisterInteractable(objectView.gameObject);
                    Destroy(objectView.gameObject);
                }
            }

            objectViews.Clear();
            InitializeZoneLists();

            if (zoneRoot != null)
            {
                Destroy(zoneRoot);
                zoneRoot = null;
            }

            if (lineRoot != null)
            {
                Destroy(lineRoot);
                lineRoot = null;
            }
        }

        private void ShowFeedback(string message, Color color)
        {
            if (feedbackPanel == null || feedbackText == null)
            {
                return;
            }

            feedbackText.text = message;
            feedbackText.color = Color.white;
            Image image = feedbackPanel.GetComponent<Image>();
            if (image != null)
            {
                image.color = color.g >= color.r
                    ? new Color(0.05f, 0.42f, 0.18f, 0.88f)
                    : new Color(0.5f, 0.18f, 0.08f, 0.88f);
            }
            feedbackPanel.SetActive(true);
        }

        public void HideFeedback()
        {
            if (feedbackOverlay != null)
            {
                feedbackOverlay.Hide();
            }
            SetPanelActive(feedbackPanel, false);
        }

        private void SetNextButtonForCurrentState()
        {
            if (nextRoundButton == null)
            {
                return;
            }

            string label = presenter != null && presenter.HasMoreRounds()
                ? SimpleLocalization.Get("btn_next")
                : ActivityFlowNavigator.TryGetNextActivityId("NumberBonds", out _) ? "B\u00e0i ti\u1ebfp" : "Ho\u00e0n th\u00e0nh";
            SetButtonLabel(nextRoundButton, label);
            SetButtonActive(nextRoundButton, true);
            UIKidFriendlyStyle.PlayFeedback(nextRoundButton, true);
        }

        private void OnNextRoundClicked()
        {
            if (activityFinished || presenter?.GetState() == ActivityState.Failed)
            {
                if (!ActivityFlowNavigator.LoadNextActivity("NumberBonds"))
                {
                    ActivityFlowNavigator.LoadProgressDashboard();
                }
                return;
            }

            if (presenter?.GetState() == ActivityState.Completed)
            {
                bool hasMoreRounds = presenter.HasMoreRounds();
                presenter.ContinueToNextRound();

                if (!hasMoreRounds && !ActivityFlowNavigator.LoadNextActivity("NumberBonds"))
                {
                    ActivityFlowNavigator.LoadProgressDashboard();
                }
            }
        }

        private void OnProgressClicked()
        {
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

        private void SetRunningControlsActive(bool active)
        {
            SetButtonActive(hintButton, active);
            SetButtonActive(listenButton, active);
            SetButtonActive(cancelButton, true);
        }

        private void WireButtonListeners()
        {
            if (confirmButton != null)
            {
                confirmButton.onClick.RemoveAllListeners();
                confirmButton.onClick.AddListener(() => OnConfirmRequested?.Invoke());
            }

            if (hintButton != null)
            {
                hintButton.onClick.RemoveAllListeners();
                hintButton.onClick.AddListener(() => OnHintRequested?.Invoke());
            }

            if (cancelButton != null)
            {
                cancelButton.onClick.RemoveAllListeners();
                cancelButton.onClick.AddListener(OnCancelClicked);
            }

            if (listenButton != null)
            {
                listenButton.onClick.RemoveAllListeners();
                listenButton.onClick.AddListener(OnListenClicked);
            }

            if (nextRoundButton != null)
            {
                nextRoundButton.onClick.RemoveAllListeners();
                nextRoundButton.onClick.AddListener(OnNextRoundClicked);
            }

            if (progressButton != null)
            {
                progressButton.onClick.RemoveAllListeners();
                progressButton.onClick.AddListener(OnProgressClicked);
            }
        }

        private void OnDestroy()
        {
            CancelInvoke();
            if (dragAdapter != null)
            {
                dragAdapter.OnObjectDropped -= HandleObjectDropped;
            }
            ClearSpawnedObjects();
            if (lineMaterial != null)
            {
                Destroy(lineMaterial);
                lineMaterial = null;
            }
        }
    }
}
