#if UNITY_EDITOR
using System.Collections.Generic;
using Project.App;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Project.Editor
{
    /// <summary>
    /// Editor menu for creating app shell scenes used by the local Unity test guide.
    /// </summary>
    public static class SceneSetupMenu
    {
        private const string BootScenePath = "Assets/_Project/Scenes/SC_Boot.unity";
        private const string MainMenuScenePath = "Assets/_Project/Scenes/SC_MainMenu.unity";
        private const string ActivitySelectScenePath = "Assets/_Project/Scenes/SC_ActivitySelect.unity";
        private const string GameplayScenePath = "Assets/_Project/Scenes/SC_ARGameplay.unity";
        private const string ProgressDashboardScenePath = "Assets/_Project/Scenes/SC_ProgressDashboard.unity";
        private const string TestSandboxScenePath = "Assets/_Project/Scenes/SC_TestSandbox.unity";

        [MenuItem("AR Learning/Setup Scenes/Setup Boot Scene")]
        public static void SetupBootScene()
        {
            Scene scene = CreateShellScene(BootScenePath);
            new GameObject("BootLoader").AddComponent<BootLoader>();

            SaveScene(scene, BootScenePath);
            Debug.Log("[SceneSetupMenu] Boot scene setup complete.");
        }

        [MenuItem("AR Learning/Setup Scenes/Setup Main Menu Scene")]
        public static void SetupMainMenuScene()
        {
            Scene scene = CreateShellScene(MainMenuScenePath);
            GameObject canvasObj = CreateCanvas();

            CreateUIText(canvasObj.transform, "TitleText", "AR Learning", 42, new Vector2(0f, 210f), new Vector2(600f, 80f));
            GameObject startButtonObj = CreateUIButton(canvasObj.transform, "StartLearningButton", "Start Learning", new Vector2(0f, 60f));
            GameObject progressButtonObj = CreateUIButton(canvasObj.transform, "ViewProgressButton", "View Progress", new Vector2(0f, -20f));

            var controller = new GameObject("MainMenuController").AddComponent<MainMenuController>();
            AssignSerializedObjectReference(controller, "startLearningButton", startButtonObj.GetComponent<Button>());
            AssignSerializedObjectReference(controller, "viewProgressButton", progressButtonObj.GetComponent<Button>());

            SaveScene(scene, MainMenuScenePath);
            Debug.Log("[SceneSetupMenu] Main Menu scene setup complete.");
        }

        [MenuItem("AR Learning/Setup Scenes/Setup Activity Select Scene")]
        public static void SetupActivitySelectScene()
        {
            Scene scene = CreateShellScene(ActivitySelectScenePath);
            GameObject canvasObj = CreateCanvas();

            CreateUIText(canvasObj.transform, "TitleText", "Choose Activity", 38, new Vector2(0f, 240f), new Vector2(700f, 80f));
            GameObject qmButtonObj = CreateUIButton(canvasObj.transform, "QuantityMatchButton", "Quantity Match", new Vector2(0f, 110f));
            GameObject cqButtonObj = CreateUIButton(canvasObj.transform, "CompareQuantityButton", "Compare Quantity", new Vector2(0f, 30f));
            GameObject nbButtonObj = CreateUIButton(canvasObj.transform, "NumberBondsButton", "Number Bonds", new Vector2(0f, -50f));
            GameObject nljButtonObj = CreateUIButton(canvasObj.transform, "NumberLineJumpButton", "Number Line Jump", new Vector2(0f, -130f));
            GameObject backButtonObj = CreateUIButton(canvasObj.transform, "BackButton", "Back", new Vector2(0f, -230f));

            var controller = new GameObject("ActivitySelectController").AddComponent<ActivitySelectController>();
            AssignActivityButtons(controller, qmButtonObj, cqButtonObj, nbButtonObj, nljButtonObj);
            AssignSerializedObjectReference(controller, "backButton", backButtonObj.GetComponent<Button>());
            AssignPlayableActivities(controller);

            SaveScene(scene, ActivitySelectScenePath);
            Debug.Log("[SceneSetupMenu] Activity Select scene setup complete.");
        }

        [MenuItem("AR Learning/Setup Scenes/Setup Progress Dashboard Scene")]
        public static void SetupProgressDashboardScene()
        {
            Scene scene = CreateShellScene(ProgressDashboardScenePath);
            GameObject canvasObj = CreateCanvas();

            CreateUIText(canvasObj.transform, "TitleText", "Progress", 38, new Vector2(0f, 260f), new Vector2(700f, 80f));
            GameObject overallStatsObj = CreateUIText(canvasObj.transform, "OverallStatsText", "Overall Stats", 22, new Vector2(-360f, 40f), new Vector2(540f, 360f));
            GameObject activityStatsObj = CreateUIText(canvasObj.transform, "ActivityStatsText", "Activity Stats", 22, new Vector2(330f, 40f), new Vector2(620f, 420f));
            GameObject backButtonObj = CreateUIButton(canvasObj.transform, "BackButton", "Back", new Vector2(0f, -260f));

            var view = new GameObject("ProgressDashboardView").AddComponent<ProgressDashboardView>();
            AssignSerializedObjectReference(view, "overallStatsText", overallStatsObj.GetComponent<Text>());
            AssignSerializedObjectReference(view, "activityStatsText", activityStatsObj.GetComponent<Text>());
            AssignSerializedObjectReference(view, "backButton", backButtonObj.GetComponent<Button>());

            SaveScene(scene, ProgressDashboardScenePath);
            Debug.Log("[SceneSetupMenu] Progress Dashboard scene setup complete.");
        }

        [MenuItem("AR Learning/Setup Scenes/Setup Product Build Settings")]
        public static void SetupProductBuildSettings()
        {
            string[] orderedScenes =
            {
                BootScenePath,
                MainMenuScenePath,
                ActivitySelectScenePath,
                GameplayScenePath,
                ProgressDashboardScenePath,
                TestSandboxScenePath
            };

            var scenes = new List<EditorBuildSettingsScene>();
            foreach (string scenePath in orderedScenes)
            {
                scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            }

            EditorBuildSettings.scenes = scenes.ToArray();
            Debug.Log("[SceneSetupMenu] Product Build Settings setup complete.");
        }

        private static Scene CreateShellScene(string scenePath)
        {
            EnsureFolder("Assets/_Project/Scenes");

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = System.IO.Path.GetFileNameWithoutExtension(scenePath);

            CreateCamera();
            CreateDirectionalLight();
            CreateEventSystem();

            return scene;
        }

        private static void CreateCamera()
        {
            var cameraGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraGo.tag = "MainCamera";
            cameraGo.transform.position = new Vector3(0f, 1f, -10f);

            Camera camera = cameraGo.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.08f, 0.1f, 0.12f);
        }

        private static void CreateDirectionalLight()
        {
            var lightGo = new GameObject("Directional Light", typeof(Light));
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            Light light = lightGo.GetComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1f;
        }

        private static void CreateEventSystem()
        {
            var esGo = new GameObject("EventSystem");
            esGo.AddComponent<EventSystem>();
            try { esGo.AddComponent<InputSystemUIInputModule>(); } catch { esGo.AddComponent<StandaloneInputModule>(); }
        }

        private static GameObject CreateCanvas()
        {
            var canvasObj = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObj.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            return canvasObj;
        }

        private static void AssignActivityButtons(ActivitySelectController controller, GameObject quantityMatchButtonObj,
            GameObject compareQuantityButtonObj, GameObject numberBondsButtonObj, GameObject numberLineJumpButtonObj)
        {
            var serializedObject = new SerializedObject(controller);
            SerializedProperty activitiesProperty = serializedObject.FindProperty("activities");

            if (activitiesProperty == null)
            {
                Debug.LogWarning("[SceneSetupMenu] Could not find ActivitySelectController.activities field.");
                return;
            }

            activitiesProperty.arraySize = 4;
            SetActivityButton(activitiesProperty.GetArrayElementAtIndex(0),
                quantityMatchButtonObj.GetComponent<Button>(), "QuantityMatch", "QuantityMatch");
            SetActivityButton(activitiesProperty.GetArrayElementAtIndex(1),
                compareQuantityButtonObj.GetComponent<Button>(), "CompareQuantity", "CompareQuantity");
            SetActivityButton(activitiesProperty.GetArrayElementAtIndex(2),
                numberBondsButtonObj.GetComponent<Button>(), "NumberBonds", "NumberBonds");
            SetActivityButton(activitiesProperty.GetArrayElementAtIndex(3),
                numberLineJumpButtonObj.GetComponent<Button>(), "NumberLineJump", "NumberLineJump");

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(controller);
        }

        private static void SetActivityButton(SerializedProperty activityProperty, Button button,
            string activityId, string sceneName)
        {
            activityProperty.FindPropertyRelative("button").objectReferenceValue = button;
            activityProperty.FindPropertyRelative("activityId").stringValue = activityId;
            activityProperty.FindPropertyRelative("sceneName").stringValue = sceneName;
        }

        private static void AssignPlayableActivities(ActivitySelectController controller)
        {
            var serializedObject = new SerializedObject(controller);
            SerializedProperty playableProperty = serializedObject.FindProperty("playableActivityIds");

            if (playableProperty == null)
            {
                return;
            }

            playableProperty.arraySize = 4;
            playableProperty.GetArrayElementAtIndex(0).stringValue = "QuantityMatch";
            playableProperty.GetArrayElementAtIndex(1).stringValue = "CompareQuantity";
            playableProperty.GetArrayElementAtIndex(2).stringValue = "NumberBonds";
            playableProperty.GetArrayElementAtIndex(3).stringValue = "NumberLineJump";
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(controller);
        }

        private static void AssignSerializedObjectReference(Object target, string fieldName, Object reference)
        {
            var serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(fieldName);

            if (property == null)
            {
                Debug.LogWarning($"[SceneSetupMenu] Could not find serialized field '{fieldName}' on {target.name}.");
                return;
            }

            property.objectReferenceValue = reference;
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
        }

        private static GameObject CreateUIButton(Transform parent, string name, string label, Vector2 anchoredPosition)
        {
            GameObject buttonObj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObj.transform.SetParent(parent, false);

            RectTransform rect = buttonObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(320f, 58f);
            rect.anchoredPosition = anchoredPosition;

            Image image = buttonObj.GetComponent<Image>();
            image.color = new Color(0.14f, 0.43f, 0.82f);

            GameObject textObj = CreateUIText(buttonObj.transform, "Text", label, 22, Vector2.zero, rect.sizeDelta);
            textObj.GetComponent<Text>().raycastTarget = false;

            return buttonObj;
        }

        private static GameObject CreateUIText(Transform parent, string name, string content, int fontSize,
            Vector2 anchoredPosition, Vector2 size)
        {
            GameObject textObj = new GameObject(name, typeof(RectTransform), typeof(Text));
            textObj.transform.SetParent(parent, false);

            RectTransform rect = textObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;

            Text text = textObj.GetComponent<Text>();
            text.text = content;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;

            return textObj;
        }

        private static void SaveScene(Scene scene, string scenePath)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, scenePath))
            {
                Debug.LogError($"[SceneSetupMenu] Failed to save {scenePath}");
            }
        }

        private static void EnsureFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                System.IO.Directory.CreateDirectory(path);
                AssetDatabase.Refresh();
            }
        }
    }
}
#endif
