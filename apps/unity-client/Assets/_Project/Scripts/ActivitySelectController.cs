using Core.Data.LocalStorage;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Project.App
{
    /// <summary>
    /// Activity select controller - displays available activities and loads selected one.
    /// </summary>
    public class ActivitySelectController : MonoBehaviour
    {
        [System.Serializable]
        public class ActivityButton
        {
            public Button button;
            public string activityId;
            public string sceneName;
        }

        [Header("Activity Buttons")]
        [SerializeField]
        private ActivityButton[] activities;

        [SerializeField]
        private Button backButton;

        [Header("Scene Names")]
        [SerializeField]
        private string mainMenuSceneName = "SC_MainMenu";

        [Header("Gameplay Scene")]
        [SerializeField]
        private string gameplaySceneName = "SC_ARGameplay";

        [Header("Playable Activities")]
        [SerializeField]
        private string[] playableActivityIds = new string[] { "QuantityMatch", "NumberLineJump", "CompareQuantity" };

        private void Start()
        {
            // Setup activity button listeners
            foreach (var activity in activities)
            {
                if (activity.button != null)
                {
                    bool isUnlocked = IsActivityUnlocked(activity.activityId);
                    activity.button.interactable = isUnlocked;
                    activity.button.onClick.AddListener(() => OnActivitySelected(activity));
                }
            }

            // Setup back button
            if (backButton != null)
            {
                backButton.onClick.AddListener(OnBack);
            }

            Debug.Log("[ActivitySelectController] Activity select loaded.");
        }

        private void OnDestroy()
        {
            // Clean up listeners
            foreach (var activity in activities)
            {
                if (activity.button != null)
                {
                    activity.button.onClick.RemoveAllListeners();
                }
            }

            if (backButton != null)
            {
                backButton.onClick.RemoveListener(OnBack);
            }
        }

        private bool IsActivityUnlocked(string activityId)
        {
            if (string.IsNullOrEmpty(activityId))
            {
                return false;
            }

            foreach (string playableActivityId in playableActivityIds)
            {
                if (activityId == playableActivityId)
                {
                    return true;
                }
            }

            return false;
        }

        private void OnActivitySelected(ActivityButton activity)
        {
            // Store selected activity ID for the gameplay scene to load
            SelectedActivityData.ActivityId = activity.activityId;
            SelectedActivityData.ConfigPath = activity.sceneName;

            SceneManager.LoadScene(gameplaySceneName);

            Debug.Log($"[ActivitySelectController] Selected activity: {activity.activityId}");
        }

        private void OnBack()
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }

    /// <summary>
    /// Static data holder for passing data between scenes.
    /// </summary>
    public static class SelectedActivityData
    {
        public static string ActivityId { get; set; }
        public static string ConfigPath { get; set; }

        public static void Clear()
        {
            ActivityId = null;
            ConfigPath = null;
        }
    }
}
