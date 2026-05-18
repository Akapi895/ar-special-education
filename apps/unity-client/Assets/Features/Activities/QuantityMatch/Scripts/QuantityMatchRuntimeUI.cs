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

            view.BuildRuntimeUi(CreateCanvas());
        }

        private Canvas CreateCanvas()
        {
            var canvasGo = new GameObject("QuantityMatchCanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
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
