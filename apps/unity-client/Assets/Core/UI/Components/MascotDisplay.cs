using System.Collections;
using Core.UI.Layout;
using UnityEngine;
using UnityEngine.UI;

namespace Core.UI.Components
{
    public class MascotDisplay : MonoBehaviour
    {
        [Header("Mascot")]
        [SerializeField] private string mascotEmoji = "\U0001F430";

        [Header("Animation")]
        [SerializeField] private float bounceAmplitude = 8f;
        [SerializeField] private float bounceSpeed = 1.2f;
        [SerializeField] private float entranceDuration = 0.4f;

        [Header("Speech")]
        [SerializeField] private string mascotGreeting = "Cùng chơi toán nào!";
        [SerializeField] private float speechDisplayDuration = 5f;

        private Text mascotText;
        private GameObject speechPanel;
        private Text speechText;
        private RectTransform mascotRect;
        private Vector2 basePosition;
        private Coroutine idleAnimation;
        private Coroutine speechCoroutine;
        private bool isInitialized;

        private void Start()
        {
            Debug.Log("[MascotDisplay] Start() called. Initializing UI...");
            EnsureUI();
            mascotText.text = mascotEmoji;
            PlayEntrance();
            SetSpeech(mascotGreeting, speechDisplayDuration);
        }

        private void OnDisable()
        {
            StopIdleAnimation();
        }

        private void EnsureUI()
        {
            if (isInitialized) return;
            isInitialized = true;

            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[MascotDisplay] No canvas found in scene. Mascot will not appear.");
                return;
            }
            Debug.Log($"[MascotDisplay] Found canvas: {canvas.name}");

            Transform root = canvas.transform;

            // Create mascot background (circular pastel colored disc)
            var mascotBg = new GameObject("MascotBg", typeof(RectTransform), typeof(Core.UI.Components.RoundedRectGraphic));
            mascotBg.transform.SetParent(root, false);
            var bgRect = mascotBg.GetComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0.5f, 0.5f);
            bgRect.anchorMax = new Vector2(0.5f, 0.5f);
            bgRect.pivot = new Vector2(0.5f, 0.5f);
            bgRect.sizeDelta = new Vector2(110f, 110f);
            bgRect.anchoredPosition = new Vector2(0f, 120f);
            var bgGraphic = mascotBg.GetComponent<Core.UI.Components.RoundedRectGraphic>();
            bgGraphic.CornerRadius = 55f; // makes it a circle
            bgGraphic.color = new Color(1f, 0.85f, 0.7f, 1f); // warm pastel orange
            bgGraphic.raycastTarget = false;
            var bgShadow = mascotBg.AddComponent<Shadow>();
            bgShadow.effectColor = new Color(0f, 0f, 0f, 0.15f);
            bgShadow.effectDistance = new Vector2(0f, -5f);
            bgShadow.useGraphicAlpha = true;

            // Create mascot emoji text (NO custom font — use default for emoji rendering)
            var mascotObj = new GameObject("MascotEmoji", typeof(RectTransform), typeof(Text));
            mascotObj.transform.SetParent(root, false);
            mascotRect = mascotObj.GetComponent<RectTransform>();
            mascotRect.anchorMin = new Vector2(0.5f, 0.5f);
            mascotRect.anchorMax = new Vector2(0.5f, 0.5f);
            mascotRect.pivot = new Vector2(0.5f, 0.5f);
            mascotRect.sizeDelta = new Vector2(100f, 100f);
            mascotRect.anchoredPosition = new Vector2(0f, 120f);
            Debug.Log($"[MascotDisplay] Mascot position: {mascotRect.anchoredPosition}, size: {mascotRect.sizeDelta}");

            mascotText = mascotObj.GetComponent<Text>();
            mascotText.text = mascotEmoji;
            mascotText.fontSize = 70;
            mascotText.alignment = TextAnchor.MiddleCenter;
            // IMPORTANT: Do NOT assign a custom font — the default Unity font supports emoji on iOS.
            // Quicksand lacks emoji glyphs and would render emoji as blank boxes.
            mascotText.raycastTarget = false;
            mascotText.color = Color.white;
            mascotText.supportRichText = false;

            speechPanel = new GameObject("MascotSpeechBubble", typeof(RectTransform));
            speechPanel.transform.SetParent(root, false);
            var speechRect = speechPanel.GetComponent<RectTransform>();
            speechRect.anchorMin = new Vector2(0.5f, 0.5f);
            speechRect.anchorMax = new Vector2(0.5f, 0.5f);
            speechRect.pivot = new Vector2(0.5f, 1f);
            speechRect.sizeDelta = new Vector2(380f, 70f);
            speechRect.anchoredPosition = new Vector2(0f, 210f);

            var rounded = speechPanel.AddComponent<Core.UI.Components.RoundedRectGraphic>();
            rounded.CornerRadius = 20f;
            rounded.color = new Color(1f, 1f, 1f, 0.88f);
            rounded.raycastTarget = false;

            var shadow = speechPanel.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.12f);
            shadow.effectDistance = new Vector2(0f, -3f);
            shadow.useGraphicAlpha = true;

            var speechTextObj = new GameObject("SpeechText", typeof(RectTransform), typeof(Text));
            speechTextObj.transform.SetParent(speechPanel.transform, false);
            speechText = speechTextObj.GetComponent<Text>();
            var speechTextRect = speechText.GetComponent<RectTransform>();
            speechTextRect.anchorMin = Vector2.zero;
            speechTextRect.anchorMax = Vector2.one;
            speechTextRect.offsetMin = new Vector2(12f, 4f);
            speechTextRect.offsetMax = new Vector2(-12f, -4f);
            speechText.font = UIKidFriendlyStyle.GetSharedFont();
            speechText.fontSize = 22;
            speechText.alignment = TextAnchor.MiddleCenter;
            speechText.color = new Color(0.18f, 0.18f, 0.18f, 1f);
            speechText.raycastTarget = false;
            speechText.resizeTextForBestFit = true;
            speechText.resizeTextMinSize = 14;
            speechText.resizeTextMaxSize = 22;

            speechPanel.SetActive(false);
        }

        public void SetMascot(string emoji)
        {
            mascotEmoji = emoji;
            if (mascotText != null)
                mascotText.text = emoji;
        }

        public void SetSpeech(string text, float duration = -1f)
        {
            if (speechCoroutine != null)
                StopCoroutine(speechCoroutine);
            speechCoroutine = StartCoroutine(ShowSpeechCoroutine(text, duration > 0f ? duration : 4f));
        }

        public void HideSpeech()
        {
            if (speechPanel != null)
                speechPanel.SetActive(false);
        }

        public void PlayEntrance()
        {
            if (mascotText == null) return;
            mascotText.text = mascotEmoji;
            mascotText.fontSize = 1;
            if (mascotRect != null)
                basePosition = mascotRect.anchoredPosition;
            StopAllCoroutines();
            StartCoroutine(EntranceCoroutine());
        }

        private IEnumerator EntranceCoroutine()
        {
            float elapsed = 0f;
            float targetSize = 70f;

            while (elapsed < entranceDuration)
            {
                float t = elapsed / entranceDuration;
                t = 1f - Mathf.Pow(1f - t, 3f);
                if (mascotText != null)
                    mascotText.fontSize = Mathf.RoundToInt(Mathf.Lerp(1f, targetSize, t));
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            if (mascotText != null)
                mascotText.fontSize = Mathf.RoundToInt(targetSize);

            StartIdleAnimation();
        }

        private void StartIdleAnimation()
        {
            StopIdleAnimation();
            if (mascotRect != null)
                basePosition = mascotRect.anchoredPosition;
            idleAnimation = StartCoroutine(IdleBounceCoroutine());
        }

        private void StopIdleAnimation()
        {
            if (idleAnimation != null)
            {
                StopCoroutine(idleAnimation);
                idleAnimation = null;
            }
        }

        private IEnumerator IdleBounceCoroutine()
        {
            while (true)
            {
                if (mascotRect != null && mascotText != null)
                {
                    float bounce = Mathf.Sin(Time.unscaledTime * bounceSpeed * 2f * Mathf.PI) * bounceAmplitude;
                    mascotRect.anchoredPosition = basePosition + new Vector2(0f, bounce);

                    float pulse = 1f + Mathf.Sin(Time.unscaledTime * bounceSpeed * 2f * Mathf.PI) * 0.03f;
                    mascotText.fontSize = Mathf.RoundToInt(70f * pulse);
                }
                yield return null;
            }
        }

        private IEnumerator ShowSpeechCoroutine(string text, float duration)
        {
            if (speechPanel != null)
                speechPanel.SetActive(true);

            if (speechText != null)
            {
                speechText.text = text;
                speechText.color = new Color(speechText.color.r, speechText.color.g, speechText.color.b, 0f);
                float elapsed = 0f;
                float fadeIn = 0.2f;
                while (elapsed < fadeIn)
                {
                    float t = elapsed / fadeIn;
                    speechText.color = new Color(speechText.color.r, speechText.color.g, speechText.color.b, t);
                    elapsed += Time.unscaledDeltaTime;
                    yield return null;
                }
                speechText.color = new Color(speechText.color.r, speechText.color.g, speechText.color.b, 1f);
            }

            yield return new WaitForSecondsRealtime(duration);

            if (speechText != null)
            {
                float elapsed = 0f;
                float fadeOut = 0.3f;
                while (elapsed < fadeOut)
                {
                    float t = 1f - elapsed / fadeOut;
                    speechText.color = new Color(speechText.color.r, speechText.color.g, speechText.color.b, t);
                    elapsed += Time.unscaledDeltaTime;
                    yield return null;
                }
                speechText.color = new Color(speechText.color.r, speechText.color.g, speechText.color.b, 0f);
            }

            if (speechPanel != null)
                speechPanel.SetActive(false);
        }
    }
}
