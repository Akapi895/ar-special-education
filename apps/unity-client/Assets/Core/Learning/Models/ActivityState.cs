namespace Core.Learning.Models
{
    /// <summary>
    /// Represents the current state of an activity.
    /// </summary>
    public enum ActivityState
    {
        /// <summary>
        /// Activity is being initialized.
        /// </summary>
        Initializing,

        /// <summary>
        /// Activity is ready and waiting for user input.
        /// </summary>
        Ready,

        /// <summary>
        /// Activity is currently in progress (user interacting).
        /// </summary>
        InProgress,

        /// <summary>
        /// Activity is paused (e.g., hint shown, user stepped away).
        /// </summary>
        Paused,

        /// <summary>
        /// Activity completed successfully.
        /// </summary>
        Completed,

        /// <summary>
        /// Activity failed (max attempts reached, etc.).
        /// </summary>
        Failed,

        /// <summary>
        /// Activity was cancelled by user.
        /// </summary>
        Cancelled
    }
}
