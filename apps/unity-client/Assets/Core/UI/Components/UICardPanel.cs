using UnityEngine;
using UnityEngine.UI;
using Core.UI.DesignSystem;

namespace Core.UI.Components
{
    [RequireComponent(typeof(Image))]
    public class UICardPanel : MonoBehaviour
    {
        public enum PanelType
        {
            DefaultBackground,
            Card,
            PrimaryAccent,
            SecondaryAccent,
            SuccessPopup,
            ErrorPopup
        }

        [Header("Styling")]
        [SerializeField] private PanelType panelType = PanelType.Card;
        [SerializeField] private UIDesignTokens designTokens;
        [SerializeField] private bool useTokens = true;

        private Image panelImage;
        private Shadow panelShadow;

        private void Awake()
        {
            panelImage = GetComponent<Image>();
            panelShadow = GetComponent<Shadow>();
            ApplyStyling();
        }

        private void OnValidate()
        {
            // Update in Editor if references exist
            if (panelImage == null) panelImage = GetComponent<Image>();
            if (panelShadow == null) panelShadow = GetComponent<Shadow>();
            ApplyStyling();
        }

        public void ApplyStyling()
        {
            if (!useTokens) return;

            // Load default design tokens if none assigned
            if (designTokens == null)
            {
                designTokens = Resources.Load<UIDesignTokens>("SO_UIDesignTokens_Default");
                // If Resources.Load returns null, we can try loading it by Guid or path in Editor
                #if UNITY_EDITOR
                if (designTokens == null)
                {
                    string[] guids = UnityEditor.AssetDatabase.FindAssets("t:UIDesignTokens");
                    if (guids.Length > 0)
                    {
                        string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                        designTokens = UnityEditor.AssetDatabase.LoadAssetAtPath<UIDesignTokens>(path);
                    }
                }
                #endif
            }

            if (designTokens == null || panelImage == null) return;

            // Set color based on panel type
            switch (panelType)
            {
                case PanelType.DefaultBackground:
                    panelImage.color = designTokens.backgroundColor;
                    break;
                case PanelType.Card:
                    // Semi-transparent off-white for glassmorphism/card look
                    panelImage.color = new Color(1f, 1f, 1f, 0.9f);
                    break;
                case PanelType.PrimaryAccent:
                    panelImage.color = designTokens.primaryColor;
                    break;
                case PanelType.SecondaryAccent:
                    panelImage.color = designTokens.secondaryColor;
                    break;
                case PanelType.SuccessPopup:
                    panelImage.color = designTokens.successColor;
                    break;
                case PanelType.ErrorPopup:
                    panelImage.color = designTokens.errorColor;
                    break;
            }

            // Set shadow if present
            if (panelShadow != null)
            {
                panelShadow.effectColor = new Color(0f, 0f, 0f, 0.15f);
                panelShadow.effectDistance = new Vector2(4f, -4f);
            }
        }
    }
}
