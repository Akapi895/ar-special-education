using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project.App
{
    /// <summary>
    /// Boot scene loader - initializes services and loads main menu.
    /// First scene loaded when app starts.
    /// </summary>
    public class BootLoader : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("Delay before loading main menu (seconds)")]
        [SerializeField]
        private float loadDelay = 0.5f;

        [Tooltip("Name of the main menu scene")]
        [SerializeField]
        private string mainMenuSceneName = "SC_MainMenu";

        private void Start()
        {
            Debug.Log($"[BootLoader] App STARTED. Unity {Application.unityVersion}, Platform: {Application.platform}, Target FPS: {Application.targetFrameRate}");
            // Initialize any services here
            InitializeServices();

            // Load main menu after delay
            Invoke(nameof(LoadMainMenu), loadDelay);
        }

        private void InitializeServices()
        {
            Core.Support.Performance.RuntimePerformanceSettings.Apply();

            // Initialize progress storage proxy
            Core.Data.LocalStorage.ProgressStorageProxy.Initialize();

            // Initialize feedback service proxy
            Core.Support.FeedbackSystem.FeedbackServiceProxy.Initialize();

            // Ensure audio preferences and replay support exist before scenes request sounds.
            Core.Support.AudioManager.SimpleAudioManager.EnsureExists();

            Application.targetFrameRate = 30;

            Debug.Log("[BootLoader] Services initialized.");
        }

        private void LoadMainMenu()
        {
            Debug.Log($"[BootLoader] Loading SCENE: {mainMenuSceneName}");
            SceneTransitionManager.LoadScene(mainMenuSceneName);
        }
    }
}
