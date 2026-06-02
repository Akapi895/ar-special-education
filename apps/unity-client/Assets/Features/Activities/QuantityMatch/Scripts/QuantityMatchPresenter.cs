using Core.Learning.ActivityRunner;
using Core.Learning.Models;
using Features.Activities;
using System;
using System.Collections.Generic;
using UnityEngine;
using Core.UI.Components;
using UnityEngine.InputSystem;

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
        private readonly List<GameObject> simulationRoamBoundaries = new List<GameObject>();
        private bool currentUsesNumberInputMode;
        private int currentRoundNumber;
        private int[] countedTapsByGroup;

        private const float MinimumReadableObjectSpacing = 0.82f;
        private const float MaximumReadableObjectSpacing = 1.0f;
        private const float MinimumReadableGroupSpacing = 1.45f;
        private const float MaximumReadableGroupSpacing = 1.8f;
        private const float GroupSeparationPadding = 0.35f;
        private const float GroupHitboxHeight = 0.95f;
        private const float GroupHitboxPadding = 0.7f;
        private const float ViewportSafeMarginX = 0.16f;
        private const float ViewportSafeMarginY = 0.18f;
        private const float SimulationFreeRoamSpeed = 0.55f;
        private const float SimulationFreeRoamMinWait = 0.15f;
        private const float SimulationFreeRoamMaxWait = 0.9f;

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
            currentRoundNumber = roundNumber;
            currentUsesNumberInputMode = quantityConfig != null
                && quantityConfig.SwitchToNumberInputAtRound > 0
                && roundNumber >= quantityConfig.SwitchToNumberInputAtRound;

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
                view?.ShowActivityFailed("Khong tim thay dich vu AR. Hay quay lai va thu lai.", currentResult);
                return;
            }

            spawnedGroups = new GameObject[currentQuestion.NumberOfGroups];
            countedTapsByGroup = new int[currentQuestion.NumberOfGroups];
            if (UseSimulationFreeRoamMode())
            {
                SimulationAnimalCameraTracker.ClearTracking();
            }

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
                if (interactionService != null && !UseSimulationFreeRoamMode())
                {
                    interactionService.RegisterInteractable(group, i);  // Store group index as data
                }

                OnGroupSpawned?.Invoke(i, group.name);
            }

            OnAllGroupsSpawned?.Invoke();

            // FIX D: Set content anchor for camera proximity check
            if (view is QuantityMatchView quantityMatchView && spawnedGroups != null && spawnedGroups.Length > 0)
            {
                // Use the first group as reference for proximity check
                quantityMatchView.SetContentAnchor(spawnedGroups[0]);
            }
        }

        /// <summary>
        /// Calculate positions for all groups based on arrangement pattern.
        /// </summary>
        private Vector3[] CalculateGroupPositions()
        {
            Vector3[] positions = new Vector3[currentQuestion.NumberOfGroups];
            Vector3 center = GetLearningAreaCenter();
            if (UseSimulationFreeRoamMode())
            {
                return CalculateSimulationGroupPositions(center);
            }

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

                        float sideOffset = spacing * 0.85f;
                        float depthOffset = spacing * 0.65f;
                        // Inverted V shape layout (triangle pointing away from camera to fill lower screen space)
                        positions[0] = center - right * sideOffset - forward * depthOffset * 0.35f;
                        positions[1] = center + forward * depthOffset * 0.35f;
                        positions[2] = center + right * sideOffset - forward * depthOffset * 0.35f;
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
                    // Arrange in a ground-plane column instead of stacking groups upward.
                    float totalDepth = (currentQuestion.NumberOfGroups - 1) * spacing;
                    float startZ = center.z - totalDepth / 2f;

                    for (int i = 0; i < currentQuestion.NumberOfGroups; i++)
                    {
                        positions[i] = new Vector3(center.x, center.y, startZ + i * spacing);
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

        private Vector3[] CalculateSimulationGroupPositions(Vector3 center)
        {
            int groupCount = currentQuestion.NumberOfGroups;
            Vector3[] positions = new Vector3[groupCount];
            if (groupCount <= 0)
            {
                return positions;
            }

            float spacing = GetSimulationGroupSpacing();
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

            if (groupCount == 1)
            {
                positions[0] = center;
                return positions;
            }

            if (groupCount <= 3)
            {
                float start = -(groupCount - 1) * spacing * 0.5f;
                for (int i = 0; i < groupCount; i++)
                {
                    positions[i] = center + right * (start + i * spacing);
                }
                return positions;
            }

            float ringRadius = spacing / (2f * Mathf.Sin(Mathf.PI / groupCount));
            for (int i = 0; i < groupCount; i++)
            {
                float angle = (360f / groupCount) * i * Mathf.Deg2Rad;
                positions[i] = center
                    + right * (Mathf.Cos(angle) * ringRadius)
                    + forward * (Mathf.Sin(angle) * ringRadius);
            }

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
            if (placementService?.HasLearningArea == true && placementService.LearningAreaContentRoot != null)
            {
                group.transform.SetParent(placementService.LearningAreaContentRoot, true);
            }

            bool freeRoamMode = UseSimulationFreeRoamMode();
            float spacing = GetReadableObjectSpacing();
            Vector3[] localPositions = CalculateReadableGroundGridPositions(objectCount, spacing, out float width, out float depth);
            float groupRoamRadius = GetSimulationGroupRoamRadius(width, depth);
            if (freeRoamMode)
            {
                CreateSimulationGroupRoamBoundary(group, groupIndex, groupRoamRadius);
            }

            // Get a specific animal prefab for this group to ensure only 1 species of animal per group
            GameObject groupPrefab = null;
            if (ActivityPrefabSetup.Instance != null)
            {
                int animalIndex = (currentRoundNumber * 3 + groupIndex);
                groupPrefab = ActivityPrefabSetup.Instance.GetAnimalPrefab(animalIndex);
            }
            if (groupPrefab == null)
            {
                groupPrefab = GetObjectPrefab() ?? fallbackPrefab;
            }

            for (int i = 0; i < localPositions.Length; i++)
            {
                Vector3 worldPosition = group.transform.TransformPoint(localPositions[i]);
                Transform animalParent = freeRoamMode && placementService?.LearningAreaContentRoot != null
                    ? placementService.LearningAreaContentRoot
                    : group.transform;
                GameObject obj = placementService?.SpawnAtPosition(groupPrefab, worldPosition, group.transform.rotation, animalParent);
                if (obj != null)
                {
                    obj.name = $"Group{groupIndex + 1}_Animal{i + 1}";
                    if (freeRoamMode)
                    {
                        obj.transform.position = worldPosition;
                        obj.transform.rotation = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);
                    }
                    else
                    {
                        obj.transform.localPosition = localPositions[i];
                        obj.transform.localRotation = Quaternion.Euler(0f, UnityEngine.Random.Range(-14f, 14f), 0f);
                    }

                    ActivityPrefabSetup.Instance?.PrepareLearningObject(obj, true);
                    if (freeRoamMode)
                    {
                        ConfigureSimulationFreeRoam(obj, group.transform.position, groupRoamRadius);
                        SimulationAnimalCameraTracker.RegisterAnimal(obj.transform);
                    }
                }
            }

            if (freeRoamMode)
            {
                return group;
            }

            AddGroupHitbox(group, width, depth);
            var indicatorGo = new GameObject("AreaIndicator");
            indicatorGo.transform.SetParent(group.transform, false);
            indicatorGo.transform.localPosition = Vector3.zero;
            var indicator = indicatorGo.AddComponent<GroupAreaIndicator>();
            
            Color groupColor;
            if (groupIndex == 0) groupColor = new Color(0.45f, 0.68f, 0.9f); // blue
            else if (groupIndex == 1) groupColor = new Color(0.95f, 0.6f, 0.4f); // orange
            else groupColor = new Color(0.45f, 0.8f, 0.5f); // green
            
            float maxFootprint = Mathf.Max(width, depth);
            indicator.radius = Mathf.Max(0.7f, maxFootprint * 0.5f + 0.35f);
            indicator.SetColor(groupColor);

            if (!currentUsesNumberInputMode)
            {
                AddGroupLabel(group, groupIndex, depth);
            }
            return group;
        }

        private void ConfigureSimulationFreeRoam(GameObject animal, Vector3 groupCenterWorld, float roamRadius)
        {
            if (animal == null)
            {
                return;
            }

            var presentation = animal.GetComponent<ARAnimalPresentation>();
            if (presentation == null)
            {
                presentation = animal.AddComponent<ARAnimalPresentation>();
            }

            presentation.ConfigureWandering(
                true,
                roamRadius,
                SimulationFreeRoamSpeed,
                SimulationFreeRoamMinWait,
                SimulationFreeRoamMaxWait,
                true);
            presentation.ConfigureFixedWanderArea(GetSimulationFreeRoamCenterLocal(animal.transform, groupCenterWorld), roamRadius);
            presentation.ResetBasePose();
        }

        private void CreateSimulationGroupRoamBoundary(GameObject group, int groupIndex, float radius)
        {
            if (group == null)
            {
                return;
            }

            var boundary = new GameObject($"SimulationGroup{groupIndex + 1}RoamCircle");
            boundary.transform.SetParent(group.transform, false);
            boundary.transform.localPosition = Vector3.zero;
            boundary.transform.localRotation = Quaternion.identity;
            simulationRoamBoundaries.Add(boundary);

            var indicator = boundary.AddComponent<GroupAreaIndicator>();
            indicator.radius = radius;
            indicator.pulseSpeed = 0.6f;
            indicator.SetColor(GetGroupColor(groupIndex));

            var selectionCollider = boundary.AddComponent<SphereCollider>();
            selectionCollider.center = Vector3.up * 0.04f;
            selectionCollider.radius = radius;
            selectionCollider.isTrigger = true;

            interactionService?.RegisterInteractable(boundary, groupIndex);
            SimulationAnimalCameraTracker.AddRoamArea(group.transform.position, radius);
        }

        private Vector3 GetSimulationFreeRoamCenterLocal(Transform target, Vector3 centerWorld)
        {
            Transform parent = target != null ? target.parent : null;
            return parent != null ? parent.InverseTransformPoint(centerWorld) : centerWorld;
        }

        private static float GetSimulationGroupRoamRadius(float width, float depth)
        {
            float contentRadius = Mathf.Max(width, depth) * 0.5f;
            return Mathf.Clamp(contentRadius + 0.55f, 0.9f, 1.7f);
        }

        private float GetSimulationGroupSpacing()
        {
            if (currentQuestion?.ObjectCountsPerGroup == null)
            {
                return 2.4f;
            }

            float spacing = GetReadableObjectSpacing();
            float maxRadius = 0.9f;
            for (int i = 0; i < currentQuestion.ObjectCountsPerGroup.Length; i++)
            {
                CalculateReadableGroundGridPositions(
                    currentQuestion.ObjectCountsPerGroup[i],
                    spacing,
                    out float width,
                    out float depth);
                maxRadius = Mathf.Max(maxRadius, GetSimulationGroupRoamRadius(width, depth));
            }

            return maxRadius * 2f + 0.6f;
        }

        private static Color GetGroupColor(int groupIndex)
        {
            if (groupIndex == 0)
            {
                return new Color(0.45f, 0.68f, 0.9f, 0.72f);
            }

            if (groupIndex == 1)
            {
                return new Color(0.95f, 0.6f, 0.4f, 0.72f);
            }

            return new Color(0.45f, 0.8f, 0.5f, 0.72f);
        }

        private static bool UseSimulationFreeRoamMode()
        {
            return Application.isEditor && !Application.isMobilePlatform;
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

        private Vector3 GetLearningAreaCenter()
        {
            if (placementService?.HasLearningArea == true && placementService.LearningAreaContentRoot != null)
            {
                return placementService.LearningAreaContentRoot.position;
            }

            return placementService != null ? placementService.CurrentPlacementPosition : Vector3.zero;
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

            float desiredSpacing = Mathf.Max(
                quantityConfig.DefaultGroupSpacing,
                MinimumReadableGroupSpacing,
                largestFootprint + GroupSeparationPadding);

            return Mathf.Clamp(desiredSpacing, MinimumReadableGroupSpacing, MaximumReadableGroupSpacing);
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
            // Position it above the animal group center (y = 0.65f)
            labelGo.transform.localPosition = new Vector3(0f, 0.65f, 0f);

            // Background Board (Pill shape)
            var bgGo = new GameObject("LabelBackground");
            bgGo.transform.SetParent(labelGo.transform, false);
            bgGo.transform.localPosition = Vector3.zero;
            bgGo.transform.localScale = new Vector3(0.35f, 0.22f, 1f);
            
            var spriteRenderer = bgGo.AddComponent<SpriteRenderer>();
            Color bgColor;
            if (groupIndex == 0) bgColor = new Color(0.2f, 0.5f, 0.9f, 0.85f); // blue
            else if (groupIndex == 1) bgColor = new Color(0.95f, 0.55f, 0.2f, 0.85f); // orange
            else bgColor = new Color(0.3f, 0.75f, 0.4f, 0.85f); // green

            spriteRenderer.sprite = CreateRoundedRectSprite(128, 64, 18, bgColor);

            // Large White Number
            var textGo = new GameObject("LabelText");
            textGo.transform.SetParent(labelGo.transform, false);
            textGo.transform.localPosition = new Vector3(0f, 0f, -0.01f); // slightly forward to prevent z-fighting

            var label = textGo.AddComponent<TextMesh>();
            label.text = $"{groupIndex + 1}"; // Just "1", "2", "3"
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.fontSize = 80;
            label.characterSize = 0.007f;
            label.color = Color.white;
            label.font = UIKidFriendlyStyle.GetSharedFont();

            // Billboard component
            labelGo.AddComponent<BillboardBehavior>();
        }

        /// <summary>
        /// Get the prefab to use for spawning objects.
        /// Uses the scene-level ActivityPrefabSetup fallback when no explicit prefab is assigned.
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
            if (currentState != ActivityState.InProgress)
            {
                return;
            }

            if (interactionService != null)
            {
                object data = interactionService.GetInteractableData(tappedObject);
                if (data is int groupIndex)
                {
                    HandleGroupCountTap(groupIndex, tappedObject);
                }
            }
        }

        private void HandleGroupCountTap(int groupIndex, GameObject tappedObject)
        {
            if (currentQuestion == null
                || groupIndex < 0
                || groupIndex >= currentQuestion.ObjectCountsPerGroup.Length)
            {
                return;
            }

            int objectCount = currentQuestion.ObjectCountsPerGroup[groupIndex];
            var indicator = tappedObject.GetComponentInChildren<GroupAreaIndicator>();
            if (indicator != null)
            {
                indicator.Highlight();
            }

            if (UseSimulationFreeRoamMode() && !currentUsesNumberInputMode)
            {
                HandleGroupSelected(groupIndex, objectCount);
                return;
            }

            if (countedTapsByGroup == null || countedTapsByGroup.Length != currentQuestion.NumberOfGroups)
            {
                countedTapsByGroup = new int[currentQuestion.NumberOfGroups];
            }

            countedTapsByGroup[groupIndex]++;
            if (objectCount > 0 && countedTapsByGroup[groupIndex] > objectCount)
            {
                countedTapsByGroup[groupIndex] = 1;
            }

            view?.ShowCountingFeedback(groupIndex, countedTapsByGroup[groupIndex], objectCount);
            Debug.Log($"[QuantityMatchPresenter] Count tap on group {groupIndex}: {countedTapsByGroup[groupIndex]}/{objectCount}");

            if (!currentUsesNumberInputMode && objectCount > 0 && countedTapsByGroup[groupIndex] >= objectCount)
            {
                HandleGroupSelected(groupIndex, objectCount);
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

            if (answer is QuantityMatchAnswer qAnswer && qAnswer.SelectedGroupIndex >= 0)
            {
                int groupIdx = qAnswer.SelectedGroupIndex;
                if (spawnedGroups != null && groupIdx >= 0 && groupIdx < spawnedGroups.Length)
                {
                    var indicator = spawnedGroups[groupIdx].GetComponentInChildren<GroupAreaIndicator>();
                    if (indicator != null)
                    {
                        indicator.SetColor(Color.green);
                        indicator.Highlight();
                    }
                }
            }

            // Call base to complete the round
            base.HandleCorrectAnswer(answer);
        }

        protected override void HandleIncorrectAnswer(ActivityAnswer answer)
        {
            // Show feedback in view
            view?.ShowIncorrectFeedback(quantityConfig.IncorrectFeedback);

            if (answer is QuantityMatchAnswer qAnswer && qAnswer.SelectedGroupIndex >= 0)
            {
                int groupIdx = qAnswer.SelectedGroupIndex;
                if (spawnedGroups != null && groupIdx >= 0 && groupIdx < spawnedGroups.Length)
                {
                    var indicator = spawnedGroups[groupIdx].GetComponentInChildren<GroupAreaIndicator>();
                    if (indicator != null)
                    {
                        indicator.FlashIncorrect();
                    }
                }
            }

            // Call base to handle retry or failure
            base.HandleIncorrectAnswer(answer);

            if (currentState == ActivityState.Failed)
            {
                view?.ShowActivityFailed(quantityConfig.FailedFeedback, currentResult);
            }
            else if (answer.AttemptNumber >= 2)
            {
                // Auto-show hint after 1.5 seconds when incorrect feedback hides
                Invoke(nameof(AutoProvideHint), 1.5f);
            }
        }

        private void AutoProvideHint()
        {
            if (currentState == ActivityState.InProgress)
            {
                RequestHint();
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
            for (int i = simulationRoamBoundaries.Count - 1; i >= 0; i--)
            {
                if (simulationRoamBoundaries[i] != null)
                {
                    interactionService?.UnregisterInteractable(simulationRoamBoundaries[i]);
                    Destroy(simulationRoamBoundaries[i]);
                }
            }
            simulationRoamBoundaries.Clear();
            SimulationAnimalCameraTracker.ClearTracking();

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

        private static Sprite CreateRoundedRectSprite(int width, int height, int radius, Color color)
        {
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int cx = x < radius ? radius : (x >= width - radius ? width - radius - 1 : x);
                    int cy = y < radius ? radius : (y >= height - radius ? height - radius - 1 : y);
                    
                    float dx = x - cx;
                    float dy = y - cy;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    
                    if (dist > radius)
                    {
                        float diff = dist - radius;
                        if (diff < 1f)
                        {
                            pixels[y * width + x] = new Color(color.r, color.g, color.b, color.a * (1f - diff));
                        }
                        else
                        {
                            pixels[y * width + x] = Color.clear;
                        }
                    }
                    else
                    {
                        pixels[y * width + x] = color;
                    }
                }
            }
            texture.SetPixels(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f));
        }
    }

    public class BillboardBehavior : MonoBehaviour
    {
        private Camera mainCamera;
        private Vector3 baseScale;

        private void Start()
        {
            mainCamera = Camera.main;
            baseScale = transform.localScale;
        }

        private void LateUpdate()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
                if (mainCamera == null) return;
            }

            Vector3 direction = transform.position - mainCamera.transform.position;

            if (direction.sqrMagnitude > 0.0001f)
            {
                transform.rotation = Quaternion.LookRotation(direction.normalized, mainCamera.transform.up);
            }

            // Gently pulsate label size to signify interactivity for kids
            float pulse = 1f + Mathf.Sin(Time.time * 2.5f) * 0.06f;
            transform.localScale = baseScale * pulse;
        }
    }

    internal sealed class SimulationAnimalCameraTracker : MonoBehaviour
    {
        private const int XrSimulationEnvironmentLayer = 30;
        private const float MinimumFarClipPlane = 250f;
        private const float AutoFrameDelaySeconds = 1.0f;
        private const float PreferredPitchDegrees = 22f;
        private const float MinimumFrameDistance = 4.0f;
        private const float MaximumFrameDistance = 18.0f;
        private const float FramePadding = 1.45f;
        private const float OrbitSensitivity = 0.16f;
        private const float ZoomSensitivity = 0.012f;
        private const float MinimumOrbitPitch = 10f;
        private const float MaximumOrbitPitch = 72f;

        private static readonly List<Transform> Animals = new List<Transform>();
        private static SimulationAnimalCameraTracker instance;
        private static bool hasRoamArea;
        private static Bounds roamAreaBounds;
        private static Vector3 roamAreaCenterWorld;
        private static float roamAreaRadius = 2.4f;

        private Camera trackedCamera;
        private float allAnimalsOffscreenSince = -1f;
        private float orbitYaw;
        private float orbitPitch = PreferredPitchDegrees;
        private float orbitDistance = MinimumFrameDistance;
        private bool orbitInitialized;

        public static void ClearTracking()
        {
            Animals.Clear();
            hasRoamArea = false;
            roamAreaBounds = default;
            roamAreaCenterWorld = Vector3.zero;
            roamAreaRadius = 2.4f;

            if (instance != null)
            {
                instance.allAnimalsOffscreenSince = -1f;
                instance.orbitInitialized = false;
            }
        }

        public static void AddRoamArea(Vector3 centerWorld, float radius)
        {
            radius = Mathf.Max(0.5f, radius);
            Bounds areaBounds = new Bounds(centerWorld, new Vector3(radius * 2f, 1f, radius * 2f));
            if (hasRoamArea)
            {
                roamAreaBounds.Encapsulate(areaBounds);
            }
            else
            {
                roamAreaBounds = areaBounds;
            }

            hasRoamArea = true;
            roamAreaCenterWorld = roamAreaBounds.center;
            roamAreaRadius = Mathf.Max(roamAreaBounds.extents.x, roamAreaBounds.extents.z, radius);
            EnsureInstance();
            if (instance != null)
            {
                instance.orbitInitialized = false;
                instance.FrameAllAnimals();
            }
        }

        public static void RegisterAnimal(Transform animal)
        {
            if (animal == null || !Application.isEditor || Application.isMobilePlatform)
            {
                return;
            }

            Animals.RemoveAll(item => item == null);
            if (!Animals.Contains(animal))
            {
                Animals.Add(animal);
            }

            EnsureInstance();
        }

        private static void EnsureInstance()
        {
            Camera camera = Camera.main != null ? Camera.main : FindFirstObjectByType<Camera>();
            if (camera == null)
            {
                return;
            }

            if (instance == null || instance.trackedCamera != camera)
            {
                instance = camera.GetComponent<SimulationAnimalCameraTracker>();
                if (instance == null)
                {
                    instance = camera.gameObject.AddComponent<SimulationAnimalCameraTracker>();
                }
            }

            instance.trackedCamera = camera;
            instance.ConfigureCamera();
        }

        private void Awake()
        {
            trackedCamera = GetComponent<Camera>();
            ConfigureCamera();
        }

        private void LateUpdate()
        {
            if (!Application.isEditor || Application.isMobilePlatform)
            {
                return;
            }

            ConfigureCamera();
            Animals.RemoveAll(item => item == null);
            if (Animals.Count == 0)
            {
                allAnimalsOffscreenSince = -1f;
                return;
            }

            if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
            {
                FrameAllAnimals();
                allAnimalsOffscreenSince = -1f;
                return;
            }

            if (HandleManualCameraControls())
            {
                allAnimalsOffscreenSince = -1f;
                return;
            }

            if (AnyAnimalVisible())
            {
                allAnimalsOffscreenSince = -1f;
                return;
            }

            if (allAnimalsOffscreenSince < 0f)
            {
                allAnimalsOffscreenSince = Time.time;
                return;
            }

            if (Time.time - allAnimalsOffscreenSince >= AutoFrameDelaySeconds)
            {
                FrameAllAnimals();
                allAnimalsOffscreenSince = -1f;
            }
        }

        private void ConfigureCamera()
        {
            if (trackedCamera == null)
            {
                trackedCamera = GetComponent<Camera>();
            }

            if (trackedCamera == null)
            {
                return;
            }

            trackedCamera.cullingMask = ~0 & ~(1 << XrSimulationEnvironmentLayer);
            trackedCamera.nearClipPlane = Mathf.Min(trackedCamera.nearClipPlane, 0.01f);
            trackedCamera.farClipPlane = Mathf.Max(trackedCamera.farClipPlane, MinimumFarClipPlane);
        }

        private bool HandleManualCameraControls()
        {
            if (trackedCamera == null || Mouse.current == null || !hasRoamArea)
            {
                return false;
            }

            float scroll = Mouse.current.scroll.ReadValue().y;
            bool rightPressed = Mouse.current.rightButton.isPressed;
            Vector2 delta = rightPressed ? Mouse.current.delta.ReadValue() : Vector2.zero;
            bool hasDrag = rightPressed && delta.sqrMagnitude > 0.01f;
            bool hasScroll = Mathf.Abs(scroll) >= 0.01f;
            if (!hasDrag && !hasScroll)
            {
                return false;
            }

            EnsureOrbitStateFromCamera();

            if (hasDrag)
            {
                orbitYaw += delta.x * OrbitSensitivity;
                orbitPitch = Mathf.Clamp(orbitPitch - delta.y * OrbitSensitivity, MinimumOrbitPitch, MaximumOrbitPitch);
            }

            if (hasScroll)
            {
                float maxDistance = Mathf.Max(MaximumFrameDistance, roamAreaRadius * 4f);
                orbitDistance = Mathf.Clamp(orbitDistance - scroll * ZoomSensitivity, MinimumFrameDistance, maxDistance);
            }

            ApplyOrbitCamera();
            return true;
        }

        private void EnsureOrbitStateFromCamera()
        {
            if (orbitInitialized || trackedCamera == null)
            {
                return;
            }

            Vector3 offset = trackedCamera.transform.position - roamAreaCenterWorld;
            orbitDistance = Mathf.Clamp(offset.magnitude, MinimumFrameDistance, Mathf.Max(MaximumFrameDistance, roamAreaRadius * 4f));
            Vector3 flat = new Vector3(offset.x, 0f, offset.z);
            if (flat.sqrMagnitude > 0.0001f)
            {
                orbitYaw = Mathf.Atan2(-flat.x, -flat.z) * Mathf.Rad2Deg;
            }

            orbitPitch = Mathf.Clamp(Mathf.Asin(Mathf.Clamp(offset.y / Mathf.Max(orbitDistance, 0.001f), -1f, 1f)) * Mathf.Rad2Deg,
                MinimumOrbitPitch,
                MaximumOrbitPitch);
            orbitInitialized = true;
        }

        private void ApplyOrbitCamera()
        {
            if (trackedCamera == null)
            {
                return;
            }

            Quaternion orbitRotation = Quaternion.Euler(orbitPitch, orbitYaw, 0f);
            Vector3 cameraPosition = roamAreaCenterWorld + orbitRotation * new Vector3(0f, 0f, -orbitDistance);
            trackedCamera.transform.SetPositionAndRotation(
                cameraPosition,
                Quaternion.LookRotation(roamAreaCenterWorld - cameraPosition, Vector3.up));
        }

        private bool AnyAnimalVisible()
        {
            if (trackedCamera == null)
            {
                return false;
            }

            for (int i = 0; i < Animals.Count; i++)
            {
                Transform animal = Animals[i];
                if (animal == null)
                {
                    continue;
                }

                Vector3 viewport = trackedCamera.WorldToViewportPoint(animal.position);
                if (viewport.z > 0f
                    && viewport.x >= -0.08f
                    && viewport.x <= 1.08f
                    && viewport.y >= -0.08f
                    && viewport.y <= 1.08f)
                {
                    return true;
                }
            }

            return false;
        }

        private void FrameAllAnimals()
        {
            if (trackedCamera == null)
            {
                return;
            }

            if (!TryGetAnimalBounds(out Bounds bounds))
            {
                if (!hasRoamArea)
                {
                    return;
                }

                bounds = new Bounds(roamAreaCenterWorld, Vector3.one * roamAreaRadius * 2f);
            }

            Vector3 center = bounds.center;
            float radius = Mathf.Max(bounds.extents.x, bounds.extents.z, bounds.extents.y, 1.0f);
            if (hasRoamArea)
            {
                center = roamAreaCenterWorld;
                radius = Mathf.Max(radius, roamAreaRadius);
            }

            float distance = Mathf.Clamp(
                radius * FramePadding / Mathf.Tan(trackedCamera.fieldOfView * 0.5f * Mathf.Deg2Rad),
                MinimumFrameDistance,
                Mathf.Max(MaximumFrameDistance, roamAreaRadius * 4f));

            Vector3 forward = trackedCamera.transform.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.0001f)
            {
                forward = Vector3.forward;
            }
            forward.Normalize();

            Quaternion lookRotation = Quaternion.LookRotation(forward, Vector3.up)
                * Quaternion.Euler(PreferredPitchDegrees, 0f, 0f);
            Vector3 lookDirection = lookRotation * Vector3.forward;
            Vector3 cameraPosition = center - lookDirection * distance;
            cameraPosition.y = Mathf.Max(cameraPosition.y, center.y + 1.4f);

            trackedCamera.transform.SetPositionAndRotation(
                cameraPosition,
                Quaternion.LookRotation(center - cameraPosition, Vector3.up));

            Vector3 offset = cameraPosition - center;
            orbitYaw = Mathf.Atan2(-offset.x, -offset.z) * Mathf.Rad2Deg;
            orbitPitch = PreferredPitchDegrees;
            orbitDistance = distance;
            orbitInitialized = true;
        }

        private static bool TryGetAnimalBounds(out Bounds bounds)
        {
            bounds = default;
            bool initialized = false;

            for (int i = 0; i < Animals.Count; i++)
            {
                Transform animal = Animals[i];
                if (animal == null)
                {
                    continue;
                }

                Renderer[] renderers = animal.GetComponentsInChildren<Renderer>();
                if (renderers.Length == 0)
                {
                    if (!initialized)
                    {
                        bounds = new Bounds(animal.position, Vector3.one * 0.5f);
                        initialized = true;
                    }
                    else
                    {
                        bounds.Encapsulate(animal.position);
                    }

                    continue;
                }

                for (int rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
                {
                    Renderer renderer = renderers[rendererIndex];
                    if (renderer == null)
                    {
                        continue;
                    }

                    if (!initialized)
                    {
                        bounds = renderer.bounds;
                        initialized = true;
                    }
                    else
                    {
                        bounds.Encapsulate(renderer.bounds);
                    }
                }
            }

            return initialized;
        }
    }
}
