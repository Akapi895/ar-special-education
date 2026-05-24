#if UNITY_EDITOR
using Core.AR;
using Core.AR.ARSession;
using Core.AR.Interaction;
using Core.AR.Placement;
using Core.AR.Sandbox;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project.Editor
{
    /// <summary>
    /// Creates or refreshes SC_TestSandbox with XR Origin and AR service components.
    /// Menu: AR Learning → Setup Test Sandbox Scene
    /// </summary>
    public static class ARTestSandboxMenu
    {
        private const string SandboxScenePath = "Assets/_Project/Scenes/SC_TestSandbox.unity";
        private const string XrOriginPrefabPath =
            "Assets/Samples/XR Interaction Toolkit/3.3.0/AR Starter Assets/Prefabs/XR Origin (AR Rig).prefab";

        [MenuItem("AR Learning/Setup Test Sandbox Scene")]
        public static void SetupTestSandboxScene()
        {
            var xrPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(XrOriginPrefabPath);
            if (xrPrefab == null)
            {
                Debug.LogError($"[ARTestSandboxMenu] Missing prefab at {XrOriginPrefabPath}");
                return;
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject xrOrigin = PrefabUtility.InstantiatePrefab(xrPrefab) as GameObject;
            if (xrOrigin == null)
            {
                Debug.LogError("[ARTestSandboxMenu] Failed to instantiate XR Origin.");
                return;
            }

            xrOrigin.name = "XR Origin (AR Rig)";
            EnsureSingleAudioListener(xrOrigin);

            UnityEngine.XR.ARFoundation.ARSession arSession = EnsureARSession();
            ARSessionService sessionService = xrOrigin.GetComponent<ARSessionService>();
            if (sessionService == null)
            {
                sessionService = xrOrigin.AddComponent<ARSessionService>();
            }

            AssignARSession(sessionService, arSession);

            if (xrOrigin.GetComponent<ARPlacementService>() == null)
            {
                xrOrigin.AddComponent<ARPlacementService>();
            }

            if (xrOrigin.GetComponent<ARInteractionService>() == null)
            {
                xrOrigin.AddComponent<ARInteractionService>();
            }

            var bootstrapGo = new GameObject("ARServiceBootstrap");
            bootstrapGo.AddComponent<ARServiceBootstrap>();

            var sandboxGo = new GameObject("ARSandboxController");
            sandboxGo.AddComponent<ARSandboxController>();

            bool saved = EditorSceneManager.SaveScene(scene, SandboxScenePath);
            if (saved)
            {
                Debug.Log($"[ARTestSandboxMenu] Saved {SandboxScenePath}. Add scene to Build Settings if needed.");
            }

            AddSceneToBuildSettingsIfMissing(SandboxScenePath);
        }

        private static void EnsureSingleAudioListener(GameObject preferredRoot)
        {
            AudioListener[] listeners = Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
            bool keptPreferred = false;

            foreach (AudioListener listener in listeners)
            {
                bool isPreferred = preferredRoot != null && listener.transform.IsChildOf(preferredRoot.transform);
                if (isPreferred && !keptPreferred)
                {
                    keptPreferred = true;
                    continue;
                }

                Object.DestroyImmediate(listener);
            }
        }

        private static UnityEngine.XR.ARFoundation.ARSession EnsureARSession()
        {
            UnityEngine.XR.ARFoundation.ARSession arSession =
                Object.FindAnyObjectByType<UnityEngine.XR.ARFoundation.ARSession>();
            if (arSession != null)
            {
                return arSession;
            }

            var sessionGo = new GameObject("AR Session");
            return sessionGo.AddComponent<UnityEngine.XR.ARFoundation.ARSession>();
        }

        private static void AssignARSession(ARSessionService service, UnityEngine.XR.ARFoundation.ARSession arSession)
        {
            var serialized = new SerializedObject(service);
            serialized.FindProperty("arSession").objectReferenceValue = arSession;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        [MenuItem("AR Learning/Add Test Sandbox To Build Settings")]
        public static void AddSandboxToBuildSettings()
        {
            AddSceneToBuildSettingsIfMissing(SandboxScenePath);
        }

        public static void AddSceneToBuildSettingsIfMissing(string scenePath)
        {
            var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(
                EditorBuildSettings.scenes);

            foreach (EditorBuildSettingsScene entry in scenes)
            {
                if (entry.path == scenePath)
                {
                    Debug.Log($"[ARTestSandboxMenu] {scenePath} already in Build Settings.");
                    return;
                }
            }

            scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            Debug.Log($"[ARTestSandboxMenu] Added {scenePath} to Build Settings.");
        }
    }
}
#endif
