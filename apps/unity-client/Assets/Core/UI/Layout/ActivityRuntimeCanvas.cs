using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Core.UI.Layout
{
    public static class ActivityRuntimeCanvas
    {
        public const float DefaultWidth = 1920f;
        public const float DefaultHeight = 1080f;

        public static Canvas Create(Transform parent, string name, float referenceWidth = DefaultWidth, float referenceHeight = DefaultHeight)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.transform.SetParent(parent, false);

            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 20;

            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.referenceResolution = new Vector2(referenceWidth, referenceHeight);
            scaler.matchWidthOrHeight = 0.5f;

            EnsureEventSystem();
            return canvas;
        }

        public static void EnsureEventSystem()
        {
            if (EventSystem.current != null)
            {
                return;
            }

            var eventSystemGO = new GameObject("EventSystem");
            eventSystemGO.AddComponent<EventSystem>();

            bool inputSystemAdded = false;
            try
            {
                eventSystemGO.AddComponent<InputSystemUIInputModule>();
                inputSystemAdded = true;
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[ActivityRuntimeCanvas] InputSystemUIInputModule not available ({ex.Message}). Falling back to StandaloneInputModule.");
            }

            if (!inputSystemAdded)
            {
                eventSystemGO.AddComponent<StandaloneInputModule>();
            }

            EventSystem.current = eventSystemGO.GetComponent<EventSystem>();
        }
    }
}
