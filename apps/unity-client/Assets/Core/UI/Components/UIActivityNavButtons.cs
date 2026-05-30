using UnityEngine;
using UnityEngine.UI;
using Core.UI.Localization;

namespace Core.UI.Components
{
    /// <summary>
    /// Shared navigation buttons for all activity games.
    /// Provides consistent positioning for Hint, Listen, and Home buttons.
    /// </summary>
    public static class UIActivityNavButtons
    {
        private static readonly Vector2 ButtonSize = new Vector2(146f, 64f);
        private static readonly Vector2 HintButtonTopLeft = new Vector2(24f, -24f);
        private static readonly Vector2 ListenButtonTopRight = new Vector2(-188f, -24f);
        private static readonly Vector2 HomeButtonTopRight = new Vector2(-24f, -24f);
        private static readonly int FontSize = 20;

        /// <summary>
        /// Create Hint button at top-left corner.
        /// </summary>
        public static Button CreateHintButton(Transform parent, UnityEngine.Events.UnityAction onClick)
        {
            return CreateHintButton(parent, SimpleLocalization.Get("btn_hint"), ButtonSize, FontSize, onClick);
        }

        /// <summary>
        /// Create Hint button at top-left corner with custom size and label.
        /// </summary>
        public static Button CreateHintButton(Transform parent, string label, Vector2 size, int fontSize, UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject("HintButton", typeof(RectTransform), typeof(Image), typeof(Button));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = size;
            rect.anchoredPosition = HintButtonTopLeft;
            go.GetComponent<Image>().color = new Color(0.98f, 0.8f, 0.2f, 1.0f); // Bright yellow

            var button = go.GetComponent<Button>();
            button.onClick.AddListener(onClick);

            CreateButtonLabel(go.transform, label, fontSize);
            UIKidFriendlyStyle.Apply(button, "HintButton", label, fontSize);
            return button;
        }

        /// <summary>
        /// Create Listen button at top-right area.
        /// </summary>
        public static Button CreateListenButton(Transform parent, UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject("ListenButton", typeof(RectTransform), typeof(Image), typeof(Button));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.sizeDelta = ButtonSize;
            rect.anchoredPosition = ListenButtonTopRight;
            go.GetComponent<Image>().color = new Color(0.2f, 0.5f, 0.9f, 0.92f);

            var button = go.GetComponent<Button>();
            button.onClick.AddListener(onClick);

            CreateButtonLabel(go.transform, SimpleLocalization.Get("btn_listen"));
            UIKidFriendlyStyle.Apply(button, "ListenButton", SimpleLocalization.Get("btn_listen"), FontSize);
            return button;
        }

        /// <summary>
        /// Create Home button at top-right corner.
        /// </summary>
        public static Button CreateHomeButton(Transform parent, UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject("HomeButton", typeof(RectTransform), typeof(Image), typeof(Button));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.sizeDelta = ButtonSize;
            rect.anchoredPosition = HomeButtonTopRight;
            go.GetComponent<Image>().color = new Color(0.85f, 0.35f, 0.35f, 0.9f);

            var button = go.GetComponent<Button>();
            button.onClick.AddListener(onClick);

            CreateButtonLabel(go.transform, "Trang chủ");
            UIKidFriendlyStyle.Apply(button, "HomeButton", "Trang chủ", FontSize);
            return button;
        }

        public static void ApplyStandardHomeButton(Button button)
        {
            if (button == null)
            {
                return;
            }

            RectTransform rect = button.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = new Vector2(1f, 1f);
                rect.anchorMax = new Vector2(1f, 1f);
                rect.pivot = new Vector2(1f, 1f);
                rect.sizeDelta = ButtonSize;
                rect.anchoredPosition = HomeButtonTopRight;
            }

            string label = SimpleLocalization.Get("btn_home");
            Text text = button.GetComponentInChildren<Text>(true);
            if (text != null)
            {
                text.text = label;
                text.fontSize = FontSize;
                text.resizeTextMinSize = FontSize - 6;
                text.resizeTextMaxSize = FontSize;
                text.alignment = TextAnchor.MiddleCenter;
            }

            UIKidFriendlyStyle.Apply(button, "HomeButton", label, FontSize);
        }

        private static void CreateButtonLabel(Transform parent, string label)
        {
            CreateButtonLabel(parent, label, FontSize);
        }

        private static void CreateButtonLabel(Transform parent, string label, int fontSize)
        {
            var go = new GameObject("Label", typeof(RectTransform), typeof(Text));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(8f, 4f);
            rect.offsetMax = new Vector2(-8f, -4f);

            var text = go.GetComponent<Text>();
            text.text = label;
            text.fontSize = fontSize;
            text.resizeTextMinSize = fontSize - 6;
            text.resizeTextMaxSize = fontSize;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
        }
    }
}
