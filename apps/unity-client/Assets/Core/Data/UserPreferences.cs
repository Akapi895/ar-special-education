using System.Collections.Generic;
using UnityEngine;

namespace Core.Data
{
    public static class UserPreferences
    {
        private const string VolumeKey = "UserPrefs.Volume";
        private const string FontScaleKey = "UserPrefs.FontScale";
        private const string AnimationsEnabledKey = "UserPrefs.AnimationsEnabled";
        private const string AudioEnabledKey = "UserPrefs.AudioEnabled";
        private const string HighContrastModeKey = "UserPrefs.HighContrastMode";
        private const string SimplifiedModeKey = "UserPrefs.SimplifiedMode";
        private const string EnforceLessonPrerequisitesKey = "UserPrefs.EnforceLessonPrerequisites";

        public static float Volume
        {
            get => PlayerPrefs.GetFloat(VolumeKey, 1f);
            set
            {
                PlayerPrefs.SetFloat(VolumeKey, value);
                PlayerPrefs.Save();
                
                // Dynamically update AudioManager if present
                var audioManager = GameObject.Find("SimpleAudioManager");
                if (audioManager != null)
                {
                    audioManager.SendMessage("SetVolume", value, SendMessageOptions.DontRequireReceiver);
                }
            }
        }

        public static float FontScale
        {
            get => PlayerPrefs.GetFloat(FontScaleKey, 1f); // 1.0f is medium, 0.8f small, 1.3f large
            set
            {
                PlayerPrefs.SetFloat(FontScaleKey, value);
                PlayerPrefs.Save();
            }
        }

        public static bool AnimationsEnabled
        {
            get => PlayerPrefs.GetInt(AnimationsEnabledKey, 1) == 1;
            set
            {
                PlayerPrefs.SetInt(AnimationsEnabledKey, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        public static bool AudioEnabled
        {
            get => PlayerPrefs.GetInt(AudioEnabledKey, 1) == 1;
            set
            {
                PlayerPrefs.SetInt(AudioEnabledKey, value ? 1 : 0);
                PlayerPrefs.Save();

                var audioManager = GameObject.Find("SimpleAudioManager");
                if (audioManager != null)
                {
                    audioManager.SendMessage("SetAudioEnabled", value, SendMessageOptions.DontRequireReceiver);
                }
            }
        }

        public static bool HighContrastMode
        {
            get => PlayerPrefs.GetInt(HighContrastModeKey, 0) == 1;
            set
            {
                PlayerPrefs.SetInt(HighContrastModeKey, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        public static bool SimplifiedMode
        {
            get => PlayerPrefs.GetInt(SimplifiedModeKey, 0) == 1;
            set
            {
                PlayerPrefs.SetInt(SimplifiedModeKey, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        public static bool EnforceLessonPrerequisites
        {
            get => PlayerPrefs.GetInt(
                EnforceLessonPrerequisitesKey,
                GetDefaultLessonPrerequisiteLockValue() ? 1 : 0) == 1;
            set
            {
                PlayerPrefs.SetInt(EnforceLessonPrerequisitesKey, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        public static void ResetLessonPrerequisitePreference()
        {
            PlayerPrefs.DeleteKey(EnforceLessonPrerequisitesKey);
            PlayerPrefs.Save();
        }

        private static bool GetDefaultLessonPrerequisiteLockValue()
        {
            return false;
        }
    }
}

namespace Core.Support.Performance
{
    public static class RuntimePerformanceSettings
    {
        public const int TargetFrameRate = 60;
        public const int MaxLearningObjectsPerGroup = 12;
        public const int MaxVisibleLearningObjects = 48;
        public const int DefaultPoolWarmCount = 8;

        private const string ReducedMotionKey = "UserPrefs.AnimationsEnabled";

        public static void Apply()
        {
            Application.targetFrameRate = TargetFrameRate;
            QualitySettings.vSyncCount = 0;

            if (QualitySettings.names != null && QualitySettings.names.Length > 0)
            {
                int qualityIndex = Mathf.Clamp(QualitySettings.names.Length / 2, 0, QualitySettings.names.Length - 1);
                QualitySettings.SetQualityLevel(qualityIndex, true);
            }
        }

        public static bool AnimationsEnabled => PlayerPrefs.GetInt(ReducedMotionKey, 1) == 1;

        public static int ClampGroupObjectCount(int requestedCount)
        {
            return Mathf.Clamp(requestedCount, 0, MaxLearningObjectsPerGroup);
        }
    }

    public class RuntimeObjectPool
    {
        private readonly GameObject prefab;
        private readonly Transform root;
        private readonly Queue<GameObject> inactiveObjects = new Queue<GameObject>();

        public RuntimeObjectPool(GameObject prefab, Transform root, int warmCount = RuntimePerformanceSettings.DefaultPoolWarmCount)
        {
            this.prefab = prefab;
            this.root = root;
            Warm(warmCount);
        }

        public void Warm(int count)
        {
            if (prefab == null || count <= 0)
            {
                return;
            }

            for (int i = 0; i < count; i++)
            {
                GameObject instance = Object.Instantiate(prefab, root);
                instance.SetActive(false);
                inactiveObjects.Enqueue(instance);
            }
        }

        public GameObject Get(Vector3 position, Quaternion rotation, Transform parent = null)
        {
            GameObject instance = inactiveObjects.Count > 0
                ? inactiveObjects.Dequeue()
                : Object.Instantiate(prefab);

            Transform targetParent = parent != null ? parent : root;
            if (targetParent != null)
            {
                instance.transform.SetParent(targetParent, false);
            }

            instance.transform.SetPositionAndRotation(position, rotation);
            instance.SetActive(true);
            return instance;
        }

        public void Release(GameObject instance)
        {
            if (instance == null)
            {
                return;
            }

            instance.SetActive(false);
            if (root != null)
            {
                instance.transform.SetParent(root, false);
            }

            inactiveObjects.Enqueue(instance);
        }

        public void Clear()
        {
            while (inactiveObjects.Count > 0)
            {
                GameObject instance = inactiveObjects.Dequeue();
                if (instance != null)
                {
                    Object.Destroy(instance);
                }
            }
        }
    }
}
