using Core.Learning.Models;
using System;
using UnityEngine;

namespace Core.Data.LocalStorage
{
    /// <summary>
    /// MonoBehaviour proxy for the LocalProgressStorage.
    /// Attach to a GameObject in the scene to enable progress storage functionality.
    /// Provides a singleton instance for easy access across activities.
    /// </summary>
    public class ProgressStorageProxy : MonoBehaviour
    {
        private static ProgressStorageProxy instance;
        private static LocalProgressStorage storage;

        [Header("Settings")]
        [Tooltip("Auto-save results immediately")]
        [SerializeField]
        private bool autoSave = true;

        [Tooltip("Clear progress on startup (for testing)")]
        [SerializeField]
        private bool clearOnStartup = false;

        /// <summary>
        /// Get the singleton LocalProgressStorage instance.
        /// </summary>
        public static LocalProgressStorage Instance
        {
            get
            {
                if (storage == null)
                {
                    storage = new LocalProgressStorage();
                    storage.Initialize();
                }
                return storage;
            }
        }

        private void Awake()
        {
            // Singleton pattern
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            // Initialize storage
            if (storage == null)
            {
                storage = new LocalProgressStorage();
                storage.Initialize();
            }

            // Clear if requested (for testing)
            if (clearOnStartup)
            {
                storage.ClearAllProgress();
            }

            // Subscribe to events
            storage.OnResultSaved += HandleResultSaved;
            storage.OnSessionStarted += HandleSessionStarted;
            storage.OnSessionEnded += HandleSessionEnded;
        }

        private void Start()
        {
            // Start a session automatically
            if (storage.GetCurrentSessionId() == null)
            {
                storage.StartSession();
            }
        }

        private void HandleResultSaved(ActivityResult result)
        {
            Debug.Log($"[ProgressStorageProxy] Result saved: {result.ActivityId} L{result.LevelNumber} = {result.IsCorrect}");
        }

        private void HandleSessionStarted(SessionData session)
        {
            Debug.Log($"[ProgressStorageProxy] Session started: {session.SessionId}");
        }

        private void HandleSessionEnded(SessionData session)
        {
            Debug.Log($"[ProgressStorageProxy] Session ended: {session.SessionId}, Duration: {session.Duration}s");
        }

        private void OnDestroy()
        {
            if (storage != null)
            {
                storage.OnResultSaved -= HandleResultSaved;
                storage.OnSessionStarted -= HandleSessionStarted;
                storage.OnSessionEnded -= HandleSessionEnded;

                // End current session if active
                if (storage.GetCurrentSessionId() != null)
                {
                    storage.EndSession();
                }
            }
        }

        /// <summary>
        /// Convenience method to save a result with auto-save check.
        /// </summary>
        public void SaveResult(ActivityResult result)
        {
            if (autoSave)
            {
                Instance.SaveResult(result);
            }
        }

        /// <summary>
        /// Get statistics for the current activity.
        /// </summary>
        public ActivityStatistics GetStatistics(string activityId)
        {
            return Instance.GetActivityStatistics(activityId);
        }

        /// <summary>
        /// Get overall statistics.
        /// </summary>
        public OverallStatistics GetOverallStatistics()
        {
            return Instance.GetOverallStatistics();
        }

        /// <summary>
        /// Export progress as JSON for debugging/backup.
        /// </summary>
        public string ExportProgress()
        {
            return Instance.ExportProgressAsJson();
        }

        /// <summary>
        /// Clear all progress data.
        /// </summary>
        public void ClearProgress()
        {
            Instance.ClearAllProgress();
        }
    }
}
