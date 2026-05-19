using Core.Data.LocalStorage;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Project.App
{
    /// <summary>
    /// Progress dashboard view - displays learning statistics from LocalProgressStorage.
    /// </summary>
    public class ProgressDashboardView : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField]
        private Text overallStatsText;

        [SerializeField]
        private Text activityStatsText;

        [SerializeField]
        private Button backButton;

        [Header("Navigation")]
        [SerializeField]
        private string mainMenuSceneName = "SC_MainMenu";

        [Header("Activity IDs")]
        [SerializeField]
        private string[] activityIds = new string[] { "QuantityMatch", "NumberLineJump", "CompareQuantity" };

        private ProgressStorageProxy progressStorage;

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
        }

        private void DisplayOverallStats(OverallStatistics overall)
        {
            if (overallStatsText == null)
            {
                return;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Overall Progress");
            sb.AppendLine("================");
            sb.AppendLine($"Activities Completed: {overall.TotalActivitiesCompleted}");
            sb.AppendLine($"Total Sessions: {overall.TotalSessions}");
            sb.AppendLine($"Total Results: {overall.TotalResults}");

            overallStatsText.text = sb.ToString();
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
                    sb.AppendLine($"  Attempts: {stats.TotalAttempts}");
                    sb.AppendLine($"  Success Rate: {(stats.SuccessRate * 100):F1}%");
                    sb.AppendLine($"  Avg Time: {stats.AverageTimePerAttempt:F1}s");
                    sb.AppendLine($"  Best Time: {stats.BestTime:F1}s");
                    sb.AppendLine($"  Hints Used: {stats.TotalHintsUsed}");
                }
                else
                {
                    sb.AppendLine();
                    sb.AppendLine($"{activityId}: No data yet");
                }
            }

            activityStatsText.text = sb.ToString();
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
