using Core.Data.LocalStorage;
using Core.Learning.Models;
using Core.UI.Components;
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
        private string[] activityIds = new string[] { "QuantityMatch", "CompareQuantity", "NumberBonds", "NumberLineJump" };

        private LocalProgressStorage progressStorage;

        private void Start()
        {
            progressStorage = ProgressStorageProxy.Instance;
            LocalizeSceneLabels();
            UIKidFriendlyStyle.ApplyReadableTextToScene(3, 24);

            // Setup back button
            if (backButton != null)
            {
                UIKidFriendlyStyle.Apply(
                    backButton,
                    KidButtonPurpose.Home,
                    "Trang chủ",
                    28);
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
                DisplayError("Không tải được dữ liệu tiến độ.");
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
                sb.AppendLine("Tiến độ học tập");
                sb.AppendLine("================");
                LearnerProfile learner = progressStorage.GetActiveLearnerProfile();
                if (learner != null)
                {
                    sb.AppendLine($"Người học: {learner.DisplayName}");
                }
                sb.AppendLine($"Lượt hoàn thành: {overall.TotalLearningRoundsCompleted}");
                sb.AppendLine($"Dạng bài đã luyện: {overall.TotalActivityTypesWithProgress}");
                sb.AppendLine($"Số buổi học: {overall.TotalSessions}");
                sb.AppendLine($"Số kết quả: {overall.TotalResults}");
                sb.AppendLine($"Lỗi kỹ thuật: {overall.TotalTechnicalIssues}");
                if (!string.IsNullOrEmpty(overall.WeakestSkillTag))
                {
                    sb.AppendLine($"Cần luyện thêm: {overall.WeakestSkillTag} ({overall.WeakestSkillScore:P0})");
                }
                if (!string.IsNullOrEmpty(overall.RecommendedLessonId))
                {
                    sb.AppendLine($"Bài tiếp theo: {GetLessonDisplayName(overall.RecommendedLessonId)}");
                }
                AdaptiveLearningRecommendation recommendation = progressStorage.GetAdaptiveRecommendation();
                if (recommendation != null && recommendation.GuidedModeRecommended)
                {
                    sb.AppendLine($"Chế độ: luyện tập có hướng dẫn ({recommendation.DifficultyAdjustment})");
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
            sb.AppendLine("Thống kê từng bài");
            sb.AppendLine("====================");

            foreach (string activityId in activityIds)
            {
                ActivityStatistics stats = progressStorage.GetActivityStatistics(activityId);
                if (stats != null)
                {
                    sb.AppendLine();
                    sb.AppendLine($"{GetActivityVietnameseName(activityId)}:");
                    sb.AppendLine($"  Vòng học: {stats.TotalLearningRounds}");
                    sb.AppendLine($"  Tỉ lệ đúng: {(stats.SuccessRate * 100):F1}%");
                    sb.AppendLine($"  Thời gian TB: {stats.AverageTimePerAttempt:F1}s");
                    sb.AppendLine($"  Nhanh nhất: {stats.BestTime:F1}s");
                    sb.AppendLine($"  Gợi ý đã dùng: {stats.TotalHintsUsed}");
                    sb.AppendLine($"  Lỗi kỹ thuật: {stats.TechnicalIssueCount}");
                    if (!string.IsNullOrEmpty(stats.MostCommonErrorType))
                    {
                        sb.AppendLine($"  Lỗi thường gặp: {stats.MostCommonErrorType} x{stats.MostCommonErrorCount}");
                    }
                    if (!string.IsNullOrEmpty(stats.WeakestSkillTag))
                    {
                        sb.AppendLine($"  Kỹ năng cần luyện: {stats.WeakestSkillTag}");
                    }
                    if (!string.IsNullOrEmpty(stats.RecommendedLessonId))
                    {
                        sb.AppendLine($"  Đề xuất: {GetLessonDisplayName(stats.RecommendedLessonId)}");
                    }
                }
                else
                {
                    sb.AppendLine();
                    sb.AppendLine($"{GetActivityVietnameseName(activityId)}: Chưa có dữ liệu");
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
                    recommendationText.text = $"Nên luyện tập thêm bài: {activityName}";
                }
                else
                {
                    recommendationText.text = "Con đang làm tốt các bài hiện tại!";
                }
            }
        }

        private string GetActivityVietnameseName(string activityId)
        {
            switch (activityId)
            {
                case "QuantityMatch": return "Ghép số với lượng";
                case "CompareQuantity": return "So sánh số lượng";
                case "NumberLineJump": return "Nhảy trên trục số";
                case "NumberBonds": return "T\u00e1ch-g\u1ed9p s\u1ed1";
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
                overallStatsText.text = $"Lỗi: {errorMessage}";
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

        private static void LocalizeSceneLabels()
        {
            Text[] texts = UnityEngine.Object.FindObjectsByType<Text>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            for (int i = 0; i < texts.Length; i++)
            {
                Text text = texts[i];
                if (text == null)
                {
                    continue;
                }

                string content = text.text.Trim();
                if (content == "Overall Stats")
                {
                    text.text = "Thống kê tổng quan";
                }
                else if (content == "Activity Stats")
                {
                    text.text = "Thống kê bài học";
                }
                else if (content == "Progress")
                {
                    text.text = "Tiến độ";
                }
            }
        }
    }
}
