using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Project.App
{
    /// <summary>
    /// Main menu controller - handles navigation to activity select and progress dashboard.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField]
        private Button startLearningButton;

        [SerializeField]
        private Button viewProgressButton;

        [Header("Scene Names")]
        [SerializeField]
        private string activitySelectSceneName = "SC_ActivitySelect";

        [SerializeField]
        private string progressDashboardSceneName = "SC_ProgressDashboard";

        private void Start()
        {
            // Setup button listeners
            if (startLearningButton != null)
            {
                startLearningButton.onClick.AddListener(OnStartLearning);
            }

            if (viewProgressButton != null)
            {
                viewProgressButton.onClick.AddListener(OnViewProgress);
            }

            Debug.Log("[MainMenuController] Main menu loaded.");
        }

        private void OnDestroy()
        {
            // Clean up listeners
            if (startLearningButton != null)
            {
                startLearningButton.onClick.RemoveListener(OnStartLearning);
            }

            if (viewProgressButton != null)
            {
                viewProgressButton.onClick.RemoveListener(OnViewProgress);
            }
        }

        private void OnStartLearning()
        {
            SceneManager.LoadScene(activitySelectSceneName);
        }

        private void OnViewProgress()
        {
            SceneManager.LoadScene(progressDashboardSceneName);
        }
    }
}
