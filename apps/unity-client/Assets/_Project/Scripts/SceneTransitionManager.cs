using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Project.App
{
    /// <summary>
    /// Manages scene transitions with a fade-to-black overlay.
    /// Prevents the white flash between scene loads on iOS.
    /// </summary>
    public static class SceneTransitionManager
    {
        private static Canvas overlayCanvas;
        private static Image overlayImage;

        /// <summary>
        /// Load a scene with a fade-to-black transition.
        /// </summary>
        public static void LoadScene(string sceneName, float fadeDuration = 0.2f)
        {
            if (Application.isPlaying)
            {
                EnsureOverlay();
                GameObject overlayObj = overlayCanvas.gameObject;
                if (overlayObj != null)
                {
                    GameObject runner = new GameObject("SceneTransitionRunner");
                    var runnerComponent = runner.AddComponent<SceneTransitionRunner>();
                    runnerComponent.StartCoroutine(FadeAndLoad(sceneName, fadeDuration));
                }
            }
            else
            {
                SceneManager.LoadScene(sceneName);
            }
        }

        private static void EnsureOverlay()
        {
            if (overlayCanvas != null) return;

            var go = new GameObject("SceneTransitionOverlay", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.hideFlags = HideFlags.HideAndDontSave;
            Object.DontDestroyOnLoad(go);

            overlayCanvas = go.GetComponent<Canvas>();
            overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            overlayCanvas.sortingOrder = 32767; // Max sorting order — always on top

            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            var imageGo = new GameObject("OverlayImage", typeof(RectTransform), typeof(Image));
            imageGo.transform.SetParent(go.transform, false);
            var imageRect = imageGo.GetComponent<RectTransform>();
            imageRect.anchorMin = Vector2.zero;
            imageRect.anchorMax = Vector2.one;
            imageRect.sizeDelta = Vector2.zero;

            overlayImage = imageGo.GetComponent<Image>();
            overlayImage.color = Color.white;
            overlayImage.raycastTarget = true;
        }

        private static IEnumerator FadeAndLoad(string sceneName, float duration)
        {
            Debug.Log($"[SceneTransitionManager] Loading scene '{sceneName}' with {duration}s fade...");
            // Fade to black
            overlayImage.color = Color.clear;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float t = elapsed / duration;
                overlayImage.color = Color.Lerp(Color.clear, Color.black, t);
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            overlayImage.color = Color.black;

            // Load scene
            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
            Debug.Log($"[SceneTransitionManager] Scene '{sceneName}' async load started...");
            while (!op.isDone)
            {
                if (Time.frameCount % 30 == 0) Debug.Log($"[SceneTransitionManager] Scene '{sceneName}' loading: {op.progress:P0}");
                yield return null;
            }
            Debug.Log($"[SceneTransitionManager] Scene '{sceneName}' loaded. Fading back in...");

            // Fade back in
            elapsed = 0f;
            while (elapsed < duration)
            {
                float t = elapsed / duration;
                overlayImage.color = Color.Lerp(Color.black, Color.clear, t);
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            overlayImage.color = Color.clear;
        }

        // Helper MonoBehaviour to run coroutines in static context
        private class SceneTransitionRunner : MonoBehaviour
        {
            private void Awake()
            {
                DontDestroyOnLoad(gameObject);
            }
        }
    }
}
