using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Core.UI.Components
{
    [RequireComponent(typeof(CanvasGroup))]
    public class UIHintBubble : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Text hintText;
        [SerializeField] private RectTransform bubblePanel;

        [Header("Settings")]
        [SerializeField] private float displayDuration = 5f;
        [SerializeField] private float slideDistance = 100f;
        [SerializeField] private float transitionDuration = 0.3f;

        private CanvasGroup canvasGroup;
        private Vector2 originalAnchoredPosition;
        private Coroutine activeTransition;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (bubblePanel != null)
            {
                originalAnchoredPosition = bubblePanel.anchoredPosition;
            }
            HideImmediate();
        }

        public void Show(string text)
        {
            if (hintText != null)
            {
                hintText.text = text;
            }

            gameObject.SetActive(true);

            if (activeTransition != null)
            {
                StopCoroutine(activeTransition);
            }

            activeTransition = StartCoroutine(ShowCoroutine());
        }

        public void Hide()
        {
            if (activeTransition != null)
            {
                StopCoroutine(activeTransition);
            }

            activeTransition = StartCoroutine(HideCoroutine());
        }

        private void HideImmediate()
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
            if (bubblePanel != null)
            {
                bubblePanel.anchoredPosition = originalAnchoredPosition - new Vector2(0, slideDistance);
            }
            gameObject.SetActive(false);
        }

        private IEnumerator ShowCoroutine()
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;

            float elapsed = 0f;
            Vector2 startPos = originalAnchoredPosition - new Vector2(0, slideDistance);
            
            bool animationsEnabled = PlayerPrefs.GetInt("AnimationsEnabled", 1) == 1;
            if (!animationsEnabled)
            {
                canvasGroup.alpha = 1f;
                if (bubblePanel != null) bubblePanel.anchoredPosition = originalAnchoredPosition;
            }
            else
            {
                while (elapsed < transitionDuration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    float t = Mathf.Clamp01(elapsed / transitionDuration);
                    t = t * t * (3f - 2f * t); // smoothstep

                    canvasGroup.alpha = t;
                    if (bubblePanel != null)
                    {
                        bubblePanel.anchoredPosition = Vector2.Lerp(startPos, originalAnchoredPosition, t);
                    }
                    yield return null;
                }
                canvasGroup.alpha = 1f;
                if (bubblePanel != null) bubblePanel.anchoredPosition = originalAnchoredPosition;
            }

            yield return new WaitForSeconds(displayDuration);
            activeTransition = StartCoroutine(HideCoroutine());
        }

        private IEnumerator HideCoroutine()
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;

            float elapsed = 0f;
            Vector2 endPos = originalAnchoredPosition - new Vector2(0, slideDistance);
            
            bool animationsEnabled = PlayerPrefs.GetInt("AnimationsEnabled", 1) == 1;
            if (!animationsEnabled)
            {
                HideImmediate();
                yield break;
            }

            while (elapsed < transitionDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / transitionDuration);
                t = t * t * (3f - 2f * t); // smoothstep

                canvasGroup.alpha = 1f - t;
                if (bubblePanel != null)
                {
                    bubblePanel.anchoredPosition = Vector2.Lerp(originalAnchoredPosition, endPos, t);
                }
                yield return null;
            }

            HideImmediate();
            activeTransition = null;
        }

        private void OnDisable()
        {
            if (activeTransition != null)
            {
                StopCoroutine(activeTransition);
                activeTransition = null;
            }
        }
    }
}
