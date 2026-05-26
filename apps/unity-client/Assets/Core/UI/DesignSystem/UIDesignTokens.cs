using UnityEngine;

namespace Core.UI.DesignSystem
{
    [CreateAssetMenu(fileName = "UIDesignTokens", menuName = "UI/Design Tokens", order = 1)]
    public class UIDesignTokens : ScriptableObject
    {
        [Header("Color Palette (Pastel/Dyscalculia-Friendly)")]
        public Color primaryColor = new Color(0.35f, 0.65f, 0.95f, 1f);   // Soft pastel blue
        public Color secondaryColor = new Color(1f, 0.62f, 0.35f, 1f);  // Soft warm orange
        public Color successColor = new Color(0.4f, 0.8f, 0.5f, 1f);     // Soft green
        public Color errorColor = new Color(0.95f, 0.45f, 0.45f, 1f);     // Gentle coral/pink-red
        public Color backgroundColor = new Color(0.98f, 0.98f, 0.95f, 1f); // Off-white cream
        public Color textColor = new Color(0.18f, 0.18f, 0.18f, 1f);      // Dark charcoal/gray (not pure black)
        public Color accentColor = new Color(0.98f, 0.85f, 0.35f, 1f);   // Warm yellow

        [Header("Typography Font Sizes")]
        public float h1FontSize = 48f;
        public float h2FontSize = 36f;
        public float bodyFontSize = 28f;
        public float captionFontSize = 22f;

        [Header("Button Dimensions")]
        public Vector2 largeButtonSize = new Vector2(240f, 90f);
        public Vector2 mediumButtonSize = new Vector2(180f, 75f);
        public Vector2 smallButtonSize = new Vector2(120f, 60f);

        [Header("Layout & Spacing")]
        public float cornerRadiusCards = 24f;
        public float cornerRadiusButtons = 32f;
        public float paddingSmall = 8f;
        public float paddingMedium = 16f;
        public float paddingLarge = 24f;

        [Header("Animation Durations (Seconds)")]
        public float quickTransition = 0.2f;
        public float normalTransition = 0.4f;
        public float slowTransition = 0.8f;
    }
}
