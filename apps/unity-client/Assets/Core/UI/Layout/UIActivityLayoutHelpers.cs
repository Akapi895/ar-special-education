using Core.UI.Components;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Core.UI.Layout
{
    public static class UIActivityLayoutHelpers
    {
        public static RectTransform CreateUiPanel(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return rect;
        }

        public static Text CreateTopText(Transform parent, string name, string content, int fontSize, float topOffset, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = size;
            rect.anchoredPosition = new Vector2(0f, -topOffset);

            var text = go.GetComponent<Text>();
            text.text = content;
            text.fontSize = fontSize;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 14;
            text.resizeTextMaxSize = fontSize;
            text.alignment = TextAnchor.MiddleCenter;
            text.font = UIKidFriendlyStyle.GetSharedFont();
            text.color = Color.white;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

        public static GameObject CreateSubPanel(Transform parent, string name, Vector2 anchoredPosition, Vector2 size, bool anchorToBottom = true)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);

            if (anchorToBottom)
            {
                rect.anchorMin = new Vector2(0.5f, 0f);
                rect.anchorMax = new Vector2(0.5f, 0f);
            }

            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;

            var image = go.GetComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0.55f);
            image.raycastTarget = false;
            return go;
        }

        public static Text CreatePanelText(Transform parent, string name, string content, int fontSize)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(16f, 6f);
            rect.offsetMax = new Vector2(-16f, -6f);

            var text = go.GetComponent<Text>();
            text.text = content;
            text.fontSize = fontSize;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 14;
            text.resizeTextMaxSize = fontSize;
            text.alignment = TextAnchor.MiddleCenter;
            text.font = UIKidFriendlyStyle.GetSharedFont();
            text.color = Color.white;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

        public static Button CreateButton(Transform parent, string name, string label, Vector2 anchoredPosition, UnityEngine.Events.UnityAction onClick, Vector2 buttonSize)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.sizeDelta = buttonSize;
            rect.anchoredPosition = anchoredPosition;
            go.GetComponent<Image>().color = new Color(0.2f, 0.5f, 0.9f, 1f);

            var button = go.GetComponent<Button>();
            button.onClick.AddListener(onClick);

            CreateButtonLabel(go.transform, label);
            UI.Components.UIKidFriendlyStyle.Apply(button, name, label, 26);
            return button;
        }

        public static Text CreateButtonLabel(Transform parent, string label)
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
            text.fontSize = 22;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 14;
            text.resizeTextMaxSize = 22;
            text.alignment = TextAnchor.MiddleCenter;
            text.font = UIKidFriendlyStyle.GetSharedFont();
            text.color = Color.white;
            text.raycastTarget = false;
            return text;
        }

        public static void SetButtonLabel(Button button, string label)
        {
            if (button == null)
            {
                return;
            }

            Text text = button.GetComponentInChildren<Text>();
            if (text != null)
            {
                text.text = label;
            }
        }

        public static void SetButtonActive(Button button, bool active)
        {
            if (button != null)
            {
                button.gameObject.SetActive(active);
            }
        }

        public static void SetPanelActive(GameObject panel, bool active)
        {
            if (panel != null)
            {
                panel.SetActive(active);
            }
        }

        public static GameObject CreateCardBackground(Transform parent, string name, Vector2 size, Vector2 anchoredPosition, float cornerRadius = 24f)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(RoundedRectGraphic));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size + new Vector2(18f, 12f);
            rect.anchoredPosition = anchoredPosition;

            var graphic = go.GetComponent<RoundedRectGraphic>();
            graphic.CornerRadius = cornerRadius;
            graphic.color = new Color(1f, 1f, 1f, 0.76f);
            graphic.raycastTarget = false;

            var shadow = go.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.12f);
            shadow.effectDistance = new Vector2(0f, -4f);
            shadow.useGraphicAlpha = true;

            go.transform.SetAsFirstSibling();
            return go;
        }

        public static void LoadSceneIfAvailable(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                return;
            }

            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.LogWarning($"[LayoutHelpers] Scene '{sceneName}' is not available in Build Settings.");
                return;
            }

            SceneManager.LoadScene(sceneName);
        }
    }
}
