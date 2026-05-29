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
        private Vector3 originalScale;
        private Coroutine activeScaleCoroutine;

        private void Awake()
        {
            button = GetComponent<Button>();
            buttonImage = GetComponent<Image>();
            originalScale = transform.localScale;

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
                StartScaleTransition(eventData.dragging ? originalScale : originalScale * hoverScale);
            }
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
            activeScaleCoroutine = StartCoroutine(ScaleCoroutine(targetScale));
        }

        private IEnumerator ScaleCoroutine(Vector3 targetScale)
        {
            Vector3 startScale = transform.localScale;
            float elapsedTime = 0f;

            while (elapsedTime < animationDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsedTime / animationDuration);
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
            transform.localScale = originalScale;
        }
    }
}
