using Core.Learning.ActivityRunner;
using Core.Learning.Models;
using Features.Activities.QuantityMatch;
using System;
using UnityEngine;

namespace Features.Activities.QuantityMatch
{
    /// <summary>
    /// Presenter for Quantity Match activity.
    /// Handles all game logic and coordinates between View, AR services, and data.
    /// </summary>
    public class QuantityMatchPresenter : ActivityPresenter
    {
        // AR Service references (will be injected or found)
        private IARPlacementService placementService;
        private IARInteractionService interactionService;

        // Current question data
        private QuantityMatchQuestion currentQuestion;
        private QuantityMatchConfig quantityConfig;

        // View reference
        private IQuantityMatchView view;

        // Spawned objects tracking
        private GameObject[] spawnedGroups;

        // Events specific to Quantity Match
        public event Action<int, string> OnGroupSpawned;  // groupIndex, groupId
        public event Action OnAllGroupsSpawned;

        /// <summary>
        /// Initialize the presenter with config and view.
        /// </summary>
        public void Initialize(QuantityMatchConfig config, IQuantityMatchView activityView,
            IARPlacementService arPlacement, IARInteractionService arInteraction)
        {
            quantityConfig = config;
            view = activityView;
            placementService = arPlacement;
            interactionService = arInteraction;

            // Initialize base presenter
            base.Initialize(config);

            // Initialize view
            view?.Initialize(this);

            // Subscribe to view events
            if (view != null)
            {
                view.OnGroupSelected += HandleGroupSelected;
                view.OnHintRequested += HandleHintRequested;
                view.OnCancelRequested += HandleCancelRequested;
            }

            // Subscribe to AR interaction events
            if (interactionService != null)
            {
                interactionService.OnObjectTapped += HandleObjectTapped;
            }
        }

        /// <summary>
        /// Load and display a specific round/question.
        /// </summary>
        protected override void LoadRound(int roundNumber)
        {
            currentQuestion = quantityConfig.GetQuestion(roundNumber - 1);
            if (currentQuestion == null)
            {
                Debug.LogError($"[QuantityMatchPresenter] Failed to load question {roundNumber}");
                return;
            }

            Debug.Log($"[QuantityMatchPresenter] Loading round {roundNumber}: Target = {currentQuestion.TargetNumber}");

            // Clear previous objects
            ClearSpawnedObjects();

            // Spawn the groups
            SpawnGroups();

            // Update view
            view?.ShowQuestion(currentQuestion.TargetNumber, currentQuestion.NumberOfGroups);
            view?.UpdateProgress(roundNumber, quantityConfig.NumberOfRounds);
        }

        /// <summary>
        /// Spawn all groups for the current question.
        /// </summary>
        private void SpawnGroups()
        {
            if (placementService == null)
            {
                Debug.LogError("[QuantityMatchPresenter] AR Placement Service not available. Cannot spawn groups.");
                // TODO: Show error in UI
                return;
            }

            spawnedGroups = new GameObject[currentQuestion.NumberOfGroups];

            // Calculate positions based on arrangement pattern
            Vector3[] groupPositions = CalculateGroupPositions();

            // Spawn each group
            for (int i = 0; i < currentQuestion.NumberOfGroups; i++)
            {
                Vector3 groupPosition = groupPositions[i];
                int objectCount = currentQuestion.ObjectCountsPerGroup[i];

                // Spawn the group as objects arranged in a grid/circle
                GameObject group = SpawnGroupObjects(i, objectCount, groupPosition);
                spawnedGroups[i] = group;

                // Register the group as interactable
                if (interactionService != null)
                {
                    interactionService.RegisterInteractable(group, i);  // Store group index as data
                }

                OnGroupSpawned?.Invoke(i, group.name);
            }

            OnAllGroupsSpawned?.Invoke();
        }

        /// <summary>
        /// Calculate positions for all groups based on arrangement pattern.
        /// </summary>
        private Vector3[] CalculateGroupPositions()
        {
            Vector3[] positions = new Vector3[currentQuestion.NumberOfGroups];
            Vector3 center = placementService.CurrentPlacementPosition;
            float spacing = quantityConfig.DefaultGroupSpacing;

            switch (quantityConfig.GroupArrangement)
            {
                case GroupArrangementPattern.Horizontal:
                    // Arrange in a horizontal row
                    float totalWidth = (currentQuestion.NumberOfGroups - 1) * spacing;
                    float startX = center.x - totalWidth / 2f;

                    for (int i = 0; i < currentQuestion.NumberOfGroups; i++)
                    {
                        positions[i] = new Vector3(startX + i * spacing, center.y, center.z);
                    }
                    break;

                case GroupArrangementPattern.Vertical:
                    // Arrange in a vertical column
                    float totalHeight = (currentQuestion.NumberOfGroups - 1) * spacing;
                    float startY = center.y - totalHeight / 2f;

                    for (int i = 0; i < currentQuestion.NumberOfGroups; i++)
                    {
                        positions[i] = new Vector3(center.x, startY + i * spacing, center.z);
                    }
                    break;

                case GroupArrangementPattern.Circular:
                    // Arrange in a circle
                    float radius = spacing * 0.8f;
                    for (int i = 0; i < currentQuestion.NumberOfGroups; i++)
                    {
                        float angle = (360f / currentQuestion.NumberOfGroups) * i * Mathf.Deg2Rad;
                        positions[i] = center + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
                    }
                    break;

                case GroupArrangementPattern.Random:
                    // Random positions within bounds
                    for (int i = 0; i < currentQuestion.NumberOfGroups; i++)
                    {
                        positions[i] = center + new Vector3(
                            UnityEngine.Random.Range(-spacing, spacing),
                            0,
                            UnityEngine.Random.Range(-spacing, spacing)
                        );
                    }
                    break;
            }

            return positions;
        }

        /// <summary>
        /// Spawn a single group with the given number of objects.
        /// </summary>
        private GameObject SpawnGroupObjects(int groupIndex, int objectCount, Vector3 position)
        {
            // Get the prefab to use
            // TODO: Load prefab based on currentQuestion.ObjectPrefabName or default
            GameObject prefab = GetObjectPrefab();
            if (prefab == null)
            {
                Debug.LogError($"[QuantityMatchPresenter] No prefab available for group {groupIndex}");
                return new GameObject($"Placeholder_Group{groupIndex}");
            }

            // Spawn objects in a small cluster
            GameObject[] objects = placementService?.SpawnCircle(prefab, position, objectCount, 0.2f);
            if (objects == null || objects.Length == 0)
            {
                return new GameObject($"Placeholder_Group{groupIndex}");
            }

            // Create a parent object for the group
            GameObject group = new GameObject($"QuantityGroup_{groupIndex}_Count{objectCount}");
            group.transform.position = position;

            foreach (GameObject obj in objects)
            {
                if (obj != null)
                {
                    obj.transform.SetParent(group.transform);
                }
            }

            return group;
        }

        /// <summary>
        /// Get the prefab to use for spawning objects.
        /// TODO: Load from resources or use a default.
        /// </summary>
        private GameObject GetObjectPrefab()
        {
            // TODO: Implement prefab loading
            // For now, return null - AR team will need to provide prefabs
            Debug.LogWarning("[QuantityMatchPresenter] GetObjectPrefab() not implemented. AR team needs to provide prefabs.");
            return null;
        }

        /// <summary>
        /// Check if the given answer is correct.
        /// </summary>
        protected override bool CheckAnswer(ActivityAnswer answer)
        {
            if (answer is not QuantityMatchAnswer quantityAnswer)
            {
                Debug.LogError($"[QuantityMatchPresenter] Answer is not a QuantityMatchAnswer");
                return false;
            }

            // Check if the selected group's count matches the target number
            bool isCorrect = currentQuestion.IsCorrectAnswer(quantityAnswer.SelectedGroupIndex);

            Debug.Log($"[QuantityMatchPresenter] Checking answer: Group {quantityAnswer.SelectedGroupIndex} " +
                     $"(count: {quantityAnswer.SelectedGroupCount}) vs Target {currentQuestion.TargetNumber} " +
                     $"-> {(isCorrect ? "CORRECT" : "INCORRECT")}");

            return isCorrect;
        }

        /// <summary>
        /// Get the error type for an incorrect answer.
        /// </summary>
        protected override ErrorType? GetErrorType(ActivityAnswer answer)
        {
            if (answer is not QuantityMatchAnswer quantityAnswer)
            {
                return Models.ErrorType.Other;
            }

            // Determine the specific error type based on the wrong selection
            int selectedCount = quantityAnswer.SelectedGroupCount;
            int targetCount = currentQuestion.TargetNumber;

            if (selectedCount < targetCount)
            {
                // Selected a group with fewer objects
                return Models.ErrorType.WrongQuantity;  // Could add "TooFew" if needed
            }
            else if (selectedCount > targetCount)
            {
                // Selected a group with more objects
                return Models.ErrorType.WrongQuantity;  // Could add "TooMany" if needed
            }
            else
            {
                // Count matches but wrong group (shouldn't happen with current design)
                return Models.ErrorType.Other;
            }
        }

        /// <summary>
        /// Handle when a group is selected via the View.
        /// </summary>
        private void HandleGroupSelected(int groupIndex, int groupObjectCount)
        {
            if (currentState != ActivityState.InProgress)
            {
                return;
            }

            Debug.Log($"[QuantityMatchPresenter] Group selected: {groupIndex}");

            // Create the answer
            var answer = new QuantityMatchAnswer(
                selectedGroupIndex: groupIndex,
                selectedGroupCount: groupObjectCount,
                targetNumber: currentQuestion.TargetNumber,
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
        /// Handle AR object tap from interaction service.
        /// </summary>
        private void HandleObjectTapped(GameObject tappedObject)
        {
            if (currentState != ActivityState.InProgress)
            {
                return;
            }

            // Get the group index from the tapped object
            if (interactionService != null)
            {
                object data = interactionService.GetInteractableData(tappedObject);
                if (data is int groupIndex)
                {
                    // Get the object count for this group
                    int objectCount = currentQuestion.ObjectCountsPerGroup[groupIndex];
                    HandleGroupSelected(groupIndex, objectCount);
                }
            }
        }

        /// <summary>
        /// Called when a hint is provided. Override to pass to View with proper formatting.
        /// </summary>
        protected override void OnHintProvided(ActivityHint hint)
        {
            base.OnHintProvided(hint);

            // Format the hint with the target number
            string formattedHint = quantityConfig.GetFormattedHintText(hint, currentQuestion.TargetNumber);
            hint.HintText = formattedHint;

            // Pass to view
            view?.ShowHint(hint);
        }

        /// <summary>
        /// Handle correct answer with feedback.
        /// </summary>
        protected override void HandleCorrectAnswer(ActivityAnswer answer)
        {
            // Show feedback in view
            view?.ShowCorrectFeedback(quantityConfig.CorrectFeedback);

            // Call base to complete the round
            base.HandleCorrectAnswer(answer);
        }

        /// <summary>
        /// Handle incorrect answer with feedback.
        /// </summary>
        protected override void HandleIncorrectAnswer(ActivityAnswer answer)
        {
            // Show feedback in view
            view?.ShowIncorrectFeedback(quantityConfig.IncorrectFeedback);

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
                view?.ShowActivityFailed(quantityConfig.FailedFeedback, currentResult);
            }
        }

        /// <summary>
        /// Clear all spawned objects.
        /// </summary>
        private void ClearSpawnedObjects()
        {
            if (spawnedGroups != null)
            {
                foreach (GameObject group in spawnedGroups)
                {
                    if (group != null)
                    {
                        // Unregister from interaction service
                        interactionService?.UnregisterInteractable(group);

                        Destroy(group);
                    }
                }
                spawnedGroups = null;
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
                view.OnGroupSelected -= HandleGroupSelected;
                view.OnHintRequested -= HandleHintRequested;
                view.OnCancelRequested -= HandleCancelRequested;
            }

            if (interactionService != null)
            {
                interactionService.OnObjectTapped -= HandleObjectTapped;
            }

            ClearSpawnedObjects();

            base.Cleanup();
        }
    }
}
