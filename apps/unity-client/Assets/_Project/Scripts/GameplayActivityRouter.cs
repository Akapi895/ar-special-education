using System.Collections.Generic;
using Core.Learning.Models;
using Features.Activities;
using Features.Activities.CompareQuantity;
using Features.Activities.NumberBonds;
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
        private const string NumberBondsResourcePath = "ActivityConfigs/SO_NumberBondsConfig_Demo";

        public static GameplayActivityRouter Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            DisableActivityAutoStart();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Start()
        {
            RouteSelectedActivity();
        }

        private void RouteSelectedActivity()
        {
            string activityId = string.IsNullOrEmpty(SelectedActivityData.ActivityId)
                ? DefaultActivityId
                : SelectedActivityData.ActivityId;
            string configPath = SelectedActivityData.ConfigPath;

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
                    CreateNumberLineJumpActivity(configPath);
                    break;

                case "CompareQuantity":
                    CreateCompareQuantityActivity(configPath);
                    break;

                case "NumberBonds":
                    CreateNumberBondsActivity(configPath);
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

        private static void DisableActivityAutoStart()
        {
            QuantityMatchActivityBootstrap quantityBootstrap = FindAnyObjectByType<QuantityMatchActivityBootstrap>();
            if (quantityBootstrap != null)
            {
                quantityBootstrap.SetAutoStartWhenReady(false);
            }

            NumberLineJumpActivityBootstrap numberLineBootstrap = FindAnyObjectByType<NumberLineJumpActivityBootstrap>();
            if (numberLineBootstrap != null)
            {
                numberLineBootstrap.SetAutoStartWhenReady(false);
            }

            CompareQuantityActivityBootstrap compareBootstrap = FindAnyObjectByType<CompareQuantityActivityBootstrap>();
            if (compareBootstrap != null)
            {
                compareBootstrap.SetAutoStartWhenReady(false);
            }

            NumberBondsActivityBootstrap numberBondsBootstrap = FindAnyObjectByType<NumberBondsActivityBootstrap>();
            if (numberBondsBootstrap != null)
            {
                numberBondsBootstrap.SetAutoStartWhenReady(false);
            }
        }

        private static void CreateNumberLineJumpActivity(string configPath)
        {
            NumberLineJumpActivityBootstrap existingBootstrap = FindAnyObjectByType<NumberLineJumpActivityBootstrap>();
            if (existingBootstrap != null)
            {
                existingBootstrap.SetAutoStartWhenReady(false);
                existingBootstrap.TryStartActivity();
                return;
            }

            NumberLineJumpConfig config = LoadActivityConfig<NumberLineJumpConfig>(configPath, NumberLineJumpResourcePath)
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

        private static void CreateCompareQuantityActivity(string configPath)
        {
            CompareQuantityActivityBootstrap existingBootstrap = FindAnyObjectByType<CompareQuantityActivityBootstrap>();
            if (existingBootstrap != null)
            {
                existingBootstrap.SetAutoStartWhenReady(false);
                existingBootstrap.TryStartActivity();
                return;
            }

            CompareQuantityConfig config = LoadActivityConfig<CompareQuantityConfig>(configPath, CompareQuantityResourcePath)
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

        private static void CreateNumberBondsActivity(string configPath)
        {
            NumberBondsActivityBootstrap existingBootstrap = FindAnyObjectByType<NumberBondsActivityBootstrap>();
            if (existingBootstrap != null)
            {
                existingBootstrap.SetAutoStartWhenReady(false);
                existingBootstrap.TryStartActivity();
                return;
            }

            NumberBondsConfig config = LoadActivityConfig<NumberBondsConfig>(configPath, NumberBondsResourcePath)
                ?? CreateRuntimeNumberBondsConfig();
            if (config == null)
            {
                Debug.LogError("[GameplayActivityRouter] NumberBonds config asset could not be loaded.");
                return;
            }

            GameObject root = new GameObject("NumberBondsActivity");
            root.SetActive(false);

            var presenter = root.AddComponent<NumberBondsPresenter>();
            var view = root.AddComponent<NumberBondsView>();
            root.AddComponent<ActivityPrefabSetup>();
            root.AddComponent<NumberBondsRuntimeUI>();
            var bootstrap = root.AddComponent<NumberBondsActivityBootstrap>();

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

        private static T LoadActivityConfig<T>(string selectedResourcePath, string defaultResourcePath)
            where T : ScriptableObject
        {
            if (!string.IsNullOrWhiteSpace(selectedResourcePath))
            {
                T selectedConfig = LoadAsset<T>(NormalizeResourcesPath(selectedResourcePath));
                if (selectedConfig != null)
                {
                    return selectedConfig;
                }

                Debug.LogWarning($"[GameplayActivityRouter] Selected config '{selectedResourcePath}' was not found or has the wrong type.");
            }

            return LoadAsset<T>(defaultResourcePath);
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

        private static string NormalizeResourcesPath(string resourcePath)
        {
            const string resourcesSegment = "/Resources/";
            string normalizedPath = resourcePath.Replace('\\', '/');
            int resourcesIndex = normalizedPath.IndexOf(resourcesSegment, System.StringComparison.OrdinalIgnoreCase);
            if (resourcesIndex >= 0)
            {
                normalizedPath = normalizedPath.Substring(resourcesIndex + resourcesSegment.Length);
            }

            if (normalizedPath.EndsWith(".asset", System.StringComparison.OrdinalIgnoreCase))
            {
                normalizedPath = normalizedPath.Substring(0, normalizedPath.Length - ".asset".Length);
            }

            return normalizedPath;
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
                "Nhảy trên trục số (Dễ)",
                "Con nh\u1ea3y tr\u00ean tr\u1ee5c s\u1ed1 \u0111\u1ec3 \u0111\u1ebfn \u0111\u00fang s\u1ed1 c\u1ea7n t\u00ecm.",
                questions,
                new ActivityHint("nlj_hint1", "Con h\u00e3y di chuy\u1ec3n nh\u00e2n v\u1eadt v\u00e0 \u0111\u1ebfm t\u1eebng b\u01b0\u1edbc nh\u1ea3y.", 1),
                new ActivityHint("nlj_hint2", "Con b\u1eaft \u0111\u1ea7u \u1edf X v\u00e0 c\u1ea7n \u0111\u1ebfn Y. V\u1eady c\u1ea7n nh\u1ea3y m\u1ea5y b\u01b0\u1edbc?", 2),
                new ActivityHint("nlj_hint3", "H\u00e3y nh\u1ea3y [direction] [N] b\u01b0\u1edbc t\u1eeb v\u1ecb tr\u00ed hi\u1ec7n t\u1ea1i.", 3),
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
                "So sánh số lượng (Dễ)",
                "Con so s\u00e1nh hai nh\u00f3m con v\u1eadt xem b\u00ean tr\u00e1i nhi\u1ec1u h\u01a1n, \u00edt h\u01a1n hay b\u1eb1ng b\u00ean ph\u1ea3i.",
                questions,
                "B\u00ean tr\u00e1i nhi\u1ec1u h\u01a1n",
                "B\u00ean tr\u00e1i \u00edt h\u01a1n",
                "B\u1eb1ng nhau",
                new ActivityHint("cq_hint1", "Con h\u00e3y \u0111\u1ebfm t\u1eebng nh\u00f3m th\u1eadt ch\u1eadm nh\u00e9.", 1),
                new ActivityHint("cq_hint2", "Nh\u00f3m b\u00ean tr\u00e1i c\u00f3 X con. B\u00e2y gi\u1edd con \u0111\u1ebfm nh\u00f3m b\u00ean ph\u1ea3i.", 2),
                new ActivityHint("cq_hint3", "So s\u00e1nh X v\u00e0 Y: s\u1ed1 n\u00e0o l\u1edbn h\u01a1n?", 3),
                new ActivityHint("cq_eq_hint1", "Con \u0111\u1ebfm c\u1ea3 hai nh\u00f3m: hai nh\u00f3m c\u00f3 b\u1eb1ng nhau kh\u00f4ng?", 1),
                new ActivityHint("cq_eq_hint2", "Nh\u00f3m b\u00ean tr\u00e1i c\u00f3 X con. Nh\u00f3m b\u00ean ph\u1ea3i c\u0169ng c\u00f3 X con.", 2),
                new ActivityHint("cq_eq_hint3", "X v\u00e0 X l\u00e0 c\u00f9ng m\u1ed9t s\u1ed1, n\u00ean hai nh\u00f3m b\u1eb1ng nhau.", 3),
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

        private static NumberBondsConfig CreateRuntimeNumberBondsConfig()
        {
            var config = ScriptableObject.CreateInstance<NumberBondsConfig>();
            var questions = new List<NumberBondsQuestion>
            {
                CreateNumberBondsQuestion(NumberBondMode.FreeSplit, 5),
                CreateNumberBondsQuestion(NumberBondMode.TargetSplit, 6, knownPartA: 2),
                CreateNumberBondsQuestion(NumberBondMode.FreeSplit, 7)
            };

            config.ConfigureRuntime(
                "NumberBonds",
                "T\u00e1ch-g\u1ed9p s\u1ed1",
                "Con k\u00e9o con v\u1eadt \u0111\u1ec3 t\u00e1ch m\u1ed9t t\u1ed5ng th\u00e0nh hai ph\u1ea7n.",
                questions,
                new ActivityHint("nb_hint1", "Con h\u00e3y k\u00e9o t\u1eebng con v\u1eadt t\u1eeb T\u1ed5ng xu\u1ed1ng hai Ph\u1ea7n.", 1),
                new ActivityHint("nb_hint2", "Con c\u00f2n C con trong T\u1ed5ng. H\u00e3y chuy\u1ec3n h\u1ebft xu\u1ed1ng hai Ph\u1ea7n.", 2),
                new ActivityHint("nb_hint3", "Khi T\u1ed5ng c\u00f2n 0, hai ph\u1ea7n c\u1ed9ng l\u1ea1i ph\u1ea3i b\u1eb1ng X.", 3),
                8,
                0.52f,
                0.66f,
                0.22f);
            return config;
        }

        private static NumberBondsQuestion CreateNumberBondsQuestion(
            NumberBondMode mode,
            int wholeTarget,
            int knownPartA = -1,
            int knownPartB = -1)
        {
            var question = new NumberBondsQuestion();
            question.Configure(mode, wholeTarget, knownPartA, knownPartB);
            return question;
        }
    }
}
