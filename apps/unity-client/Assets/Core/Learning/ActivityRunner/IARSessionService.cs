using UnityEngine;
using System;

namespace Core.Learning.ActivityRunner
{
    /// <summary>
    /// Interface for AR session management services.
    /// Learning activities use this to check AR session state.
    ///
    /// TODO: KHÁNH'S TEAM - Implement this interface in the AR Core module.
    /// This interface is defined by Learning layer but implemented by AR layer.
    /// </summary>
    public interface IARSessionService
    {
        /// <summary>
        /// Event fired when AR session is ready.
        /// </summary>
        event Action OnSessionReady;

        /// <summary>
        /// Event fired when AR session is lost.
        /// </summary>
        event Action OnSessionLost;

        /// <summary>
        /// Check if the AR session is currently ready.
        /// </summary>
        bool IsSessionReady { get; }

        /// <summary>
        /// Check if tracking is currently stable.
        /// </summary>
        bool IsTrackingStable { get; }

        /// <summary>
        /// Get the current tracking quality.
        /// </summary>
        TrackingQuality TrackingQuality { get; }

        /// <summary>
        /// Initialize the AR session.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Start the AR session.
        /// </summary>
        void StartSession();

        /// <summary>
        /// Stop the AR session.
        /// </summary>
        void StopSession();

        /// <summary>
        /// Reset the AR session.
        /// </summary>
        void ResetSession();
    }

    /// <summary>
    /// Tracking quality levels.
    /// </summary>
    public enum TrackingQuality
    {
        /// <summary>
        /// Tracking not available.
        /// </summary>
        None,

        /// <summary>
        /// Poor tracking - not suitable for activities.
        /// </summary>
        Poor,

        /// <summary>
        /// Acceptable tracking - activities possible but may be unstable.
        /// </summary>
        Fair,

        /// <summary>
        /// Good tracking - optimal for activities.
        /// </summary>
        Good,

        /// <summary>
        /// Excellent tracking - best possible quality.
        /// </summary>
        Excellent
    }
}
