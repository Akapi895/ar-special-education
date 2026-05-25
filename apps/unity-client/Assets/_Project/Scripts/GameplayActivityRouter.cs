using System.Reflection;
using System.Collections.Generic;
using Core.Learning.Models;
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
        private const string NumberLineJumpResourcePath = "ActivityConfigs/SO_NumberLineJumpConfig_Easy";
        private const string CompareQuantityResourcePath = "ActivityConfigs/SO_CompareQuantityConfig_Easy";

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

            NumberLineJumpConfig config = LoadAsset<NumberLineJumpConfig>(NumberLineJumpResourcePath, NumberLineJumpConfigPath)
                ?? CreateRuntimeNumberLineJumpConfig();
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

            SetInstanceField(bootstrap, "presenter", presenter);
            SetInstanceField(bootstrap, "view", view);
            SetInstanceField(bootstrap, "config", config);

            root.SetActive(true);
        }

        private static void CreateCompareQuantityActivity()
        {
            if (FindAnyObjectByType<CompareQuantityActivityBootstrap>() != null)
            {
                return;
            }

            CompareQuantityConfig config = LoadAsset<CompareQuantityConfig>(CompareQuantityResourcePath, CompareQuantityConfigPath)
                ?? CreateRuntimeCompareQuantityConfig();
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

            SetInstanceField(bootstrap, "presenter", presenter);
            SetInstanceField(bootstrap, "view", view);
            SetInstanceField(bootstrap, "config", config);

            root.SetActive(true);
        }

        private static void SetInstanceField(object target, string fieldName, object value)
        {
            FieldInfo field = FindInstanceField(target.GetType(), fieldName);
            if (field == null)
            {
                Debug.LogError($"[GameplayActivityRouter] Could not find field '{fieldName}' on {target.GetType().Name}.");
                return;
            }

            field.SetValue(target, value);
        }

        private static FieldInfo FindInstanceField(System.Type type, string fieldName)
        {
            while (type != null)
            {
                FieldInfo field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (field != null)
                {
                    return field;
                }

                type = type.BaseType;
            }

            return null;
        }

        private static T LoadAsset<T>(string resourcePath, string assetPath) where T : ScriptableObject
        {
            T resourceAsset = Resources.Load<T>(resourcePath);
            if (resourceAsset != null)
            {
                return resourceAsset;
            }

#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(assetPath);
#else
            Debug.LogWarning($"[GameplayActivityRouter] Direct asset loading is only available in the Unity Editor: {assetPath}");
            return null;
#endif
        }

        private static NumberLineJumpConfig CreateRuntimeNumberLineJumpConfig()
        {
            var config = ScriptableObject.CreateInstance<NumberLineJumpConfig>();
            var questions = new List<NumberLineJumpQuestion>
            {
                CreateNumberLineQuestion(3, 5, JumpDirection.RightOnly),
                CreateNumberLineQuestion(7, 4, JumpDirection.LeftOnly),
                CreateNumberLineQuestion(2, 8, JumpDirection.RightOnly),
                CreateNumberLineQuestion(9, 6, JumpDirection.LeftOnly),
                CreateNumberLineQuestion(1, 4, JumpDirection.RightOnly),
                CreateNumberLineQuestion(2, 6, JumpDirection.RightOnly),
                CreateNumberLineQuestion(8, 3, JumpDirection.LeftOnly),
                CreateNumberLineQuestion(1, 9, JumpDirection.RightOnly),
                CreateNumberLineQuestion(10, 4, JumpDirection.LeftOnly),
                CreateNumberLineQuestion(0, 7, JumpDirection.RightOnly)
            };

            SetInstanceField(config, "activityId", "NumberLineJump");
            SetInstanceField(config, "displayName", "Number Line Jump (Easy)");
            SetInstanceField(config, "description", "Jump along the number line to reach the target number.");
            SetInstanceField(config, "numberOfRounds", questions.Count);
            SetInstanceField(config, "maxAttemptsPerQuestion", 3);
            SetInstanceField(config, "maxHintsPerQuestion", 3);
            SetInstanceField(config, "questions", questions);
            SetInstanceField(config, "hintLevel1", new ActivityHint("nlj_hint1", "Move the character and count your steps.", 1));
            SetInstanceField(config, "hintLevel2", new ActivityHint("nlj_hint2", "You started at X. You need to reach Y. How many steps is that?", 2));
            SetInstanceField(config, "hintLevel3", new ActivityHint("nlj_hint3", "Try jumping [direction] [N] times from where you are.", 3));
            SetInstanceField(config, "tileSpacing", 0.32f);
            SetInstanceField(config, "numberLineHeight", 0.1f);
            SetInstanceField(config, "jumpAnimationDuration", 0.5f);
            return config;
        }

        private static NumberLineJumpQuestion CreateNumberLineQuestion(int start, int target, JumpDirection direction)
        {
            var question = new NumberLineJumpQuestion();
            SetInstanceField(question, "numberLineMin", 0);
            SetInstanceField(question, "numberLineMax", 10);
            SetInstanceField(question, "startNumber", start);
            SetInstanceField(question, "targetNumber", target);
            SetInstanceField(question, "jumpDirection", direction);
            SetInstanceField(question, "maxJumpsAllowed", 10);
            SetInstanceField(question, "showEquationDuringJumps", true);
            SetInstanceField(question, "customHints", new List<ActivityHint>());
            SetInstanceField(question, "characterPrefabName", string.Empty);
            SetInstanceField(question, "tilePrefabName", string.Empty);
            return question;
        }

        private static CompareQuantityConfig CreateRuntimeCompareQuantityConfig()
        {
            var config = ScriptableObject.CreateInstance<CompareQuantityConfig>();
            var questions = new List<CompareQuantityQuestion>
            {
                CreateCompareQuestion(3, 5),
                CreateCompareQuestion(6, 4),
                CreateCompareQuestion(4, 4),
                CreateCompareQuestion(2, 7),
                CreateCompareQuestion(8, 3),
                CreateCompareQuestion(5, 5),
                CreateCompareQuestion(9, 6),
                CreateCompareQuestion(4, 8),
                CreateCompareQuestion(7, 7),
                CreateCompareQuestion(10, 6)
            };

            SetInstanceField(config, "activityId", "CompareQuantity");
            SetInstanceField(config, "displayName", "Compare Quantity (Easy)");
            SetInstanceField(config, "description", "Compare two groups of objects to find which has more, fewer, or if they are equal.");
            SetInstanceField(config, "numberOfRounds", questions.Count);
            SetInstanceField(config, "maxAttemptsPerQuestion", 3);
            SetInstanceField(config, "maxHintsPerQuestion", 3);
            SetInstanceField(config, "questions", questions);
            SetInstanceField(config, "moreButtonLabel", "More");
            SetInstanceField(config, "fewerButtonLabel", "Fewer");
            SetInstanceField(config, "equalButtonLabel", "Equal");
            SetInstanceField(config, "hintLevel1", new ActivityHint("cq_hint1", "Count each group carefully.", 1));
            SetInstanceField(config, "hintLevel2", new ActivityHint("cq_hint2", "The left group has X. Now count the right group.", 2));
            SetInstanceField(config, "hintLevel3", new ActivityHint("cq_hint3", "Compare X and Y - which is bigger?", 3));
            SetInstanceField(config, "equalityHintLevel1", new ActivityHint("cq_eq_hint1", "Count both groups - are they the same?", 1));
            SetInstanceField(config, "equalityHintLevel2", new ActivityHint("cq_eq_hint2", "The left group has X. The right group also has X.", 2));
            SetInstanceField(config, "equalityHintLevel3", new ActivityHint("cq_eq_hint3", "X and X are the same number - they're EQUAL!", 3));
            SetInstanceField(config, "groupSpacing", 1.5f);
            return config;
        }

        private static CompareQuantityQuestion CreateCompareQuestion(int left, int right)
        {
            var question = new CompareQuantityQuestion
            {
                LeftGroupCount = left,
                RightGroupCount = right,
                CorrectAnswer = left == right
                    ? ComparisonAnswer.Equal
                    : left > right ? ComparisonAnswer.More : ComparisonAnswer.Fewer,
                CustomHints = new List<ActivityHint>(),
                ObjectPrefabName = string.Empty
            };

            return question;
        }
    }
}
