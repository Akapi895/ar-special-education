using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Core.UI.Components
{
    public class UIFeedbackOverlay : MonoBehaviour
    {
        [Header("UI Panels & Groups")]
        [SerializeField] private GameObject overlayContainer;
        [SerializeField] private Image overlayBackground;
        [SerializeField] private RectTransform contentPanel;

        [Header("Feedback Graphics")]
        [SerializeField] private GameObject correctIcon;
        [SerializeField] private GameObject incorrectIcon;
        [SerializeField] private GameObject successIcon;

        [Header("Text References")]
        [SerializeField] private Text messageText;

        [Header("Visual Particles (Optional)")]
        [SerializeField] private ParticleSystem confettiParticleSystem;
        [SerializeField] private ParticleSystem successParticleSystem;

        [Header("Settings")]
        [SerializeField] private float autoHideDelay = 1.8f;
        [SerializeField] private float shakeDuration = 0.5f;
        [SerializeField] private float shakeAmount = 25f;
        [SerializeField] private Vector2 childFriendlyPanelSize = new Vector2(820f, 150f);
        [SerializeField] private Vector2 childFriendlyPanelPosition = new Vector2(0f, 0f);  // Centered
        [SerializeField] private int childFriendlyMessageFontSize = 36;

        private Vector2 originalContentPosition;
        private Coroutine activeOverlayCoroutine;
        private Coroutine shakeCoroutine;

        private void Awake()
        {
            if (contentPanel != null)
            {
                originalContentPosition = contentPanel.anchoredPosition;
            }
            Hide();
        }

        public void ShowCorrect(string message)
        {
            PrepareDisplay(message);

            if (correctIcon != null) correctIcon.SetActive(true);
            
            // Soft green background glow
            if (overlayBackground != null)
            {
                overlayBackground.color = new Color(0.4f, 0.8f, 0.5f, 0.18f);
            }

            if (confettiParticleSystem != null)
            {
                confettiParticleSystem.gameObject.SetActive(true);
                confettiParticleSystem.Play();
            }
            else
            {
                SpawnProceduralConfetti();
            }

            TriggerEntranceAnimation();
            StartAutoHide(autoHideDelay);
        }

        public void ShowIncorrect(string message)
        {
            PrepareDisplay(message);

            if (incorrectIcon != null) incorrectIcon.SetActive(true);

            // Soft orange background glow (not harsh red)
            if (overlayBackground != null)
            {
                overlayBackground.color = new Color(1.0f, 0.8f, 0.5f, 0.2f);
            }

            TriggerEntranceAnimation();
            TriggerShakeAnimation(12f); // softer shake for kids
            StartAutoHide(1.2f); // hide faster so they can retry
        }

        public void ShowSuccess(string message)
        {
            PrepareDisplay(message);

            if (successIcon != null) successIcon.SetActive(true);

            // Warm yellow/gold background glow
            if (overlayBackground != null)
            {
                overlayBackground.color = new Color(0.98f, 0.85f, 0.35f, 0.22f);
            }

            if (successParticleSystem != null)
            {
                successParticleSystem.gameObject.SetActive(true);
                successParticleSystem.Play();
            }

            TriggerEntranceAnimation();
            // Don't auto-hide success panel, let user click next manually
        }

        public void Hide()
        {
            StopAllActiveCoroutines();

            if (confettiParticleSystem != null) confettiParticleSystem.Stop();
            if (successParticleSystem != null) successParticleSystem.Stop();

            if (correctIcon != null) correctIcon.SetActive(false);
            if (incorrectIcon != null) incorrectIcon.SetActive(false);
            if (successIcon != null) successIcon.SetActive(false);

            if (overlayContainer != null)
            {
                overlayContainer.SetActive(false);
            }

            if (contentPanel != null)
            {
                contentPanel.anchoredPosition = originalContentPosition;
            }
        }

        private void PrepareDisplay(string message)
        {
            StopAllActiveCoroutines();

            if (correctIcon != null) correctIcon.SetActive(false);
            if (incorrectIcon != null) incorrectIcon.SetActive(false);
            if (successIcon != null) successIcon.SetActive(false);

            if (messageText != null)
            {
                messageText.text = message;
                messageText.fontSize = childFriendlyMessageFontSize;
                messageText.resizeTextForBestFit = true;
                messageText.resizeTextMinSize = 22;
                messageText.resizeTextMaxSize = childFriendlyMessageFontSize;
                messageText.alignment = TextAnchor.MiddleCenter;
                messageText.color = Color.white;
            }

            if (overlayContainer != null)
            {
                overlayContainer.SetActive(true);
            }

            if (overlayBackground != null)
            {
                overlayBackground.raycastTarget = false;
            }

            if (contentPanel != null)
            {
                contentPanel.anchorMin = new Vector2(0.5f, 0.5f);
                contentPanel.anchorMax = new Vector2(0.5f, 0.5f);
                contentPanel.pivot = new Vector2(0.5f, 0.5f);
                contentPanel.sizeDelta = childFriendlyPanelSize;
                originalContentPosition = childFriendlyPanelPosition;
                contentPanel.anchoredPosition = originalContentPosition;
            }
        }

        private void StartAutoHide(float delay)
        {
            activeOverlayCoroutine = StartCoroutine(AutoHideCoroutine(delay));
        }

        private IEnumerator AutoHideCoroutine(float delay)
        {
            yield return new WaitForSeconds(delay);
            Hide();
            activeOverlayCoroutine = null;
        }

        private void TriggerEntranceAnimation()
        {
            bool animationsEnabled = PlayerPrefs.GetInt("AnimationsEnabled", 1) == 1;
            if (!animationsEnabled || contentPanel == null) return;

            StartCoroutine(EntranceScaleCoroutine());
        }

        private IEnumerator EntranceScaleCoroutine()
        {
            contentPanel.localScale = Vector3.zero;
            float elapsed = 0f;
            float duration = 0.3f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                
                // Elastic bounce effect (0 -> 1.2 -> 1.0)
                float scaleVal;
                if (t < 0.7f)
                {
                    scaleVal = Mathf.Lerp(0f, 1.2f, t / 0.7f);
                }
                else
                {
                    scaleVal = Mathf.Lerp(1.2f, 1.0f, (t - 0.7f) / 0.3f);
                }
                
                contentPanel.localScale = new Vector3(scaleVal, scaleVal, 1f);
                yield return null;
            }
            contentPanel.localScale = Vector3.one;
        }

        private void TriggerShakeAnimation(float customAmount = -1f)
        {
            bool animationsEnabled = PlayerPrefs.GetInt("AnimationsEnabled", 1) == 1;
            if (!animationsEnabled || contentPanel == null) return;

            if (shakeCoroutine != null) StopCoroutine(shakeCoroutine);
            float amount = customAmount >= 0f ? customAmount : shakeAmount;
            shakeCoroutine = StartCoroutine(ShakeCoroutine(amount));
        }

        private IEnumerator ShakeCoroutine(float amount)
        {
            float elapsed = 0f;

            while (elapsed < shakeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float percent = elapsed / shakeDuration;
                // Decrease shake magnitude over time
                float damperedAmount = amount * (1f - percent);
                float offsetX = Random.Range(-1f, 1f) * damperedAmount;
                float offsetY = Random.Range(-1f, 1f) * damperedAmount;

                contentPanel.anchoredPosition = originalContentPosition + new Vector2(offsetX, offsetY);
                yield return null;
            }

            contentPanel.anchoredPosition = originalContentPosition;
            shakeCoroutine = null;
        }

        private void SpawnProceduralConfetti()
        {
            var confettiGo = new GameObject("ProceduralConfetti", typeof(ParticleSystem));
            confettiGo.transform.SetParent(transform, false);
            confettiGo.transform.localPosition = Vector3.zero;

            var particles = confettiGo.GetComponent<ParticleSystem>();
            var main = particles.main;
            main.playOnAwake = false;
            main.duration = 0.8f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1.0f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.15f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.maxParticles = 40;
            main.startColor = new ParticleSystem.MinMaxGradient(
                new Color(1f, 0.9f, 0.2f),
                new Color(0.2f, 1f, 0.4f));
            main.gravityModifier = -0.03f;

            var emission = particles.emission;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)30) });

            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.3f;

            var velocity = particles.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.Local;
            velocity.y = new ParticleSystem.MinMaxCurve(0.2f, 0.6f);

            var sizeOverLifetime = particles.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 1f, 1f, 0f));

            var renderer = particles.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.material = Resources.GetBuiltinResource<Material>("Sprites-Default.mat");

            particles.Play();
            Destroy(confettiGo, 3f);
        }

        private void StopAllActiveCoroutines()
        {
            if (activeOverlayCoroutine != null)
            {
                StopCoroutine(activeOverlayCoroutine);
                activeOverlayCoroutine = null;
            }
            if (shakeCoroutine != null)
            {
                StopCoroutine(shakeCoroutine);
                shakeCoroutine = null;
            }
        }

        private void OnDisable()
        {
            StopAllActiveCoroutines();
        }
    }
}
