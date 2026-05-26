using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

namespace Project.App
{
    /// <summary>
    /// Main menu controller - handles navigation to activity select and progress dashboard.
    /// Incorporates kid-friendly entrance transitions and styling.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button startLearningButton;
        [SerializeField] private Button viewProgressButton;

        [Header("Scene Names")]
        [SerializeField] private string activitySelectSceneName = "SC_ActivitySelect";
        [SerializeField] private string progressDashboardSceneName = "SC_ProgressDashboard";

        [Header("Visual Elements (Optional)")]
        [SerializeField] private CanvasGroup menuCanvasGroup;
        [SerializeField] private RectTransform titleTransform;

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

            // Perform entrance animation if canvas group is available
            if (menuCanvasGroup != null)
            {
                StartCoroutine(EntranceCoroutine());
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

        private IEnumerator EntranceCoroutine()
        {
            menuCanvasGroup.alpha = 0f;
            Vector3 originalTitleScale = Vector3.one;

            if (titleTransform != null)
            {
                originalTitleScale = titleTransform.localScale;
                titleTransform.localScale = originalTitleScale * 0.7f;
            }

            float elapsed = 0f;
            float duration = 0.5f;

            bool animationsEnabled = PlayerPrefs.GetInt("AnimationsEnabled", 1) == 1;
            if (!animationsEnabled)
            {
                menuCanvasGroup.alpha = 1f;
                if (titleTransform != null) titleTransform.localScale = originalTitleScale;
                yield break;
            }

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                
                // Smooth step
                t = t * t * (3f - 2f * t);

                menuCanvasGroup.alpha = t;
                if (titleTransform != null)
                {
                    titleTransform.localScale = Vector3.Lerp(originalTitleScale * 0.7f, originalTitleScale, t);
                }

                yield return null;
            }

            menuCanvasGroup.alpha = 1f;
            if (titleTransform != null) titleTransform.localScale = originalTitleScale;
        }
    }
}
