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

        private const string NumberLineJumpResourcePath = "ActivityConfigs/SO_NumberLineJumpConfig_Easy";
        private const string CompareQuantityResourcePath = "ActivityConfigs/SO_CompareQuantityConfig_Easy";

        private void Start()
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
                StartExistingQuantityMatchActivity();
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
                    StartExistingQuantityMatchActivity();
                    break;
            }

            SelectedActivityData.Clear();
        }

        private static void CreateNumberLineJumpActivity()
        {
            NumberLineJumpActivityBootstrap existingBootstrap = FindAnyObjectByType<NumberLineJumpActivityBootstrap>();
            if (existingBootstrap != null)
            {
                existingBootstrap.SetAutoStartWhenReady(false);
                existingBootstrap.TryStartActivity();
                return;
            }

            NumberLineJumpConfig config = LoadAsset<NumberLineJumpConfig>(NumberLineJumpResourcePath)
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

            bootstrap.Configure(presenter, view, config);

            root.SetActive(true);
            bootstrap.TryStartActivity();
        }

        private static void CreateCompareQuantityActivity()
        {
            CompareQuantityActivityBootstrap existingBootstrap = FindAnyObjectByType<CompareQuantityActivityBootstrap>();
            if (existingBootstrap != null)
            {
                existingBootstrap.SetAutoStartWhenReady(false);
                existingBootstrap.TryStartActivity();
                return;
            }

            CompareQuantityConfig config = LoadAsset<CompareQuantityConfig>(CompareQuantityResourcePath)
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

            bootstrap.Configure(presenter, view, config);

            root.SetActive(true);
            bootstrap.TryStartActivity();
        }

        private static void StartExistingQuantityMatchActivity()
        {
            QuantityMatchActivityBootstrap bootstrap = FindAnyObjectByType<QuantityMatchActivityBootstrap>();
            if (bootstrap == null)
            {
                Debug.LogWarning("[GameplayActivityRouter] Quantity Match bootstrap not found in scene.");
                return;
            }

            bootstrap.SetAutoStartWhenReady(false);
            bootstrap.TryStartActivity();
        }

        private static T LoadAsset<T>(string resourcePath) where T : ScriptableObject
        {
            T resourceAsset = Resources.Load<T>(resourcePath);
            if (resourceAsset != null)
            {
                return resourceAsset;
            }

            Debug.LogWarning($"[GameplayActivityRouter] Resource config not found at Resources/{resourcePath}; using runtime fallback.");
            return null;
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

            config.ConfigureRuntime(
                "NumberLineJump",
                "Number Line Jump (Easy)",
                "Jump along the number line to reach the target number.",
                questions,
                new ActivityHint("nlj_hint1", "Move the character and count your steps.", 1),
                new ActivityHint("nlj_hint2", "You started at X. You need to reach Y. How many steps is that?", 2),
                new ActivityHint("nlj_hint3", "Try jumping [direction] [N] times from where you are.", 3),
                0.32f,
                0.1f,
                0.5f);
            return config;
        }

        private static NumberLineJumpQuestion CreateNumberLineQuestion(int start, int target, JumpDirection direction)
        {
            var question = new NumberLineJumpQuestion();
            question.Configure(0, 10, start, target, direction);
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

            config.ConfigureRuntime(
                "CompareQuantity",
                "Compare Quantity (Easy)",
                "Compare two groups of objects to find which has more, fewer, or if they are equal.",
                questions,
                "B\u00ean tr\u00e1i nhi\u1ec1u h\u01a1n",
                "B\u00ean tr\u00e1i \u00edt h\u01a1n",
                "B\u1eb1ng nhau",
                new ActivityHint("cq_hint1", "Count each group carefully.", 1),
                new ActivityHint("cq_hint2", "The left group has X. Now count the right group.", 2),
                new ActivityHint("cq_hint3", "Compare X and Y - which is bigger?", 3),
                new ActivityHint("cq_eq_hint1", "Count both groups - are they the same?", 1),
                new ActivityHint("cq_eq_hint2", "The left group has X. The right group also has X.", 2),
                new ActivityHint("cq_eq_hint3", "X and X are the same number - they're EQUAL!", 3),
                3.0f);
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
