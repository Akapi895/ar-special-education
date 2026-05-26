using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Core.UI.Components
{
    public class UIProgressBar : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image fillImage;
        [SerializeField] private Text progressPercentageText;

        [Header("Settings")]
        [SerializeField] private float transitionDuration = 0.3f;
        [SerializeField] private bool smoothTransitions = true;

        private float currentProgress = 0f;
        private Coroutine fillCoroutine;

        public void SetProgress(float targetProgress)
        {
            targetProgress = Mathf.Clamp01(targetProgress);

            if (fillCoroutine != null)
            {
                StopCoroutine(fillCoroutine);
            }

            if (smoothTransitions && gameObject.activeInHierarchy)
            {
                fillCoroutine = StartCoroutine(SmoothFillCoroutine(targetProgress));
            }
            else
            {
                UpdateFillAmount(targetProgress);
            }
        }

        private void UpdateFillAmount(float value)
        {
            currentProgress = value;

            if (fillImage != null)
            {
                // Support both filled image type and simple horizontal scaling
                if (fillImage.type == Image.Type.Filled)
                {
                    fillImage.fillAmount = value;
                }
                else
                {
                    // Scale width
                    RectTransform rect = fillImage.rectTransform;
                    rect.anchorMax = new Vector2(value, rect.anchorMax.y);
                }
            }

            if (progressPercentageText != null)
            {
                progressPercentageText.text = $"{Mathf.RoundToInt(value * 100f)}%";
            }
        }

        private IEnumerator SmoothFillCoroutine(float targetProgress)
        {
            float startProgress = currentProgress;
            float elapsedTime = 0f;

            // Check if animations are enabled via user preferences
            bool animationsEnabled = PlayerPrefs.GetInt("AnimationsEnabled", 1) == 1;
            if (!animationsEnabled)
            {
                UpdateFillAmount(targetProgress);
                yield break;
            }

            while (elapsedTime < transitionDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsedTime / transitionDuration);
                // Ease out cubic
                t = 1f - Mathf.Pow(1f - t, 3f);
                
                float interpolated = Mathf.Lerp(startProgress, targetProgress, t);
                UpdateFillAmount(interpolated);
                yield return null;
            }

            UpdateFillAmount(targetProgress);
            fillCoroutine = null;
        }

        private void OnDisable()
        {
            if (fillCoroutine != null)
            {
                StopCoroutine(fillCoroutine);
                fillCoroutine = null;
            }
        }
    }
}
