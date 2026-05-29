#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Core.AR;
using Core.AR.Sandbox;
using Core.Data.LocalStorage;
using Core.Learning.Models;
using Features.Activities.CompareQuantity;
using Features.Activities.NumberBonds;
using Features.Activities.NumberLineJump;
using Features.Activities.QuantityMatch;
using Project.App;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Project.Editor
{
    /// <summary>
    /// Batch-friendly validation for .agent/LOCAL_UNITY_FULL_TEST_GUIDE.md.
    /// </summary>
    public static class LocalUnityFullTestRunner
    {
        private const string QuantityMatchConfigPath =
            "Assets/Features/Activities/QuantityMatch/ScriptableObjects/SO_QuantityMatchConfig_Easy.asset";
        private const string NumberLineJumpConfigPath =
            "Assets/Features/Activities/NumberLineJump/ScriptableObjects/SO_NumberLineJumpConfig_Easy.asset";
        private const string CompareQuantityConfigPath =
            "Assets/Features/Activities/CompareQuantity/ScriptableObjects/SO_CompareQuantityConfig_Easy.asset";
        private const string NumberBondsConfigPath =
            "Assets/_Project/Resources/ActivityConfigs/SO_NumberBondsConfig_Demo.asset";

        private const string BootScenePath = "Assets/_Project/Scenes/SC_Boot.unity";
        private const string MainMenuScenePath = "Assets/_Project/Scenes/SC_MainMenu.unity";
        private const string ActivitySelectScenePath = "Assets/_Project/Scenes/SC_ActivitySelect.unity";
        private const string GameplayScenePath = "Assets/_Project/Scenes/SC_ARGameplay.unity";
        private const string ProgressDashboardScenePath = "Assets/_Project/Scenes/SC_ProgressDashboard.unity";
        private const string TestSandboxScenePath = "Assets/_Project/Scenes/SC_TestSandbox.unity";

        private static readonly List<string> Failures = new List<string>();
        private static readonly List<string> Notes = new List<string>();

        private static Stage stage = Stage.None;
        private static double stageStartedAt;
        private static bool oldEnterPlayModeOptionsEnabled;
        private static EnterPlayModeOptions oldEnterPlayModeOptions;
        private static string progressJsonPath;
        private static bool assembliesLocked;

        private enum Stage
        {
            None,
            WaitEditorIdle,
            EnterSandbox,
            WaitSandboxReady,
            StopAfterSandbox,
            WaitSandboxStopped,
            EnterGameplay,
            WaitGameplayReady,
            WaitRound2,
            CompleteRound2,
            WaitProgressSaved,
            WaitQuantityCompletionControls,
            WaitCompareReady,
            StopAfterGameplay,
            WaitGameplayStopped,
            EnterBoot,
            WaitMainMenu,
            WaitDashboard,
            WaitMainMenuReturn,
            WaitActivitySelect,
            WaitShellNumberBondsReady,
            StopAfterShell,
            WaitShellStopped,
            Aborting
        }

        [MenuItem("AR Learning/Local Full Test/Run Setup Only")]
        public static void RunSetupOnly()
        {
            Failures.Clear();
            Notes.Clear();
            SetupGuideAssetsAndScenes();
            ValidateStaticSetup();
            PrintResult();
        }

        public static void RunSetupOnlyBatch()
        {
            RunSetupOnly();
            EditorApplication.Exit(Failures.Count == 0 ? 0 : 1);
        }

        public static void RunFullBatch()
        {
            Failures.Clear();
            Notes.Clear();

            try
            {
                SetupGuideAssetsAndScenes();
                ValidateStaticSetup();
            }
            catch (Exception ex)
            {
                AddFailure($"Setup/static validation threw: {ex}");
            }

            if (Failures.Count > 0)
            {
                PrintResult();
                EditorApplication.Exit(1);
                return;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            PreparePlayValidation();
            Advance(Stage.WaitEditorIdle);
        }

        public static void RunPlayValidationBatch()
        {
            Failures.Clear();
            Notes.Clear();

            try
            {
                ValidateStaticSetup();
            }
            catch (Exception ex)
            {
                AddFailure($"Static validation threw: {ex}");
            }

            if (Failures.Count > 0)
            {
                PrintResult();
                EditorApplication.Exit(1);
                return;
            }

            PreparePlayValidation();
            Advance(Stage.WaitEditorIdle);
        }

        private static void SetupGuideAssetsAndScenes()
        {
            QuantityMatchConfigFactory.CreateEasyConfigAsset();
            NumberLineJumpConfigFactory.CreateEasyConfigAsset();
            CompareQuantityConfigFactory.CreateEasyConfigAsset();

            ARTestSandboxMenu.SetupTestSandboxScene();
            ARGameplaySceneMenu.SetupARGameplayScene();

            SceneSetupMenu.SetupBootScene();
            SceneSetupMenu.SetupMainMenuScene();
            SceneSetupMenu.SetupActivitySelectScene();
            SceneSetupMenu.SetupProgressDashboardScene();
            SceneSetupMenu.SetupProductBuildSettings();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Notes.Add("Guide setup menus completed.");
        }

        private static void ValidateStaticSetup()
        {
            ValidateConfig<QuantityMatchConfig>(QuantityMatchConfigPath);
            ValidateConfig<NumberLineJumpConfig>(NumberLineJumpConfigPath);
            ValidateConfig<CompareQuantityConfig>(CompareQuantityConfigPath);
            ValidateConfig<NumberBondsConfig>(NumberBondsConfigPath);

            ValidateFileExists(BootScenePath);
            ValidateFileExists(MainMenuScenePath);
            ValidateFileExists(ActivitySelectScenePath);
            ValidateFileExists(GameplayScenePath);
            ValidateFileExists(ProgressDashboardScenePath);
            ValidateFileExists(TestSandboxScenePath);
            ValidateBuildSettings();
            ValidateProgressSchema();
            ValidateProductionRuntimePath();
            ValidatePerformanceAndDeviceReadiness();
            ValidateLearnerProfileAndAdaptiveReadiness();

            ValidateMainMenuScene();
            ValidateActivitySelectScene();
            ValidateGameplayScene();
            ValidateProgressDashboardScene();
            ValidateSceneComponent<ARSandboxController>(TestSandboxScenePath, "ARSandboxController");

            Notes.Add("Static scene/config validation passed.");
        }

        private static void Tick()
        {
            try
            {
                if (EditorApplication.timeSinceStartup - stageStartedAt > TimeoutForStage(stage))
                {
                    AddFailure($"Timed out in stage {stage}.");
                    Abort();
                    return;
                }

                switch (stage)
                {
                    case Stage.WaitEditorIdle:
                        if (!EditorApplication.isCompiling && !EditorApplication.isUpdating && Elapsed() > 2d)
                        {
                            Advance(Stage.EnterSandbox);
                        }
                        break;

                    case Stage.EnterSandbox:
                        OpenSceneAndPlay(TestSandboxScenePath);
                        Advance(Stage.WaitSandboxReady);
                        break;

                    case Stage.WaitSandboxReady:
                        if (EditorApplication.isPlaying && Elapsed() > 1.0d)
                        {
                            ValidateSandboxPlayMode();
                            Advance(Stage.StopAfterSandbox);
                        }
                        break;

                    case Stage.StopAfterSandbox:
                        StopPlayMode(Stage.WaitSandboxStopped);
                        break;

                    case Stage.WaitSandboxStopped:
                        if (!EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode)
                        {
                            Advance(Stage.EnterGameplay);
                        }
                        break;

                    case Stage.EnterGameplay:
                        OpenSceneAndPlay(GameplayScenePath);
                        Advance(Stage.WaitGameplayReady);
                        break;

                    case Stage.WaitGameplayReady:
                        if (IsGameplayReady())
                        {
                            ValidateGameplayInitialStateAndAnswerRoundOne();
                            Advance(Stage.WaitRound2);
                        }
                        break;

                    case Stage.WaitRound2:
                        if (IsTextValue("Progress", "Question 2 of 2") && IsPresenterInState(ActivityState.InProgress))
                        {
                            Advance(Stage.CompleteRound2);
                        }
                        break;

                    case Stage.CompleteRound2:
                        ClickButton("GroupButton_1");
                        Advance(Stage.WaitProgressSaved);
                        break;

                    case Stage.WaitProgressSaved:
                        if (HasSavedQuantityMatchResults())
                        {
                            Notes.Add($"Progress JSON saved at {progressJsonPath}");
                            Advance(Stage.WaitQuantityCompletionControls);
                        }
                        break;

                    case Stage.WaitQuantityCompletionControls:
                        if (IsActiveButton("NextButton"))
                        {
                            ClickButton("NextButton");
                            Advance(Stage.WaitCompareReady);
                        }
                        break;

                    case Stage.WaitCompareReady:
                        if (IsCompareReady())
                        {
                            ValidateCompareRuntimeUi();
                            Notes.Add("Quantity Match Next routes to Compare Quantity.");
                            Advance(Stage.StopAfterGameplay);
                        }
                        break;

                    case Stage.StopAfterGameplay:
                        StopPlayMode(Stage.WaitGameplayStopped);
                        break;

                    case Stage.WaitGameplayStopped:
                        if (!EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode)
                        {
                            Advance(Stage.EnterBoot);
                        }
                        break;

                    case Stage.EnterBoot:
                        OpenSceneAndPlay(BootScenePath);
                        Advance(Stage.WaitMainMenu);
                        break;

                    case Stage.WaitMainMenu:
                        if (SceneManager.GetActiveScene().name == "SC_MainMenu")
                        {
                            ValidateMainMenuPlayMode();
                            ClickButton("ViewProgressButton");
                            Advance(Stage.WaitDashboard);
                        }
                        break;

                    case Stage.WaitDashboard:
                        if (SceneManager.GetActiveScene().name == "SC_ProgressDashboard")
                        {
                            ValidateDashboardPlayMode();
                            ClickButton("BackButton");
                            Advance(Stage.WaitMainMenuReturn);
                        }
                        break;

                    case Stage.WaitMainMenuReturn:
                        if (SceneManager.GetActiveScene().name == "SC_MainMenu")
                        {
                            ClickButton("StartLearningButton");
                            Advance(Stage.WaitActivitySelect);
                        }
                        break;

                    case Stage.WaitActivitySelect:
                        if (SceneManager.GetActiveScene().name == "SC_ActivitySelect")
                        {
                            ValidateActivitySelectPlayMode();
                            ClickButton("NumberBondsButton");
                            Advance(Stage.WaitShellNumberBondsReady);
                        }
                        break;

                    case Stage.WaitShellNumberBondsReady:
                        if (IsNumberBondsReady())
                        {
                            ValidateNumberBondsRuntimeUi();
                            Notes.Add("Boot -> MainMenu -> Dashboard -> ActivitySelect -> NumberBonds flow passed.");
                            Advance(Stage.StopAfterShell);
                        }
                        break;

                    case Stage.StopAfterShell:
                        StopPlayMode(Stage.WaitShellStopped);
                        break;

                    case Stage.WaitShellStopped:
                        if (!EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode)
                        {
                            Finish();
                        }
                        break;

                    case Stage.Aborting:
                        if (!EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode)
                        {
                            Finish();
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                AddFailure($"Validation threw in stage {stage}: {ex}");
                Abort();
            }
        }

        private static void ValidateSandboxPlayMode()
        {
            ARServiceBootstrap bootstrap = ARServiceBootstrap.Instance;
            Require(bootstrap != null, "SC_TestSandbox has ARServiceBootstrap at runtime.");
            Require(bootstrap?.Placement != null, "SC_TestSandbox has placement service.");
            Require(bootstrap?.Placement?.GetType().Name == "ARPlacementServiceMock", "SC_TestSandbox uses ARPlacementServiceMock in Editor.");
            Require(bootstrap?.Placement?.HasLearningArea == true, "SC_TestSandbox mock creates a learning area.");
            Require(bootstrap?.Placement?.LearningAreaContentRoot != null, "SC_TestSandbox mock exposes a learning area content root.");
            Require(bootstrap?.Interaction != null, "SC_TestSandbox has interaction service.");
            Require(Object.FindFirstObjectByType<ARSandboxController>() != null, "SC_TestSandbox has ARSandboxController.");

            var prefab = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            prefab.name = "BatchSandboxSphere";
            GameObject[] grid = bootstrap.Placement.SpawnGrid(prefab, bootstrap.Placement.CurrentPlacementPosition, 6, 0.25f);
            Require(grid != null && grid.Length == 6, "Sandbox placement spawns grid objects.");

            GameObject[] circle = bootstrap.Placement.SpawnCircle(prefab, bootstrap.Placement.CurrentPlacementPosition, 5, 0.35f);
            Require(circle != null && circle.Length == 5, "Sandbox placement spawns circle objects.");

            bootstrap.Placement.ClearSpawnedObjects();
            Object.Destroy(prefab);
            Notes.Add("Phase 1 sandbox runtime checks passed.");
        }

        private static void ValidateGameplayInitialStateAndAnswerRoundOne()
        {
            ProgressStorageProxy.Instance.ClearAllProgress();

            Require(IsTextValue("TargetNumber", "exactly 3"), "Quantity Match question text clearly asks for 3 balls on round 1.");
            Require(IsTextValue("Progress", "Question 1 of 2"), "Quantity Match progress shows round 1.");
            Require(IsButtonUnderCanvas("HintButton"), "Hint button is under a Canvas.");
            Require(IsButtonUnderCanvas("GroupButton_0"), "Group 1 button is under a Canvas.");
            Require(IsButtonUnderCanvas("GroupButton_1"), "Group 2 button is under a Canvas.");
            Require(IsButtonUnderCanvas("GroupButton_2"), "Group 3 button is under a Canvas.");
            ValidateRuntimeButtonLayout();
            ValidateReadableQuantityGroups();

            ClickButton("HintButton");
            ClickButton("HintButton");
            ClickButton("HintButton");
            Require(!string.IsNullOrWhiteSpace(FindText("HintText")?.text), "Hint text updates after hint requests.");

            ClickButton("GroupButton_0");
            Require(IsPresenterInState(ActivityState.InProgress), "Wrong answer keeps Quantity Match in progress.");
            Require(!string.IsNullOrWhiteSpace(FindText("FeedbackText")?.text), "Wrong answer displays feedback.");

            ClickButton("GroupButton_1");
            Require(IsPresenterInState(ActivityState.Completed), "Correct answer completes round 1.");
            Require(IsActiveButton("NextButton"), "Correct answer shows explicit Next button.");
            ClickButton("NextButton");
            Notes.Add("Phase 2 round 1 hint/wrong/correct checks passed.");
        }

        private static void ValidateRuntimeButtonLayout()
        {
            string[] buttonNames =
            {
                "GroupButton_0",
                "GroupButton_1",
                "GroupButton_2",
                "HintButton",
                "CancelButton"
            };

            for (int i = 0; i < buttonNames.Length; i++)
            {
                for (int j = i + 1; j < buttonNames.Length; j++)
                {
                    Require(!ButtonsOverlap(buttonNames[i], buttonNames[j]),
                        $"{buttonNames[i]} does not overlap {buttonNames[j]}.");
                }
            }
        }

        private static void ValidateReadableQuantityGroups()
        {
            Transform[] groups = Object.FindObjectsByType<Transform>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                .Where(transform => transform.name.StartsWith("QuantityGroup_", StringComparison.Ordinal))
                .ToArray();

            Require(groups.Length == 3, "Quantity Match spawns three visible groups.");

            foreach (Transform group in groups)
            {
                int expectedCount = ParseCountFromGroupName(group.name);
                int actualCount = group.Cast<Transform>().Count(child => child.name.Contains("_Ball"));
                Require(expectedCount > 0 && actualCount == expectedCount,
                    $"{group.name} has a readable ball count.");
                Require(group.GetComponent<BoxCollider>() != null, $"{group.name} has a group hitbox.");
                Require(group.GetComponentInChildren<TextMesh>() != null, $"{group.name} has a world group label.");
                Require(IsGroupInCameraView(group), $"{group.name} is inside the camera view.");
            }
        }

        private static bool HasSavedQuantityMatchResults()
        {
            List<ActivityResult> results = ProgressStorageProxy.Instance.GetResultsForActivity("QuantityMatch");
            bool hasRoundOne = results.Any(result => result.LevelNumber == 1 && result.IsCorrect && result.TotalAttempts >= 2);
            bool hasRoundTwo = results.Any(result => result.LevelNumber == 2 && result.IsCorrect && result.TotalAttempts >= 1);

            progressJsonPath = Path.Combine(Application.persistentDataPath, "learning_progress.json");
            bool fileExists = File.Exists(progressJsonPath);

            return results.Count >= 2 && hasRoundOne && hasRoundTwo && fileExists;
        }

        private static void ValidateMainMenuPlayMode()
        {
            Require(Object.FindFirstObjectByType<MainMenuController>() != null, "Main Menu controller exists at runtime.");
            Require(FindButton("StartLearningButton") != null, "Start Learning button exists.");
            Require(FindButton("ViewProgressButton") != null, "View Progress button exists.");
        }

        private static void ValidateDashboardPlayMode()
        {
            Require(Object.FindFirstObjectByType<ProgressDashboardView>() != null, "Progress Dashboard view exists at runtime.");
            Require(FindText("OverallStatsText")?.text.Contains("Learning Progress") == true, "Dashboard shows learning progress.");
            Require(FindText("ActivityStatsText")?.text.Contains("QuantityMatch") == true, "Dashboard shows QuantityMatch statistics.");
        }

        private static void ValidateActivitySelectPlayMode()
        {
            Require(Object.FindFirstObjectByType<ActivitySelectController>() != null, "Activity Select controller exists at runtime.");
            Require(FindButton("QuantityMatchButton")?.interactable == true, "Quantity Match button is playable.");
            Require(FindButton("NumberLineJumpButton") != null, "Number Line Jump button exists.");
            Require(FindButton("CompareQuantityButton") != null, "Compare Quantity button exists.");
            Require(FindButton("NumberBondsButton") != null, "Number Bonds button exists.");
        }

        private static bool IsGameplayReady()
        {
            if (SceneManager.GetActiveScene().name != "SC_ARGameplay")
            {
                return false;
            }

            var presenter = Object.FindFirstObjectByType<QuantityMatchPresenter>();
            return presenter != null
                && presenter.CurrentState == ActivityState.InProgress
                && FindButton("GroupButton_1") != null;
        }

        private static bool IsCompareReady()
        {
            if (SceneManager.GetActiveScene().name != "SC_ARGameplay")
            {
                return false;
            }

            var presenter = Object.FindFirstObjectByType<CompareQuantityPresenter>();
            return presenter != null
                && presenter.CurrentState == ActivityState.InProgress
                && IsActiveButton("MoreButton")
                && IsActiveButton("FewerButton")
                && IsActiveButton("EqualButton");
        }

        private static void ValidateCompareRuntimeUi()
        {
            Require(FindText("QuestionText")?.text.Length > 0, "Compare Quantity question text is visible.");
            Require(IsActiveButton("MoreButton"), "Compare Quantity more button is active.");
            Require(IsActiveButton("FewerButton"), "Compare Quantity fewer button is active.");
            Require(IsActiveButton("EqualButton"), "Compare Quantity equal button is active.");
            Require(!ButtonsOverlap("MoreButton", "FewerButton"), "Compare Quantity answer buttons do not overlap.");
            Require(!ButtonsOverlap("FewerButton", "EqualButton"), "Compare Quantity answer buttons do not overlap.");
        }

        private static bool IsNumberBondsReady()
        {
            if (SceneManager.GetActiveScene().name != "SC_ARGameplay")
            {
                return false;
            }

            var presenter = Object.FindFirstObjectByType<NumberBondsPresenter>();
            return presenter != null
                && presenter.CurrentState == ActivityState.InProgress
                && IsActiveButton("ConfirmButton")
                && FindText("ExpressionText") != null;
        }

        private static void ValidateNumberBondsRuntimeUi()
        {
            Require(FindText("InstructionText")?.text.Length > 0, "Number Bonds instruction text is visible.");
            Require(FindText("ExpressionText")?.text.Contains("=") == true, "Number Bonds expression text is visible.");
            Require(IsActiveButton("ConfirmButton"), "Number Bonds confirm button is active.");
            Require(IsActiveButton("HintButton"), "Number Bonds hint button is active.");
            Require(IsActiveButton("HomeButton"), "Number Bonds home button is active.");
            Require(IsActiveButton("ListenButton"), "Number Bonds listen button is active.");
            Require(!ButtonsOverlap("ConfirmButton", "HintButton"), "Number Bonds confirm and hint buttons do not overlap.");
        }

        private static bool IsNumberLineReady()
        {
            if (SceneManager.GetActiveScene().name != "SC_ARGameplay")
            {
                return false;
            }

            var presenter = Object.FindFirstObjectByType<NumberLineJumpPresenter>();
            return presenter != null
                && presenter.CurrentState == ActivityState.InProgress
                && IsActiveButton("ConfirmButton")
                && IsActiveButton("ResetButton");
        }

        private static void ValidateNumberLineRuntimeUi()
        {
            Require(FindText("TargetNumber")?.text.Contains("Jump from") == true, "Number Line Jump question text is clear.");
            Require(IsActiveButton("LeftJumpButton"), "Number Line Jump left button is active.");
            Require(IsActiveButton("RightJumpButton"), "Number Line Jump right button is active.");
            Require(IsActiveButton("ConfirmButton"), "Number Line Jump confirm button is active.");
            Require(IsActiveButton("ResetButton"), "Number Line Jump reset button is active.");
            Require(!ButtonsOverlap("LeftJumpButton", "RightJumpButton"), "Number Line Jump movement buttons do not overlap.");
            Require(!ButtonsOverlap("ConfirmButton", "ResetButton"), "Number Line Jump action buttons do not overlap.");
        }

        private static bool IsPresenterInState(ActivityState state)
        {
            var presenter = Object.FindFirstObjectByType<QuantityMatchPresenter>();
            return presenter != null && presenter.CurrentState == state;
        }

        private static bool IsTextValue(string name, string expected)
        {
            Text text = FindText(name);
            return text != null && text.text.Contains(expected);
        }

        private static bool IsButtonUnderCanvas(string name)
        {
            Button button = FindButton(name);
            return button != null && button.GetComponentInParent<Canvas>() != null;
        }

        private static bool ButtonsOverlap(string firstName, string secondName)
        {
            Button first = FindButton(firstName);
            Button second = FindButton(secondName);
            if (first == null || second == null || !first.gameObject.activeInHierarchy || !second.gameObject.activeInHierarchy)
            {
                return false;
            }

            return GetWorldRect(first.GetComponent<RectTransform>())
                .Overlaps(GetWorldRect(second.GetComponent<RectTransform>()));
        }

        private static Rect GetWorldRect(RectTransform rectTransform)
        {
            var corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            float minX = corners.Min(corner => corner.x);
            float maxX = corners.Max(corner => corner.x);
            float minY = corners.Min(corner => corner.y);
            float maxY = corners.Max(corner => corner.y);
            return Rect.MinMaxRect(minX, minY, maxX, maxY);
        }

        private static int ParseCountFromGroupName(string groupName)
        {
            int countIndex = groupName.LastIndexOf("_Count", StringComparison.Ordinal);
            if (countIndex < 0)
            {
                return -1;
            }

            return int.TryParse(groupName.Substring(countIndex + "_Count".Length), out int count)
                ? count
                : -1;
        }

        private static bool IsGroupInCameraView(Transform group)
        {
            Camera camera = Camera.main;
            if (camera == null)
            {
                return false;
            }

            Vector3 viewport = camera.WorldToViewportPoint(group.position);
            return viewport.z > 0f
                && viewport.x >= 0.05f
                && viewport.x <= 0.95f
                && viewport.y >= 0.15f
                && viewport.y <= 0.9f;
        }

        private static void ClickButton(string name)
        {
            Button button = FindButton(name);
            if (button == null)
            {
                AddFailure($"Button '{name}' was not found.");
                return;
            }

            if (!button.interactable)
            {
                AddFailure($"Button '{name}' is not interactable.");
                return;
            }

            button.onClick.Invoke();
        }

        private static Button FindButton(string name)
        {
            return Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .FirstOrDefault(button => button.name == name);
        }

        private static bool IsActiveButton(string name)
        {
            Button button = FindButton(name);
            return button != null && button.gameObject.activeInHierarchy;
        }

        private static Text FindText(string name)
        {
            return Object.FindObjectsByType<Text>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .FirstOrDefault(text => text.name == name);
        }

        private static void ValidateConfig<T>(string path) where T : Core.Learning.ActivityRunner.ActivityConfig
        {
            T config = AssetDatabase.LoadAssetAtPath<T>(path);
            Require(config != null, $"{path} exists.");
            Require(config == null || config.IsValid(), $"{path} is valid.");
        }

        private static void ValidateFileExists(string assetPath)
        {
            Require(File.Exists(Path.Combine(Application.dataPath, assetPath.Substring("Assets/".Length))), $"{assetPath} exists.");
        }

        private static void ValidateBuildSettings()
        {
            string[] expected =
            {
                BootScenePath,
                MainMenuScenePath,
                ActivitySelectScenePath,
                GameplayScenePath,
                ProgressDashboardScenePath,
                TestSandboxScenePath
            };

            string[] actual = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            Require(actual.SequenceEqual(expected), "Build Settings scene order matches the guide.");
        }

        private static void ValidateProgressSchema()
        {
            ActivityResult result = new ActivityResult("QuantityMatch", "schema-test", 1, DifficultyLevel.Easy);
            result.SetLessonContext("L01", new[] { "Counting", "Subitizing" });
            result.Complete(false, ErrorType.WrongQuantity);

            string json = JsonUtility.ToJson(result);
            Require(json.Contains("learningIssue"), "ActivityResult serializes learning issue structure.");
            Require(json.Contains("LessonId"), "ActivityResult serializes lesson id.");
            Require(json.Contains("skillTags"), "ActivityResult serializes skill tags.");

            ActivityResult technicalResult = new ActivityResult("ARPlacement", "schema-test", 0, DifficultyLevel.Easy);
            technicalResult.SetTechnicalIssue(TechnicalIssueType.ARPlaneNotFound, "No plane", "LocalUnityFullTestRunner");
            json = JsonUtility.ToJson(technicalResult);
            Require(json.Contains("technicalIssue"), "ActivityResult serializes technical issue structure.");
            Require(technicalResult.CountsTowardMastery == false, "Technical issue does not count toward mastery.");
            Require(LessonMapRegistry.AllLessons.Count >= 6, "Lesson map contains at least six lessons.");
        }

        private static void ValidateProductionRuntimePath()
        {
            string routerPath = Path.Combine(Application.dataPath, "_Project/Scripts/GameplayActivityRouter.cs");
            string routerSource = File.Exists(routerPath) ? File.ReadAllText(routerPath) : string.Empty;

            Require(!routerSource.Contains("System.Reflection"), "Gameplay router does not use runtime reflection.");
            Require(!routerSource.Contains("AssetDatabase"), "Gameplay router does not use editor-only AssetDatabase.");
            Require(!routerSource.Contains("SetInstanceField"), "Gameplay router uses public configure APIs.");

            string architectureNotesPath = Path.Combine(Application.dataPath, "Docs/ArchitectureNotes.md");
            Require(File.Exists(architectureNotesPath), "Architecture notes document runtime namespaces and asmdef plan.");
        }

        private static void ValidatePerformanceAndDeviceReadiness()
        {
            Require(Core.Support.Performance.RuntimePerformanceSettings.TargetFrameRate == 60,
                "Runtime target frame rate is configured.");
            Require(Core.Support.Performance.RuntimePerformanceSettings.MaxLearningObjectsPerGroup <= 12,
                "Per-group object count is budgeted for mobile AR.");
            Require(Core.Support.Performance.RuntimePerformanceSettings.MaxVisibleLearningObjects <= 48,
                "Visible learning object budget is bounded.");

            string qaPath = Path.GetFullPath(Path.Combine(Application.dataPath, "../../../.agent/device_qa_checklist.md"));
            Require(File.Exists(qaPath), "Device QA checklist exists.");
        }

        private static void ValidateLearnerProfileAndAdaptiveReadiness()
        {
            LearnerProfile profile = LearnerProfileStore.GetActiveOrCreateDefault();
            Require(profile != null && !string.IsNullOrEmpty(profile.LearnerId), "Default learner profile is available.");

            AdaptiveLearningRecommendation recommendation = ProgressStorageProxy.Instance.GetAdaptiveRecommendation();
            Require(recommendation != null && !string.IsNullOrEmpty(recommendation.SuggestedLessonId),
                "Adaptive recommendation API returns a lesson.");

            ActivityResult result = new ActivityResult("QuantityMatch", "learner-schema", 1, DifficultyLevel.Easy)
            {
                LearnerId = profile.LearnerId
            };
            string json = JsonUtility.ToJson(result);
            Require(json.Contains("LearnerId"), "ActivityResult serializes learner id.");
        }

        private static void ValidateSceneComponent<T>(string scenePath, string componentName) where T : Object
        {
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            Require(Object.FindFirstObjectByType<T>() != null, $"{scenePath} has {componentName}.");
        }

        private static void ValidateMainMenuScene()
        {
            EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Single);
            var controller = Object.FindFirstObjectByType<MainMenuController>();
            Require(controller != null, "SC_MainMenu has MainMenuController.");
            RequireSerializedReference(controller, "startLearningButton");
            RequireSerializedReference(controller, "viewProgressButton");
        }

        private static void ValidateActivitySelectScene()
        {
            EditorSceneManager.OpenScene(ActivitySelectScenePath, OpenSceneMode.Single);
            var controller = Object.FindFirstObjectByType<ActivitySelectController>();
            Require(controller != null, "SC_ActivitySelect has ActivitySelectController.");

            var serialized = new SerializedObject(controller);
            SerializedProperty activities = serialized.FindProperty("activities");
            Require(activities != null && activities.arraySize >= 3, "Activity Select has serialized activity entries.");
            RequireSerializedReference(controller, "backButton");

            SerializedProperty playable = serialized.FindProperty("playableActivityIds");
            Require(playable != null && playable.arraySize >= 3,
                "Activity Select enables all implemented activities.");
        }

        private static void ValidateGameplayScene()
        {
            EditorSceneManager.OpenScene(GameplayScenePath, OpenSceneMode.Single);
            Require(Object.FindFirstObjectByType<ARServiceBootstrap>() != null, "SC_ARGameplay has ARServiceBootstrap.");
            var bootstrap = Object.FindFirstObjectByType<QuantityMatchActivityBootstrap>();
            Require(bootstrap != null, "SC_ARGameplay has QuantityMatchActivityBootstrap.");
            RequireSerializedReference(bootstrap, "presenter");
            RequireSerializedReference(bootstrap, "view");
            RequireSerializedReference(bootstrap, "config");
        }

        private static void ValidateProgressDashboardScene()
        {
            EditorSceneManager.OpenScene(ProgressDashboardScenePath, OpenSceneMode.Single);
            var view = Object.FindFirstObjectByType<ProgressDashboardView>();
            Require(view != null, "SC_ProgressDashboard has ProgressDashboardView.");
            RequireSerializedReference(view, "overallStatsText");
            RequireSerializedReference(view, "activityStatsText");
            RequireSerializedReference(view, "backButton");
        }

        private static void RequireSerializedReference(Object target, string fieldName)
        {
            if (target == null)
            {
                return;
            }

            var serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(fieldName);
            Require(property != null && property.objectReferenceValue != null,
                $"{target.name}.{fieldName} is assigned.");
        }

        private static void OpenSceneAndPlay(string scenePath)
        {
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            EditorApplication.isPlaying = true;
        }

        private static void StopPlayMode(Stage nextStage)
        {
            EditorApplication.isPlaying = false;
            Advance(nextStage);
        }

        private static void Advance(Stage nextStage)
        {
            stage = nextStage;
            stageStartedAt = EditorApplication.timeSinceStartup;
        }

        private static double Elapsed()
        {
            return EditorApplication.timeSinceStartup - stageStartedAt;
        }

        private static double TimeoutForStage(Stage currentStage)
        {
            return currentStage switch
            {
                Stage.WaitGameplayReady => 15d,
                Stage.WaitEditorIdle => 30d,
                Stage.WaitRound2 => 10d,
                Stage.WaitProgressSaved => 10d,
                Stage.WaitQuantityCompletionControls => 8d,
                Stage.WaitCompareReady => 12d,
                Stage.WaitMainMenu => 8d,
                Stage.WaitDashboard => 8d,
                Stage.WaitMainMenuReturn => 8d,
                Stage.WaitActivitySelect => 8d,
                Stage.WaitShellNumberBondsReady => 15d,
                _ => 5d
            };
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
            {
                AddFailure(message);
            }
        }

        private static void AddFailure(string message)
        {
            if (!Failures.Contains(message))
            {
                Failures.Add(message);
                Debug.LogWarning($"[LocalUnityFullTestRunner] FAIL: {message}");
            }
        }

        private static void HandleLogMessage(string condition, string stackTrace, LogType type)
        {
            if (type != LogType.Assert && type != LogType.Error && type != LogType.Exception)
            {
                return;
            }

            if (condition.Contains("[LocalUnityFullTestRunner]"))
            {
                return;
            }

            AddFailure($"Unexpected {type}: {condition.Split('\n')[0]}");
        }

        private static void Abort()
        {
            if (EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorApplication.isPlaying = false;
                Advance(Stage.Aborting);
                return;
            }

            Finish();
        }

        private static void Finish()
        {
            Application.logMessageReceived -= HandleLogMessage;
            EditorApplication.update -= Tick;

            if (assembliesLocked)
            {
                EditorApplication.UnlockReloadAssemblies();
                assembliesLocked = false;
            }

            EditorSettings.enterPlayModeOptionsEnabled = oldEnterPlayModeOptionsEnabled;
            EditorSettings.enterPlayModeOptions = oldEnterPlayModeOptions;

            PrintResult();
            EditorApplication.Exit(Failures.Count == 0 ? 0 : 1);
        }

        private static void PreparePlayValidation()
        {
            oldEnterPlayModeOptionsEnabled = EditorSettings.enterPlayModeOptionsEnabled;
            oldEnterPlayModeOptions = EditorSettings.enterPlayModeOptions;
            EditorSettings.enterPlayModeOptionsEnabled = true;
            EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;

            if (!assembliesLocked)
            {
                EditorApplication.LockReloadAssemblies();
                assembliesLocked = true;
            }

            Application.logMessageReceived += HandleLogMessage;
            EditorApplication.update += Tick;
        }

        private static void PrintResult()
        {
            if (Failures.Count == 0)
            {
                Debug.Log("[LocalUnityFullTestRunner] PASS");
                foreach (string note in Notes)
                {
                    Debug.Log($"[LocalUnityFullTestRunner] {note}");
                }

                return;
            }

            Debug.LogError($"[LocalUnityFullTestRunner] FAIL ({Failures.Count})");
            foreach (string failure in Failures)
            {
                Debug.LogError($"[LocalUnityFullTestRunner] {failure}");
            }
        }
    }
}
#endif
