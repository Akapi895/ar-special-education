using Core.Data.LocalStorage;
using Core.Learning.Models;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Project.App
{
    /// <summary>
    /// Progress dashboard view - displays learning statistics from LocalProgressStorage.
    /// Supports premium visual UI elements like sliders, stars, and practice recommendations.
    /// </summary>
    public class ProgressDashboardView : MonoBehaviour
    {
        [System.Serializable]
        public class ActivityDashboardCard
        {
            public string activityId;
            public Slider progressSlider;
            public Text progressPercentageText;
            public GameObject[] stars; // 1-3 stars
            public Text totalAttemptsText;
            public Text averageTimeText;
            public GameObject practiceRecommendationIndicator; // Highlight if low accuracy
        }

        [Header("Simple UI Fallback References")]
        [SerializeField]
        private Text overallStatsText;

        [SerializeField]
        private Text activityStatsText;

        [SerializeField]
        private Button backButton;

        [Header("Premium UI References (Optional)")]
        [SerializeField] private Text totalSessionsValText;
        [SerializeField] private Text totalResultsValText;
        [SerializeField] private Slider overallProgressSlider;
        [SerializeField] private Text recommendationText;
        [SerializeField] private ActivityDashboardCard[] activityCards;

        [Header("Navigation")]
        [SerializeField]
        private string mainMenuSceneName = "SC_MainMenu";

        [Header("Activity IDs")]
        [SerializeField]
        private string[] activityIds = new string[] { "QuantityMatch", "NumberLineJump", "CompareQuantity" };

        private LocalProgressStorage progressStorage;

        private void Start()
        {
            progressStorage = ProgressStorageProxy.Instance;

            // Setup back button
            if (backButton != null)
            {
                backButton.onClick.AddListener(OnBack);
            }

            // Load and display progress
            LoadAndDisplayProgress();

            Debug.Log("[ProgressDashboardView] Progress dashboard loaded.");
        }

        private void OnDestroy()
        {
            if (backButton != null)
            {
                backButton.onClick.RemoveListener(OnBack);
            }
        }

        private void LoadAndDisplayProgress()
        {
            if (progressStorage == null)
            {
                DisplayError("Progress storage not available.");
                return;
            }

            // Get overall statistics
            OverallStatistics overall = progressStorage.GetOverallStatistics();
            DisplayOverallStats(overall);

            // Get per-activity statistics
            DisplayActivityStats();
            DisplayPremiumActivityCards();
        }

        private void DisplayOverallStats(OverallStatistics overall)
        {
            // Simple text display fallback
            if (overallStatsText != null)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Learning Progress");
                sb.AppendLine("================");
                LearnerProfile learner = progressStorage.GetActiveLearnerProfile();
                if (learner != null)
                {
                    sb.AppendLine($"Learner: {learner.DisplayName}");
                }
                sb.AppendLine($"Rounds Completed: {overall.TotalLearningRoundsCompleted}");
                sb.AppendLine($"Activity Types Practiced: {overall.TotalActivityTypesWithProgress}");
                sb.AppendLine($"Total Sessions: {overall.TotalSessions}");
                sb.AppendLine($"Total Results: {overall.TotalResults}");
                sb.AppendLine($"Technical Issues: {overall.TotalTechnicalIssues}");
                if (!string.IsNullOrEmpty(overall.WeakestSkillTag))
                {
                    sb.AppendLine($"Needs Practice: {overall.WeakestSkillTag} ({overall.WeakestSkillScore:P0})");
                }
                if (!string.IsNullOrEmpty(overall.RecommendedLessonId))
                {
                    sb.AppendLine($"Next Lesson: {GetLessonDisplayName(overall.RecommendedLessonId)}");
                }
                AdaptiveLearningRecommendation recommendation = progressStorage.GetAdaptiveRecommendation();
                if (recommendation != null && recommendation.GuidedModeRecommended)
                {
                    sb.AppendLine($"Mode: Guided practice ({recommendation.DifficultyAdjustment})");
                }
                overallStatsText.text = sb.ToString();
            }

            // Premium visual display
            if (totalSessionsValText != null)
            {
                totalSessionsValText.text = overall.TotalSessions.ToString();
            }
            if (totalResultsValText != null)
            {
                totalResultsValText.text = overall.TotalResults.ToString();
            }
            if (overallProgressSlider != null)
            {
                // Simple average of the completed status
                float totalProgress = 0f;
                int count = 0;
                foreach (string activityId in activityIds)
                {
                    ActivityStatistics stats = progressStorage.GetActivityStatistics(activityId);
                    if (stats != null)
                    {
                        totalProgress += Mathf.Clamp01(stats.TotalLearningRounds / 10f);
                    }
                    count++;
                }
                overallProgressSlider.value = count > 0 ? (totalProgress / count) : 0f;
            }
        }

        private void DisplayActivityStats()
        {
            if (activityStatsText == null || progressStorage == null)
            {
                return;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Activity Statistics");
            sb.AppendLine("====================");

            foreach (string activityId in activityIds)
            {
                ActivityStatistics stats = progressStorage.GetActivityStatistics(activityId);
                if (stats != null)
                {
                    sb.AppendLine();
                    sb.AppendLine($"{activityId}:");
                    sb.AppendLine($"  Learning Rounds: {stats.TotalLearningRounds}");
                    sb.AppendLine($"  Success Rate: {(stats.SuccessRate * 100):F1}%");
                    sb.AppendLine($"  Avg Time: {stats.AverageTimePerAttempt:F1}s");
                    sb.AppendLine($"  Best Time: {stats.BestTime:F1}s");
                    sb.AppendLine($"  Hints Used: {stats.TotalHintsUsed}");
                    sb.AppendLine($"  Technical Issues: {stats.TechnicalIssueCount}");
                    if (!string.IsNullOrEmpty(stats.MostCommonErrorType))
                    {
                        sb.AppendLine($"  Common Error: {stats.MostCommonErrorType} x{stats.MostCommonErrorCount}");
                    }
                    if (!string.IsNullOrEmpty(stats.WeakestSkillTag))
                    {
                        sb.AppendLine($"  Weak Skill: {stats.WeakestSkillTag}");
                    }
                    if (!string.IsNullOrEmpty(stats.RecommendedLessonId))
                    {
                        sb.AppendLine($"  Recommended: {GetLessonDisplayName(stats.RecommendedLessonId)}");
                    }
                }
                else
                {
                    sb.AppendLine();
                    sb.AppendLine($"{activityId}: No data yet");
                }
            }

            activityStatsText.text = sb.ToString();
        }

        private void DisplayPremiumActivityCards()
        {
            if (activityCards == null || progressStorage == null) return;

            string lowestAccuracyActivity = null;
            float lowestAccuracy = 1.1f;

            foreach (var card in activityCards)
            {
                if (card == null || string.IsNullOrEmpty(card.activityId)) continue;

                ActivityStatistics stats = progressStorage.GetActivityStatistics(card.activityId);

                if (stats != null && stats.TotalAttempts > 0)
                {
                    float completion = Mathf.Clamp01(stats.TotalLearningRounds / 10f);
                    if (card.progressSlider != null) card.progressSlider.value = completion;
                    if (card.progressPercentageText != null) card.progressPercentageText.text = $"{Mathf.RoundToInt(completion * 100f)}%";
                    
                    if (card.totalAttemptsText != null) card.totalAttemptsText.text = $"{stats.TotalLearningRounds}";
                    if (card.averageTimeText != null) card.averageTimeText.text = $"{stats.AverageTimePerAttempt:F1}s";

                    // Star rating display: <50% = 1 star, 50-80% = 2 stars, >80% = 3 stars
                    int starCount = 0;
                    if (stats.SuccessRate > 0.8f) starCount = 3;
                    else if (stats.SuccessRate > 0.5f) starCount = 2;
                    else if (stats.SuccessRate > 0f) starCount = 1;

                    if (card.stars != null)
                    {
                        for (int i = 0; i < card.stars.Length; i++)
                        {
                            if (card.stars[i] != null)
                            {
                                card.stars[i].SetActive(i < starCount);
                            }
                        }
                    }

                    // Track activity with lowest success rate for practice recommendation
                    if (stats.SuccessRate < lowestAccuracy)
                    {
                        lowestAccuracy = stats.SuccessRate;
                        lowestAccuracyActivity = card.activityId;
                    }
                }
                else
                {
                    // No data fallback
                    if (card.progressSlider != null) card.progressSlider.value = 0f;
                    if (card.progressPercentageText != null) card.progressPercentageText.text = "0%";
                    if (card.totalAttemptsText != null) card.totalAttemptsText.text = "0";
                    if (card.averageTimeText != null) card.averageTimeText.text = "--";
                    if (card.stars != null)
                    {
                        foreach (var star in card.stars)
                        {
                            if (star != null) star.SetActive(false);
                        }
                    }
                }
            }

            // Update recommendations and indicators
            foreach (var card in activityCards)
            {
                if (card == null) continue;
                bool isRecommended = (card.activityId == lowestAccuracyActivity) && (lowestAccuracy < 0.8f);
                if (card.practiceRecommendationIndicator != null)
                {
                    card.practiceRecommendationIndicator.SetActive(isRecommended);
                }
            }

            if (recommendationText != null)
            {
                if (lowestAccuracyActivity != null && lowestAccuracy < 0.8f)
                {
                    // Vietnamese localization
                    string activityName = GetActivityVietnameseName(lowestAccuracyActivity);
                    recommendationText.text = $"Nen luyen tap them bai: {activityName}";
                }
                else
                {
                    recommendationText.text = "Con dang lam tot cac bai hien tai!";
                }
            }
        }

        private string GetActivityVietnameseName(string activityId)
        {
            switch (activityId)
            {
                case "QuantityMatch": return "Ghep so voi luong";
                case "CompareQuantity": return "So sanh so luong";
                case "NumberLineJump": return "Nhay tren truc so";
                default: return activityId;
            }
        }

        private static string GetLessonDisplayName(string lessonId)
        {
            LessonDefinition lesson = LessonMapRegistry.GetLesson(lessonId);
            return lesson != null ? $"{lesson.LessonId} - {lesson.Title}" : lessonId;
        }

        private void DisplayError(string errorMessage)
        {
            if (overallStatsText != null)
            {
                overallStatsText.text = $"Error: {errorMessage}";
            }

            if (activityStatsText != null)
            {
                activityStatsText.text = string.Empty;
            }

            Debug.LogError($"[ProgressDashboardView] {errorMessage}");
        }

        private void OnBack()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}
