using System;
using System.Collections;
using UnityEngine;
using Core.UI.DesignSystem;

namespace Core.UI.Navigation
{
    [RequireComponent(typeof(CanvasGroup))]
    public class UIScreen : MonoBehaviour
    {
        [Header("Screen Settings")]
        [SerializeField] protected UIDesignTokens designTokens;
        [SerializeField] private bool showOnStart = false;

        protected CanvasGroup canvasGroup;
        protected Coroutine activeTransitionCoroutine;

        public event Action OnEnter;
        public event Action OnExit;

        protected virtual void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            
            // Ensure default tokens if not set
            if (designTokens == null)
            {
                designTokens = Resources.Load<UIDesignTokens>("SO_UIDesignTokens_Default");
            }
        }

        protected virtual void Start()
        {
            if (showOnStart)
            {
                ShowImmediate();
            }
            else
            {
                HideImmediate();
            }
        }

        public virtual void Show(float duration = 0.3f, Action onComplete = null)
        {
            if (activeTransitionCoroutine != null) StopCoroutine(activeTransitionCoroutine);
            
            gameObject.SetActive(true);
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;

            bool animationsEnabled = PlayerPrefs.GetInt("AnimationsEnabled", 1) == 1;
            if (!animationsEnabled || duration <= 0)
            {
                ShowImmediate();
                onComplete?.Invoke();
                return;
            }

            activeTransitionCoroutine = StartCoroutine(FadeCoroutine(0f, 1f, duration, () => {
                OnScreenEnter();
                onComplete?.Invoke();
            }));
        }

        public virtual void Hide(float duration = 0.3f, Action onComplete = null)
        {
            if (activeTransitionCoroutine != null) StopCoroutine(activeTransitionCoroutine);

            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;

            bool animationsEnabled = PlayerPrefs.GetInt("AnimationsEnabled", 1) == 1;
            if (!animationsEnabled || duration <= 0)
            {
                HideImmediate();
                onComplete?.Invoke();
                return;
            }

            activeTransitionCoroutine = StartCoroutine(FadeCoroutine(1f, 0f, duration, () => {
                gameObject.SetActive(false);
                OnScreenExit();
                onComplete?.Invoke();
            }));
        }

        public void ShowImmediate()
        {
            if (activeTransitionCoroutine != null) StopCoroutine(activeTransitionCoroutine);
            gameObject.SetActive(true);
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
            OnScreenEnter();
        }

        public void HideImmediate()
        {
            if (activeTransitionCoroutine != null) StopCoroutine(activeTransitionCoroutine);
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
            gameObject.SetActive(false);
            OnScreenExit();
        }

        protected virtual void OnScreenEnter()
        {
            OnEnter?.Invoke();
        }

        protected virtual void OnScreenExit()
        {
            OnExit?.Invoke();
        }

        private IEnumerator FadeCoroutine(float startAlpha, float endAlpha, float duration, Action onComplete)
        {
            canvasGroup.alpha = startAlpha;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                // Smooth step
                t = t * t * (3f - 2f * t);
                canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
                yield return null;
            }

            canvasGroup.alpha = endAlpha;
            activeTransitionCoroutine = null;
            onComplete?.Invoke();
        }
    }
}
