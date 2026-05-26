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
        private LearnerProfile currentLearner;

        // Events
        public event Action<ActivityResult> OnResultSaved;
        public event Action<SessionData> OnSessionStarted;
        public event Action<SessionData> OnSessionEnded;

        /// <summary>
        /// Initialize the storage system.
        /// </summary>
        public void Initialize()
        {
            currentLearner = LearnerProfileStore.GetActiveOrCreateDefault();
            SetStoragePathsForLearner(currentLearner.LearnerId);

            // Load existing progress or create new
            LoadProgress();

            Debug.Log($"[LocalProgressStorage] Initialized for learner {currentLearner.LearnerId}. Progress path: {progressFilePath}");
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
                ActivityId = activityId,
                LearnerId = currentLearner?.LearnerId
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
            result.LearnerId = currentLearner?.LearnerId;
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

        public void SaveTechnicalIssue(
            TechnicalIssueType issueType,
            string activityId = null,
            string lessonId = null,
            string note = null,
            string source = null)
        {
            string sessionId = GetCurrentSessionId() ?? Guid.NewGuid().ToString();
            ActivityResult result = new ActivityResult(activityId ?? "TechnicalIssue", sessionId, 0, DifficultyLevel.Easy);
            result.LessonId = lessonId;
            result.RoundId = $"{result.ActivityId}-TECH-{DateTime.UtcNow:yyyyMMddHHmmss}";
            result.SetTechnicalIssue(issueType, note, source);
            result.Complete(false, null);
            SaveResult(result);
        }

        /// <summary>
        /// Get all results for a specific activity.
        /// </summary>
        public List<ActivityResult> GetResultsForActivity(string activityId)
        {
            return progressData.GetResultsForActivity(activityId);
        }

        public List<ActivityResult> GetResultsForLesson(string lessonId)
        {
            return progressData.GetResultsForLesson(lessonId);
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

        public ActivityStatistics GetLessonStatistics(string lessonId)
        {
            return progressData.GetStatisticsForLesson(lessonId);
        }

        public List<SkillMastery> GetSkillMasteries()
        {
            return progressData.GetSkillMasteries();
        }

        public SkillMastery GetWeakestSkillMastery()
        {
            return progressData.GetWeakestSkillMastery();
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

        public LearnerProfile GetActiveLearnerProfile()
        {
            return currentLearner;
        }

        public List<LearnerProfile> GetLearnerProfiles()
        {
            return LearnerProfileStore.LoadProfiles();
        }

        public LearnerProfile CreateOrUpdateLearnerProfile(string displayName, int ageYears = 0, string grade = null)
        {
            LearnerProfile profile = LearnerProfileStore.CreateOrUpdate(displayName, ageYears, grade);
            SetActiveLearnerProfile(profile.LearnerId);
            return profile;
        }

        public bool SetActiveLearnerProfile(string learnerId)
        {
            LearnerProfile profile = LearnerProfileStore.GetProfile(learnerId);
            if (profile == null)
            {
                return false;
            }

            if (currentSession != null)
            {
                EndSession();
            }

            currentLearner = profile;
            LearnerProfileStore.SetActiveLearnerId(profile.LearnerId);
            SetStoragePathsForLearner(profile.LearnerId);
            LoadProgress();
            return true;
        }

        public AdaptiveLearningRecommendation GetAdaptiveRecommendation()
        {
            OverallStatistics overall = GetOverallStatistics();
            string lessonId = !string.IsNullOrEmpty(overall.RecommendedLessonId)
                ? overall.RecommendedLessonId
                : "L01";

            bool guidedMode = overall.WeakestSkillScore > 0f && overall.WeakestSkillScore < 0.7f;
            return new AdaptiveLearningRecommendation
            {
                LearnerId = currentLearner?.LearnerId,
                SuggestedLessonId = lessonId,
                WeakestSkillTag = overall.WeakestSkillTag,
                GuidedModeRecommended = guidedMode,
                DifficultyAdjustment = guidedMode ? "DecreaseChoices" : "Maintain",
                Reason = guidedMode
                    ? $"Practice {overall.WeakestSkillTag} with fewer choices and more prompts."
                    : "Continue current lesson path."
            };
        }

        public ParentTeacherSummary GetParentTeacherSummary()
        {
            return new ParentTeacherSummary
            {
                Learner = currentLearner,
                Overall = GetOverallStatistics(),
                Recommendation = GetAdaptiveRecommendation(),
                ExportedAtString = DateTime.UtcNow.ToString("o")
            };
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

        private void SetStoragePathsForLearner(string learnerId)
        {
            string safeLearnerId = LearnerProfileStore.SanitizeLearnerId(learnerId);
            progressFilePath = Path.Combine(Application.persistentDataPath, $"{Path.GetFileNameWithoutExtension(ProgressFileName)}_{safeLearnerId}.json");
            sessionFilePath = Path.Combine(Application.persistentDataPath, $"{Path.GetFileNameWithoutExtension(SessionFileName)}_{safeLearnerId}.json");
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
                    WorstTime = stats.WorstTime,
                    TotalLearningRounds = stats.TotalLearningRounds,
                    TechnicalIssueCount = stats.TechnicalIssueCount,
                    MostCommonErrorType = stats.MostCommonErrorType,
                    MostCommonErrorCount = stats.MostCommonErrorCount,
                    WeakestSkillTag = stats.WeakestSkillTag,
                    RecommendedLessonId = stats.RecommendedLessonId,
                    RecommendationReason = stats.RecommendationReason
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

        public List<ActivityResult> GetResultsForLesson(string lessonId)
        {
            List<ActivityResult> results = new List<ActivityResult>();

            foreach (var result in allResults)
            {
                if (result.LessonId == lessonId)
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

        public ActivityStatistics GetStatisticsForLesson(string lessonId)
        {
            var results = GetResultsForLesson(lessonId);
            return CalculateStatistics(results);
        }

        public List<SkillMastery> GetSkillMasteries()
        {
            return CalculateSkillMasteries(allResults);
        }

        public SkillMastery GetWeakestSkillMastery()
        {
            SkillMastery weakest = null;
            foreach (SkillMastery mastery in CalculateSkillMasteries(allResults))
            {
                if (mastery.Attempts <= 0)
                {
                    continue;
                }

                if (weakest == null || mastery.MasteryScore < weakest.MasteryScore)
                {
                    weakest = mastery;
                }
            }

            return weakest;
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
                    WorstTime = entry.WorstTime,
                    TotalLearningRounds = entry.TotalLearningRounds,
                    TechnicalIssueCount = entry.TechnicalIssueCount,
                    MostCommonErrorType = entry.MostCommonErrorType,
                    MostCommonErrorCount = entry.MostCommonErrorCount,
                    WeakestSkillTag = entry.WeakestSkillTag,
                    RecommendedLessonId = entry.RecommendedLessonId,
                    RecommendationReason = entry.RecommendationReason
                };
                overall.ActivityStatistics[entry.ActivityId] = stats;
            }

            overall.TotalActivityTypesWithProgress = activityStatistics.Count;
            overall.TotalSessions = GetAllSessionCount();
            overall.TotalResults = allResults.Count;
            overall.TotalLearningRoundsCompleted = CountLearningRounds(allResults);
            overall.TotalTechnicalIssues = CountTechnicalIssues(allResults);
            overall.SkillMasteries = CalculateSkillMasteries(allResults);

            SkillMastery weakest = GetWeakestSkillMastery();
            if (weakest != null)
            {
                overall.WeakestSkillTag = weakest.SkillTag;
                overall.WeakestSkillScore = weakest.MasteryScore;
                overall.RecommendedLessonId = RecommendLessonForSkill(weakest.SkillTag);
            }

            overall.TotalActivitiesCompleted = overall.TotalLearningRoundsCompleted;

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

            Dictionary<string, int> errorCounts = new Dictionary<string, int>();

            foreach (var result in results)
            {
                if (result.HasTechnicalIssue)
                {
                    stats.TechnicalIssueCount++;
                    continue;
                }

                if (!result.CountsTowardMastery)
                {
                    continue;
                }

                stats.TotalAttempts++;
                stats.TotalLearningRounds++;

                if (result.IsCorrect)
                {
                    stats.SuccessfulAttempts++;
                }

                stats.TotalHintsUsed += result.HintsUsedCount;
                stats.TotalTimeSpent += result.TimeSpentSeconds;

                if (result.LearningIssue.HasError)
                {
                    string errorKey = string.IsNullOrEmpty(result.LearningIssue.ErrorCode)
                        ? result.LearningIssue.ErrorType.ToString()
                        : result.LearningIssue.ErrorCode;

                    if (!errorCounts.ContainsKey(errorKey))
                    {
                        errorCounts[errorKey] = 0;
                    }

                    errorCounts[errorKey]++;
                }
            }

            if (stats.TotalAttempts == 0)
            {
                stats.WeakestSkillTag = string.Empty;
                return stats;
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
                if (result.HasTechnicalIssue || !result.CountsTowardMastery)
                {
                    continue;
                }

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
            ApplyMostCommonError(stats, errorCounts);
            ApplySkillRecommendation(stats, CalculateSkillMasteries(results));

            return stats;
        }

        private static void ApplyMostCommonError(ActivityStatistics stats, Dictionary<string, int> errorCounts)
        {
            foreach (var kvp in errorCounts)
            {
                if (kvp.Value > stats.MostCommonErrorCount)
                {
                    stats.MostCommonErrorType = kvp.Key;
                    stats.MostCommonErrorCount = kvp.Value;
                }
            }
        }

        private static void ApplySkillRecommendation(ActivityStatistics stats, List<SkillMastery> masteries)
        {
            SkillMastery weakest = null;
            foreach (SkillMastery mastery in masteries)
            {
                if (mastery.Attempts <= 0)
                {
                    continue;
                }

                if (weakest == null || mastery.MasteryScore < weakest.MasteryScore)
                {
                    weakest = mastery;
                }
            }

            if (weakest == null)
            {
                return;
            }

            stats.WeakestSkillTag = weakest.SkillTag;
            stats.RecommendedLessonId = RecommendLessonForSkill(weakest.SkillTag);
            stats.RecommendationReason = weakest.MasteryScore < 0.7f
                ? $"Practice {weakest.SkillTag}: accuracy/hint score is {weakest.MasteryScore:P0}."
                : $"Keep reinforcing {weakest.SkillTag}.";
        }

        private static List<SkillMastery> CalculateSkillMasteries(List<ActivityResult> results)
        {
            Dictionary<string, SkillAccumulator> bySkill = new Dictionary<string, SkillAccumulator>();

            foreach (ActivityResult result in results)
            {
                if (result.HasTechnicalIssue || !result.CountsTowardMastery || result.SkillTags == null)
                {
                    continue;
                }

                foreach (string skill in result.SkillTags)
                {
                    if (string.IsNullOrEmpty(skill))
                    {
                        continue;
                    }

                    if (!bySkill.TryGetValue(skill, out SkillAccumulator accumulator))
                    {
                        accumulator = new SkillAccumulator();
                        bySkill[skill] = accumulator;
                    }

                    accumulator.Attempts++;
                    accumulator.HintsUsed += result.HintsUsedCount;
                    accumulator.TotalTimeSeconds += result.TimeSpentSeconds;
                    if (result.IsCorrect)
                    {
                        accumulator.CorrectAttempts++;
                    }
                }
            }

            List<SkillMastery> masteries = new List<SkillMastery>();
            foreach (var kvp in bySkill)
            {
                SkillAccumulator accumulator = kvp.Value;
                float accuracy = accumulator.Attempts > 0 ? (float)accumulator.CorrectAttempts / accumulator.Attempts : 0f;
                float hintPenalty = accumulator.Attempts > 0 ? Mathf.Clamp01((float)accumulator.HintsUsed / (accumulator.Attempts * 3f)) : 0f;

                masteries.Add(new SkillMastery
                {
                    SkillTag = kvp.Key,
                    Attempts = accumulator.Attempts,
                    CorrectAttempts = accumulator.CorrectAttempts,
                    HintsUsed = accumulator.HintsUsed,
                    AverageTimeSeconds = accumulator.Attempts > 0 ? accumulator.TotalTimeSeconds / accumulator.Attempts : 0f,
                    MasteryScore = Mathf.Clamp01(accuracy * (1f - hintPenalty * 0.35f))
                });
            }

            return masteries;
        }

        private static int CountLearningRounds(List<ActivityResult> results)
        {
            int count = 0;
            foreach (ActivityResult result in results)
            {
                if (!result.HasTechnicalIssue && result.CountsTowardMastery)
                {
                    count++;
                }
            }

            return count;
        }

        private static int CountTechnicalIssues(List<ActivityResult> results)
        {
            int count = 0;
            foreach (ActivityResult result in results)
            {
                if (result.HasTechnicalIssue)
                {
                    count++;
                }
            }

            return count;
        }

        private static string RecommendLessonForSkill(string skillTag)
        {
            foreach (LessonDefinition lesson in LessonMapRegistry.AllLessons)
            {
                if (lesson.SkillTags == null)
                {
                    continue;
                }

                foreach (string lessonSkill in lesson.SkillTags)
                {
                    if (lessonSkill == skillTag)
                    {
                        return lesson.LessonId;
                    }
                }
            }

            return string.Empty;
        }

        private class SkillAccumulator
        {
            public int Attempts;
            public int CorrectAttempts;
            public int HintsUsed;
            public float TotalTimeSeconds;
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
        public int TotalLearningRounds;
        public int TechnicalIssueCount;
        public string MostCommonErrorType;
        public int MostCommonErrorCount;
        public string WeakestSkillTag;
        public string RecommendedLessonId;
        public string RecommendationReason;
    }

    /// <summary>
    /// Data for a single learning session.
    /// Fixed version - uses string for DateTime.
    /// </summary>
    [Serializable]
    public class SessionData
    {
        public string SessionId;
        public string LearnerId;
        public string ActivityId;
        public string StartTimeString;  // ISO 8601 string
        public string EndTimeString;    // ISO 8601 string
        public float Duration;          // seconds

        // Runtime properties for convenience
        public DateTime StartTime => DateTime.TryParse(StartTimeString, null, System.Globalization.DateTimeStyles.RoundtripKind, out var dt) ? dt : default;

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
        public int TotalLearningRounds;
        public int TechnicalIssueCount;
        public string MostCommonErrorType;
        public int MostCommonErrorCount;
        public string WeakestSkillTag;
        public string RecommendedLessonId;
        public string RecommendationReason;
    }

    /// <summary>
    /// Overall statistics across all activities.
    /// Fixed version - uses runtime dictionary for access.
    /// </summary>
    [Serializable]
    public class OverallStatistics
    {
        public int TotalActivitiesCompleted;
        public int TotalActivityTypesWithProgress;
        public int TotalSessions;
        public int TotalResults;
        public int TotalLearningRoundsCompleted;
        public int TotalTechnicalIssues;
        public string WeakestSkillTag;
        public float WeakestSkillScore;
        public string RecommendedLessonId;
        public List<SkillMastery> SkillMasteries = new List<SkillMastery>();

        // Runtime access to activity stats
        [NonSerialized]
        public Dictionary<string, ActivityStatistics> ActivityStatistics = new Dictionary<string, ActivityStatistics>();
    }

    [Serializable]
    public class LearnerProfile
    {
        public string LearnerId;
        public string DisplayName;
        public int AgeYears;
        public string Grade;
        public string CreatedAtString;
        public string UpdatedAtString;
        public float PreferredVolume = 1f;
        public float FontScale = 1f;
        public bool AudioEnabled = true;
        public bool AnimationsEnabled = true;
        public bool SimplifiedMode;
    }

    [Serializable]
    public class LearnerProfileList
    {
        public List<LearnerProfile> Profiles = new List<LearnerProfile>();
    }

    [Serializable]
    public class AdaptiveLearningRecommendation
    {
        public string LearnerId;
        public string SuggestedLessonId;
        public string WeakestSkillTag;
        public string DifficultyAdjustment;
        public bool GuidedModeRecommended;
        public string Reason;
    }

    [Serializable]
    public class ParentTeacherSummary
    {
        public LearnerProfile Learner;
        public OverallStatistics Overall;
        public AdaptiveLearningRecommendation Recommendation;
        public string ExportedAtString;
    }

    public static class LearnerProfileStore
    {
        private const string ActiveLearnerIdKey = "UserPrefs.ActiveLearnerId";
        private const string ProfilesFileName = "learner_profiles.json";
        private const string DefaultLearnerId = "default";

        public static LearnerProfile GetActiveOrCreateDefault()
        {
            List<LearnerProfile> profiles = LoadProfiles();
            if (profiles.Count == 0)
            {
                LearnerProfile profile = CreateDefaultProfile();
                SaveProfiles(new List<LearnerProfile> { profile });
                SetActiveLearnerId(profile.LearnerId);
                return profile;
            }

            string activeId = PlayerPrefs.GetString(ActiveLearnerIdKey, profiles[0].LearnerId);
            LearnerProfile active = FindProfile(profiles, activeId) ?? profiles[0];
            SetActiveLearnerId(active.LearnerId);
            return active;
        }

        public static LearnerProfile CreateOrUpdate(string displayName, int ageYears = 0, string grade = null)
        {
            List<LearnerProfile> profiles = LoadProfiles();
            string normalizedName = string.IsNullOrWhiteSpace(displayName) ? "Learner" : displayName.Trim();
            LearnerProfile profile = null;

            foreach (LearnerProfile existing in profiles)
            {
                if (existing.DisplayName == normalizedName)
                {
                    profile = existing;
                    break;
                }
            }

            if (profile == null)
            {
                profile = new LearnerProfile
                {
                    LearnerId = Guid.NewGuid().ToString("N"),
                    DisplayName = normalizedName,
                    CreatedAtString = DateTime.UtcNow.ToString("o")
                };
                profiles.Add(profile);
            }

            profile.AgeYears = ageYears;
            profile.Grade = grade;
            profile.UpdatedAtString = DateTime.UtcNow.ToString("o");
            SaveProfiles(profiles);
            SetActiveLearnerId(profile.LearnerId);
            return profile;
        }

        public static LearnerProfile GetProfile(string learnerId)
        {
            return FindProfile(LoadProfiles(), learnerId);
        }

        public static List<LearnerProfile> LoadProfiles()
        {
            try
            {
                string path = GetProfilesPath();
                if (!File.Exists(path))
                {
                    return new List<LearnerProfile>();
                }

                LearnerProfileList list = JsonUtility.FromJson<LearnerProfileList>(File.ReadAllText(path));
                return list?.Profiles ?? new List<LearnerProfile>();
            }
            catch (Exception e)
            {
                Debug.LogError($"[LearnerProfileStore] Failed to load profiles: {e.Message}");
                return new List<LearnerProfile>();
            }
        }

        public static void SetActiveLearnerId(string learnerId)
        {
            PlayerPrefs.SetString(ActiveLearnerIdKey, SanitizeLearnerId(learnerId));
            PlayerPrefs.Save();
        }

        public static string SanitizeLearnerId(string learnerId)
        {
            if (string.IsNullOrWhiteSpace(learnerId))
            {
                return DefaultLearnerId;
            }

            foreach (char invalid in Path.GetInvalidFileNameChars())
            {
                learnerId = learnerId.Replace(invalid, '_');
            }

            return learnerId;
        }

        private static LearnerProfile CreateDefaultProfile()
        {
            string now = DateTime.UtcNow.ToString("o");
            return new LearnerProfile
            {
                LearnerId = DefaultLearnerId,
                DisplayName = "Default Learner",
                CreatedAtString = now,
                UpdatedAtString = now
            };
        }

        private static LearnerProfile FindProfile(List<LearnerProfile> profiles, string learnerId)
        {
            string safeLearnerId = SanitizeLearnerId(learnerId);
            foreach (LearnerProfile profile in profiles)
            {
                if (profile.LearnerId == safeLearnerId)
                {
                    return profile;
                }
            }

            return null;
        }

        private static void SaveProfiles(List<LearnerProfile> profiles)
        {
            LearnerProfileList list = new LearnerProfileList { Profiles = profiles };
            File.WriteAllText(GetProfilesPath(), JsonUtility.ToJson(list, true));
        }

        private static string GetProfilesPath()
        {
            return Path.Combine(Application.persistentDataPath, ProfilesFileName);
        }
    }
}
