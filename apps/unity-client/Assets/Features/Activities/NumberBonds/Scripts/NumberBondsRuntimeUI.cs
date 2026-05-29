using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Features.Activities.NumberBonds
{
    public class NumberBondsRuntimeUI : MonoBehaviour
    {
        [SerializeField]
        private NumberBondsView view;

        private void Awake()
        {
            if (view == null)
            {
                view = GetComponent<NumberBondsView>();
            }

            CreateRuntimeUI();
        }

        private void CreateRuntimeUI()
        {
            if (view == null || view.HasUiReferences)
            {
                return;
            }

            view.BuildRuntimeUi(CreateCanvas(transform));
        }

        private static Canvas CreateCanvas(Transform parent)
        {
            var go = new GameObject("NumberBondsRuntimeCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.transform.SetParent(parent, false);
            Canvas canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 20;

            CanvasScaler scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            EnsureEventSystem();
            return canvas;
        }

        private static void EnsureEventSystem()
        {
            if (EventSystem.current != null)
            {
                return;
            }

            var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            EventSystem.current = eventSystem.GetComponent<EventSystem>();
        }
    }
}
