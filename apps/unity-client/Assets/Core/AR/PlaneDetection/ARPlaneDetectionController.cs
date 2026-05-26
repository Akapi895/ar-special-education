using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace ARSpecialEducation.Core.AR
{
    [DisallowMultipleComponent]
    public sealed class ARPlaneDetectionController : MonoBehaviour
    {
        [SerializeField] ARPlaneManager planeManager;
        [SerializeField] bool visualizePlanes = true;
        [SerializeField] bool requireHorizontalUp = true;
        [SerializeField] float minimumPlaneArea = 0.15f;

        readonly HashSet<TrackableId> knownValidPlanes = new HashSet<TrackableId>();

        public event Action<ARPlane> OnPlaneDetected;
        public event Action<ARPlane> OnPlaneLost;
        public event Action<PlaneScanState> OnPlaneScanUpdated;

        public bool VisualizePlanes => visualizePlanes;
        public int ValidPlaneCount => knownValidPlanes.Count;

        void Awake()
        {
            ResolveReferences();
        }

        void OnEnable()
        {
            ResolveReferences();

            if (planeManager != null)
            {
                planeManager.trackablesChanged.AddListener(HandlePlanesChanged);
                SetPlaneVisualization(visualizePlanes);
                PublishScanState();
            }
        }

        void OnDisable()
        {
            if (planeManager != null)
            {
                planeManager.trackablesChanged.RemoveListener(HandlePlanesChanged);
            }
        }

        public void ResolveReferences()
        {
            if (planeManager == null)
            {
                planeManager = FindFirstObjectByType<ARPlaneManager>();
            }
        }

        public void SetDetectionEnabled(bool isEnabled)
        {
            ResolveReferences();

            if (planeManager != null)
            {
                planeManager.enabled = isEnabled;
            }
        }

        public void SetPlaneVisualization(bool isVisible)
        {
            visualizePlanes = isVisible;

            if (planeManager == null)
            {
                return;
            }

            foreach (var plane in planeManager.trackables)
            {
                SetPlaneVisualization(plane, visualizePlanes);
            }
        }

        public bool IsValidPlane(ARPlane plane)
        {
            if (plane == null)
            {
                return false;
            }

            if (requireHorizontalUp && plane.alignment != PlaneAlignment.HorizontalUp)
            {
                return false;
            }

            var area = plane.size.x * plane.size.y;
            return area >= minimumPlaneArea && plane.trackingState == TrackingState.Tracking;
        }

        void HandlePlanesChanged(ARTrackablesChangedEventArgs<ARPlane> eventArgs)
        {
            foreach (var plane in eventArgs.added)
            {
                HandlePlaneCandidate(plane);
            }

            foreach (var plane in eventArgs.updated)
            {
                HandlePlaneCandidate(plane);
            }

            foreach (var removedPlane in eventArgs.removed)
            {
                var plane = removedPlane.Value;
                if (knownValidPlanes.Remove(removedPlane.Key))
                {
                    OnPlaneLost?.Invoke(plane);
                }
            }

            PublishScanState();
        }

        void HandlePlaneCandidate(ARPlane plane)
        {
            SetPlaneVisualization(plane, visualizePlanes);

            if (IsValidPlane(plane))
            {
                if (knownValidPlanes.Add(plane.trackableId))
                {
                    OnPlaneDetected?.Invoke(plane);
                }
            }
            else if (knownValidPlanes.Remove(plane.trackableId))
            {
                OnPlaneLost?.Invoke(plane);
            }
        }

        void PublishScanState()
        {
            if (planeManager == null)
            {
                OnPlaneScanUpdated?.Invoke(new PlaneScanState(0, 0, null));
                return;
            }

            var totalPlanes = 0;
            var validPlanes = 0;
            ARPlane bestPlane = null;
            var bestArea = 0f;

            foreach (var plane in planeManager.trackables)
            {
                totalPlanes++;

                if (!IsValidPlane(plane))
                {
                    continue;
                }

                validPlanes++;

                var area = plane.size.x * plane.size.y;
                if (area > bestArea)
                {
                    bestArea = area;
                    bestPlane = plane;
                }
            }

            OnPlaneScanUpdated?.Invoke(new PlaneScanState(totalPlanes, validPlanes, bestPlane));
        }

        static void SetPlaneVisualization(ARPlane plane, bool isVisible)
        {
            if (plane == null)
            {
                return;
            }

            if (plane.TryGetComponent<ARPlaneMeshVisualizer>(out var meshVisualizer))
            {
                meshVisualizer.enabled = isVisible;
            }

            if (plane.TryGetComponent<MeshRenderer>(out var meshRenderer))
            {
                meshRenderer.enabled = isVisible;
            }

            if (plane.TryGetComponent<LineRenderer>(out var lineRenderer))
            {
                lineRenderer.enabled = isVisible;
            }
        }

        public readonly struct PlaneScanState
        {
            public PlaneScanState(int totalPlanes, int validPlanes, ARPlane bestPlane)
            {
                TotalPlanes = totalPlanes;
                ValidPlanes = validPlanes;
                BestPlane = bestPlane;
            }

            public int TotalPlanes { get; }
            public int ValidPlanes { get; }
            public ARPlane BestPlane { get; }
            public bool HasValidPlane => BestPlane != null;
        }
    }
}
