#if UNITY_EDITOR
using System.Collections.Generic;
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
    /// Repairs product scene placeholders by creating real Unity scenes through editor APIs.
    /// </summary>
    public static class ProductSceneRepairMenu
    {
        private static readonly string[] ShellScenePaths =
        {
            "Assets/_Project/Scenes/SC_Boot.unity",
            "Assets/_Project/Scenes/SC_MainMenu.unity",
            "Assets/_Project/Scenes/SC_ActivitySelect.unity",
            "Assets/_Project/Scenes/SC_ProgressDashboard.unity"
        };

        private const string TestSandboxScenePath = "Assets/_Project/Scenes/SC_TestSandbox.unity";
        private const string GameplayScenePath = "Assets/_Project/Scenes/SC_ARGameplay.unity";

        [MenuItem("AR Learning/Repair Product Scenes")]
        public static void RepairProductScenes()
        {
            ARTestSandboxMenu.SetupTestSandboxScene();
            ARGameplaySceneMenu.SetupARGameplayScene();

            foreach (string scenePath in ShellScenePaths)
            {
                CreateShellScene(scenePath);
            }

            SetProductBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[ProductSceneRepairMenu] Repaired product scenes and build settings.");
        }

        private static void CreateShellScene(string scenePath)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = System.IO.Path.GetFileNameWithoutExtension(scenePath);

            CreateCamera();
            CreateDirectionalLight();
            CreateEventSystem();
            CreateCanvas(scene.name);

            if (!EditorSceneManager.SaveScene(scene, scenePath))
            {
                Debug.LogError($"[ProductSceneRepairMenu] Failed to save {scenePath}");
            }
        }

        private static void CreateCamera()
        {
            var cameraGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraGo.tag = "MainCamera";
            cameraGo.transform.position = new Vector3(0f, 1f, -10f);
            cameraGo.transform.rotation = Quaternion.identity;

            Camera camera = cameraGo.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.Skybox;
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
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        }

        private static void CreateCanvas(string sceneName)
        {
            var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            CreateLabel(canvasGo.transform, sceneName);
        }

        private static void CreateLabel(Transform parent, string sceneName)
        {
            var labelGo = new GameObject("Scene Label", typeof(RectTransform), typeof(Text));
            labelGo.transform.SetParent(parent, false);

            RectTransform rect = labelGo.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Text text = labelGo.GetComponent<Text>();
            text.text = sceneName;
            text.alignment = TextAnchor.MiddleCenter;
            text.fontSize = 42;
            text.color = Color.white;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        private static void SetProductBuildSettings()
        {
            string[] orderedScenes =
            {
                "Assets/_Project/Scenes/SC_Boot.unity",
                "Assets/_Project/Scenes/SC_MainMenu.unity",
                "Assets/_Project/Scenes/SC_ActivitySelect.unity",
                GameplayScenePath,
                "Assets/_Project/Scenes/SC_ProgressDashboard.unity",
                TestSandboxScenePath
            };

            var buildScenes = new List<EditorBuildSettingsScene>();
            foreach (string scenePath in orderedScenes)
            {
                buildScenes.Add(new EditorBuildSettingsScene(scenePath, true));
            }

            EditorBuildSettings.scenes = buildScenes.ToArray();
        }
    }
}
#endif
