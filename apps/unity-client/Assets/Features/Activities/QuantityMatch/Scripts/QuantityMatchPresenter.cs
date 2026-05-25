using Core.Learning.ActivityRunner;
using Core.Learning.Models;
using Features.Activities;
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
        private const int SelectionQuestionCount = 5;
        private const float MinimumReadableObjectSpacing = 0.64f;
        private const float MaximumReadableObjectSpacing = 0.78f;
        private const float MinimumReadableGroupSpacing = 2.1f;
        private const float GroupSeparationPadding = 0.5f;
        private const float GroupHitboxHeight = 0.62f;
        private const float GroupHitboxPadding = 0.44f;
        private const float ViewportSafeMarginX = 0.16f;
        private const float ViewportSafeMarginY = 0.18f;
        private bool currentUsesNumberInputMode;

        [Header("Prefabs")]
        [SerializeField]
        private GameObject defaultObjectPrefab;

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
                view.OnNumberAnswerSubmitted += HandleNumberAnswerSubmitted;
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
            currentUsesNumberInputMode = roundNumber > SelectionQuestionCount;

            // Clear previous objects
            ClearSpawnedObjects();

            // Spawn the groups
            SpawnGroups();

            // Update view
            view?.ShowQuestion(currentQuestion.TargetNumber, currentQuestion.NumberOfGroups, currentUsesNumberInputMode);
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
            float spacing = GetReadableGroupSpacing();

            switch (quantityConfig.GroupArrangement)
            {
                case GroupArrangementPattern.Horizontal:
                    if (currentQuestion.NumberOfGroups == 3)
                    {
                        Vector3 right = Vector3.right;
                        Vector3 forward = Vector3.forward;
                        Camera camera = Camera.main;

                        if (camera != null)
                        {
                            right = camera.transform.right;
                            right.y = 0f;
                            if (right.sqrMagnitude < 0.0001f)
                            {
                                right = Vector3.right;
                            }
                            right.Normalize();

                            forward = camera.transform.forward;
                            forward.y = 0f;
                            if (forward.sqrMagnitude < 0.0001f)
                            {
                                forward = Vector3.forward;
                            }
                            forward.Normalize();
                        }

                        float sideOffset = spacing * 0.74f;
                        float depthOffset = spacing * 0.5f;
                        positions[0] = center - right * sideOffset + forward * depthOffset * 0.28f;
                        positions[1] = center - forward * depthOffset * 0.2f;
                        positions[2] = center + right * sideOffset + forward * depthOffset * 0.28f;
                    }
                    else
                    {
                        float totalWidth = (currentQuestion.NumberOfGroups - 1) * spacing;
                        float startX = center.x - totalWidth / 2f;

                        for (int i = 0; i < currentQuestion.NumberOfGroups; i++)
                        {
                            positions[i] = new Vector3(startX + i * spacing, center.y, center.z);
                        }
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

            KeepGroupPositionsInsideCameraView(positions);
            return positions;
        }

        /// <summary>
        /// Spawn a single group with the given number of objects.
        /// </summary>
        private GameObject SpawnGroupObjects(int groupIndex, int objectCount, Vector3 position)
        {
            GameObject fallbackPrefab = GetObjectPrefab();
            if (fallbackPrefab == null)
            {
                Debug.LogError($"[QuantityMatchPresenter] No prefab available for group {groupIndex}");
                return new GameObject($"Placeholder_Group{groupIndex}");
            }

            GameObject group = new GameObject($"QuantityGroup_{groupIndex + 1}_Count{objectCount}");
            group.transform.SetPositionAndRotation(position, CalculateReadableGroupRotation(position));

            float spacing = GetReadableObjectSpacing();
            Vector3[] localPositions = CalculateReadableGroundGridPositions(objectCount, spacing, out float width, out float depth);

            for (int i = 0; i < localPositions.Length; i++)
            {
                Vector3 worldPosition = group.transform.TransformPoint(localPositions[i]);
                GameObject objectPrefab = GetObjectPrefab() ?? fallbackPrefab;
                GameObject obj = placementService?.SpawnAtPosition(objectPrefab, worldPosition, group.transform.rotation, group.transform);
                if (obj != null)
                {
                    obj.name = $"Group{groupIndex + 1}_Animal{i + 1}";
                    obj.transform.localPosition = localPositions[i];
                    obj.transform.localRotation = Quaternion.Euler(0f, UnityEngine.Random.Range(-14f, 14f), 0f);
                    ActivityPrefabSetup.Instance?.PrepareLearningObject(obj);
                }
            }

            AddGroupHitbox(group, width, depth);
            if (!currentUsesNumberInputMode)
            {
                AddGroupLabel(group, groupIndex, depth);
            }
            return group;
        }

        private static Quaternion CalculateReadableGroupRotation(Vector3 groupPosition)
        {
            Camera camera = Camera.main;
            if (camera == null)
            {
                return Quaternion.identity;
            }

            Vector3 direction = camera.transform.position - groupPosition;
            direction.y = 0f;

            if (direction.sqrMagnitude < 0.0001f)
            {
                return Quaternion.identity;
            }

            return Quaternion.LookRotation(direction.normalized, Vector3.up);
        }

        private float GetReadableObjectSpacing()
        {
            return Mathf.Clamp(quantityConfig.DefaultObjectSpacing, MinimumReadableObjectSpacing, MaximumReadableObjectSpacing);
        }

        private float GetReadableGroupSpacing()
        {
            if (currentQuestion?.ObjectCountsPerGroup == null)
            {
                return Mathf.Max(quantityConfig.DefaultGroupSpacing, MinimumReadableGroupSpacing);
            }

            float objectSpacing = GetReadableObjectSpacing();
            float largestFootprint = 0f;
            for (int i = 0; i < currentQuestion.ObjectCountsPerGroup.Length; i++)
            {
                CalculateReadableGroundGridPositions(
                    currentQuestion.ObjectCountsPerGroup[i],
                    objectSpacing,
                    out float width,
                    out float depth);

                largestFootprint = Mathf.Max(largestFootprint, Mathf.Max(width, depth) + objectSpacing);
            }

            return Mathf.Max(
                quantityConfig.DefaultGroupSpacing,
                MinimumReadableGroupSpacing,
                largestFootprint + GroupSeparationPadding);
        }

        private static Vector3[] CalculateReadableGroundGridPositions(int count, float spacing, out float width, out float depth)
        {
            if (count <= 0)
            {
                width = 0f;
                depth = 0f;
                return Array.Empty<Vector3>();
            }

            int columns = Mathf.CeilToInt(Mathf.Sqrt(count));
            int rows = Mathf.CeilToInt((float)count / columns);
            var positions = new Vector3[count];

            for (int i = 0; i < count; i++)
            {
                int row = i / columns;
                int col = i % columns;
                int remaining = count - row * columns;
                int itemsInRow = Mathf.Min(columns, remaining);
                float rowWidth = Mathf.Max(0f, (itemsInRow - 1) * spacing);
                float rowStagger = rows > 1 && row % 2 == 1 ? spacing * 0.42f : 0f;
                positions[i] = new Vector3(col * spacing - rowWidth * 0.5f + rowStagger, 0f, -row * spacing);
            }

            CenterPositionsOnOrigin(positions, out width, out depth);

            return positions;
        }

        private static void CenterPositionsOnOrigin(Vector3[] positions, out float width, out float depth)
        {
            float minX = positions[0].x;
            float maxX = positions[0].x;
            float minZ = positions[0].z;
            float maxZ = positions[0].z;

            for (int i = 1; i < positions.Length; i++)
            {
                minX = Mathf.Min(minX, positions[i].x);
                maxX = Mathf.Max(maxX, positions[i].x);
                minZ = Mathf.Min(minZ, positions[i].z);
                maxZ = Mathf.Max(maxZ, positions[i].z);
            }

            Vector3 offset = new Vector3((minX + maxX) * 0.5f, 0f, (minZ + maxZ) * 0.5f);
            for (int i = 0; i < positions.Length; i++)
            {
                positions[i] -= offset;
            }

            width = maxX - minX;
            depth = maxZ - minZ;
        }

        private static void KeepGroupPositionsInsideCameraView(Vector3[] positions)
        {
            Camera camera = Camera.main;
            if (camera == null || positions == null)
            {
                return;
            }

            for (int i = 0; i < positions.Length; i++)
            {
                Vector3 viewport = camera.WorldToViewportPoint(positions[i]);
                if (viewport.z <= 0f)
                {
                    continue;
                }

                float safeX = Mathf.Clamp(viewport.x, ViewportSafeMarginX, 1f - ViewportSafeMarginX);
                float safeY = Mathf.Clamp(viewport.y, ViewportSafeMarginY, 1f - ViewportSafeMarginY);
                if (Mathf.Approximately(safeX, viewport.x) && Mathf.Approximately(safeY, viewport.y))
                {
                    continue;
                }

                Vector3 clampedWorld = camera.ViewportToWorldPoint(new Vector3(safeX, safeY, viewport.z));
                clampedWorld.y = positions[i].y;
                positions[i] = clampedWorld;
            }
        }

        private static void AddGroupHitbox(GameObject group, float width, float depth)
        {
            var collider = group.AddComponent<BoxCollider>();
            collider.center = Vector3.up * (GroupHitboxHeight * 0.5f);
            collider.size = new Vector3(
                Mathf.Max(0.55f, width + GroupHitboxPadding),
                GroupHitboxHeight,
                Mathf.Max(0.55f, depth + GroupHitboxPadding));
        }

        private static void AddGroupLabel(GameObject group, int groupIndex, float groupDepth)
        {
            var labelGo = new GameObject("GroupLabel");
            labelGo.transform.SetParent(group.transform, false);
            labelGo.transform.localPosition = new Vector3(0f, 0.58f, -groupDepth * 0.5f - 0.12f);

            var label = labelGo.AddComponent<TextMesh>();
            label.text = $"Group {groupIndex + 1}";
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.fontSize = 56;
            label.characterSize = 0.012f;
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
            if (defaultObjectPrefab != null)
            {
                return defaultObjectPrefab;
            }

            if (ActivityPrefabSetup.Instance != null)
            {
                return ActivityPrefabSetup.Instance.GetLearningObjectPrefab();
            }

            Debug.LogWarning("[QuantityMatchPresenter] No object prefab assigned. Assign defaultObjectPrefab or add ActivityPrefabSetup.");
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

            bool isCorrect = currentUsesNumberInputMode
                ? quantityAnswer.SelectedGroupCount == currentQuestion.TargetNumber
                : currentQuestion.IsCorrectAnswer(quantityAnswer.SelectedGroupIndex);

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
                return ErrorType.Other;
            }

            // Determine the specific error type based on the wrong selection
            int selectedCount = quantityAnswer.SelectedGroupCount;
            int targetCount = currentQuestion.TargetNumber;

            if (selectedCount < targetCount)
            {
                // Selected a group with fewer objects
                return ErrorType.WrongQuantity;  // Could add "TooFew" if needed
            }
            else if (selectedCount > targetCount)
            {
                // Selected a group with more objects
                return ErrorType.WrongQuantity;  // Could add "TooMany" if needed
            }
            else
            {
                // Count matches but wrong group (shouldn't happen with current design)
                return ErrorType.Other;
            }
        }

        /// <summary>
        /// Handle when a group is selected via the View.
        /// </summary>
        private void HandleGroupSelected(int groupIndex, int groupObjectCount)
        {
            if (currentState != ActivityState.InProgress || currentUsesNumberInputMode)
            {
                return;
            }

            if (groupObjectCount <= 0 && currentQuestion != null
                && groupIndex >= 0 && groupIndex < currentQuestion.ObjectCountsPerGroup.Length)
            {
                groupObjectCount = currentQuestion.ObjectCountsPerGroup[groupIndex];
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

        private void HandleNumberAnswerSubmitted(int enteredNumber)
        {
            if (currentState != ActivityState.InProgress || !currentUsesNumberInputMode)
            {
                return;
            }

            Debug.Log($"[QuantityMatchPresenter] Number answer submitted: {enteredNumber}");

            var answer = new QuantityMatchAnswer(
                selectedGroupIndex: -1,
                selectedGroupCount: enteredNumber,
                targetNumber: currentQuestion.TargetNumber,
                responseTimeSeconds: Time.time - roundStartTime,
                attemptNumber: currentResult.TotalAttempts + 1,
                hintsUsed: hintsUsedInCurrentRound > 0
            );

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
            if (currentState != ActivityState.InProgress || currentUsesNumberInputMode)
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

            if (currentState == ActivityState.Failed)
            {
                view?.ShowActivityFailed(quantityConfig.FailedFeedback, currentResult);
            }
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
                view.OnNumberAnswerSubmitted -= HandleNumberAnswerSubmitted;
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
