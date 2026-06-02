using UnityEngine;
using UnityEngine.UI;

namespace Core.UI.Components
{
    public enum KidButtonPurpose
    {
        Primary,
        Secondary,
        Confirm,
        Hint,
        Home,
        Retry,
        Progress,
        Listen,
        ComparisonMore,
        ComparisonFewer,
        ComparisonEqual,
        Neutral
    }

    public static class UIKidFriendlyStyle
    {
        private const string VisualName = "KidButtonVisual";
        private const string DropFillName = "KidDropZoneFill";
        private const string DropBorderName = "KidDropZoneDash";

        private static readonly Color TextDark = new Color(0.16f, 0.12f, 0.08f, 1f);
        private static readonly Color Primary = new Color(1f, 0.75f, 0.23f, 1f);
        private static readonly Color Secondary = new Color(0.66f, 0.88f, 1f, 1f);
        private static readonly Color Confirm = new Color(0.48f, 0.83f, 0.43f, 1f);
        private static readonly Color Hint = new Color(1f, 0.88f, 0.28f, 1f);
        private static readonly Color Home = new Color(1f, 0.54f, 0.58f, 1f);
        private static readonly Color Retry = new Color(0.77f, 0.79f, 0.86f, 1f);
        private static readonly Color CompareMore = new Color(1f, 0.66f, 0.32f, 1f);
        private static readonly Color CompareFewer = new Color(0.58f, 0.78f, 1f, 1f);
        private static readonly Color CompareEqual = new Color(0.58f, 0.88f, 0.54f, 1f);

        private static Font childFont;

        public static void Apply(Button button, KidButtonPurpose purpose, string label = null, int fontSize = 28, bool square = false)
        {
            if (button == null)
            {
                return;
            }

            RectTransform rect = button.GetComponent<RectTransform>();
            Color normalColor = GetColor(purpose);
            Image image = button.GetComponent<Image>();
            if (image != null)
            {
                image.color = normalColor;
                image.raycastTarget = true;
            }

            RoundedRectGraphic background = EnsureButtonBackground(button.transform);
            background.color = normalColor;
            background.CornerRadius = ResolveRadius(rect, square);
            background.raycastTarget = false;

            Shadow shadow = background.GetComponent<Shadow>();
            if (shadow == null)
            {
                shadow = background.gameObject.AddComponent<Shadow>();
            }

            shadow.effectColor = new Color(0.14f, 0.11f, 0.08f, 0.18f);
            shadow.effectDistance = new Vector2(0f, -6f);
            shadow.useGraphicAlpha = true;

            Outline outline = background.GetComponent<Outline>();
            if (outline == null)
            {
                outline = background.gameObject.AddComponent<Outline>();
            }
            outline.effectColor = new Color(1f, 1f, 1f, 0.20f);
            outline.effectDistance = new Vector2(2f, -2f);
            outline.useGraphicAlpha = true;

            EnsureCardBackground(button.transform, rect);
            button.targetGraphic = background;
            button.transition = Selectable.Transition.ColorTint;
            button.colors = CreateColorBlock(normalColor);

            if (button.GetComponent<UIButtonKids>() == null)
            {
                button.gameObject.AddComponent<UIButtonKids>();
            }

            Text text = EnsureButtonLabel(button.transform);
            string content = string.IsNullOrWhiteSpace(label) ? text.text : label;
            text.text = DecorateLabel(purpose, content);
            text.font = GetChildFont();
            int effectiveFontSize = purpose == KidButtonPurpose.Home
                ? Mathf.Max(16, fontSize - 2)
                : fontSize;
            text.fontSize = effectiveFontSize;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = purpose == KidButtonPurpose.Home
                ? Mathf.Max(12, effectiveFontSize - 8)
                : Mathf.Max(16, effectiveFontSize - 14);
            text.resizeTextMaxSize = effectiveFontSize;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = GetTextColor(purpose);
            text.raycastTarget = false;
            text.horizontalOverflow = purpose == KidButtonPurpose.Home
                ? HorizontalWrapMode.Overflow
                : HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.transform.SetAsLastSibling();

            if (purpose == KidButtonPurpose.Home)
            {
                RectTransform labelRect = text.rectTransform;
                if (labelRect != null)
                {
                    labelRect.offsetMin = new Vector2(6f, labelRect.offsetMin.y);
                    labelRect.offsetMax = new Vector2(-6f, labelRect.offsetMax.y);
                }
            }
        }

        public static void SetButtonTextColor(Button button, Color color)
        {
            Text text = button != null ? button.GetComponentInChildren<Text>(true) : null;
            if (text != null)
            {
                text.color = color;
            }
        }

        public static void SetActivityBlockStyle(Button button, Color blockColor)
        {
            if (button == null) return;

            Image image = button.GetComponent<Image>();
            if (image != null)
            {
                image.color = blockColor;
                image.raycastTarget = true;
            }

            RoundedRectGraphic background = GetButtonBackground(button);
            if (background != null)
            {
                background.color = blockColor;
                background.raycastTarget = false;
                Shadow shadow = background.GetComponent<Shadow>();
                if (shadow == null) shadow = background.gameObject.AddComponent<Shadow>();
                shadow.effectColor = new Color(0f, 0f, 0f, 0.25f);
                shadow.effectDistance = new Vector2(0f, -6f);
                shadow.useGraphicAlpha = true;
                Outline outline = background.GetComponent<Outline>();
                if (outline == null) outline = background.gameObject.AddComponent<Outline>();
                outline.effectColor = new Color(1f, 1f, 1f, 0.18f);
                outline.effectDistance = new Vector2(2f, -2f);
                outline.useGraphicAlpha = true;
            }

            Text text = button.GetComponentInChildren<Text>(true);
            if (text != null)
            {
                text.color = Color.black;
                text.fontStyle = FontStyle.Bold;
                text.fontSize = Mathf.Max(text.fontSize, 30);
                Outline textOutline = text.GetComponent<Outline>();
                if (textOutline != null) Object.Destroy(textOutline);
                Shadow textShadow = text.GetComponent<Shadow>();
                if (textShadow != null) Object.Destroy(textShadow);
            }

            ColorBlock colors = button.colors;
            colors.normalColor = blockColor;
            colors.highlightedColor = new Color(blockColor.r + 0.08f, blockColor.g + 0.08f, blockColor.b + 0.08f, blockColor.a);
            colors.pressedColor = new Color(blockColor.r - 0.08f, blockColor.g - 0.08f, blockColor.b - 0.08f, blockColor.a);
            colors.disabledColor = new Color(blockColor.r * 0.5f, blockColor.g * 0.5f, blockColor.b * 0.5f, blockColor.a * 0.5f);
            button.colors = colors;
        }

        public static void SetButtonTextColorWithOutline(Button button, Color color)
        {
            Text text = button != null ? button.GetComponentInChildren<Text>(true) : null;
            if (text == null)
            {
                return;
            }

            text.color = color;

            Outline outline = text.GetComponent<Outline>();
            if (outline == null)
            {
                outline = text.gameObject.AddComponent<Outline>();
            }

            outline.effectColor = new Color(0f, 0f, 0f, 0.5f);
            outline.effectDistance = new Vector2(2f, -2f);
            outline.useGraphicAlpha = true;
        }

        public static void HideButtonBackground(Button button)
        {
            if (button == null)
            {
                return;
            }

            Image image = button.GetComponent<Image>();
            if (image != null)
            {
                image.color = Color.clear;
                image.raycastTarget = true;
            }

            RoundedRectGraphic background = GetButtonBackground(button);
            if (background != null)
            {
                background.color = Color.clear;
                Shadow shadow = background.GetComponent<Shadow>();
                if (shadow != null)
                {
                    shadow.enabled = false;
                }

                Outline outline = background.GetComponent<Outline>();
                if (outline != null)
                {
                    outline.enabled = false;
                }
            }

            ColorBlock colors = button.colors;
            colors.normalColor = Color.clear;
            colors.highlightedColor = Color.clear;
            colors.pressedColor = Color.clear;
            colors.selectedColor = Color.clear;
            colors.disabledColor = Color.clear;
            button.colors = colors;
        }

        public static void ApplyReadableText(Text text, int fontSizeBoost = 4, int minimumFontSize = 24, Color? colorOverride = null)
        {
            if (text == null || text.GetComponentInParent<Button>(true) != null)
            {
                return;
            }

            int fontSize = Mathf.Max(minimumFontSize, text.fontSize + fontSizeBoost);
            text.font = GetChildFont();
            text.fontSize = fontSize;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = Mathf.Max(16, fontSize - 16);
            text.resizeTextMaxSize = fontSize;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = colorOverride ?? text.color;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
        }

        public static void ApplyReadableTextToScene(int fontSizeBoost = 4, int minimumFontSize = 24)
        {
            Text[] texts = UnityEngine.Object.FindObjectsByType<Text>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            for (int i = 0; i < texts.Length; i++)
            {
                ApplyReadableText(texts[i], fontSizeBoost, minimumFontSize);
            }
        }

        public static void Apply(Button button, string objectName, string label, int fontSize = 28, bool square = false)
        {
            Apply(button, InferPurpose(objectName, label), label, fontSize, square);
        }

        public static void ApplyToTree(Transform root)
        {
            if (root == null)
            {
                return;
            }

            Button[] buttons = root.GetComponentsInChildren<Button>(true);
            for (int i = 0; i < buttons.Length; i++)
            {
                Button button = buttons[i];
                string label = GetButtonLabel(button);
                Apply(button, InferPurpose(button.name, label), label, 28, false);
            }
        }

        public static void ApplyDropZone(GameObject zone, bool active = true)
        {
            if (zone == null)
            {
                return;
            }

            Image image = zone.GetComponent<Image>();
            if (image != null)
            {
                image.color = Color.clear;
                image.raycastTarget = false;
            }

            RectTransform rect = zone.GetComponent<RectTransform>();
            RectTransform fill = EnsureChildRect(zone.transform, DropFillName);
            fill.SetAsFirstSibling();
            RoundedRectGraphic fillGraphic = GetOrAdd<RoundedRectGraphic>(fill.gameObject);
            fillGraphic.color = active
                ? new Color(1f, 0.94f, 0.58f, 0.86f)
                : new Color(1f, 0.94f, 0.58f, 0.52f);
            fillGraphic.CornerRadius = ResolveRadius(rect, false);
            fillGraphic.raycastTarget = false;

            Shadow shadow = fillGraphic.GetComponent<Shadow>();
            if (shadow == null)
            {
                shadow = fillGraphic.gameObject.AddComponent<Shadow>();
            }

            shadow.effectColor = new Color(0.22f, 0.16f, 0.06f, 0.15f);
            shadow.effectDistance = new Vector2(0f, -5f);
            shadow.useGraphicAlpha = true;

            RectTransform border = EnsureChildRect(zone.transform, DropBorderName);
            border.SetAsLastSibling();
            RoundedDashedBorderGraphic borderGraphic = GetOrAdd<RoundedDashedBorderGraphic>(border.gameObject);
            borderGraphic.color = new Color(1f, 0.62f, 0.12f, active ? 0.95f : 0.55f);
            borderGraphic.CornerRadius = ResolveRadius(rect, false);
            borderGraphic.BorderThickness = 8f;
            borderGraphic.DashLength = 24f;
            borderGraphic.GapLength = 12f;
            borderGraphic.raycastTarget = false;
        }

        public static void SetSelected(Button button, bool selected, KidButtonPurpose purpose)
        {
            if (button == null)
            {
                return;
            }

            Apply(button, purpose, GetButtonLabel(button), 62, true);
            RoundedRectGraphic background = GetButtonBackground(button);
            if (background != null)
            {
                background.color = selected ? Lighten(GetColor(purpose), 0.14f) : GetColor(purpose);
                Outline outline = background.GetComponent<Outline>();
                if (outline == null)
                {
                    outline = background.gameObject.AddComponent<Outline>();
                }

                outline.enabled = selected;
                outline.effectColor = new Color(1f, 0.98f, 0.78f, 0.95f);
                outline.effectDistance = new Vector2(5f, -5f);
                outline.useGraphicAlpha = true;

                Shadow shadow = background.GetComponent<Shadow>();
                if (shadow != null)
                {
                    shadow.effectColor = selected
                        ? new Color(1f, 0.9f, 0.28f, 0.55f)
                        : new Color(0.14f, 0.11f, 0.08f, 0.18f);
                    shadow.effectDistance = selected ? new Vector2(0f, -8f) : new Vector2(0f, -6f);
                }
            }
        }

        public static void PlayFeedback(Button button, bool correct)
        {
            if (button == null)
            {
                return;
            }

            UIButtonKids kids = button.GetComponent<UIButtonKids>();
            if (kids == null)
            {
                kids = button.gameObject.AddComponent<UIButtonKids>();
            }

            if (correct)
            {
                kids.PlayCorrectFeedback();
            }
            else
            {
                kids.PlayWrongFeedback();
            }
        }

        public static KidButtonPurpose InferPurpose(string objectName, string label)
        {
            string value = $"{objectName} {label}".ToLowerInvariant();
            if (value.Contains("→") || value.Contains("right")) return KidButtonPurpose.ComparisonMore;
            if (value.Contains("←") || value.Contains("left")) return KidButtonPurpose.ComparisonFewer;
            if (value.Contains("more") || value.Contains(">")) return KidButtonPurpose.ComparisonMore;
            if (value.Contains("fewer") || value.Contains("less") || value.Contains("<")) return KidButtonPurpose.ComparisonFewer;
            if (value.Contains("equal") || value.Contains("=")) return KidButtonPurpose.ComparisonEqual;
            if (value.Contains("confirm") || value.Contains("submit") || value.Contains("ok") || value.Contains("answer") || value.Contains("trả lời")) return KidButtonPurpose.Confirm;
            if (value.Contains("hint") || value.Contains("gợi ý") || value.Contains("💡")) return KidButtonPurpose.Hint;
            if (value.Contains("home") || value.Contains("back") || value.Contains("cancel") || value.Contains("trang chủ")) return KidButtonPurpose.Home;
            if (value.Contains("retry") || value.Contains("reset") || value.Contains("clear") || value.Contains("làm lại") || value.Contains("xóa")) return KidButtonPurpose.Retry;
            if (value.Contains("progress") || value.Contains("tiến độ")) return KidButtonPurpose.Progress;
            if (value.Contains("listen") || value.Contains("nghe")) return KidButtonPurpose.Listen;
            if (value.Contains("start") || value.Contains("next") || value.Contains("continue") || value.Contains("learning") || value.Contains("tiếp") || value.Contains("học")) return KidButtonPurpose.Primary;
            return KidButtonPurpose.Secondary;
        }

        public static string GetButtonLabel(Button button)
        {
            Text text = button != null ? button.GetComponentInChildren<Text>(true) : null;
            return text != null ? text.text : string.Empty;
        }

        private static RoundedRectGraphic EnsureButtonBackground(Transform buttonTransform)
        {
            Transform existing = buttonTransform.Find(VisualName);
            RectTransform visualRect = existing != null
                ? existing.GetComponent<RectTransform>()
                : null;

            if (visualRect == null)
            {
                var visualGo = new GameObject(VisualName, typeof(RectTransform));
                visualRect = visualGo.GetComponent<RectTransform>();
            }

            visualRect.SetParent(buttonTransform, false);
            visualRect.anchorMin = Vector2.zero;
            visualRect.anchorMax = Vector2.one;
            visualRect.offsetMin = Vector2.zero;
            visualRect.offsetMax = Vector2.zero;
            visualRect.SetAsFirstSibling();
            return GetOrAdd<RoundedRectGraphic>(visualRect.gameObject);
        }

        private static RoundedRectGraphic GetButtonBackground(Button button)
        {
            if (button == null)
            {
                return null;
            }

            Transform visual = button.transform.Find(VisualName);
            return visual != null ? visual.GetComponent<RoundedRectGraphic>() : null;
        }

        private static Text EnsureButtonLabel(Transform parent)
        {
            Text text = parent.GetComponentInChildren<Text>(true);
            if (text != null)
            {
                return text;
            }

            var go = new GameObject("Label", typeof(RectTransform), typeof(Text));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(18f, 8f);
            rect.offsetMax = new Vector2(-18f, -8f);
            return go.GetComponent<Text>();
        }

        private static RectTransform EnsureChildRect(Transform parent, string name)
        {
            Transform child = parent.Find(name);
            RectTransform rect = child != null ? child.GetComponent<RectTransform>() : null;
            if (rect == null)
            {
                var go = new GameObject(name, typeof(RectTransform));
                rect = go.GetComponent<RectTransform>();
            }

            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return rect;
        }

        private static ColorBlock CreateColorBlock(Color normalColor)
        {
            return new ColorBlock
            {
                normalColor = normalColor,
                highlightedColor = Lighten(normalColor, 0.08f),
                pressedColor = Darken(normalColor, 0.08f),
                selectedColor = Lighten(normalColor, 0.1f),
                disabledColor = new Color(normalColor.r, normalColor.g, normalColor.b, 0.45f),
                colorMultiplier = 1f,
                fadeDuration = 0.08f
            };
        }

        private static float ResolveRadius(RectTransform rect, bool square)
        {
            if (rect == null)
            {
                return square ? 32f : 38f;
            }

            float height = Mathf.Abs(rect.sizeDelta.y);
            float width = Mathf.Abs(rect.sizeDelta.x);
            float baseSize = square ? Mathf.Min(width, height) : height;
            return Mathf.Clamp(baseSize * (square ? 0.28f : 0.48f), 24f, 54f);
        }

        private static Color GetColor(KidButtonPurpose purpose)
        {
            return purpose switch
            {
                KidButtonPurpose.Primary => Primary,
                KidButtonPurpose.Confirm => Confirm,
                KidButtonPurpose.Hint => Hint,
                KidButtonPurpose.Home => Home,
                KidButtonPurpose.Retry => Retry,
                KidButtonPurpose.Progress => Secondary,
                KidButtonPurpose.Listen => Secondary,
                KidButtonPurpose.ComparisonMore => CompareMore,
                KidButtonPurpose.ComparisonFewer => CompareFewer,
                KidButtonPurpose.ComparisonEqual => CompareEqual,
                KidButtonPurpose.Neutral => new Color(0.94f, 0.95f, 0.98f, 1f),
                _ => Secondary
            };
        }

        private static Color GetTextColor(KidButtonPurpose purpose)
        {
            return TextDark;
        }

        private static string DecorateLabel(KidButtonPurpose purpose, string label)
        {
            string cleanLabel = StripKnownPrefix(label ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(cleanLabel))
            {
                return GetIcon(purpose);
            }

            if (purpose == KidButtonPurpose.ComparisonMore
                || purpose == KidButtonPurpose.ComparisonFewer
                || purpose == KidButtonPurpose.ComparisonEqual)
            {
                return cleanLabel;
            }

            string icon = GetIcon(purpose);
            return string.IsNullOrEmpty(icon) ? cleanLabel : $"{icon} {cleanLabel}";
        }

        private static string GetIcon(KidButtonPurpose purpose)
        {
            return purpose switch
            {
                KidButtonPurpose.Primary => "▶",
                KidButtonPurpose.Confirm => "✓",
                KidButtonPurpose.Hint => "💡",
                KidButtonPurpose.Home => "⌂",
                KidButtonPurpose.Retry => "↻",
                KidButtonPurpose.Progress => "▥",
                KidButtonPurpose.Listen => "🔊",
                _ => string.Empty
            };
        }

        private static string StripKnownPrefix(string label)
        {
            string[] icons = { "▶", "✓", "💡", "⌂", "↻", "▥", "🔊" };
            for (int i = 0; i < icons.Length; i++)
            {
                string icon = icons[i];
                if (label == icon)
                {
                    return string.Empty;
                }

                if (label.StartsWith(icon + " ", System.StringComparison.Ordinal))
                {
                    return label.Substring(icon.Length + 1);
                }
            }

            return label;
        }

        public static Font GetSharedFont()
        {
            return GetChildFont();
        }

        private static Font GetChildFont()
        {
            if (childFont != null)
                return childFont;

            // Try Quicksand (child-friendly, Vietnamese support, bundled in Resources)
            childFont = Resources.Load<Font>("Fonts/Quicksand-VariableFont_wght");
            if (childFont != null)
                return childFont;

            // Fallback: LegacyRuntime (guaranteed to exist)
            childFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return childFont;
        }

        private static void EnsureCardBackground(Transform buttonTransform, RectTransform buttonRect)
        {
            Transform existing = buttonTransform.parent.Find("CardBackground_" + buttonTransform.name);
            if (existing != null)
                return;

            var cardGo = new GameObject("CardBackground_" + buttonTransform.name, typeof(RectTransform), typeof(RoundedRectGraphic));
            var cardRect = cardGo.GetComponent<RectTransform>();
            cardRect.SetParent(buttonTransform.parent, false);
            cardRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.sizeDelta = buttonRect.sizeDelta + new Vector2(24f, 16f);
            cardRect.anchoredPosition = buttonRect.anchoredPosition;

            var graphic = cardGo.GetComponent<RoundedRectGraphic>();
            graphic.CornerRadius = Mathf.Max(12f, buttonRect.sizeDelta.y * 0.3f);
            graphic.color = new Color(1f, 1f, 1f, 0.68f);
            graphic.raycastTarget = false;

            var shadow = cardGo.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.10f);
            shadow.effectDistance = new Vector2(0f, -3f);
            shadow.useGraphicAlpha = true;

            cardGo.transform.SetAsFirstSibling();
        }

        private static Color Lighten(Color color, float amount)
        {
            return new Color(
                Mathf.Clamp01(color.r + amount),
                Mathf.Clamp01(color.g + amount),
                Mathf.Clamp01(color.b + amount),
                color.a);
        }

        private static Color Darken(Color color, float amount)
        {
            return new Color(
                Mathf.Clamp01(color.r - amount),
                Mathf.Clamp01(color.g - amount),
                Mathf.Clamp01(color.b - amount),
                color.a);
        }

        private static T GetOrAdd<T>(GameObject go) where T : Component
        {
            T component = go.GetComponent<T>();
            return component != null ? component : go.AddComponent<T>();
        }
    }
}
