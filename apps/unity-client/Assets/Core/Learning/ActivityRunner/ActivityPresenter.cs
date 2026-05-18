using Core.Data.LocalStorage;
using Core.Learning.Models;
using Core.Support.FeedbackSystem;
using System;
using UnityEngine;

namespace Core.Learning.ActivityRunner
{
    /// <summary>
    /// Base presenter for activities.
    /// Handles all business logic and coordinates between View, AR services, and data.
    /// </summary>
    public abstract class ActivityPresenter : MonoBehaviour, IActivityRunner, IActivityPresenter
    {
        [Header("Configuration")]
        [SerializeField]
        protected ActivityConfig config;

        [Header("State")]
        [SerializeField]
        protected ActivityState currentState = ActivityState.Initializing;

        // Result tracking
        protected ActivityResult currentResult;
        protected string sessionId;
        protected int currentRound = 0;
        protected int hintsUsedInCurrentRound = 0;
        protected float roundStartTime;

        // Events from IActivityRunner
        public event Action<ActivityState> OnStateChanged;
        public event Action<ActivityAnswer, bool> OnAnswerSubmitted;
        public event Action<ActivityResult> OnActivityCompleted;

        // Properties
        public ActivityState CurrentState => currentState;
        public ActivityResult CurrentResult => currentResult;
        public ActivityConfig Config => config;

        // Abstract methods that derived activities must implement
        protected abstract void LoadRound(int roundNumber);
        protected abstract bool CheckAnswer(ActivityAnswer answer);
        protected abstract ErrorType? GetErrorType(ActivityAnswer answer);

        /// <summary>
        /// Initialize with a config.
        /// </summary>
        public virtual void Initialize(ActivityConfig activityConfig)
        {
            if (activityConfig == null || !activityConfig.IsValid())
            {
                Debug.LogError($"[ActivityPresenter] Invalid config provided.");
                return;
            }

            config = activityConfig;
            sessionId = Guid.NewGuid().ToString();
            currentRound = 0;
            ChangeState(ActivityState.Ready);

            Debug.Log($"[ActivityPresenter] Initialized activity {config.ActivityId} with session {sessionId}");
        }

        /// <summary>
        /// Start the activity.
        /// </summary>
        public virtual void StartActivity()
        {
            if (currentState != ActivityState.Ready && currentState != ActivityState.Cancelled)
            {
                Debug.LogWarning($"[ActivityPresenter] Cannot start activity from state {currentState}");
                return;
            }

            StartNextRound();
        }

        /// <summary>
        /// Start the next round/question.
        /// </summary>
        protected virtual void StartNextRound()
        {
            currentRound++;
            hintsUsedInCurrentRound = 0;
            roundStartTime = Time.time;

            if (currentRound > config.NumberOfRounds)
            {
                CompleteActivity(true);
                return;
            }

            InitializeRoundResult();
            LoadRound(currentRound);
            ChangeState(ActivityState.InProgress);
        }

        /// <summary>
        /// Initialize the result for the current round.
        /// </summary>
        protected virtual void InitializeRoundResult()
        {
            currentResult = new ActivityResult(
                config.ActivityId,
                sessionId,
                currentRound,
                config.Difficulty
            );
        }

        /// <summary>
        /// Pause the activity.
        /// </summary>
        public virtual void PauseActivity()
        {
            if (currentState != ActivityState.InProgress)
            {
                return;
            }

            ChangeState(ActivityState.Paused);
        }

        /// <summary>
        /// Resume the activity.
        /// </summary>
        public virtual void ResumeActivity()
        {
            if (currentState != ActivityState.Paused)
            {
                return;
            }

            ChangeState(ActivityState.InProgress);
        }

        /// <summary>
        /// Reset the activity.
        /// </summary>
        public virtual void ResetActivity()
        {
            currentRound = 0;
            hintsUsedInCurrentRound = 0;
            currentResult = null;
            StartActivity();
        }

        /// <summary>
        /// Cancel the activity.
        /// </summary>
        public virtual void Cancel()
        {
            if (currentResult != null)
            {
                currentResult.Complete(false, ErrorType.Other);
            }
            ChangeState(ActivityState.Cancelled);
            Cleanup();
        }

        /// <summary>
        /// Submit an answer from the user.
        /// </summary>
        public virtual void SubmitAnswer(ActivityAnswer answer)
        {
            if (currentState != ActivityState.InProgress)
            {
                Debug.LogWarning($"[ActivityPresenter] Cannot submit answer in state {currentState}");
                return;
            }

            // Update answer metadata
            answer.AttemptNumber = currentResult.TotalAttempts + 1;
            answer.HintsUsed = hintsUsedInCurrentRound > 0;
            answer.ResponseTimeSeconds = Time.time - roundStartTime;

            // Check the answer
            bool isCorrect = CheckAnswer(answer);
            currentResult.IncrementAttempts();

            // Fire event
            OnAnswerSubmitted?.Invoke(answer, isCorrect);

            if (isCorrect)
            {
                HandleCorrectAnswer(answer);
            }
            else
            {
                HandleIncorrectAnswer(answer);
            }
        }

        /// <summary>
        /// Handle a correct answer.
        /// </summary>
        protected virtual void HandleCorrectAnswer(ActivityAnswer answer)
        {
            currentResult.Complete(true, null);
            PlayAnswerFeedback(isCorrect: true);
            PersistRoundResult();
            ChangeState(ActivityState.Completed);

            // Wait a moment then go to next round or complete
            Invoke(nameof(StartNextRound), 2f);
        }

        /// <summary>
        /// Handle an incorrect answer.
        /// </summary>
        protected virtual void HandleIncorrectAnswer(ActivityAnswer answer)
        {
            PlayAnswerFeedback(isCorrect: false);

            if (currentResult.TotalAttempts >= config.MaxAttemptsPerQuestion)
            {
                // Max attempts reached - fail the round
                currentResult.Complete(false, GetErrorType(answer));
                PersistRoundResult();
                ChangeState(ActivityState.Failed);
                OnActivityCompleted?.Invoke(currentResult);
            }
            else
            {
                // Allow retry
                Debug.Log($"[ActivityPresenter] Incorrect answer. Attempt {currentResult.TotalAttempts} of {config.MaxAttemptsPerQuestion}");
            }
        }

        /// <summary>
        /// Request a hint for the current question.
        /// </summary>
        public virtual void RequestHint()
        {
            if (currentState != ActivityState.InProgress)
            {
                return;
            }

            if (hintsUsedInCurrentRound >= config.MaxHintsPerQuestion)
            {
                Debug.Log($"[ActivityPresenter] Max hints ({config.MaxHintsPerQuestion}) already used.");
                return;
            }

            hintsUsedInCurrentRound++;
            currentResult.IncrementHintsUsed();

            // Get the appropriate hint
            var hints = config.GetHintsForLevel(currentRound);
            if (hints != null && hints.Count > 0)
            {
                // Return the hint for the current level (or default to first)
                var hintIndex = Mathf.Min(hintsUsedInCurrentRound - 1, hints.Count - 1);
                var hint = hints[hintIndex];

                // This would be sent to the View - derived classes should handle this
                OnHintProvided(hint);
            }
        }

        /// <summary>
        /// Called when a hint is provided.
        /// Override in derived classes to pass to the View.
        /// </summary>
        protected virtual void OnHintProvided(ActivityHint hint)
        {
            Debug.Log($"[ActivityPresenter] Hint provided: {hint.HintText}");
        }

        /// <summary>
        /// Complete the activity.
        /// </summary>
        protected virtual void CompleteActivity(bool success)
        {
            PlayAnswerFeedback(isCorrect: success, isActivityComplete: true);
            ChangeState(success ? ActivityState.Completed : ActivityState.Failed);
            OnActivityCompleted?.Invoke(currentResult);
        }

        /// <summary>
        /// Persist the current round result to local storage.
        /// </summary>
        protected virtual void PersistRoundResult()
        {
            if (currentResult == null)
            {
                return;
            }

            ProgressStorageProxy.Instance.SaveResult(currentResult);
        }

        /// <summary>
        /// Trigger global feedback (sound/VFX hooks via FeedbackServiceProxy).
        /// </summary>
        protected virtual void PlayAnswerFeedback(bool isCorrect, bool isActivityComplete = false)
        {
            var proxy = FindAnyObjectByType<FeedbackServiceProxy>();
            if (proxy != null)
            {
                if (isActivityComplete)
                {
                    proxy.ShowSuccess();
                }
                else if (isCorrect)
                {
                    proxy.ShowCorrect();
                }
                else
                {
                    proxy.ShowIncorrect();
                }

                return;
            }

            // Fallback without scene proxy
            if (isActivityComplete)
            {
                FeedbackServiceProxy.Instance.ShowSuccess();
            }
            else if (isCorrect)
            {
                FeedbackServiceProxy.Instance.ShowCorrect();
            }
            else
            {
                FeedbackServiceProxy.Instance.ShowIncorrect();
            }
        }

        /// <summary>
        /// Change the activity state.
        /// </summary>
        protected virtual void ChangeState(ActivityState newState)
        {
            if (currentState != newState)
            {
                currentState = newState;
                OnStateChanged?.Invoke(newState);
                Debug.Log($"[ActivityPresenter] State changed to {newState}");
            }
        }

        /// <summary>
        /// Get the current state (for IActivityPresenter).
        /// </summary>
        public ActivityState GetState()
        {
            return currentState;
        }

        /// <summary>
        /// Clean up resources.
        /// </summary>
        public virtual void Cleanup()
        {
            CancelInvoke();
            currentResult = null;
            config = null;
        }

        protected virtual void OnDestroy()
        {
            Cleanup();
        }
    }
}
