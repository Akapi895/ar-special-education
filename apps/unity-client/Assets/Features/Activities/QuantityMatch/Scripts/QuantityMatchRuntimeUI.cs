using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Features.Activities.QuantityMatch
{
    /// <summary>
    /// Builds a minimal runtime UI when serialized references on <see cref="QuantityMatchView"/> are missing.
    /// </summary>
    [RequireComponent(typeof(QuantityMatchView))]
    public class QuantityMatchRuntimeUI : MonoBehaviour
    {
        private void Awake()
        {
            var view = GetComponent<QuantityMatchView>();
            if (view == null || view.HasUiReferences)
            {
                return;
            }

            view.BuildRuntimeUi(CreateCanvas(transform));
        }

        private static Canvas CreateCanvas(Transform parent)
        {
            var canvasGo = new GameObject("QuantityMatchCanvas");
            canvasGo.transform.SetParent(parent, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            if (FindAnyObjectByType<EventSystem>() == null)
            {
                var eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<EventSystem>();
                eventSystem.AddComponent<InputSystemUIInputModule>();
            }

            return canvas;
        }
    }
}
