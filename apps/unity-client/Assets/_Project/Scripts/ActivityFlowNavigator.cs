using UnityEngine.SceneManagement;

namespace Project.App
{
    public static class ActivityFlowNavigator
    {
        private static readonly string[] ActivityOrder =
        {
            "QuantityMatch",
            "NumberLineJump",
            "CompareQuantity"
        };

        private const string GameplaySceneName = "SC_ARGameplay";
        private const string ProgressSceneName = "SC_ProgressDashboard";

        public static bool TryGetNextActivityId(string currentActivityId, out string nextActivityId)
        {
            for (int i = 0; i < ActivityOrder.Length - 1; i++)
            {
                if (ActivityOrder[i] == currentActivityId)
                {
                    nextActivityId = ActivityOrder[i + 1];
                    return true;
                }
            }

            nextActivityId = null;
            return false;
        }

        public static bool LoadNextActivity(string currentActivityId)
        {
            if (!TryGetNextActivityId(currentActivityId, out string nextActivityId))
            {
                return false;
            }

            LoadActivity(nextActivityId);
            return true;
        }

        public static void LoadActivity(string activityId)
        {
            SelectedActivityData.ActivityId = activityId;
            SelectedActivityData.ConfigPath = activityId;
            SceneManager.LoadScene(GameplaySceneName);
        }

        public static void LoadProgressDashboard()
        {
            SceneManager.LoadScene(ProgressSceneName);
        }
    }
}
