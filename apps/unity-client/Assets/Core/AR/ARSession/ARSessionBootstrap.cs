using System;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace ARSpecialEducation.Core.AR
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-100)]
    public sealed class ARSessionBootstrap : MonoBehaviour
    {
        [SerializeField] ARSession arSession;
        [SerializeField] XROrigin xrOrigin;
        [SerializeField] Camera arCamera;
        [SerializeField] ARPlaneManager planeManager;
        [SerializeField] ARRaycastManager raycastManager;
        [SerializeField] bool autoStartSession = true;

        bool hasReportedReady;

        public event Action<ARSessionState> OnARSessionStateChanged;
        public event Action OnARReady;
        public event Action<string> OnARSessionUnavailable;

        public ARSession Session => arSession;
        public XROrigin Origin => xrOrigin;
        public Camera ARCamera => arCamera;
        public ARPlaneManager PlaneManager => planeManager;
        public ARRaycastManager RaycastManager => raycastManager;

        void Awake()
        {
            ResolveReferences();
            ValidateRequiredReferences();
        }

        void OnEnable()
        {
            ARSession.stateChanged += HandleSessionStateChanged;

            if (autoStartSession)
            {
                SetSessionEnabled(true);
            }
        }

        void OnDisable()
        {
            ARSession.stateChanged -= HandleSessionStateChanged;
        }

        public void StartSession()
        {
            SetSessionEnabled(true);
        }

        public void StopSession()
        {
            SetSessionEnabled(false);
            hasReportedReady = false;
        }

        public void ResetSession()
        {
            if (arSession == null)
            {
                ResolveReferences();
            }

            if (arSession != null)
            {
                arSession.Reset();
                hasReportedReady = false;
            }
        }

        public void ResolveReferences()
        {
            if (arSession == null)
            {
                arSession = FindFirstObjectByType<ARSession>();
            }

            if (xrOrigin == null)
            {
                xrOrigin = FindFirstObjectByType<XROrigin>();
            }

            if (arCamera == null && xrOrigin != null)
            {
                arCamera = xrOrigin.Camera;
            }

            if (arCamera == null)
            {
                arCamera = Camera.main != null ? Camera.main : FindFirstObjectByType<Camera>();
            }

            if (planeManager == null)
            {
                planeManager = FindFirstObjectByType<ARPlaneManager>();
            }

            if (raycastManager == null)
            {
                raycastManager = FindFirstObjectByType<ARRaycastManager>();
            }
        }

        void SetSessionEnabled(bool isEnabled)
        {
            ResolveReferences();

            if (arSession != null)
            {
                arSession.enabled = isEnabled;
            }

            if (planeManager != null)
            {
                planeManager.enabled = isEnabled;
            }

            if (raycastManager != null)
            {
                raycastManager.enabled = isEnabled;
            }
        }

        void ValidateRequiredReferences()
        {
            if (arSession == null)
            {
                ReportUnavailable("AR Session is missing from the gameplay scene.");
            }

            if (xrOrigin == null)
            {
                ReportUnavailable("XR Origin is missing from the gameplay scene.");
            }

            if (arCamera == null)
            {
                ReportUnavailable("AR Camera is missing from the gameplay scene.");
            }

            if (planeManager == null)
            {
                ReportUnavailable("AR Plane Manager is missing from the gameplay scene.");
            }

            if (raycastManager == null)
            {
                ReportUnavailable("AR Raycast Manager is missing from the gameplay scene.");
            }
        }

        void ReportUnavailable(string message)
        {
            Debug.LogError(message, this);
            OnARSessionUnavailable?.Invoke(message);
        }

        void HandleSessionStateChanged(ARSessionStateChangedEventArgs eventArgs)
        {
            OnARSessionStateChanged?.Invoke(eventArgs.state);

            if (eventArgs.state == ARSessionState.Unsupported || eventArgs.state == ARSessionState.None)
            {
                ReportUnavailable($"AR session state is {eventArgs.state}.");
                return;
            }

            if (!hasReportedReady &&
                (eventArgs.state == ARSessionState.Ready ||
                 eventArgs.state == ARSessionState.SessionInitializing ||
                 eventArgs.state == ARSessionState.SessionTracking))
            {
                hasReportedReady = true;
                OnARReady?.Invoke();
            }
        }
    }
}
