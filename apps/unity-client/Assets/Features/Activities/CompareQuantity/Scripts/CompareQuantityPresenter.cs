using Core.Learning.ActivityRunner;
using Core.Learning.Models;
using Core.Learning.Utils;
using Features.Activities;
using Features.Activities.CompareQuantity;
using System;
using UnityEngine;

namespace Features.Activities.CompareQuantity
{
    /// <summary>
    /// Presenter for Compare Quantity activity.
    /// Handles all game logic and coordinates between View, AR services, and data.
    /// </summary>
    public class CompareQuantityPresenter : ActivityPresenter
    {
        // AR Service references
        private IARPlacementService placementService;
        private IARInteractionService interactionService;

        // Current question data
        private CompareQuantityQuestion currentQuestion;
        private CompareQuantityConfig compareConfig;

        // View reference
        private ICompareQuantityView view;

        // Spawned objects tracking
        private GameObject leftGroup;
        private GameObject rightGroup;

        // Events specific to Compare Quantity
        public event Action OnGroupsSpawned;

        /// <summary>
        /// Initialize the presenter with config and view.
        /// </summary>
        public void Initialize(CompareQuantityConfig config, ICompareQuantityView activityView,
            IARPlacementService arPlacement, IARInteractionService arInteraction)
        {
            compareConfig = config;
            view = activityView;
            placementService = arPlacement;
            interactionService = arInteraction;

            // Initialize base presenter
            base.Initialize(config);

            // Initialize view
            view?.Initialize(this);
            view?.UpdateButtonLabels(config.MoreButtonLabel, config.FewerButtonLabel, config.EqualButtonLabel);

            // Subscribe to view events
            if (view != null)
            {
                view.OnAnswerSelected += HandleAnswerSelected;
                view.OnHintRequested += HandleHintRequested;
                view.OnCancelRequested += HandleCancelRequested;
            }

            Debug.Log($"[CompareQuantityPresenter] Initialized with {config.Questions.Count} questions");
        }

        /// <summary>
        /// Load and display a specific round/question.
        /// </summary>
        protected override void LoadRound(int roundNumber)
        {
            currentQuestion = compareConfig.GetQuestion(roundNumber - 1);
            if (currentQuestion == null)
            {
                Debug.LogError($"[CompareQuantityPresenter] Failed to load question {roundNumber}");
                return;
            }

            bool isEquality = currentQuestion.IsEqualityQuestion();
            Debug.Log($"[CompareQuantityPresenter] Loading round {roundNumber}: " +
                     $"Left={currentQuestion.LeftGroupCount}, Right={currentQuestion.RightGroupCount}, " +
                     $"Correct={currentQuestion.CorrectAnswer}, IsEquality={isEquality}");

            // Clear previous objects
            ClearSpawnedObjects();

            // Spawn the two groups
            SpawnGroups();

            // Update view with question data
            view?.ShowQuestion(currentQuestion.LeftGroupCount, currentQuestion.RightGroupCount, isEquality);
            view?.UpdateProgress(roundNumber, compareConfig.NumberOfRounds);
        }

        /// <summary>
        /// Spawn both groups for the current question using shared utility.
        /// </summary>
        private void SpawnGroups()
        {
            if (placementService == null)
            {
                Debug.LogError("[CompareQuantityPresenter] AR Placement Service not available. Cannot spawn groups.");
                return;
            }

            // Calculate positions for side-by-side arrangement
            Vector3[] positions = ARGroupSpawnUtility.CalculateGroupPositions(
                numberOfGroups: 2,
                centerPosition: placementService.CurrentPlacementPosition,
                spacing: compareConfig.GroupSpacing,
                arrangementPattern: GroupArrangementPattern.SideBySide
            );

            // Get the prefab to use
            GameObject prefab = GetObjectPrefab();

            // Spawn left group
            leftGroup = ARGroupSpawnUtility.SpawnGroup(
                placementService: placementService,
                prefab: prefab,
                position: positions[0],
                objectCount: currentQuestion.LeftGroupCount,
                arrangementPattern: ObjectArrangementPattern.Circle,
                groupName: "LeftGroup"
            );

            // Register left group as interactable
            if (interactionService != null && leftGroup != null)
            {
                interactionService.RegisterInteractable(leftGroup, "Left");
            }
            ActivityPrefabSetup.Instance?.PrepareLearningObjectGroup(leftGroup);
            AddGroupLabel(leftGroup, "Left Group");

            // Spawn right group
            rightGroup = ARGroupSpawnUtility.SpawnGroup(
                placementService: placementService,
                prefab: prefab,
                position: positions[1],
                objectCount: currentQuestion.RightGroupCount,
                arrangementPattern: ObjectArrangementPattern.Circle,
                groupName: "RightGroup"
            );

            // Register right group as interactable
            if (interactionService != null && rightGroup != null)
            {
                interactionService.RegisterInteractable(rightGroup, "Right");
            }
            ActivityPrefabSetup.Instance?.PrepareLearningObjectGroup(rightGroup);
            AddGroupLabel(rightGroup, "Right Group");

            OnGroupsSpawned?.Invoke();
        }

        private static void AddGroupLabel(GameObject group, string labelText)
        {
            if (group == null)
            {
                return;
            }

            var labelGo = new GameObject("GroupLabel");
            labelGo.transform.SetParent(group.transform, false);
            labelGo.transform.localPosition = new Vector3(0f, 0.34f, 0f);

            var label = labelGo.AddComponent<TextMesh>();
            label.text = labelText;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.fontSize = 64;
            label.characterSize = 0.016f;
            label.color = Color.white;

            Camera camera = Camera.main;
            if (camera != null)
            {
                labelGo.transform.rotation = Quaternion.LookRotation(labelGo.transform.position - camera.transform.position, Vector3.up);
            }
        }

        /// <summary>
        /// Get the prefab to use for spawning objects.
        /// TODO: Load from resources or use a default.
        /// </summary>
        private GameObject GetObjectPrefab()
        {
            if (ActivityPrefabSetup.Instance != null)
            {
                return ActivityPrefabSetup.Instance.GetLearningObjectPrefab();
            }

            Debug.LogWarning("[CompareQuantityPresenter] No object prefab assigned. Add ActivityPrefabSetup for runtime placeholders.");
            return null;
        }

        /// <summary>
        /// Check if the given answer is correct.
        /// </summary>
        protected override bool CheckAnswer(ActivityAnswer answer)
        {
            if (answer is not CompareQuantityAnswer compareAnswer)
            {
                Debug.LogError($"[CompareQuantityPresenter] Answer is not a CompareQuantityAnswer");
                return false;
            }

            // Use the answer's own IsCorrect method which compares SelectedComparison to expected
            bool isCorrect = compareAnswer.IsCorrect();

            Debug.Log($"[CompareQuantityPresenter] Checking answer: {compareAnswer.SelectedComparison} " +
                     $"(Left: {compareAnswer.LeftGroupCount}, Right: {compareAnswer.RightGroupCount}) " +
                     $"vs Expected: {compareAnswer.CalculateExpectedAnswer()} " +
                     $"-> {(isCorrect ? "CORRECT" : "INCORRECT")}");

            return isCorrect;
        }

        /// <summary>
        /// Get the error type for an incorrect answer.
        /// </summary>
        protected override ErrorType? GetErrorType(ActivityAnswer answer)
        {
            if (answer is not CompareQuantityAnswer compareAnswer)
            {
                return ErrorType.Other;
            }

            // Determine the specific error type based on the wrong comparison
            ComparisonAnswer selected = compareAnswer.SelectedComparison;
            ComparisonAnswer expected = compareAnswer.CalculateExpectedAnswer();

            if (expected == ComparisonAnswer.Equal)
            {
                // User didn't recognize equality
                return ErrorType.WrongComparison;
            }
            else if (selected == ComparisonAnswer.Equal)
            {
                // User thought it was equal but it wasn't
                return ErrorType.WrongComparison;
            }
            else if (selected == ComparisonAnswer.More && expected == ComparisonAnswer.Fewer)
            {
                // User said more when it was fewer
                return ErrorType.WrongComparison;
            }
            else
            {
                // User said fewer when it was more
                return ErrorType.WrongComparison;
            }
        }

        /// <summary>
        /// Handle when an answer is selected via the View.
        /// </summary>
        private void HandleAnswerSelected(ComparisonAnswer selectedAnswer)
        {
            if (currentState != ActivityState.InProgress)
            {
                return;
            }

            Debug.Log($"[CompareQuantityPresenter] Answer selected: {selectedAnswer}");

            // Create the answer
            var answer = new CompareQuantityAnswer(
                selectedComparison: selectedAnswer,
                leftCount: currentQuestion.LeftGroupCount,
                rightCount: currentQuestion.RightGroupCount,
                responseTimeSeconds: Time.time - roundStartTime,
                attemptNumber: currentResult.TotalAttempts + 1,
                hintsUsed: hintsUsedInCurrentRound > 0
            );

            // Submit the answer via base presenter
            SubmitAnswer(answer);
        }

        /// <summary>
        /// Handle hint request from View.
        /// </summary>
        private void HandleHintRequested()
        {
            RequestHint();
        }

        /// <summary>
        /// Handle cancel request from View.
        /// </summary>
        private void HandleCancelRequested()
        {
            Cancel();
        }

        /// <summary>
        /// Called when a hint is provided. Override to pass to View with proper formatting.
        /// </summary>
        protected override void OnHintProvided(ActivityHint hint)
        {
            base.OnHintProvided(hint);

            // Format the hint based on whether it's an equality question
            bool isEquality = currentQuestion.IsEqualityQuestion();
            string formattedHint;

            if (isEquality)
            {
                formattedHint = compareConfig.GetFormattedEqualityHintText(hint, currentQuestion.LeftGroupCount);
            }
            else
            {
                formattedHint = compareConfig.GetFormattedHintText(hint,
                    currentQuestion.LeftGroupCount, currentQuestion.RightGroupCount);
            }

            hint.HintText = formattedHint;

            // Pass to view
            view?.ShowHint(hint);
        }

        /// <summary>
        /// Handle correct answer with outcome-specific feedback.
        /// </summary>
        protected override void HandleCorrectAnswer(ActivityAnswer answer)
        {
            if (answer is not CompareQuantityAnswer compareAnswer)
            {
                base.HandleCorrectAnswer(answer);
                return;
            }

            // Get outcome-specific feedback
            string feedback = compareConfig.GetFeedbackString(compareAnswer.SelectedComparison, isCorrect: true);

            // Show feedback in view
            view?.ShowCorrectFeedback(feedback);

            // Call base to complete the round
            base.HandleCorrectAnswer(answer);
        }

        /// <summary>
        /// Handle incorrect answer with outcome-specific feedback.
        /// </summary>
        protected override void HandleIncorrectAnswer(ActivityAnswer answer)
        {
            if (answer is not CompareQuantityAnswer compareAnswer)
            {
                base.HandleIncorrectAnswer(answer);
                return;
            }

            // Get outcome-specific feedback
            string feedback = compareConfig.GetFeedbackString(compareAnswer.SelectedComparison, isCorrect: false);

            // Show feedback in view
            view?.ShowIncorrectFeedback(feedback);

            // Call base to handle retry or failure
            base.HandleIncorrectAnswer(answer);
        }

        /// <summary>
        /// Handle activity completion (all rounds done).
        /// </summary>
        protected override void CompleteActivity(bool success)
        {
            base.CompleteActivity(success);

            if (success)
            {
                view?.ShowActivityComplete(currentResult);
            }
            else
            {
                view?.ShowActivityFailed(compareConfig.FailedFeedback, currentResult);
            }
        }

        /// <summary>
        /// Clear all spawned objects.
        /// </summary>
        private void ClearSpawnedObjects()
        {
            if (leftGroup != null)
            {
                interactionService?.UnregisterInteractable(leftGroup);
                Destroy(leftGroup);
                leftGroup = null;
            }

            if (rightGroup != null)
            {
                interactionService?.UnregisterInteractable(rightGroup);
                Destroy(rightGroup);
                rightGroup = null;
            }

            // Also clear via placement service
            placementService?.ClearSpawnedObjects();
        }

        /// <summary>
        /// Clean up resources.
        /// </summary>
        public override void Cleanup()
        {
            // Unsubscribe from events
            if (view != null)
            {
                view.OnAnswerSelected -= HandleAnswerSelected;
                view.OnHintRequested -= HandleHintRequested;
                view.OnCancelRequested -= HandleCancelRequested;
            }

            ClearSpawnedObjects();

            base.Cleanup();
        }
    }
}
