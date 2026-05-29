using System.Collections;
using Core.Data;
using Core.Support.AudioManager;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Core.UI.Components
{
    [RequireComponent(typeof(Button))]
    public class UIButtonKids : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Kid-Friendly Settings")]
        [SerializeField] private float hoverScale = 1.08f;
        [SerializeField] private float pressScale = 0.95f;
        [SerializeField] private float animationDuration = 0.15f;
        [SerializeField] private float clickPadding = -20f; // Expand raycast target by 20px on all sides

        [Header("Audio Settings")]
        [SerializeField] private string clickSoundName = "click";

        private Button button;
        private Image buttonImage;
        private RectTransform rectTransform;
        private Shadow feedbackShadow;
        private Vector3 originalScale;
        private Vector2 originalAnchoredPosition;
        private Color originalShadowColor;
        private Vector2 originalShadowDistance;
        private Coroutine activeScaleCoroutine;
        private Coroutine activeFeedbackCoroutine;

        private void Awake()
        {
            button = GetComponent<Button>();
            buttonImage = GetComponent<Image>();
            rectTransform = GetComponent<RectTransform>();
            feedbackShadow = GetComponentInChildren<Shadow>(true);
            originalScale = transform.localScale;
            originalAnchoredPosition = rectTransform != null ? rectTransform.anchoredPosition : Vector2.zero;
            if (feedbackShadow != null)
            {
                originalShadowColor = feedbackShadow.effectColor;
                originalShadowDistance = feedbackShadow.effectDistance;
            }

            // Expand raycast target area for kid-friendly large touch targets
            if (buttonImage != null)
            {
                buttonImage.raycastPadding = new Vector4(clickPadding, clickPadding, clickPadding, clickPadding);
            }

            // Hook click listener to play audio
            button.onClick.AddListener(PlayClickSound);
        }

        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(PlayClickSound);
            }
        }

        private void PlayClickSound()
        {
            #if UNITY_EDITOR
            Debug.Log($"[UIButtonKids] Click sound triggered: {clickSoundName}");
            #endif

            SimpleAudioManager.EnsureExists().PlaySound(clickSoundName);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (button.interactable)
            {
                StartScaleTransition(originalScale * hoverScale);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            StartScaleTransition(originalScale);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (button.interactable)
            {
                StartScaleTransition(originalScale * pressScale);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (button.interactable)
            {
                StartScaleTransition(originalScale);
            }
        }

        public void PlayCorrectFeedback()
        {
            StartFeedbackCoroutine(CorrectFeedbackCoroutine());
        }

        public void PlayWrongFeedback()
        {
            StartFeedbackCoroutine(WrongFeedbackCoroutine());
        }

        private void StartFeedbackCoroutine(IEnumerator routine)
        {
            if (!UserPreferences.AnimationsEnabled)
            {
                return;
            }

            if (activeFeedbackCoroutine != null)
            {
                StopCoroutine(activeFeedbackCoroutine);
            }

            if (activeScaleCoroutine != null)
            {
                StopCoroutine(activeScaleCoroutine);
                activeScaleCoroutine = null;
            }

            activeFeedbackCoroutine = StartCoroutine(routine);
        }

        private IEnumerator CorrectFeedbackCoroutine()
        {
            Shadow shadow = feedbackShadow != null ? feedbackShadow : GetComponentInChildren<Shadow>(true);
            if (shadow != null)
            {
                shadow.effectColor = new Color(0.32f, 0.9f, 0.35f, 0.65f);
                shadow.effectDistance = new Vector2(0f, -10f);
            }

            yield return ScaleCoroutine(originalScale * 1.12f, 0.12f);
            yield return ScaleCoroutine(originalScale * 0.98f, 0.08f);
            yield return ScaleCoroutine(originalScale, 0.12f);

            if (shadow != null)
            {
                shadow.effectColor = originalShadowColor;
                shadow.effectDistance = originalShadowDistance;
            }

            activeFeedbackCoroutine = null;
        }

        private IEnumerator WrongFeedbackCoroutine()
        {
            if (rectTransform == null)
            {
                yield break;
            }

            const float duration = 0.34f;
            const float amplitude = 12f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float shake = Mathf.Sin(t * Mathf.PI * 6f) * amplitude * (1f - t);
                rectTransform.anchoredPosition = originalAnchoredPosition + Vector2.right * shake;
                yield return null;
            }

            rectTransform.anchoredPosition = originalAnchoredPosition;
            activeFeedbackCoroutine = null;
        }

        private void StartScaleTransition(Vector3 targetScale)
        {
            if (!UserPreferences.AnimationsEnabled)
            {
                transform.localScale = targetScale;
                return;
            }

            if (activeScaleCoroutine != null)
            {
                StopCoroutine(activeScaleCoroutine);
            }
            activeScaleCoroutine = StartCoroutine(ScaleCoroutine(targetScale, animationDuration));
        }

        private IEnumerator ScaleCoroutine(Vector3 targetScale, float duration)
        {
            Vector3 startScale = transform.localScale;
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsedTime / duration);
                t = t * t * (3f - 2f * t);
                transform.localScale = Vector3.Lerp(startScale, targetScale, t);
                yield return null;
            }

            transform.localScale = targetScale;
            activeScaleCoroutine = null;
        }

        private void OnEnable()
        {
            // Reset to original scale when enabled
            transform.localScale = originalScale;
        }

        private void OnDisable()
        {
            if (activeScaleCoroutine != null)
            {
                StopCoroutine(activeScaleCoroutine);
                activeScaleCoroutine = null;
            }

            if (activeFeedbackCoroutine != null)
            {
                StopCoroutine(activeFeedbackCoroutine);
                activeFeedbackCoroutine = null;
            }

            transform.localScale = originalScale;
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = originalAnchoredPosition;
            }
        }
    }
}
