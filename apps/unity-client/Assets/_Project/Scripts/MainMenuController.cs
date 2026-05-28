using Core.UI.Components;
using Core.UI.Localization;
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
            LocalizeAndStyleMenuText();

            // Setup button listeners
            if (startLearningButton != null)
            {
                string startLabel = "Bắt đầu học";
                UIKidFriendlyStyle.Apply(
                    startLearningButton,
                    KidButtonPurpose.Primary,
                    startLabel,
                    34);
                UIKidFriendlyStyle.HideButtonBackground(startLearningButton);
                UIKidFriendlyStyle.SetButtonTextColorWithOutline(startLearningButton, new Color(1f, 0.94f, 0.48f, 1f));
                startLearningButton.onClick.AddListener(OnStartLearning);
            }

            if (viewProgressButton != null)
            {
                string progressLabel = "Xem tiến độ";
                UIKidFriendlyStyle.Apply(
                    viewProgressButton,
                    KidButtonPurpose.Progress,
                    progressLabel,
                    32);
                UIKidFriendlyStyle.HideButtonBackground(viewProgressButton);
                UIKidFriendlyStyle.SetButtonTextColorWithOutline(viewProgressButton, new Color(0.55f, 0.9f, 1f, 1f));
                viewProgressButton.onClick.AddListener(OnViewProgress);
            }

            // Perform entrance animation if canvas group is available
            if (menuCanvasGroup != null)
            {
                StartCoroutine(EntranceCoroutine());
            }

            Debug.Log("[MainMenuController] Main menu loaded.");
        }

        private void LocalizeAndStyleMenuText()
        {
            Text titleText = titleTransform != null
                ? titleTransform.GetComponent<Text>()
                : FindSceneText("TitleText");

            if (titleText != null)
            {
                titleText.text = SimpleLocalization.Get("app_title");
                titleText.fontSize = 50;
                titleText.resizeTextForBestFit = true;
                titleText.resizeTextMinSize = 34;
                titleText.resizeTextMaxSize = 50;
                titleText.fontStyle = FontStyle.Bold;
                titleText.color = new Color(1f, 0.86f, 0.36f, 1f);
                Shadow shadow = titleText.GetComponent<Shadow>();
                if (shadow == null)
                {
                    shadow = titleText.gameObject.AddComponent<Shadow>();
                }

                shadow.effectColor = new Color(0f, 0f, 0f, 0.35f);
                shadow.effectDistance = new Vector2(0f, -4f);
            }
        }

        private static Text FindSceneText(string objectName)
        {
            Text[] texts = UnityEngine.Object.FindObjectsByType<Text>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            for (int i = 0; i < texts.Length; i++)
            {
                if (texts[i] != null && texts[i].gameObject.name == objectName)
                {
                    return texts[i];
                }
            }

            return null;
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
