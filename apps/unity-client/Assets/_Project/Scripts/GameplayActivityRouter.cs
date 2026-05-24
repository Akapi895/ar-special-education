using System.Reflection;
using Features.Activities;
using Features.Activities.CompareQuantity;
using Features.Activities.NumberLineJump;
using Features.Activities.QuantityMatch;
using UnityEngine;

namespace Project.App
{
    /// <summary>
    /// Creates the selected activity in SC_ARGameplay when it is not already placed in the scene.
    /// </summary>
    [DefaultExecutionOrder(-140)]
    public class GameplayActivityRouter : MonoBehaviour
    {
        private const string DefaultActivityId = "QuantityMatch";
        private const string QuantityMatchRootName = "QuantityMatchActivity";

        private const string NumberLineJumpConfigPath =
            "Assets/Features/Activities/NumberLineJump/ScriptableObjects/SO_NumberLineJumpConfig_Easy.asset";
        private const string CompareQuantityConfigPath =
            "Assets/Features/Activities/CompareQuantity/ScriptableObjects/SO_CompareQuantityConfig_Easy.asset";

        private void Awake()
        {
            RouteSelectedActivity();
        }

        private void RouteSelectedActivity()
        {
            string activityId = string.IsNullOrEmpty(SelectedActivityData.ActivityId)
                ? DefaultActivityId
                : SelectedActivityData.ActivityId;

            if (activityId == DefaultActivityId)
            {
                SelectedActivityData.Clear();
                return;
            }

            GameObject quantityRoot = GameObject.Find(QuantityMatchRootName);
            if (quantityRoot != null)
            {
                quantityRoot.SetActive(false);
            }

            switch (activityId)
            {
                case "NumberLineJump":
                    CreateNumberLineJumpActivity();
                    break;

                case "CompareQuantity":
                    CreateCompareQuantityActivity();
                    break;

                default:
                    Debug.LogWarning($"[GameplayActivityRouter] Unknown activity '{activityId}', falling back to Quantity Match.");
                    if (quantityRoot != null)
                    {
                        quantityRoot.SetActive(true);
                    }
                    break;
            }

            SelectedActivityData.Clear();
        }

        private static void CreateNumberLineJumpActivity()
        {
            if (FindAnyObjectByType<NumberLineJumpActivityBootstrap>() != null)
            {
                return;
            }

            NumberLineJumpConfig config = LoadAsset<NumberLineJumpConfig>(NumberLineJumpConfigPath);
            if (config == null)
            {
                Debug.LogError("[GameplayActivityRouter] NumberLineJump config asset could not be loaded.");
                return;
            }

            GameObject root = new GameObject("NumberLineJumpActivity");
            root.SetActive(false);

            var presenter = root.AddComponent<NumberLineJumpPresenter>();
            var view = root.AddComponent<NumberLineJumpView>();
            root.AddComponent<ActivityPrefabSetup>();
            root.AddComponent<NumberLineJumpRuntimeUI>();
            var bootstrap = root.AddComponent<NumberLineJumpActivityBootstrap>();

            AssignPrivateField(bootstrap, "presenter", presenter);
            AssignPrivateField(bootstrap, "view", view);
            AssignPrivateField(bootstrap, "config", config);

            root.SetActive(true);
        }

        private static void CreateCompareQuantityActivity()
        {
            if (FindAnyObjectByType<CompareQuantityActivityBootstrap>() != null)
            {
                return;
            }

            CompareQuantityConfig config = LoadAsset<CompareQuantityConfig>(CompareQuantityConfigPath);
            if (config == null)
            {
                Debug.LogError("[GameplayActivityRouter] CompareQuantity config asset could not be loaded.");
                return;
            }

            GameObject root = new GameObject("CompareQuantityActivity");
            root.SetActive(false);

            var presenter = root.AddComponent<CompareQuantityPresenter>();
            var view = root.AddComponent<CompareQuantityView>();
            root.AddComponent<ActivityPrefabSetup>();
            root.AddComponent<CompareQuantityRuntimeUI>();
            var bootstrap = root.AddComponent<CompareQuantityActivityBootstrap>();

            AssignPrivateField(bootstrap, "presenter", presenter);
            AssignPrivateField(bootstrap, "view", view);
            AssignPrivateField(bootstrap, "config", config);

            root.SetActive(true);
        }

        private static void AssignPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null)
            {
                Debug.LogError($"[GameplayActivityRouter] Could not find field '{fieldName}' on {target.GetType().Name}.");
                return;
            }

            field.SetValue(target, value);
        }

        private static T LoadAsset<T>(string assetPath) where T : ScriptableObject
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(assetPath);
#else
            Debug.LogWarning($"[GameplayActivityRouter] Direct asset loading is only available in the Unity Editor: {assetPath}");
            return null;
#endif
        }
    }
}
