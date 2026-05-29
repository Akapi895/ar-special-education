using Core.UI.Components;
using Core.UI.Layout;
using Core.UI.Localization;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Features.Activities.NumberBonds
{
    public partial class NumberBondsView
    {
        public void BuildRuntimeUi(Canvas canvas)
        {
            RectTransform panel = CreateUiPanel(canvas.transform, "NumberBondsPanel");
            progressText = CreateTopText(panel, "Progress", "", 24, 18f, new Vector2(360f, 42f));
            progressText.GetComponent<RectTransform>().anchoredPosition = new Vector2(-420f, -18f);
            instructionText = CreateTopText(panel, "InstructionText", "", 34, 48f, new Vector2(820f, 64f));
            expressionText = CreateTopText(panel, "ExpressionText", "", 44, 112f, new Vector2(620f, 70f));

            feedbackPanel = CreateSubPanel(panel, "FeedbackPanel", new Vector2(0f, RuntimeFeedbackPanelBottomY), RuntimeFeedbackPanelSize);
            feedbackText = CreatePanelText(feedbackPanel.transform, "FeedbackText", "", 28);
            SetPanelActive(feedbackPanel, false);

            hintPanel = CreateSubPanel(panel, "HintPanel", new Vector2(0f, RuntimeHintPanelBottomY), new Vector2(790f, 64f));
            hintText = CreatePanelText(hintPanel.transform, "HintText", "", 24);
            SetPanelActive(hintPanel, false);

            confirmButton = CreateButton(panel, "ConfirmButton", SimpleLocalization.Get("btn_confirm"),
                new Vector2(0f, RuntimeConfirmBottomY), () => OnConfirmRequested?.Invoke(), KidButtonPurpose.Confirm);

            float actionOffset = (RuntimeButtonSize.x + ButtonGap) * 0.5f;
            hintButton = UIActivityNavButtons.CreateHintButton(panel, () => OnHintRequested?.Invoke());
            cancelButton = UIActivityNavButtons.CreateHomeButton(panel, OnCancelClicked);
            listenButton = UIActivityNavButtons.CreateListenButton(panel, OnListenClicked);
            nextRoundButton = CreateButton(panel, "NextButton", SimpleLocalization.Get("btn_next"),
                new Vector2(-actionOffset, RuntimeActionButtonBottomY), OnNextRoundClicked, KidButtonPurpose.Primary);
            progressButton = CreateButton(panel, "ProgressButton", SimpleLocalization.Get("btn_progress"),
                new Vector2(actionOffset, RuntimeActionButtonBottomY), OnProgressClicked, KidButtonPurpose.Progress);

            SetButtonActive(nextRoundButton, false);
            SetButtonActive(progressButton, false);
            WireButtonListeners();
            UIKidFriendlyStyle.ApplyReadableTextToScene(3, 24);
        }

        private static RectTransform CreateUiPanel(Transform parent, string name)
        {
            return UIActivityLayoutHelpers.CreateUiPanel(parent, name);
        }

        private static Text CreateTopText(Transform parent, string name, string content, int fontSize, float topOffset, Vector2 size)
        {
            return UIActivityLayoutHelpers.CreateTopText(parent, name, content, fontSize, topOffset, size);
        }

        private static GameObject CreateSubPanel(Transform parent, string name, Vector2 anchoredPosition, Vector2 size)
        {
            return UIActivityLayoutHelpers.CreateSubPanel(parent, name, anchoredPosition, size);
        }

        private static Text CreatePanelText(Transform parent, string name, string content, int fontSize)
        {
            return UIActivityLayoutHelpers.CreatePanelText(parent, name, content, fontSize);
        }

        private static Button CreateButton(Transform parent, string name, string label,
            Vector2 anchoredPosition, UnityEngine.Events.UnityAction onClick, KidButtonPurpose purpose)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.sizeDelta = RuntimeButtonSize;
            rect.anchoredPosition = anchoredPosition;
            Button button = go.GetComponent<Button>();
            button.onClick.AddListener(onClick);
            CreateButtonLabel(go.transform, label);
            UIKidFriendlyStyle.Apply(button, purpose, label, 26);
            return button;
        }

        private static Text CreateButtonLabel(Transform parent, string label)
        {
            return UIActivityLayoutHelpers.CreateButtonLabel(parent, label);
        }

        private static void ConfigureText(Text text, string content, int fontSize)
        {
            text.text = content;
            text.fontSize = fontSize;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = Mathf.Max(14, fontSize - 14);
            text.resizeTextMaxSize = fontSize;
            text.alignment = TextAnchor.MiddleCenter;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.color = Color.white;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
        }

        private static void SetButtonActive(Button button, bool active)
        {
            UIActivityLayoutHelpers.SetButtonActive(button, active);
        }

        private static void SetPanelActive(GameObject panel, bool active)
        {
            UIActivityLayoutHelpers.SetPanelActive(panel, active);
        }

        private static void SetButtonLabel(Button button, string label)
        {
            UIActivityLayoutHelpers.SetButtonLabel(button, label);
        }

        private static void LoadSceneIfAvailable(string sceneName)
        {
            UIActivityLayoutHelpers.LoadSceneIfAvailable(sceneName);
        }
    }
}
