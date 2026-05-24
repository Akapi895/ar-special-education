using Core.Learning.Models;
using System;

namespace Core.Learning.ActivityRunner
{
    /// <summary>
    /// Interface for managing the lifecycle of a learning activity.
    /// </summary>
    public interface IActivityRunner
    {
        /// <summary>
        /// Current state of the activity.
        /// </summary>
        ActivityState CurrentState { get; }

        /// <summary>
        /// The current result being tracked.
        /// </summary>
        ActivityResult CurrentResult { get; }

        /// <summary>
        /// Event fired when activity state changes.
        /// </summary>
        event Action<ActivityState> OnStateChanged;

        /// <summary>
        /// Event fired when an answer is submitted.
        /// </summary>
        event Action<ActivityAnswer, bool> OnAnswerSubmitted;

        /// <summary>
        /// Event fired when activity completes (success or failure).
        /// </summary>
        event Action<ActivityResult> OnActivityCompleted;

        /// <summary>
        /// Initialize the activity with the given config.
        /// </summary>
        void Initialize(ActivityConfig config);

        /// <summary>
        /// Start the activity.
        /// </summary>
        void StartActivity();

        /// <summary>
        /// Pause the activity.
        /// </summary>
        void PauseActivity();

        /// <summary>
        /// Resume the activity.
        /// </summary>
        void ResumeActivity();

        /// <summary>
        /// Reset the activity to initial state.
        /// </summary>
        void ResetActivity();

        /// <summary>
        /// Clean up resources.
        /// </summary>
        void Cleanup();
    }
}
