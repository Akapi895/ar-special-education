#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Project.Editor
{
    /// <summary>
    /// Editor menu for setting up app shell scenes.
    /// </summary>
    public static class SceneSetupMenu
    {
        [MenuItem("AR Learning/Setup Scenes/Setup Boot Scene")]
        public static void SetupBootScene()
        {
            string scenePath = "Assets/_Project/Scenes/SC_Boot.unity";
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            // Create or find BootLoader
            BootLoader bootLoader = Object.FindFirstObjectByType<BootLoader>();
            if (bootLoader == null)
            {
                GameObject bootObj = new GameObject("BootLoader");
                bootLoader = bootObj.AddComponent<BootLoader>();
            }

            // Create EventSystem if needed
            if (UnityEngine.EventSystems.EventSystem.current == null)
            {
                GameObject eventSystemObj = new GameObject("EventSystem");
                eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[SceneSetupMenu] Boot scene setup complete.");
        }

        [MenuItem("AR Learning/Setup Scenes/Setup Main Menu Scene")]
        public static void SetupMainMenuScene()
        {
            string scenePath = "Assets/_Project/Scenes/SC_MainMenu.unity";
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            // Create Canvas
            GameObject canvasObj = new GameObject("Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            // Create EventSystem if needed
            if (UnityEngine.EventSystems.EventSystem.current == null)
            {
                GameObject eventSystemObj = new GameObject("EventSystem");
                eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            // Create MainMenuController
            GameObject controllerObj = new GameObject("MainMenuController");
            MainMenuController controller = controllerObj.AddComponent<MainMenuController>();

            // Create buttons
            GameObject startButtonObj = CreateUIButton(canvasObj.transform, "StartLearningButton", "Start Learning");
            GameObject progressButtonObj = CreateUIButton(canvasObj.transform, "ViewProgressButton", "View Progress");

            // Assign buttons to controller (will need manual assignment in Inspector)
            controller.StartLearningButton = startButtonObj.GetComponent<UnityEngine.UI.Button>();
            controller.ViewProgressButton = progressButtonObj.GetComponent<UnityEngine.UI.Button>();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[SceneSetupMenu] Main Menu scene setup complete. Please verify button assignments in Inspector.");
        }

        [MenuItem("AR Learning/Setup Scenes/Setup Activity Select Scene")]
        public static void SetupActivitySelectScene()
        {
            string scenePath = "Assets/_Project/Scenes/SC_ActivitySelect.unity";
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            // Create Canvas
            GameObject canvasObj = new GameObject("Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            // Create EventSystem if needed
            if (UnityEngine.EventSystems.EventSystem.current == null)
            {
                GameObject eventSystemObj = new GameObject("EventSystem");
                eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            // Create ActivitySelectController
            GameObject controllerObj = new GameObject("ActivitySelectController");
            ActivitySelectController controller = controllerObj.AddComponent<ActivitySelectController>();

            // Create activity buttons
            GameObject qmButtonObj = CreateUIButton(canvasObj.transform, "QuantityMatchButton", "Quantity Match");
            GameObject nljButtonObj = CreateUIButton(canvasObj.transform, "NumberLineJumpButton", "Number Line Jump");
            GameObject cqButtonObj = CreateUIButton(canvasObj.transform, "CompareQuantityButton", "Compare Quantity");
            GameObject backButtonObj = CreateUIButton(canvasObj.transform, "BackButton", "Back");

            // Note: Button-to-activity mapping needs to be done manually in Inspector
            // This is a limitation of not having serialized references

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[SceneSetupMenu] Activity Select scene setup complete. Please configure activity buttons in Inspector.");
        }

        [MenuItem("AR Learning/Setup Scenes/Setup Progress Dashboard Scene")]
        public static void SetupProgressDashboardScene()
        {
            string scenePath = "Assets/_Project/Scenes/SC_ProgressDashboard.unity";
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            // Create Canvas
            GameObject canvasObj = new GameObject("Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            // Create EventSystem if needed
            if (UnityEngine.EventSystems.EventSystem.current == null)
            {
                GameObject eventSystemObj = new GameObject("EventSystem");
                eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            // Create ProgressDashboardView
            GameObject controllerObj = new GameObject("ProgressDashboardView");
            ProgressDashboardView view = controllerObj.AddComponent<ProgressDashboardView>();

            // Create stats text objects
            GameObject overallStatsObj = CreateUIText(canvasObj.transform, "OverallStatsText", "Overall Stats");
            GameObject activityStatsObj = CreateUIText(canvasObj.transform, "ActivityStatsText", "Activity Stats");
            GameObject backButtonObj = CreateUIButton(canvasObj.transform, "BackButton", "Back");

            // Assign references (will need manual adjustment in Inspector)
            view.OverallStatsText = overallStatsObj.GetComponent<UnityEngine.UI.Text>();
            view.ActivityStatsText = activityStatsObj.GetComponent<UnityEngine.UI.Text>();
            view.BackButton = backButtonObj.GetComponent<UnityEngine.UI.Button>();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[SceneSetupMenu] Progress Dashboard scene setup complete. Please verify assignments in Inspector.");
        }

        private static GameObject CreateUIButton(Transform parent, string name, string label)
        {
            GameObject buttonObj = new GameObject(name);
            buttonObj.transform.SetParent(parent, false);

            RectTransform rect = buttonObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(200, 50);

            UnityEngine.UI.Image image = buttonObj.AddComponent<UnityEngine.UI.Image>();
            image.color = new Color(0.2f, 0.6f, 1f);

            UnityEngine.UI.Button button = buttonObj.AddComponent<UnityEngine.UI.Button>();

            // Create text child
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            UnityEngine.UI.Text text = textObj.AddComponent<UnityEngine.UI.Text>();
            text.text = label;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 18;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;

            return buttonObj;
        }

        private static GameObject CreateUIText(Transform parent, string name, string content)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(parent, false);

            RectTransform rect = textObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(400, 300);

            UnityEngine.UI.Text text = textObj.AddComponent<UnityEngine.UI.Text>();
            text.text = content;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 16;
            text.alignment = TextAnchor.UpperLeft;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;

            return textObj;
        }
    }
}
#endif
