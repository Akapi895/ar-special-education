using UnityEngine;
using System;

namespace Core.Support.SessionManager
{
    public class SessionTimeManager : MonoBehaviour
    {
        public static SessionTimeManager Instance { get; private set; }

        [Header("Settings")]
        [Tooltip("Max session duration in seconds before recommending a break (default 5 minutes)")]
        [SerializeField] private float maxSessionDuration = 300f;
        [SerializeField] private float checkInterval = 5f;

        public event Action OnBreakRecommended;

        private float elapsedSessionTime = 0f;
        private bool breakRecommended = false;
        private float nextCheckTime = 0f;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            elapsedSessionTime += Time.unscaledDeltaTime;

            if (Time.unscaledTime >= nextCheckTime)
            {
                nextCheckTime = Time.unscaledTime + checkInterval;
                CheckSessionDuration();
            }
        }

        private void CheckSessionDuration()
        {
            if (!breakRecommended && elapsedSessionTime >= maxSessionDuration)
            {
                breakRecommended = true;
                OnBreakRecommended?.Invoke();
                TriggerBreakOverlay();
            }
        }

        private void TriggerBreakOverlay()
        {
            Debug.Log("[SessionTimeManager] Break recommended! Kid has been playing for 5+ minutes.");
            
            // Try to find UIFeedbackOverlay in scene to show break warning
            var overlay = FindAnyObjectByType<Core.UI.Components.UIFeedbackOverlay>();
            if (overlay != null)
            {
                overlay.ShowIncorrect("Con đã học chăm chỉ rồi, hãy nghỉ ngơi 1 lát nhé!");
            }
        }

        public void ResetSessionTimer()
        {
            elapsedSessionTime = 0f;
            breakRecommended = false;
            nextCheckTime = Time.unscaledTime + checkInterval;
        }

        public float GetElapsedSessionTime()
        {
            return elapsedSessionTime;
        }
    }
}
