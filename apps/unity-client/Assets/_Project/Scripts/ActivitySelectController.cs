using Core.Data;
using Core.Data.LocalStorage;
using Core.Learning.Models;
using Core.UI.Components;
using Core.UI.Localization;
using System.Collections.Generic;
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
        private string[] playableActivityIds = new string[] { "QuantityMatch", "CompareQuantity", "NumberBonds", "NumberLineJump" };

        private static readonly string[] RequiredPlayableActivityIds =
        {
            "QuantityMatch",
            "CompareQuantity",
            "NumberBonds",
            "NumberLineJump"
        };

        private readonly List<ActivityButton> runtimeActivities = new List<ActivityButton>();

        private void Start()
        {
            LocalizeSceneLabels();
            ActivityButton[] availableActivities = EnsureRuntimeActivityButtons();

            // Setup activity button listeners
            foreach (var activity in availableActivities)
            {
                if (activity.button != null)
                {
                    bool isUnlocked = IsActivityUnlocked(activity);
                    string label = GetActivityDisplayName(activity.activityId);
                    UIKidFriendlyStyle.Apply(
                        activity.button,
                        isUnlocked ? KidButtonPurpose.Primary : KidButtonPurpose.Neutral,
                        label,
                        30);
                    UIKidFriendlyStyle.HideButtonBackground(activity.button);
                    UIKidFriendlyStyle.SetButtonTextColorWithOutline(
                        activity.button,
                        GetActivityTextColor(activity.activityId));
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
                UIKidFriendlyStyle.Apply(
                    backButton,
                    KidButtonPurpose.Home,
                    SimpleLocalization.Get("btn_home"),
                    28);
                UIKidFriendlyStyle.HideButtonBackground(backButton);
                UIKidFriendlyStyle.SetButtonTextColorWithOutline(backButton, new Color(1f, 0.7f, 0.78f, 1f));
                backButton.onClick.AddListener(OnBack);
            }

            Debug.Log("[ActivitySelectController] Activity select loaded.");
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
                if (content == "Choose Activity")
                {
                    text.text = "Chọn bài học";
                    UIKidFriendlyStyle.ApplyReadableText(text, 6, 34, new Color(1f, 0.86f, 0.36f, 1f));
                }
            }
        }

        private static string GetActivityDisplayName(string activityId)
        {
            return activityId switch
            {
                "QuantityMatch" => SimpleLocalization.Get("activity_quantity_match"),
                "NumberLineJump" => SimpleLocalization.Get("activity_number_line"),
                "CompareQuantity" => SimpleLocalization.Get("activity_compare_quantity"),
                "NumberBonds" => SimpleLocalization.Get("activity_number_bonds"),
                _ => activityId
            };
        }

        private static Color GetActivityTextColor(string activityId)
        {
            return activityId switch
            {
                "QuantityMatch" => new Color(1f, 0.88f, 0.32f, 1f),
                "NumberLineJump" => new Color(0.62f, 0.92f, 1f, 1f),
                "CompareQuantity" => new Color(0.62f, 1f, 0.6f, 1f),
                "NumberBonds" => new Color(1f, 0.72f, 0.92f, 1f),
                _ => new Color(1f, 0.94f, 0.48f, 1f)
            };
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
            RemoveActivityButtonListeners(activities);
            RemoveActivityButtonListeners(runtimeActivities);

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

            if (!IsPlayableActivity(activityId))
            {
                return false;
            }

            return true;
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

        private ActivityButton[] EnsureRuntimeActivityButtons()
        {
            runtimeActivities.Clear();
            var allActivities = new List<ActivityButton>();
            var existingIds = new HashSet<string>();

            if (activities != null)
            {
                foreach (ActivityButton activity in activities)
                {
                    if (activity == null || activity.button == null || string.IsNullOrEmpty(activity.activityId))
                    {
                        continue;
                    }

                    allActivities.Add(activity);
                    existingIds.Add(activity.activityId);
                }
            }

            Transform parent = ResolveRuntimeButtonParent();
            int missingIndex = 0;
            foreach (string activityId in RequiredPlayableActivityIds)
            {
                if (existingIds.Contains(activityId) || parent == null)
                {
                    continue;
                }

                ActivityButton runtimeActivity = CreateRuntimeActivityButton(parent, activityId, missingIndex);
                runtimeActivities.Add(runtimeActivity);
                allActivities.Add(runtimeActivity);
                existingIds.Add(activityId);
                missingIndex++;
            }

            ArrangeActivityButtons(allActivities);

            return allActivities.ToArray();
        }

        private Transform ResolveRuntimeButtonParent()
        {
            if (backButton != null)
            {
                return backButton.transform.parent;
            }

            Canvas canvas = FindFirstObjectByType<Canvas>();
            return canvas != null ? canvas.transform : null;
        }

        private ActivityButton CreateRuntimeActivityButton(Transform parent, string activityId, int missingIndex)
        {
            var go = new GameObject($"{activityId}Button", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);

            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(360f, 64f);
            rect.anchoredPosition = new Vector2(0f, -130f - missingIndex * 78f);

            Image image = go.GetComponent<Image>();
            image.color = new Color(0.14f, 0.43f, 0.82f, 0.92f);

            var labelObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
            labelObj.transform.SetParent(go.transform, false);
            RectTransform labelRect = labelObj.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            Text text = labelObj.GetComponent<Text>();
            string mascot = activityId switch
            {
                "QuantityMatch" => "\U0001F431",   // Cat 🐱
                "CompareQuantity" => "\U0001F43B",  // Bear 🐻
                "NumberBonds" => "\U0001F43C",      // Panda 🐼
                "NumberLineJump" => "\U0001F438",   // Frog 🐸
                _ => "\U0001F430"                    // Bunny 🐰
            };
            text.text = mascot + " " + GetActivityDisplayName(activityId);
            text.alignment = TextAnchor.MiddleCenter;
            text.font = UIKidFriendlyStyle.GetSharedFont();
            text.fontSize = 24;
            text.color = Color.white;
            text.raycastTarget = false;

            return new ActivityButton
            {
                button = go.GetComponent<Button>(),
                activityId = activityId,
                sceneName = activityId
            };
        }

        private void ArrangeActivityButtons(List<ActivityButton> allActivities)
        {
            // 2x2 grid layout with larger cards
            Vector2[] gridPositions = new Vector2[]
            {
                new Vector2(-230f, 80f),   // Button 0: top-left
                new Vector2(230f, 80f),    // Button 1: top-right
                new Vector2(-230f, -70f),  // Button 2: bottom-left
                new Vector2(230f, -70f),   // Button 3: bottom-right
            };

            int orderedIndex = 0;
            foreach (string activityId in RequiredPlayableActivityIds)
            {
                ActivityButton activity = FindActivityButton(allActivities, activityId);
                RectTransform rect = activity?.button != null
                    ? activity.button.GetComponent<RectTransform>()
                    : null;
                if (rect == null) continue;

                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                
                if (orderedIndex < gridPositions.Length)
                {
                    rect.sizeDelta = new Vector2(380f, 110f);
                    rect.anchoredPosition = gridPositions[orderedIndex];
                }
                
                orderedIndex++;
            }

            MoveBackButtonBelowActivities(orderedIndex);
        }

        private static ActivityButton FindActivityButton(List<ActivityButton> allActivities, string activityId)
        {
            if (allActivities == null)
            {
                return null;
            }

            for (int i = 0; i < allActivities.Count; i++)
            {
                if (allActivities[i] != null && allActivities[i].activityId == activityId)
                {
                    return allActivities[i];
                }
            }

            return null;
        }

        private void MoveBackButtonBelowActivities(int activityCount)
        {
            RectTransform backRect = backButton != null ? backButton.GetComponent<RectTransform>() : null;
            if (backRect != null)
            {
                backRect.anchoredPosition = new Vector2(backRect.anchoredPosition.x, 110f - activityCount * 80f - 20f);
            }
        }

        private bool IsPlayableActivity(string activityId)
        {
            if (playableActivityIds != null)
            {
                foreach (string playableActivityId in playableActivityIds)
                {
                    if (activityId == playableActivityId)
                    {
                        return true;
                    }
                }
            }

            foreach (string requiredId in RequiredPlayableActivityIds)
            {
                if (activityId == requiredId)
                {
                    return true;
                }
            }

            return false;
        }

        private static void RemoveActivityButtonListeners(IEnumerable<ActivityButton> buttons)
        {
            if (buttons == null)
            {
                return;
            }

            foreach (ActivityButton activity in buttons)
            {
                if (activity?.button != null)
                {
                    activity.button.onClick.RemoveAllListeners();
                }
            }
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
