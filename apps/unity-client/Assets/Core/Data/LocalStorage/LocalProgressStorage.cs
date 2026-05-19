using Core.Learning.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace Core.Data.LocalStorage
{
    /// <summary>
    /// Service for storing and loading learning progress locally.
    /// Uses JSON serialization for persistent storage.
    /// Fixed version - DateTime and Dictionary serialization issues resolved.
    /// </summary>
    public class LocalProgressStorage
    {
        private const string ProgressFileName = "learning_progress.json";
        private const string SessionFileName = "session_data.json";

        private string progressFilePath;
        private string sessionFilePath;

        private ProgressData progressData;
        private SessionData currentSession;

        // Events
        public event Action<ActivityResult> OnResultSaved;
        public event Action<SessionData> OnSessionStarted;
        public event Action<SessionData> OnSessionEnded;

        /// <summary>
        /// Initialize the storage system.
        /// </summary>
        public void Initialize()
        {
            // Set file paths
            progressFilePath = Path.Combine(Application.persistentDataPath, ProgressFileName);
            sessionFilePath = Path.Combine(Application.persistentDataPath, SessionFileName);

            // Load existing progress or create new
            LoadProgress();

            Debug.Log($"[LocalProgressStorage] Initialized. Progress path: {progressFilePath}");
        }

        /// <summary>
        /// Start a new learning session.
        /// </summary>
        public SessionData StartSession(string activityId = null)
        {
            currentSession = new SessionData
            {
                SessionId = Guid.NewGuid().ToString(),
                StartTimeString = DateTime.UtcNow.ToString("o"),
                ActivityId = activityId
            };

            OnSessionStarted?.Invoke(currentSession);

            // Save session data
            SaveSession();

            Debug.Log($"[LocalProgressStorage] Started session: {currentSession.SessionId}");

            return currentSession;
        }

        /// <summary>
        /// End the current session.
        /// </summary>
        public void EndSession()
        {
            if (currentSession == null)
            {
                return;
            }

            currentSession.EndTimeString = DateTime.UtcNow.ToString("o");
            currentSession.Duration = CalculateSessionDurationSeconds(currentSession.StartTimeString, currentSession.EndTimeString);

            OnSessionEnded?.Invoke(currentSession);

            // Save updated session data
            SaveSession();

            Debug.Log($"[LocalProgressStorage] Ended session: {currentSession.SessionId}, Duration: {currentSession.Duration}s");

            currentSession = null;
        }

        /// <summary>
        /// Get the current session ID.
        /// </summary>
        public string GetCurrentSessionId()
        {
            return currentSession?.SessionId;
        }

        /// <summary>
        /// Save an activity result.
        /// </summary>
        public void SaveResult(ActivityResult result)
        {
            if (result == null)
            {
                Debug.LogError("[LocalProgressStorage] Cannot save null result");
                return;
            }

            // Ensure DateTime fields are serialized as strings
            if (result.StartTime != default)
            {
                result.StartTimeString = result.StartTime.ToString("o");
            }
            if (result.EndTime != default)
            {
                result.EndTimeString = result.EndTime.ToString("o");
            }

            // Add to progress data
            progressData.AddResult(result);

            // Add to current session if active
            if (currentSession != null)
            {
                currentSession.AddResult(result);
            }

            // Save to file
            SaveProgress();

            OnResultSaved?.Invoke(result);

            Debug.Log($"[LocalProgressStorage] Saved result for {result.ActivityId}, Level {result.LevelNumber}, " +
                     $"Correct: {result.IsCorrect}, Time: {result.TimeSpentSeconds:F1}s");
        }

        /// <summary>
        /// Get all results for a specific activity.
        /// </summary>
        public List<ActivityResult> GetResultsForActivity(string activityId)
        {
            return progressData.GetResultsForActivity(activityId);
        }

        /// <summary>
        /// Get all results for a specific session.
        /// </summary>
        public List<ActivityResult> GetResultsForSession(string sessionId)
        {
            return progressData.GetResultsForSession(sessionId);
        }

        /// <summary>
        /// Get statistics for a specific activity.
        /// </summary>
        public ActivityStatistics GetActivityStatistics(string activityId)
        {
            return progressData.GetStatisticsForActivity(activityId);
        }

        /// <summary>
        /// Get overall statistics across all activities.
        /// </summary>
        public OverallStatistics GetOverallStatistics()
        {
            return progressData.GetOverallStatistics();
        }

        /// <summary>
        /// Get the current session data.
        /// </summary>
        public SessionData GetCurrentSession()
        {
            return currentSession;
        }

        /// <summary>
        /// Clear all progress data.
        /// Use with caution - this cannot be undone.
        /// </summary>
        public void ClearAllProgress()
        {
            progressData = new ProgressData();
            SaveProgress();

            Debug.LogWarning("[LocalProgressStorage] All progress cleared");
        }

        /// <summary>
        /// Clear progress for a specific activity.
        /// </summary>
        public void ClearActivityProgress(string activityId)
        {
            progressData.ClearActivityResults(activityId);
            SaveProgress();

            Debug.Log($"[LocalProgressStorage] Cleared progress for {activityId}");
        }

        /// <summary>
        /// Export progress data as JSON string.
        /// </summary>
        public string ExportProgressAsJson()
        {
            // Before exporting, ensure all DateTime fields are serialized
            progressData.PrepareForSerialization();
            return JsonUtility.ToJson(progressData, true);
        }

        /// <summary>
        /// Import progress data from JSON string.
        /// </summary>
        public bool ImportProgressFromJson(string json)
        {
            try
            {
                ProgressData imported = JsonUtility.FromJson<ProgressData>(json);
                if (imported != null)
                {
                    imported.DeserializeAfterLoad();
                    progressData = imported;
                    SaveProgress();
                    return true;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[LocalProgressStorage] Failed to import progress: {e.Message}");
            }

            return false;
        }

        /// <summary>
        /// Save progress data to file.
        /// </summary>
        private void SaveProgress()
        {
            try
            {
                progressData.PrepareForSerialization();
                string json = JsonUtility.ToJson(progressData, true);
                File.WriteAllText(progressFilePath, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[LocalProgressStorage] Failed to save progress: {e.Message}");
            }
        }

        /// <summary>
        /// Load progress data from file.
        /// </summary>
        private void LoadProgress()
        {
            try
            {
                if (File.Exists(progressFilePath))
                {
                    string json = File.ReadAllText(progressFilePath);
                    progressData = JsonUtility.FromJson<ProgressData>(json);

                    if (progressData == null)
                    {
                        progressData = new ProgressData();
                    }
                    else
                    {
                        progressData.DeserializeAfterLoad();
                        Debug.Log($"[LocalProgressStorage] Loaded {progressData.AllResults.Count} results");
                    }
                }
                else
                {
                    progressData = new ProgressData();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[LocalProgressStorage] Failed to load progress: {e.Message}");
                progressData = new ProgressData();
            }
        }

        /// <summary>
        /// Save session data to file.
        /// </summary>
        private void SaveSession()
        {
            try
            {
                if (currentSession != null)
                {
                    string json = JsonUtility.ToJson(currentSession, true);
                    File.WriteAllText(sessionFilePath, json);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[LocalProgressStorage] Failed to save session: {e.Message}");
            }
        }

        private static float CalculateSessionDurationSeconds(string startTimeString, string endTimeString)
        {
            if (DateTime.TryParse(startTimeString, null, DateTimeStyles.RoundtripKind, out DateTime start)
                && DateTime.TryParse(endTimeString, null, DateTimeStyles.RoundtripKind, out DateTime end))
            {
                return (float)(end - start).TotalSeconds;
            }

            return 0f;
        }
    }

    /// <summary>
    /// Container for all progress data.
    /// Fixed version - properly handles serialization.
    /// </summary>
    [Serializable]
    public class ProgressData
    {
        [SerializeField]
        private List<ActivityResult> allResults = new List<ActivityResult>();

        [SerializeField]
        private string lastUpdated;

        // Fixed: Use serializable list instead of Dictionary
        [SerializeField]
        private List<ActivityStatisticsEntry> activityStatistics = new List<ActivityStatisticsEntry>();

        public List<ActivityResult> AllResults => allResults;
        public string LastUpdated => lastUpdated;

        public ProgressData()
        {
            lastUpdated = DateTime.UtcNow.ToString("o");
        }

        /// <summary>
        /// Prepare data for serialization - convert DateTimes to strings.
        /// </summary>
        public void PrepareForSerialization()
        {
            lastUpdated = DateTime.UtcNow.ToString("o");

            foreach (var result in allResults)
            {
                if (result.StartTime != default && string.IsNullOrEmpty(result.StartTimeString))
                {
                    result.StartTimeString = result.StartTime.ToString("o");
                }
                if (result.EndTime != default && string.IsNullOrEmpty(result.EndTimeString))
                {
                    result.EndTimeString = result.EndTime.ToString("o");
                }
            }

            // Update activity statistics
            RebuildActivityStatistics();
        }

        /// <summary>
        /// Deserialize DateTime strings back to DateTime objects after loading.
        /// </summary>
        public void DeserializeAfterLoad()
        {
            foreach (var result in allResults)
            {
                if (!string.IsNullOrEmpty(result.StartTimeString))
                {
                    DateTime.TryParse(result.StartTimeString, null, DateTimeStyles.RoundtripKind, out result.StartTime);
                }
                if (!string.IsNullOrEmpty(result.EndTimeString))
                {
                    DateTime.TryParse(result.EndTimeString, null, DateTimeStyles.RoundtripKind, out result.EndTime);
                }
            }

            // Rebuild activity statistics from deserialized list
            RebuildActivityStatistics();
        }

        private void RebuildActivityStatistics()
        {
            activityStatistics.Clear();

            // Group by activity
            Dictionary<string, List<ActivityResult>> byActivity = new Dictionary<string, List<ActivityResult>>();
            foreach (var result in allResults)
            {
                if (!byActivity.ContainsKey(result.ActivityId))
                {
                    byActivity[result.ActivityId] = new List<ActivityResult>();
                }
                byActivity[result.ActivityId].Add(result);
            }

            // Calculate statistics for each activity
            foreach (var kvp in byActivity)
            {
                var stats = CalculateStatistics(kvp.Value);
                activityStatistics.Add(new ActivityStatisticsEntry
                {
                    ActivityId = kvp.Key,
                    TotalAttempts = stats.TotalAttempts,
                    SuccessfulAttempts = stats.SuccessfulAttempts,
                    SuccessRate = stats.SuccessRate,
                    TotalHintsUsed = stats.TotalHintsUsed,
                    AverageHintsPerAttempt = stats.AverageHintsPerAttempt,
                    TotalTimeSpent = stats.TotalTimeSpent,
                    AverageTimePerAttempt = stats.AverageTimePerAttempt,
                    BestTime = stats.BestTime,
                    WorstTime = stats.WorstTime
                });
            }
        }

        /// <summary>
        /// Add a result to the progress data.
        /// </summary>
        public void AddResult(ActivityResult result)
        {
            allResults.Add(result);
            lastUpdated = DateTime.UtcNow.ToString("o");
        }

        /// <summary>
        /// Get all results for a specific activity.
        /// </summary>
        public List<ActivityResult> GetResultsForActivity(string activityId)
        {
            List<ActivityResult> results = new List<ActivityResult>();

            foreach (var result in allResults)
            {
                if (result.ActivityId == activityId)
                {
                    results.Add(result);
                }
            }

            return results;
        }

        /// <summary>
        /// Get all results for a specific session.
        /// </summary>
        public List<ActivityResult> GetResultsForSession(string sessionId)
        {
            List<ActivityResult> results = new List<ActivityResult>();

            foreach (var result in allResults)
            {
                if (result.SessionId == sessionId)
                {
                    results.Add(result);
                }
            }

            return results;
        }

        /// <summary>
        /// Get statistics for a specific activity.
        /// </summary>
        public ActivityStatistics GetStatisticsForActivity(string activityId)
        {
            var results = GetResultsForActivity(activityId);
            return CalculateStatistics(results);
        }

        /// <summary>
        /// Get overall statistics across all activities.
        /// </summary>
        public OverallStatistics GetOverallStatistics()
        {
            OverallStatistics overall = new OverallStatistics();

            DeserializeAfterLoad(); // Ensure statistics are up to date

            // Convert list to dictionary for easier access
            foreach (var entry in activityStatistics)
            {
                var stats = new ActivityStatistics
                {
                    TotalAttempts = entry.TotalAttempts,
                    SuccessfulAttempts = entry.SuccessfulAttempts,
                    SuccessRate = entry.SuccessRate,
                    TotalHintsUsed = entry.TotalHintsUsed,
                    AverageHintsPerAttempt = entry.AverageHintsPerAttempt,
                    TotalTimeSpent = entry.TotalTimeSpent,
                    AverageTimePerAttempt = entry.AverageTimePerAttempt,
                    BestTime = entry.BestTime,
                    WorstTime = entry.WorstTime
                };
                overall.ActivityStatistics[entry.ActivityId] = stats;
            }

            overall.TotalActivitiesCompleted = activityStatistics.Count;
            overall.TotalSessions = GetAllSessionCount();
            overall.TotalResults = allResults.Count;

            return overall;
        }

        /// <summary>
        /// Clear results for a specific activity.
        /// </summary>
        public void ClearActivityResults(string activityId)
        {
            allResults.RemoveAll(r => r.ActivityId == activityId);
            lastUpdated = DateTime.UtcNow.ToString("o");
        }

        /// <summary>
        /// Calculate statistics from a list of results.
        /// </summary>
        private ActivityStatistics CalculateStatistics(List<ActivityResult> results)
        {
            ActivityStatistics stats = new ActivityStatistics();

            if (results.Count == 0)
            {
                return stats;
            }

            stats.TotalAttempts = results.Count;

            foreach (var result in results)
            {
                if (result.IsCorrect)
                {
                    stats.SuccessfulAttempts++;
                }

                stats.TotalHintsUsed += result.HintsUsedCount;
                stats.TotalTimeSpent += result.TimeSpentSeconds;
            }

            // Calculate averages
            stats.SuccessRate = (float)stats.SuccessfulAttempts / stats.TotalAttempts;
            stats.AverageHintsPerAttempt = (float)stats.TotalHintsUsed / stats.TotalAttempts;
            stats.AverageTimePerAttempt = stats.TotalTimeSpent / stats.TotalAttempts;

            // Calculate best/worst times
            float bestTime = float.MaxValue;
            float worstTime = 0f;

            foreach (var result in results)
            {
                if (result.TimeSpentSeconds < bestTime)
                {
                    bestTime = result.TimeSpentSeconds;
                }

                if (result.TimeSpentSeconds > worstTime)
                {
                    worstTime = result.TimeSpentSeconds;
                }
            }

            stats.BestTime = bestTime < float.MaxValue ? bestTime : 0f;
            stats.WorstTime = worstTime;

            return stats;
        }

        /// <summary>
        /// Get count of unique sessions.
        /// </summary>
        private int GetAllSessionCount()
        {
            HashSet<string> sessionIds = new HashSet<string>();

            foreach (var result in allResults)
            {
                sessionIds.Add(result.SessionId);
            }

            return sessionIds.Count;
        }
    }

    /// <summary>
    /// Fixed version: Serializable entry for activity statistics.
    /// </summary>
    [Serializable]
    public class ActivityStatisticsEntry
    {
        public string ActivityId;
        public int TotalAttempts;
        public int SuccessfulAttempts;
        public float SuccessRate;
        public int TotalHintsUsed;
        public float AverageHintsPerAttempt;
        public float TotalTimeSpent;
        public float AverageTimePerAttempt;
        public float BestTime;
        public float WorstTime;
    }

    /// <summary>
    /// Data for a single learning session.
    /// Fixed version - uses string for DateTime.
    /// </summary>
    [Serializable]
    public class SessionData
    {
        public string SessionId;
        public string ActivityId;
        public string StartTimeString;  // ISO 8601 string
        public string EndTimeString;    // ISO 8601 string
        public float Duration;          // seconds

        // Runtime properties for convenience
        [NonSerialized]
        public DateTime StartTime => DateTime.TryParse(StartTimeString, null, System.Globalization.DateTimeStyles.RoundtripKind, out var dt) ? dt : default;

        [NonSerialized]
        public DateTime EndTime => DateTime.TryParse(EndTimeString, null, System.Globalization.DateTimeStyles.RoundtripKind, out var dt) ? dt : default;

        [SerializeField]
        private List<ActivityResult> sessionResults = new List<ActivityResult>();

        public List<ActivityResult> Results => sessionResults;

        /// <summary>
        /// Add a result to this session.
        /// </summary>
        public void AddResult(ActivityResult result)
        {
            sessionResults.Add(result);
        }

        /// <summary>
        /// Get the count of results in this session.
        /// </summary>
        public int GetResultCount()
        {
            return sessionResults.Count;
        }
    }

    /// <summary>
    /// Statistics for a single activity.
    /// </summary>
    [Serializable]
    public class ActivityStatistics
    {
        public int TotalAttempts;
        public int SuccessfulAttempts;
        public float SuccessRate;
        public int TotalHintsUsed;
        public float AverageHintsPerAttempt;
        public float TotalTimeSpent;
        public float AverageTimePerAttempt;
        public float BestTime;
        public float WorstTime;
    }

    /// <summary>
    /// Overall statistics across all activities.
    /// Fixed version - uses runtime dictionary for access.
    /// </summary>
    [Serializable]
    public class OverallStatistics
    {
        public int TotalActivitiesCompleted;
        public int TotalSessions;
        public int TotalResults;

        // Runtime access to activity stats
        [NonSerialized]
        public Dictionary<string, ActivityStatistics> ActivityStatistics = new Dictionary<string, ActivityStatistics>();
    }
}
