using Core.Learning.ActivityRunner;
using Core.Learning.Models;
using Core.Learning.Utils;
using Features.Activities.NumberLineJump;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Features.Activities.NumberLineJump
{
    /// <summary>
    /// Presenter for Number Line Jump activity.
    /// Handles character movement, equation tracking, and all game logic.
    /// </summary>
    public class NumberLineJumpPresenter : ActivityPresenter
    {
        // AR Service references
        private IARPlacementService placementService;
        private IARInteractionService interactionService;

        // Current question data
        private NumberLineJumpQuestion currentQuestion;
        private NumberLineJumpConfig jumpConfig;

        // View reference
        private INumberLineJumpView view;

        // Character and number line tracking
        private GameObject characterObject;
        private GameObject[] numberTiles;
        private int currentPosition;
        private List<JumpRecord> jumpHistory;
        private int jumpsRemainingBeforeWarning;
        private bool currentUsesEquationPromptMode;
        private string currentEquationPrompt;

        // State tracking
        private bool hasOvershot;
        private bool hitBoundary;
        private bool exceededMaxJumps;
        private bool isJumping;
        private Coroutine jumpAnimationCoroutine;
        private Coroutine boundaryBumpCoroutine;

        private const float TileWidth = 0.28f;
        private const float TileHeight = 0.04f;
        private const float TileDepth = 0.22f;
        private const float CharacterYOffset = 0.18f;
        private const float JumpArcHeight = 0.22f;
        private const float BoundaryBumpDistance = 0.08f;
        private const float BoundaryBumpDuration = 0.18f;
        private const int TargetPromptQuestionCount = 5;

        private static readonly Color NormalTileColor = new Color(0.44f, 0.52f, 0.62f);
        private static readonly Color AlternateTileColor = new Color(0.52f, 0.59f, 0.68f);
        private static readonly Color StartTileColor = new Color(0.22f, 0.48f, 0.9f);
        private static readonly Color TargetTileColor = new Color(0.18f, 0.72f, 0.42f);
        private static readonly Color CurrentTileColor = new Color(0.12f, 0.42f, 0.95f);
        private static readonly Color CharacterColor = new Color(0.16f, 0.55f, 1f);

        // Events
        public event Action<int> OnCharacterMoved;  // newPosition
        public event Action<int, int> OnNumberLineCreated;  // min, max
        public event Action OnCharacterCreated;

        /// <summary>
        /// Initialize the presenter with config and view.
        /// </summary>
        public void Initialize(NumberLineJumpConfig config, INumberLineJumpView activityView,
            IARPlacementService arPlacement, IARInteractionService arInteraction)
        {
            jumpConfig = config;
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
                view.OnJumpRequested += HandleJumpRequested;
                view.OnConfirmRequested += HandleConfirmRequested;
                view.OnResetRequested += HandleResetRequested;
                view.OnHintRequested += HandleHintRequested;
                view.OnCancelRequested += HandleCancelRequested;
                view.OnTileTapped += HandleTileTapped;
            }

            Debug.Log($"[NumberLineJumpPresenter] Initialized with {config.Questions.Count} questions");
        }

        /// <summary>
        /// Load and display a specific round/question.
        /// </summary>
        protected override void LoadRound(int roundNumber)
        {
            currentQuestion = jumpConfig.GetQuestion(roundNumber - 1);
            if (currentQuestion == null)
            {
                Debug.LogError($"[NumberLineJumpPresenter] Failed to load question {roundNumber}");
                return;
            }

            Debug.Log($"[NumberLineJumpPresenter] Loading round {roundNumber}: " +
                     $"Start={currentQuestion.StartNumber}, Target={currentQuestion.TargetNumber}, " +
                     $"Range=[{currentQuestion.NumberLineMin}, {currentQuestion.NumberLineMax}]");

            // Clear previous objects
            ClearSpawnedObjects();

            // Reset state
            currentPosition = currentQuestion.StartNumber;
            jumpHistory = new List<JumpRecord>();
            hasOvershot = false;
            hitBoundary = false;
            exceededMaxJumps = false;
            isJumping = false;
            jumpsRemainingBeforeWarning = currentQuestion.MaxJumpsAllowed - jumpConfig.MaxJumpsWarningThreshold;
            currentUsesEquationPromptMode = roundNumber > TargetPromptQuestionCount;
            currentEquationPrompt = currentUsesEquationPromptMode ? BuildEquationPrompt(currentQuestion) : null;

            // Create the number line
            CreateNumberLine();

            // Create and place the character
            CreateCharacter();

            // Update view with question data
            view?.ShowQuestion(
                currentQuestion.StartNumber,
                currentQuestion.TargetNumber,
                currentQuestion.NumberLineMin,
                currentQuestion.NumberLineMax,
                currentQuestion.JumpDirection,
                currentUsesEquationPromptMode,
                currentEquationPrompt
            );

            view?.UpdateProgress(roundNumber, jumpConfig.NumberOfRounds);

            // Show initial equation
            UpdateEquationDisplay();
        }

        /// <summary>
        /// Create the number line with numbered tiles.
        /// </summary>
        private void CreateNumberLine()
        {
            if (placementService == null)
            {
                Debug.LogError("[NumberLineJumpPresenter] AR Placement Service not available.");
                return;
            }

            int min = currentQuestion.NumberLineMin;
            int max = currentQuestion.NumberLineMax;
            int tileCount = max - min + 1;

            numberTiles = new GameObject[tileCount];

            // Calculate positions for the number line (horizontal arrangement)
            Vector3 centerPosition = GetLearningAreaCenter();
            Vector3[] positions = ARGroupSpawnUtility.CalculateGroupPositions(
                numberOfGroups: tileCount,
                centerPosition: centerPosition,
                spacing: jumpConfig.TileSpacing,
                arrangementPattern: GroupArrangementPattern.Horizontal
            );

            // Create each number tile
            for (int i = 0; i < tileCount; i++)
            {
                int number = min + i;
                Vector3 position = positions[i] + Vector3.up * jumpConfig.NumberLineHeight;
                GameObject tile = CreateNumberTile(number, position);

                numberTiles[i] = tile;

                // Register tile as interactable
                if (interactionService != null)
                {
                    interactionService.RegisterInteractable(tile, number);
                }
            }

            OnNumberLineCreated?.Invoke(min, max);
        }

        private GameObject CreateNumberTile(int number, Vector3 position)
        {
            var tile = new GameObject($"NumberTile_{number}");
            tile.transform.position = position;
            if (placementService?.HasLearningArea == true && placementService.LearningAreaContentRoot != null)
            {
                tile.transform.SetParent(placementService.LearningAreaContentRoot, true);
            }

            var collider = tile.AddComponent<BoxCollider>();
            collider.size = new Vector3(TileWidth, TileHeight, TileDepth);

            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "TileBody";
            body.transform.SetParent(tile.transform, false);
            body.transform.localScale = new Vector3(TileWidth, TileHeight, TileDepth);

            Collider bodyCollider = body.GetComponent<Collider>();
            if (bodyCollider != null)
            {
                Destroy(bodyCollider);
            }

            ApplyTileColor(tile, number);
            AddNumberLabel(tile, number);
            return tile;
        }

        private static void AddNumberLabel(GameObject tile, int number)
        {
            var labelGo = new GameObject("NumberLabel");
            labelGo.transform.SetParent(tile.transform, false);
            labelGo.transform.localPosition = new Vector3(0f, TileHeight * 1.5f, 0f);

            var label = labelGo.AddComponent<TextMesh>();
            label.text = number.ToString();
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.fontSize = 64;
            label.characterSize = 0.018f;
            label.color = Color.white;

            Camera camera = Camera.main;
            if (camera != null)
            {
                labelGo.transform.rotation = Quaternion.LookRotation(labelGo.transform.position - camera.transform.position, Vector3.up);
            }

            labelGo.AddComponent<NumberLineBillboardBehavior>();
        }

        private void ApplyTileColor(GameObject tile, int number)
        {
            Renderer renderer = tile != null ? tile.GetComponentInChildren<Renderer>() : null;
            if (renderer == null)
            {
                return;
            }

            renderer.material.color = GetTileColor(number);
        }

        private Color GetTileColor(int number)
        {
            if (number == currentPosition)
            {
                return CurrentTileColor;
            }

            if (!currentUsesEquationPromptMode && number == currentQuestion.TargetNumber)
            {
                return TargetTileColor;
            }

            if (number == currentQuestion.StartNumber)
            {
                return StartTileColor;
            }

            return number % 2 == 0 ? NormalTileColor : AlternateTileColor;
        }

        private void RefreshNumberLineVisuals()
        {
            if (numberTiles == null || currentQuestion == null)
            {
                return;
            }

            for (int i = 0; i < numberTiles.Length; i++)
            {
                ApplyTileColor(numberTiles[i], currentQuestion.NumberLineMin + i);
            }
        }

        /// <summary>
        /// Create the character at the starting position.
        /// </summary>
        private void CreateCharacter()
        {
            if (placementService == null)
            {
                Debug.LogError("[NumberLineJumpPresenter] AR Placement Service not available.");
                return;
            }

            // Get the tile position for the starting number
            int startIndex = currentQuestion.StartNumber - currentQuestion.NumberLineMin;
            if (startIndex < 0 || startIndex >= numberTiles.Length)
            {
                Debug.LogError($"[NumberLineJumpPresenter] Invalid start index: {startIndex}");
                return;
            }

            Vector3 startTilePosition = numberTiles[startIndex].transform.position;

            // Get character prefab
            GameObject characterPrefab = GetCharacterPrefab();

            // Create the character
            if (characterPrefab != null)
            {
                characterObject = placementService.SpawnAtPosition(
                    characterPrefab,
                    startTilePosition + Vector3.up * CharacterYOffset,
                    Quaternion.identity,
                    null
                );
                ActivityPrefabSetup.Instance?.PrepareLearningObject(characterObject);
            }
            else
            {
                // Create placeholder character
                characterObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                characterObject.name = "JumpCharacter";
                characterObject.transform.position = startTilePosition + Vector3.up * CharacterYOffset;
                if (placementService.HasLearningArea && placementService.LearningAreaContentRoot != null)
                {
                    characterObject.transform.SetParent(placementService.LearningAreaContentRoot, true);
                }
                characterObject.transform.localScale = Vector3.one * 0.16f;

                Renderer renderer = characterObject.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = CharacterColor;
                }
            }

            currentPosition = currentQuestion.StartNumber;

            // Register character as interactable
            if (interactionService != null)
            {
                interactionService.RegisterInteractable(characterObject, "Character");
            }

            OnCharacterCreated?.Invoke();
        }

        /// <summary>
        /// Get the character prefab.
        /// Uses the scene-level ActivityPrefabSetup fallback.
        /// </summary>
        private GameObject GetCharacterPrefab()
        {
            if (ActivityPrefabSetup.Instance != null)
            {
                return ActivityPrefabSetup.Instance.GetJumpCharacterPrefab();
            }

            return null;
        }

        private Vector3 GetLearningAreaCenter()
        {
            if (placementService?.HasLearningArea == true && placementService.LearningAreaContentRoot != null)
            {
                return placementService.LearningAreaContentRoot.position;
            }

            return placementService != null ? placementService.CurrentPlacementPosition : Vector3.zero;
        }

        /// <summary>
        /// Handle a jump request from the view.
        /// </summary>
        private void HandleJumpRequested(JumpStepDirection direction)
        {
            if (currentState != ActivityState.InProgress || isJumping)
            {
                return;
            }

            if (!currentQuestion.IsDirectionAllowed(direction))
            {
                Debug.Log($"[NumberLineJumpPresenter] Direction {direction} not allowed for this question (config: {currentQuestion.JumpDirection})");
                PlayBoundaryBump(direction);
                view?.ShowBoundaryHit(currentPosition);
                return;
            }

            // Calculate new position
            int newPosition = direction == JumpStepDirection.Right
                ? currentPosition + 1
                : currentPosition - 1;

            // Check boundary
            if (!currentQuestion.IsWithinBounds(newPosition))
            {
                Debug.Log($"[NumberLineJumpPresenter] Cannot jump to {newPosition}: out of bounds");
                hitBoundary = true;
                PlayBoundaryBump(direction);
                view?.ShowBoundaryHit(currentPosition);
                return;
            }

            // Check max jumps
            if (currentQuestion.MaxJumpsAllowed > 0 && jumpHistory.Count >= currentQuestion.MaxJumpsAllowed)
            {
                Debug.Log($"[NumberLineJumpPresenter] Max jumps exceeded");
                exceededMaxJumps = true;
                view?.ShowMaxJumpsExceeded();
                DisableJumpInput();
                return;
            }

            // Perform the jump
            PerformJump(direction, newPosition);
        }

        /// <summary>
        /// Perform a jump to the new position.
        /// </summary>
        private void PerformJump(JumpStepDirection direction, int newPosition)
        {
            isJumping = true;

            int previousPosition = currentPosition;
            currentPosition = newPosition;

            // Record the jump
            float jumpTime = Time.time;
            var jumpRecord = new JumpRecord(direction, previousPosition, currentPosition, jumpTime);
            jumpHistory.Add(jumpRecord);

            // Check for overshoot
            if (currentQuestion.HasOvershotTarget(currentPosition, direction))
            {
                hasOvershot = true;
                Debug.Log($"[NumberLineJumpPresenter] Overshot target at position {currentPosition}");
            }

            view?.UpdateCurrentPosition(currentPosition);
            RefreshNumberLineVisuals();
            DisableJumpInput();

            // Move the character visually
            MoveCharacterToPosition(currentPosition, animate: true);

            // Update equation display
            if (currentQuestion.ShowEquationDuringJumps)
            {
                UpdateEquationDisplay();
            }

            // Check for max jumps warning
            if (currentQuestion.MaxJumpsAllowed > 0)
            {
                int remainingJumps = currentQuestion.MaxJumpsAllowed - jumpHistory.Count;
                if (remainingJumps <= jumpConfig.MaxJumpsWarningThreshold && remainingJumps != jumpsRemainingBeforeWarning)
                {
                    jumpsRemainingBeforeWarning = remainingJumps;
                    view?.ShowMaxJumpsWarning(remainingJumps);
                }
            }

            OnCharacterMoved?.Invoke(currentPosition);

            Debug.Log($"[NumberLineJumpPresenter] Jumped {direction}: {previousPosition} -> {currentPosition}");
        }

        /// <summary>
        /// Move the character to a specific position on the number line.
        /// </summary>
        private void MoveCharacterToPosition(int position, bool animate = false)
        {
            if (characterObject == null || numberTiles == null)
            {
                return;
            }

            int tileIndex = position - currentQuestion.NumberLineMin;
            if (tileIndex < 0 || tileIndex >= numberTiles.Length)
            {
                return;
            }

            Vector3 targetPosition = numberTiles[tileIndex].transform.position;
            Vector3 characterTarget = targetPosition + Vector3.up * CharacterYOffset;

            if (!animate || jumpConfig.JumpAnimationDuration <= 0f)
            {
                characterObject.transform.position = characterTarget;
                EnableJumpInput();
                return;
            }

            if (jumpAnimationCoroutine != null)
            {
                StopCoroutine(jumpAnimationCoroutine);
            }

            jumpAnimationCoroutine = StartCoroutine(AnimateCharacterJump(
                characterObject.transform.position,
                characterTarget,
                jumpConfig.JumpAnimationDuration));
        }

        private void PlayBoundaryBump(JumpStepDirection direction)
        {
            if (characterObject == null)
            {
                return;
            }

            if (boundaryBumpCoroutine != null)
            {
                StopCoroutine(boundaryBumpCoroutine);
            }

            boundaryBumpCoroutine = StartCoroutine(AnimateBoundaryBump(direction));
        }

        private IEnumerator AnimateBoundaryBump(JumpStepDirection direction)
        {
            Vector3 startPosition = characterObject.transform.position;
            Vector3 bumpDirection = direction == JumpStepDirection.Right ? Vector3.right : Vector3.left;
            Vector3 bumpPosition = startPosition + bumpDirection * BoundaryBumpDistance;
            float halfDuration = BoundaryBumpDuration * 0.5f;

            float elapsed = 0f;
            while (elapsed < halfDuration)
            {
                float t = Mathf.Clamp01(elapsed / halfDuration);
                characterObject.transform.position = Vector3.Lerp(startPosition, bumpPosition, t);
                elapsed += Time.deltaTime;
                yield return null;
            }

            elapsed = 0f;
            while (elapsed < halfDuration)
            {
                float t = Mathf.Clamp01(elapsed / halfDuration);
                characterObject.transform.position = Vector3.Lerp(bumpPosition, startPosition, t);
                elapsed += Time.deltaTime;
                yield return null;
            }

            characterObject.transform.position = startPosition;
            boundaryBumpCoroutine = null;
        }

        private IEnumerator AnimateCharacterJump(Vector3 startPosition, Vector3 targetPosition, float duration)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float t = Mathf.Clamp01(elapsed / duration);
                float eased = t * t * (3f - 2f * t);
                float arc = Mathf.Sin(t * Mathf.PI) * JumpArcHeight;
                characterObject.transform.position = Vector3.Lerp(startPosition, targetPosition, eased) + Vector3.up * arc;

                elapsed += Time.deltaTime;
                yield return null;
            }

            characterObject.transform.position = targetPosition;
            jumpAnimationCoroutine = null;
            EnableJumpInput();
        }

        /// <summary>
        /// Update the equation display.
        /// </summary>
        private void UpdateEquationDisplay()
        {
            if (currentUsesEquationPromptMode)
            {
                view?.UpdateEquation(currentEquationPrompt);
                return;
            }

            string equation = NumberLineJumpAnswer.GetCurrentEquation(
                currentQuestion.StartNumber,
                currentPosition,
                currentQuestion.TargetNumber
            );
            view?.UpdateEquation(equation);
        }

        private static string BuildEquationPrompt(NumberLineJumpQuestion question)
        {
            int change = question.TargetNumber - question.StartNumber;
            string operation = change >= 0 ? "+" : "-";
            return $"{question.StartNumber} {operation} {Mathf.Abs(change)} = ?";
        }

        /// <summary>
        /// Handle tile tap from interaction service.
        /// </summary>
        private void HandleTileTapped(int tileNumber)
        {
            if (currentState != ActivityState.InProgress || isJumping)
            {
                return;
            }

            // Calculate jump direction and distance
            if (tileNumber > currentPosition)
            {
                // Jump right - perform multiple jumps or one big jump?
                // For now, let's do one jump at a time
                HandleJumpRequested(JumpStepDirection.Right);
            }
            else if (tileNumber < currentPosition)
            {
                // Jump left
                HandleJumpRequested(JumpStepDirection.Left);
            }
            // If same position, ignore
        }

        /// <summary>
        /// Handle confirm request from view.
        /// </summary>
        private void HandleConfirmRequested()
        {
            if (currentState != ActivityState.InProgress)
            {
                return;
            }

            Debug.Log($"[NumberLineJumpPresenter] Confirm requested at position {currentPosition}");

            // Create the answer
            var answer = new NumberLineJumpAnswer(
                startNumber: currentQuestion.StartNumber,
                finalPosition: currentPosition,
                targetNumber: currentQuestion.TargetNumber,
                jumps: new List<JumpRecord>(jumpHistory),
                hasOvershot: hasOvershot,
                hitBoundary: hitBoundary,
                exceededMaxJumps: exceededMaxJumps,
                responseTimeSeconds: Time.time - roundStartTime,
                attemptNumber: currentResult.TotalAttempts + 1,
                hintsUsed: hintsUsedInCurrentRound > 0
            );

            // Submit the answer via base presenter
            SubmitAnswer(answer);
        }

        /// <summary>
        /// Handle reset request from view.
        /// </summary>
        private void HandleResetRequested()
        {
            Debug.Log("[NumberLineJumpPresenter] Reset requested");

            // Reset character to starting position
            ResetCharacterPosition();

            // Clear jump history
            jumpHistory.Clear();
            hasOvershot = false;
            hitBoundary = false;
            exceededMaxJumps = false;
            jumpsRemainingBeforeWarning = currentQuestion.MaxJumpsAllowed - jumpConfig.MaxJumpsWarningThreshold;

            // Re-enable input
            EnableJumpInput();

            // Update equation
            UpdateEquationDisplay();
        }

        /// <summary>
        /// Reset character to starting position.
        /// </summary>
        private void ResetCharacterPosition()
        {
            if (jumpAnimationCoroutine != null)
            {
                StopCoroutine(jumpAnimationCoroutine);
                jumpAnimationCoroutine = null;
            }

            if (boundaryBumpCoroutine != null)
            {
                StopCoroutine(boundaryBumpCoroutine);
                boundaryBumpCoroutine = null;
            }

            isJumping = false;
            currentPosition = currentQuestion.StartNumber;
            MoveCharacterToPosition(currentPosition);
            RefreshNumberLineVisuals();
            view?.UpdateCurrentPosition(currentPosition);
        }

        /// <summary>
        /// Check if the given answer is correct.
        /// </summary>
        protected override bool CheckAnswer(ActivityAnswer answer)
        {
            if (answer is not NumberLineJumpAnswer jumpAnswer)
            {
                Debug.LogError($"[NumberLineJumpPresenter] Answer is not a NumberLineJumpAnswer");
                return false;
            }

            // Check if final position matches target
            bool isCorrect = jumpAnswer.IsCorrect();

            Debug.Log($"[NumberLineJumpPresenter] Checking answer: Final={jumpAnswer.FinalPosition} vs Target={currentQuestion.TargetNumber} " +
                     $"-> {(isCorrect ? "CORRECT" : "INCORRECT")} " +
                     $"(Overshot={jumpAnswer.HasOvershot}, Boundary={jumpAnswer.HitBoundary}, MaxJumps={jumpAnswer.ExceededMaxJumps})");

            return isCorrect;
        }

        /// <summary>
        /// Get the error type for an incorrect answer.
        /// </summary>
        protected override ErrorType? GetErrorType(ActivityAnswer answer)
        {
            if (answer is not NumberLineJumpAnswer jumpAnswer)
            {
                return ErrorType.Other;
            }

            if (jumpAnswer.HasOvershot)
            {
                return ErrorType.WrongDirection;
            }

            if (jumpAnswer.HitBoundary)
            {
                return ErrorType.WrongDirection;
            }

            if (jumpAnswer.ExceededMaxJumps)
            {
                return ErrorType.WrongJumpCount;
            }

            int actualJumps = jumpAnswer.GetTotalJumps();
            int expectedJumps = jumpAnswer.TargetNumber - jumpAnswer.StartNumber;

            if (actualJumps != Mathf.Abs(expectedJumps))
            {
                return ErrorType.WrongJumpCount;
            }

            return ErrorType.Other;
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
        /// Called when a hint is provided.
        /// </summary>
        protected override void OnHintProvided(ActivityHint hint)
        {
            base.OnHintProvided(hint);

            // Format the hint with start and target numbers
            string formattedHint = jumpConfig.GetFormattedHintText(
                hint,
                currentQuestion.StartNumber,
                currentQuestion.TargetNumber
            );
            hint.HintText = formattedHint;

            // Pass to view
            view?.ShowHint(hint);
        }

        /// <summary>
        /// Handle correct answer with outcome-specific feedback.
        /// </summary>
        protected override void HandleCorrectAnswer(ActivityAnswer answer)
        {
            if (answer is not NumberLineJumpAnswer jumpAnswer)
            {
                base.HandleCorrectAnswer(answer);
                return;
            }

            // Get outcome-specific feedback
            string feedback = jumpConfig.GetFeedbackString(jumpAnswer);

            // Show feedback in view
            view?.ShowCorrectFeedback(feedback, jumpAnswer.FinalEquation);

            // Call base to complete the round
            base.HandleCorrectAnswer(answer);
        }

        /// <summary>
        /// Handle incorrect answer with outcome-specific feedback.
        /// </summary>
        protected override void HandleIncorrectAnswer(ActivityAnswer answer)
        {
            if (answer is not NumberLineJumpAnswer jumpAnswer)
            {
                base.HandleIncorrectAnswer(answer);
                return;
            }

            // Get outcome-specific feedback
            string feedback = jumpConfig.GetFeedbackString(jumpAnswer);

            // Show feedback in view
            view?.ShowIncorrectFeedback(feedback, jumpAnswer.FinalEquation);

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
                view?.ShowActivityFailed(jumpConfig.FailedFeedback, currentResult);
            }
        }

        /// <summary>
        /// Enable jump input.
        /// </summary>
        private void EnableJumpInput()
        {
            isJumping = false;
            view?.SetJumpInputEnabled(true);
        }

        /// <summary>
        /// Disable jump input.
        /// </summary>
        private void DisableJumpInput()
        {
            view?.SetJumpInputEnabled(false);
        }

        /// <summary>
        /// Clear all spawned objects.
        /// </summary>
        private void ClearSpawnedObjects()
        {
            if (jumpAnimationCoroutine != null)
            {
                StopCoroutine(jumpAnimationCoroutine);
                jumpAnimationCoroutine = null;
            }

            if (characterObject != null)
            {
                interactionService?.UnregisterInteractable(characterObject);
                Destroy(characterObject);
                characterObject = null;
            }

            if (numberTiles != null)
            {
                foreach (GameObject tile in numberTiles)
                {
                    if (tile != null)
                    {
                        interactionService?.UnregisterInteractable(tile);
                        Destroy(tile);
                    }
                }
                numberTiles = null;
            }

            CancelInvoke(nameof(EnableJumpInput));
        }

        /// <summary>
        /// Clean up resources.
        /// </summary>
        public override void Cleanup()
        {
            // Unsubscribe from events
            if (view != null)
            {
                view.OnJumpRequested -= HandleJumpRequested;
                view.OnConfirmRequested -= HandleConfirmRequested;
                view.OnResetRequested -= HandleResetRequested;
                view.OnHintRequested -= HandleHintRequested;
                view.OnCancelRequested -= HandleCancelRequested;
                view.OnTileTapped -= HandleTileTapped;
            }

            ClearSpawnedObjects();

            base.Cleanup();
        }
    }

    public class NumberLineBillboardBehavior : MonoBehaviour
    {
        private Camera mainCamera;

        private void LateUpdate()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
                if (mainCamera == null)
                {
                    return;
                }
            }

            Vector3 direction = transform.position - mainCamera.transform.position;
            if (direction.sqrMagnitude > 0.0001f)
            {
                transform.rotation = Quaternion.LookRotation(direction.normalized, mainCamera.transform.up);
            }
        }
    }
}
