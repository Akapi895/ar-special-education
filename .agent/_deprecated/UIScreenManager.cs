using System.Collections.Generic;
using UnityEngine;

namespace Core.UI.Navigation
{
    public class UIScreenManager : MonoBehaviour
    {
        public static UIScreenManager Instance { get; private set; }

        [Header("Screens")]
        [SerializeField] private UIScreen initialScreen;
        [SerializeField] private List<UIScreen> registeredScreens = new List<UIScreen>();

        private Stack<UIScreen> screenStack = new Stack<UIScreen>();
        private UIScreen currentScreen;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                // If it is root in a persistent boot scene, we could DontDestroyOnLoad:
                // DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            // Initialize screens
            foreach (var screen in registeredScreens)
            {
                if (screen != null && screen != initialScreen)
                {
                    screen.HideImmediate();
                }
            }

            if (initialScreen != null)
            {
                PushScreen(initialScreen);
            }
        }

        public void PushScreen(UIScreen newScreen)
        {
            if (newScreen == null) return;

            if (currentScreen != null)
            {
                // Hide current screen
                currentScreen.Hide(0.2f);
            }

            screenStack.Push(newScreen);
            currentScreen = newScreen;
            currentScreen.Show(0.25f);
        }

        public void PopScreen()
        {
            if (screenStack.Count <= 1)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("[UIScreenManager] Cannot pop screen, stack has 1 or 0 screens.");
                #endif
                return;
            }

            var poppedScreen = screenStack.Pop();
            poppedScreen.Hide(0.2f);

            currentScreen = screenStack.Peek();
            currentScreen.Show(0.25f);
        }

        public void ClearAndPush(UIScreen newScreen)
        {
            if (newScreen == null) return;

            while (screenStack.Count > 0)
            {
                var screen = screenStack.Pop();
                screen.HideImmediate();
            }

            screenStack.Push(newScreen);
            currentScreen = newScreen;
            currentScreen.Show(0.25f);
        }
    }
}
