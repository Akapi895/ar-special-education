using Core.UI.Components;
using Core.UI.Layout;
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

        [SerializeField] private MascotDisplay mascotDisplay;

        private void Start()
        {
            ActivityRuntimeCanvas.EnsureEventSystem();
            LocalizeAndStyleMenuText();
            ApplyMenuWideCardBackground();

            // Show mascot greeting (auto-created if not assigned in scene)
            EnsureMascot();
            Debug.Log($"[MainMenuController] MainMenu STARTED. Screen: {Screen.width}x{Screen.height}, SafeArea: {Screen.safeArea}");

            // Setup button listeners
            if (startLearningButton != null)
            {
                string startLabel = "Bắt đầu học";
                var rect = startLearningButton.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = new Vector2(440f, 110f);
                rect.anchoredPosition = new Vector2(0f, -40f);
                UIKidFriendlyStyle.Apply(
                    startLearningButton,
                    KidButtonPurpose.Primary,
                    startLabel,
                    38);
                startLearningButton.onClick.AddListener(OnStartLearning);
            }

            if (viewProgressButton != null)
            {
                string progressLabel = "Xem tiến độ";
                var rect = viewProgressButton.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(1f, 1f);
                rect.anchorMax = new Vector2(1f, 1f);
                rect.pivot = new Vector2(1f, 1f);
                rect.sizeDelta = new Vector2(200f, 60f);
                rect.anchoredPosition = new Vector2(-24f, -24f);
                UIKidFriendlyStyle.Apply(
                    viewProgressButton,
                    KidButtonPurpose.Progress,
                    progressLabel,
                    24);
                viewProgressButton.onClick.AddListener(OnViewProgress);
            }

            // Perform entrance animation if canvas group is available
            if (menuCanvasGroup != null)
            {
                StartCoroutine(EntranceCoroutine());
            }

            Debug.Log("[MainMenuController] Main menu loaded.");
        }

        private void ApplyMenuWideCardBackground()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            var bgGo = new GameObject("MenuBackgroundCard", typeof(RectTransform), typeof(Core.UI.Components.RoundedRectGraphic));
            var bgRect = bgGo.GetComponent<RectTransform>();
            bgRect.SetParent(canvas.transform, false);
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = new Vector2(20f, 20f);
            bgRect.offsetMax = new Vector2(-20f, -20f);

            var bgGraphic = bgGo.GetComponent<Core.UI.Components.RoundedRectGraphic>();
            bgGraphic.CornerRadius = 32f;
            bgGraphic.color = new Color(0.12f, 0.12f, 0.14f, 0.82f);
            bgGraphic.raycastTarget = false;

            bgGo.transform.SetAsFirstSibling();

            // Apply safe area padding for iOS notched devices
            Rect safeArea = Screen.safeArea;
            if (safeArea.x != 0f || safeArea.y != 0f ||
                safeArea.width != Screen.width || safeArea.height != Screen.height)
            {
                // Safe area differs from full screen — apply padding to the card
                float leftPad = safeArea.xMin;
                float rightPad = Screen.width - safeArea.xMax;
                float topPad = Screen.height - safeArea.yMax;
                float bottomPad = safeArea.yMin;
                bgRect.offsetMin = new Vector2(leftPad + 20f, bottomPad + 20f);
                bgRect.offsetMax = new Vector2(-(rightPad + 20f), -(topPad + 20f));
                Debug.Log($"[MainMenuController] Safe area applied: L={leftPad} R={rightPad} T={topPad} B={bottomPad}");
            }
        }

        private void EnsureMascot()
        {
            if (mascotDisplay != null) return;
            mascotDisplay = gameObject.AddComponent<MascotDisplay>();
        }

        private void LocalizeAndStyleMenuText()
        {
            Text titleText = titleTransform != null
                ? titleTransform.GetComponent<Text>()
                : FindSceneText("TitleText");

            if (titleText != null)
            {
                titleText.text = "\U0001F430 " + SimpleLocalization.Get("app_title");
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
