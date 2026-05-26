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
            WaitNumberLineReady,
            StopAfterGameplay,
            WaitGameplayStopped,
            EnterBoot,
            WaitMainMenu,
            WaitDashboard,
            WaitMainMenuReturn,
            WaitActivitySelect,
            WaitShellGameplay,
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

            ValidateFileExists(MainMenuScenePath);
            ValidateFileExists(ActivitySelectScenePath);
            ValidateFileExists(GameplayScenePath);
            ValidateFileExists(ProgressDashboardScenePath);
            ValidateFileExists(TestSandboxScenePath);
            ValidateBuildSettings();

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
                        if (IsActiveButton("NextButton") && IsActiveButton("ProgressButton"))
                        {
                            Require(!ButtonsOverlap("NextButton", "ProgressButton"), "Next and Progress buttons do not overlap.");
                            ClickButton("NextButton");
                            Advance(Stage.WaitNumberLineReady);
                        }
                        break;

                    case Stage.WaitNumberLineReady:
                        if (IsNumberLineReady())
                        {
                            ValidateNumberLineRuntimeUi();
                            Notes.Add("Quantity Match Next routes to Number Line Jump.");
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
                        OpenSceneAndPlay(MainMenuScenePath);
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
                            ClickButton("QuantityMatchButton");
                            Advance(Stage.WaitShellGameplay);
                        }
                        break;

                    case Stage.WaitShellGameplay:
                        if (IsGameplayReady())
                        {
                            Notes.Add("Boot -> MainMenu -> Dashboard -> ActivitySelect -> QuantityMatch flow passed.");
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
            Require(FindText("OverallStatsText")?.text.Contains("Overall Progress") == true, "Dashboard shows overall progress.");
            Require(FindText("ActivityStatsText")?.text.Contains("QuantityMatch") == true, "Dashboard shows QuantityMatch statistics.");
        }

        private static void ValidateActivitySelectPlayMode()
        {
            Require(Object.FindFirstObjectByType<ActivitySelectController>() != null, "Activity Select controller exists at runtime.");
            Require(FindButton("QuantityMatchButton")?.interactable == true, "Quantity Match button is playable.");
            Require(FindButton("NumberLineJumpButton")?.interactable == true, "Number Line Jump button is playable.");
            Require(FindButton("CompareQuantityButton")?.interactable == true, "Compare Quantity button is playable.");
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
            Require(activities != null && activities.arraySize == 3, "Activity Select has three activity entries.");
            RequireSerializedReference(controller, "backButton");

            SerializedProperty playable = serialized.FindProperty("playableActivityIds");
            Require(playable != null && playable.arraySize == 3
                && playable.GetArrayElementAtIndex(0).stringValue == "QuantityMatch"
                && playable.GetArrayElementAtIndex(1).stringValue == "NumberLineJump"
                && playable.GetArrayElementAtIndex(2).stringValue == "CompareQuantity",
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
                Stage.WaitNumberLineReady => 12d,
                Stage.WaitMainMenu => 8d,
                Stage.WaitDashboard => 8d,
                Stage.WaitMainMenuReturn => 8d,
                Stage.WaitActivitySelect => 8d,
                Stage.WaitShellGameplay => 15d,
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
