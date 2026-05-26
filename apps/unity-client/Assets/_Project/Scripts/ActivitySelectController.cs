using Core.Data;
using Core.Data.LocalStorage;
using Core.Learning.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Project.App
{
    /// <summary>
    /// Activity select controller - displays available activities, checks locking status, 
    /// and displays progress markers using the Design System components.
    /// </summary>
    public class ActivitySelectController : MonoBehaviour
    {
        [System.Serializable]
        public class ActivityButton
        {
            public Button button;
            public string activityId;
            public string lessonId;
            public string sceneName;
            
            [Header("Visual Progress (Optional)")]
            public Slider progressSlider;
            public Text progressText;
            public GameObject lockIcon;
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
                    bool isUnlocked = IsActivityUnlocked(activity);
                    activity.button.interactable = isUnlocked;
                    
                    if (activity.lockIcon != null)
                    {
                        activity.lockIcon.SetActive(!isUnlocked);
                    }
                    
                    activity.button.onClick.AddListener(() => OnActivitySelected(activity));
                    
                    // Display progress if possible
                    UpdateActivityProgress(activity);
                }
            }

            // Setup back button
            if (backButton != null)
            {
                backButton.onClick.AddListener(OnBack);
            }

            Debug.Log("[ActivitySelectController] Activity select loaded.");
        }

        private void UpdateActivityProgress(ActivityButton activity)
        {
            try
            {
                // Retrieve statistics from local storage
                string lessonId = ResolveLessonId(activity);
                ActivityStatistics stats = string.IsNullOrEmpty(lessonId)
                    ? ProgressStorageProxy.Instance.GetActivityStatistics(activity.activityId)
                    : ProgressStorageProxy.Instance.GetLessonStatistics(lessonId);
                
                if (stats != null)
                {
                    // Target 10 rounds for 100% completion in Easy level
                    float progressFraction = Mathf.Clamp01(stats.TotalLearningRounds / 10f);
                    
                    if (activity.progressSlider != null)
                    {
                        activity.progressSlider.value = progressFraction;
                    }
                    
                    if (activity.progressText != null)
                    {
                        activity.progressText.text = $"{Mathf.RoundToInt(progressFraction * 100f)}%";
                    }
                }
                else
                {
                    if (activity.progressSlider != null) activity.progressSlider.value = 0f;
                    if (activity.progressText != null) activity.progressText.text = "0%";
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[ActivitySelectController] Could not fetch progress for {activity.activityId}: {ex.Message}");
                if (activity.progressSlider != null) activity.progressSlider.value = 0f;
                if (activity.progressText != null) activity.progressText.text = "0%";
            }
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

        private bool IsActivityUnlocked(ActivityButton activity)
        {
            string activityId = activity?.activityId;
            if (string.IsNullOrEmpty(activityId))
            {
                return false;
            }

            bool isPlayable = false;
            foreach (string playableActivityId in playableActivityIds)
            {
                if (activityId == playableActivityId)
                {
                    isPlayable = true;
                    break;
                }
            }

            if (!isPlayable)
            {
                return false;
            }

            if (!UserPreferences.EnforceLessonPrerequisites)
            {
                return true;
            }

            string lessonId = ResolveLessonId(activity);
            return string.IsNullOrEmpty(lessonId) || LessonMapRegistry.IsLessonUnlocked(lessonId, IsLessonMastered);
        }

        private bool IsLessonMastered(string lessonId)
        {
            LessonDefinition lesson = LessonMapRegistry.GetLesson(lessonId);
            if (lesson == null)
            {
                return false;
            }

            ActivityStatistics stats = ProgressStorageProxy.Instance.GetLessonStatistics(lessonId);
            return stats.TotalLearningRounds >= lesson.MinimumRoundsForMastery
                && stats.SuccessRate >= lesson.MinimumAccuracyForMastery;
        }

        private static string ResolveLessonId(ActivityButton activity)
        {
            if (activity == null)
            {
                return null;
            }

            if (!string.IsNullOrEmpty(activity.lessonId))
            {
                return activity.lessonId;
            }

            LessonDefinition lesson = LessonMapRegistry.GetFirstLessonForActivity(activity.activityId);
            return lesson?.LessonId;
        }

        private void OnActivitySelected(ActivityButton activity)
        {
            // Store selected activity ID for the gameplay scene to load
            SelectedActivityData.ActivityId = activity.activityId;
            SelectedActivityData.LessonId = ResolveLessonId(activity);

            SceneManager.LoadScene(gameplaySceneName);

            Debug.Log($"[ActivitySelectController] Selected activity: {activity.activityId}, lesson: {SelectedActivityData.LessonId}");
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
        private const string ActivityIdKey = "SelectedActivityData.ActivityId";
        private const string LessonIdKey = "SelectedActivityData.LessonId";
        private const string ConfigPathKey = "SelectedActivityData.ConfigPath";

        private static string activityId;
        private static string lessonId;
        private static string configPath;

        public static string ActivityId
        {
            get
            {
                if (!string.IsNullOrEmpty(activityId))
                {
                    return activityId;
                }

                return PlayerPrefs.GetString(ActivityIdKey, null);
            }
            set
            {
                activityId = value;
                SetStoredValue(ActivityIdKey, value);
            }
        }

        public static string LessonId
        {
            get
            {
                if (!string.IsNullOrEmpty(lessonId))
                {
                    return lessonId;
                }

                return PlayerPrefs.GetString(LessonIdKey, null);
            }
            set
            {
                lessonId = value;
                SetStoredValue(LessonIdKey, value);
            }
        }

        public static string ConfigPath
        {
            get
            {
                if (!string.IsNullOrEmpty(configPath))
                {
                    return configPath;
                }

                return PlayerPrefs.GetString(ConfigPathKey, null);
            }
            set
            {
                configPath = value;
                SetStoredValue(ConfigPathKey, value);
            }
        }

        public static void Clear()
        {
            activityId = null;
            lessonId = null;
            configPath = null;
            PlayerPrefs.DeleteKey(ActivityIdKey);
            PlayerPrefs.DeleteKey(LessonIdKey);
            PlayerPrefs.DeleteKey(ConfigPathKey);
            PlayerPrefs.Save();
        }

        private static void SetStoredValue(string key, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                PlayerPrefs.DeleteKey(key);
            }
            else
            {
                PlayerPrefs.SetString(key, value);
            }

            PlayerPrefs.Save();
        }
    }
}
