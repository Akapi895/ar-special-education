using System;
using Core.Learning.ActivityRunner;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace Core.AR.ARSession
{
    /// <summary>
    /// AR Foundation implementation of <see cref="IARSessionService"/>.
    /// Attach to the same hierarchy as <see cref="ARSession"/> (typically under XR Origin).
    /// </summary>
    [DisallowMultipleComponent]
    public class ARSessionService : MonoBehaviour, IARSessionService
    {
        [SerializeField]
        private UnityEngine.XR.ARFoundation.ARSession arSession;

        [SerializeField]
        private bool autoStartSession = true;

        private bool sessionReady;
        private TrackingQuality trackingQuality = TrackingQuality.None;
        private bool initialized;

        public event Action OnSessionReady;
        public event Action OnSessionLost;

        public bool IsSessionReady => sessionReady;
        public bool IsTrackingStable =>
            sessionReady && trackingQuality >= TrackingQuality.Good;

        public TrackingQuality TrackingQuality => trackingQuality;

        private void Awake()
        {
            EnsureSessionReference();
        }

        private void OnEnable()
        {
            UnityEngine.XR.ARFoundation.ARSession.stateChanged += OnARSessionStateChanged;
        }

        private void OnDisable()
        {
            UnityEngine.XR.ARFoundation.ARSession.stateChanged -= OnARSessionStateChanged;
        }

        public void Initialize()
        {
            EnsureSessionReference();

            if (arSession == null)
            {
                Debug.LogWarning("[ARSessionService] ARSession unavailable. Falling back to not-ready tracking state.");
                SetSessionReady(false);
                return;
            }

            if (initialized)
            {
                return;
            }

            initialized = true;
            UpdateFromSessionState(UnityEngine.XR.ARFoundation.ARSession.state);
            Debug.Log("[ARSessionService] Initialized.");
        }

        public void StartSession()
        {
            EnsureSessionReference();

            if (arSession == null)
            {
                return;
            }

            if (arSession.subsystem != null && !arSession.subsystem.running)
            {
                arSession.subsystem.Start();
            }
        }

        public void StopSession()
        {
            if (arSession?.subsystem != null && arSession.subsystem.running)
            {
                arSession.subsystem.Stop();
            }

            SetSessionReady(false);
        }

        public void ResetSession()
        {
            EnsureSessionReference();

            if (arSession == null)
            {
                return;
            }

            arSession.Reset();
            SetSessionReady(false);
        }

        private void Start()
        {
            Initialize();

            if (autoStartSession)
            {
                StartSession();
            }
        }

        private void EnsureSessionReference()
        {
            if (arSession != null)
            {
                return;
            }

            arSession = FindAnyObjectByType<UnityEngine.XR.ARFoundation.ARSession>();
            if (arSession != null)
            {
                return;
            }

            var sessionObject = new GameObject("AR Session");
            arSession = sessionObject.AddComponent<UnityEngine.XR.ARFoundation.ARSession>();
            Debug.Log("[ARSessionService] Created missing ARSession component.");
        }

        private void OnARSessionStateChanged(ARSessionStateChangedEventArgs args)
        {
            UpdateFromSessionState(args.state);
        }

        private void UpdateFromSessionState(ARSessionState state)
        {
            switch (state)
            {
                case ARSessionState.SessionInitializing:
                    trackingQuality = TrackingQuality.Fair;
                    SetSessionReady(false);
                    break;
                case ARSessionState.SessionTracking:
                    trackingQuality = TrackingQuality.Good;
                    SetSessionReady(true);
                    break;
                case ARSessionState.Ready:
                    trackingQuality = TrackingQuality.Fair;
                    SetSessionReady(true);
                    break;
                case ARSessionState.None:
                case ARSessionState.Unsupported:
                case ARSessionState.CheckingAvailability:
                case ARSessionState.NeedsInstall:
                case ARSessionState.Installing:
                    trackingQuality = TrackingQuality.None;
                    SetSessionReady(false);
                    break;
                default:
                    trackingQuality = TrackingQuality.Fair;
                    SetSessionReady(false);
                    break;
            }
        }

        private void SetSessionReady(bool ready)
        {
            if (sessionReady == ready)
            {
                return;
            }

            sessionReady = ready;

            if (ready)
            {
                OnSessionReady?.Invoke();
                Debug.Log("[ARSessionService] Session ready.");
            }
            else
            {
                OnSessionLost?.Invoke();
                Debug.Log("[ARSessionService] Session lost or not tracking.");
            }
        }
    }
}
