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
            EnsureUI();
            PlayEntrance();
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
                Debug.LogError("[MascotDisplay] No canvas found in scene.");
                return;
            }

            Transform root = canvas.transform;

            var mascotObj = new GameObject("MascotEmoji", typeof(RectTransform), typeof(Text));
            mascotObj.transform.SetParent(root, false);
            mascotRect = mascotObj.GetComponent<RectTransform>();
            mascotRect.anchorMin = new Vector2(0.5f, 0.5f);
            mascotRect.anchorMax = new Vector2(0.5f, 0.5f);
            mascotRect.pivot = new Vector2(0.5f, 0.5f);
            mascotRect.sizeDelta = new Vector2(120f, 120f);
            mascotRect.anchoredPosition = new Vector2(0f, 120f);

            mascotText = mascotObj.GetComponent<Text>();
            mascotText.text = mascotEmoji;
            mascotText.fontSize = 80;
            mascotText.alignment = TextAnchor.MiddleCenter;
            mascotText.font = UIKidFriendlyStyle.GetSharedFont();
            mascotText.raycastTarget = false;
            mascotText.color = Color.white;

            speechPanel = new GameObject("MascotSpeechBubble", typeof(RectTransform), typeof(Image));
            var speechRect = speechPanel.GetComponent<RectTransform>();
            speechRect.SetParent(root, false);
            speechRect.anchorMin = new Vector2(0.5f, 0.5f);
            speechRect.anchorMax = new Vector2(0.5f, 0.5f);
            speechRect.pivot = new Vector2(0.5f, 1f);
            speechRect.sizeDelta = new Vector2(340f, 70f);
            speechRect.anchoredPosition = new Vector2(0f, 210f);

            var speechImage = speechPanel.GetComponent<Image>();
            speechImage.color = new Color(1f, 1f, 1f, 0.88f);
            speechImage.raycastTarget = false;
            var rounded = speechPanel.AddComponent<Core.UI.Components.RoundedRectGraphic>();
            rounded.CornerRadius = 20f;
            rounded.color = new Color(1f, 1f, 1f, 0.88f);
            rounded.raycastTarget = false;
            DestroyImmediate(speechImage);

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
            float targetSize = 80f;

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
                    mascotText.fontSize = Mathf.RoundToInt(80f * pulse);
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
