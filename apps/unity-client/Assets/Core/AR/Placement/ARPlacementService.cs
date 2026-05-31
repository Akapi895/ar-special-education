using System;
using System.Collections;
using System.Collections.Generic;
using ARSpecialEducation.Core.AR;
using Core.Support.Performance;
using Core.Learning.ActivityRunner;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI;

namespace Core.AR.Placement
{
    /// <summary>
    /// AR Foundation placement and spawn service implementing <see cref="IARPlacementService"/>.
    /// FIX A: Now gates content spawning behind actual horizontal plane detection.
    /// </summary>
    [DisallowMultipleComponent]
    public class ARPlacementService : MonoBehaviour, IARPlacementService
    {
        [SerializeField]
        private ARRaycastManager raycastManager;

        [SerializeField]
        private ARPlaneManager planeManager;

        [SerializeField]
        private Camera arCamera;

        [SerializeField]
        private bool addCollidersToSpawned = true;

        [SerializeField]
        private float defaultColliderRadius = 0.08f;

        [SerializeField]
        private TrackableType placementTrackableTypes = TrackableType.PlaneWithinPolygon;

        [SerializeField]
        private float minimumPlaneArea = 0.15f;

        [Header("Learning Area")]
        [SerializeField]
        private LearningAreaAnchor learningAreaAnchor;

        [SerializeField]
        private ARPlacementController placementController;

        [SerializeField]
        private Vector2 defaultLearningAreaSizeMeters = new Vector2(2.4f, 1.8f);

        [SerializeField]
        private bool autoCreateLearningAreaFromPlacement = true;

        [SerializeField]
        private bool hidePlaneVisualizationAfterPlacement = true;

        // FIX A: Plane detection gating
        [Header("Plane Detection Gating")]
        [SerializeField]
        private float planeDetectionTimeout = 15f;

        [SerializeField]
        private float forcePlacementDistance = 1.2f;

        [SerializeField]
        private Canvas scanningCanvas;

        [SerializeField]
        private Text scanningText;

        private readonly List<GameObject> spawnedObjects = new List<GameObject>();
        private readonly List<ARRaycastHit> raycastHits = new List<ARRaycastHit>();

        private Vector3 currentPlacementPosition;
        private Quaternion currentPlacementRotation = Quaternion.identity;
        private bool placementAvailable;
        private bool horizontalPlaneDetected;
        private float planeDetectionStartTime;
        private bool isSearchingForPlane;
        private float lastPlaneCheckTime; // FIX A: Throttle plane polling

        public event Action<Vector3> OnPlacementPositionAvailable;
        public event Action OnPlacementPositionLost;
        public event Action OnLearningAreaPlaced;

        public bool IsPlacementAvailable => placementAvailable;
        public Vector3 CurrentPlacementPosition => currentPlacementPosition;
        public bool HasLearningArea => learningAreaAnchor != null && learningAreaAnchor.IsPlaced;
        public Transform LearningAreaContentRoot => HasLearningArea ? learningAreaAnchor.ContentRoot : transform;
        public Vector2 LearningAreaSizeMeters => HasLearningArea ? learningAreaAnchor.AreaSizeMeters : defaultLearningAreaSizeMeters;

        private void Awake()
        {
            ResolveReferences();
            CreateScanningUI();
        }

        private void OnDestroy()
        {
            if (placementController != null)
            {
                placementController.OnLearningAreaPlaced -= HandleLearningAreaPlaced;
            }
            // FIX A: No need to unsubscribe from trackablesChanged (using polling approach)
        }

        private void Update()
        {
            // FIX A: Poll for planes when searching (AR Foundation 6.x compatible approach)
            if (isSearchingForPlane && !horizontalPlaneDetected)
            {
                // Check for timeout during plane detection
                if (Time.time - planeDetectionStartTime >= planeDetectionTimeout)
                {
                    ForcePlacementAtCameraForward();
                    return;
                }

                // Poll for horizontal planes
                CheckForHorizontalPlanes();
            }

            // Normal placement update
            if (!isSearchingForPlane)
            {
                UpdatePlacementPosition();
            }
        }

        // FIX A: Polling approach for plane detection (AR Foundation 6.x compatible)
        private void CheckForHorizontalPlanes()
        {
            if (planeManager == null)
            {
                return;
            }

            // Check all currently tracked planes
            foreach (var plane in planeManager.trackables)
            {
                if (plane == null || plane.trackingState != TrackingState.Tracking)
                {
                    continue;
                }

                if (IsValidHorizontalPlane(plane))
                {
                    OnHorizontalPlaneFound(plane);
                    return;
                }
            }
        }

        public void Initialize()
        {
            ResolveReferences();
            ResolveLearningAreaController();

            if (raycastManager == null)
            {
                Debug.LogError("[ARPlacementService] ARRaycastManager is missing.");
                return;
            }

            // FIX A: Start waiting for horizontal plane before allowing placement
            StartPlaneDetectionSearch();

            Debug.Log("[ARPlacementService] Initialized. Waiting for horizontal plane...");
        }

        // FIX A: Start searching for a horizontal plane before allowing content spawn
        private void StartPlaneDetectionSearch()
        {
            // FIX A: Using polling approach instead of event subscription (AR Foundation 6.x compatible)
            isSearchingForPlane = true;
            horizontalPlaneDetected = false;
            planeDetectionStartTime = Time.time;
            placementAvailable = false;

            ShowScanningUI(true);
            Debug.Log("[ARPlacementService] FIX A: Started searching for horizontal plane (polling mode). Content spawn is gated.");
        }

        // FIX A: Check if plane is horizontal and large enough
        private bool IsValidHorizontalPlane(ARPlane plane)
        {
            if (plane == null || plane.trackingState != TrackingState.Tracking)
            {
                return false;
            }

            if (plane.alignment != PlaneAlignment.HorizontalUp && plane.alignment != PlaneAlignment.HorizontalDown)
            {
                return false;
            }

            float area = plane.size.x * plane.size.y;
            return area >= minimumPlaneArea;
        }

        // FIX A: When a valid horizontal plane is found, raycast to find spawn point
        private void OnHorizontalPlaneFound(ARPlane plane)
        {
            horizontalPlaneDetected = true;
            isSearchingForPlane = false;

            Debug.Log($"[ARPlacementService] FIX A: Horizontal plane detected! Area: {plane.size.x * plane.size.y:F2}m²");

            // Raycast from screen center to the plane
            if (raycastManager != null && arCamera != null)
            {
                Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
                raycastHits.Clear();

                if (raycastManager.Raycast(screenCenter, raycastHits, TrackableType.PlaneWithinPolygon) && raycastHits.Count > 0)
                {
                    // Use the first hit (closest to camera)
                    ARPlane hitPlane = planeManager.GetPlane(raycastHits[0].trackableId);
                    if (hitPlane != null)
                    {
                        currentPlacementPosition = raycastHits[0].pose.position;
                        currentPlacementRotation = raycastHits[0].pose.rotation;
                        SetPlacementAvailable(true);
                        EnsureLearningArea(raycastHits[0].pose, hitPlane);
                        ShowScanningUI(false);
                        return;
                    }
                }
            }

            // Fallback: use plane center
            currentPlacementPosition = plane.center;
            currentPlacementRotation = plane.transform.rotation;
            SetPlacementAvailable(true);
            EnsureLearningArea(new Pose(currentPlacementPosition, currentPlacementRotation), plane);
            ShowScanningUI(false);
        }

        // FIX A: Force placement after timeout or when no plane is detected
        private void ForcePlacementAtCameraForward()
        {
            if (placementAvailable)
            {
                return;
            }

            isSearchingForPlane = false;
            Debug.LogWarning("[ARPlacementService] FIX A: Plane detection timeout. Force-placing at camera forward.");

            if (arCamera != null)
            {
                Vector3 forward = arCamera.transform.forward;
                forward.y = 0f;
                if (forward.sqrMagnitude < 0.0001f)
                {
                    forward = Vector3.forward;
                }
                else
                {
                    forward.Normalize();
                }

                currentPlacementPosition = arCamera.transform.position + forward * forcePlacementDistance;
                currentPlacementPosition.y = 0f; // Place on ground
                currentPlacementRotation = Quaternion.identity;
            }
            else
            {
                currentPlacementPosition = Vector3.forward * forcePlacementDistance;
                currentPlacementRotation = Quaternion.identity;
            }

            SetPlacementAvailable(true);
            EnsureLearningArea(new Pose(currentPlacementPosition, currentPlacementRotation), null);
            ShowScanningUI(false);
        }

        // FIX A: Create scanning UI
        private void CreateScanningUI()
        {
            if (scanningCanvas != null)
            {
                return;
            }

            var canvasGo = new GameObject("ScanningCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            scanningCanvas = canvasGo.GetComponent<Canvas>();
            scanningCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            scanningCanvas.sortingOrder = 100;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1334f, 750f);

            // Create panel
            var panelGo = new GameObject("ScanningPanel", typeof(RectTransform), typeof(Image));
            panelGo.transform.SetParent(scanningCanvas.transform, false);
            var panelRect = panelGo.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(400f, 80f);
            panelRect.anchoredPosition = new Vector2(0f, -200f);

            var panelImage = panelGo.GetComponent<Image>();
            panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);

            // Create text
            var textGo = new GameObject("ScanningText", typeof(RectTransform), typeof(Text));
            textGo.transform.SetParent(panelRect, false);
            var textRect = textGo.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10f, 5f);
            textRect.offsetMax = new Vector2(-10f, -5f);

            scanningText = textGo.GetComponent<Text>();
            scanningText.text = "Đang tìm mặt phẳng...";
            scanningText.alignment = TextAnchor.MiddleCenter;
            scanningText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            scanningText.fontSize = 24;
            scanningText.color = Color.white;

            scanningCanvas.gameObject.SetActive(false);
        }

        // FIX A: Show/hide scanning UI
        private void ShowScanningUI(bool show)
        {
            if (scanningCanvas != null)
            {
                scanningCanvas.gameObject.SetActive(show);
            }

            if (scanningText != null)
            {
                scanningText.text = "Đang tìm mặt phẳng...";
            }
        }

        public GameObject SpawnAtPlacementPosition(GameObject prefab, Transform parent = null)
        {
            if (!placementAvailable)
            {
                Debug.LogWarning("[ARPlacementService] No placement position available.");
                return null;
            }

            return SpawnAtPosition(prefab, currentPlacementPosition, currentPlacementRotation, parent);
        }

        public Vector3 LearningAreaToWorldPoint(Vector3 localPosition)
        {
            Transform root = LearningAreaContentRoot;
            return root != null ? root.TransformPoint(localPosition) : localPosition;
        }

        public GameObject SpawnAtPosition(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            if (prefab == null)
            {
                Debug.LogError("[ARPlacementService] Cannot spawn null prefab.");
                return null;
            }

            // Use provided parent, LearningAreaContentRoot (if exists), or null for world space
            // Never use ARPlacementService.transform as parent to avoid objects following camera
            Transform resolvedParent = parent != null ? parent : (HasLearningArea ? LearningAreaContentRoot : null);
            GameObject instance = ObjectPoolManager.Spawn(prefab, position, rotation, resolvedParent);
            instance.SetActive(true);

            // iOS scale adjustment is now handled in ActivityPrefabSetup.PrepareLearningObject

            TrackSpawned(instance);
            return instance;
        }

        public GameObject SpawnAtLearningAreaPosition(GameObject prefab, Vector3 localPosition, Quaternion localRotation, Transform parent = null)
        {
            Transform root = LearningAreaContentRoot;
            Vector3 worldPosition = root != null ? root.TransformPoint(localPosition) : localPosition;
            Quaternion worldRotation = root != null ? root.rotation * localRotation : localRotation;
            // Only use root as parent if it's a valid LearningAreaAnchor, not ARPlacementService.transform
            Transform resolvedParent = parent != null ? parent : (HasLearningArea ? root : null);
            return SpawnAtPosition(prefab, worldPosition, worldRotation, resolvedParent);
        }

        public GameObject[] SpawnGrid(GameObject prefab, Vector3 centerPosition, int count, float spacing)
        {
            if (prefab == null || count <= 0)
            {
                return Array.Empty<GameObject>();
            }

            int columns = Mathf.CeilToInt(Mathf.Sqrt(count));
            int rows = Mathf.CeilToInt((float)count / columns);
            var results = new List<GameObject>(count);

            float offsetX = (columns - 1) * spacing * 0.5f;
            float offsetZ = (rows - 1) * spacing * 0.5f;

            int spawned = 0;
            for (int row = 0; row < rows && spawned < count; row++)
            {
                for (int col = 0; col < columns && spawned < count; col++)
                {
                    Vector3 pos = centerPosition + new Vector3(
                        col * spacing - offsetX,
                        0f,
                        row * spacing - offsetZ);

                    GameObject obj = SpawnAtPosition(prefab, pos, Quaternion.identity);
                    if (obj != null)
                    {
                        results.Add(obj);
                        spawned++;
                    }
                }
            }

            return results.ToArray();
        }

        public GameObject[] SpawnCircle(GameObject prefab, Vector3 centerPosition, int count, float radius)
        {
            if (prefab == null || count <= 0)
            {
                return Array.Empty<GameObject>();
            }

            var results = new GameObject[count];

            for (int i = 0; i < count; i++)
            {
                float angle = (360f / count) * i * Mathf.Deg2Rad;
                Vector3 pos = centerPosition + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
                results[i] = SpawnAtPosition(prefab, pos, Quaternion.identity);
            }

            return results;
        }

        public void ClearSpawnedObjects()
        {
            for (int i = spawnedObjects.Count - 1; i >= 0; i--)
            {
                GameObject obj = spawnedObjects[i];
                if (obj != null)
                {
                    ObjectPoolManager.Release(obj);
                }
            }

            spawnedObjects.Clear();
        }

        private void UpdatePlacementPosition()
        {
            ResolveReferences();

            if (raycastManager == null || arCamera == null)
            {
                if (TryUseBestDetectedPlane())
                {
                    return;
                }

                SetPlacementAvailable(false);
                return;
            }

            Vector2 screenPoint = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            raycastHits.Clear();

            if (raycastManager.Raycast(screenPoint, raycastHits, placementTrackableTypes))
            {
                Pose pose = raycastHits[0].pose;
                currentPlacementPosition = pose.position;
                currentPlacementRotation = pose.rotation;
                ARPlane hitPlane = planeManager != null ? planeManager.GetPlane(raycastHits[0].trackableId) : null;
                EnsureLearningArea(pose, hitPlane);

                if (!placementAvailable)
                {
                    SetPlacementAvailable(true);
                }
                else
                {
                    OnPlacementPositionAvailable?.Invoke(currentPlacementPosition);
                }
            }
            else
            {
                if (!TryUseBestDetectedPlane())
                {
                    SetPlacementAvailable(false);
                }
            }
        }

        private void ResolveReferences()
        {
            XROrigin xrOrigin = null;
            ARSessionBootstrap bootstrap = FindAnyObjectByType<ARSessionBootstrap>();
            if (bootstrap != null)
            {
                bootstrap.ResolveReferences();
                xrOrigin = bootstrap.Origin;

                if (raycastManager == null)
                {
                    raycastManager = bootstrap.RaycastManager;
                }

                if (planeManager == null)
                {
                    planeManager = bootstrap.PlaneManager;
                }

                if (arCamera == null)
                {
                    arCamera = bootstrap.ARCamera;
                }
            }

            if (xrOrigin == null)
            {
                xrOrigin = FindAnyObjectByType<XROrigin>();
            }

            if (raycastManager == null)
            {
                raycastManager = FindAnyObjectByType<ARRaycastManager>();
                if (raycastManager == null)
                {
                    raycastManager = xrOrigin != null
                        ? xrOrigin.GetComponent<ARRaycastManager>()
                        : null;

                    if (raycastManager == null && xrOrigin != null)
                    {
                        raycastManager = xrOrigin.gameObject.AddComponent<ARRaycastManager>();
                    }
                }
            }

            if (planeManager == null)
            {
                planeManager = FindAnyObjectByType<ARPlaneManager>();
            }

            if (planeManager == null && xrOrigin != null)
            {
                planeManager = xrOrigin.gameObject.AddComponent<ARPlaneManager>();
            }

            if (planeManager != null)
            {
                planeManager.requestedDetectionMode = PlaneDetectionMode.Horizontal;
            }

            if (arCamera == null)
            {
                arCamera = Camera.main;
            }
        }

        private void ResolveLearningAreaController()
        {
            if (placementController == null)
            {
                placementController = FindAnyObjectByType<ARPlacementController>();
            }

            if (placementController == null)
            {
                return;
            }

            if (placementController.HasLearningArea)
            {
                learningAreaAnchor = placementController.CurrentLearningArea;
            }

            placementController.OnLearningAreaPlaced -= HandleLearningAreaPlaced;
            placementController.OnLearningAreaPlaced += HandleLearningAreaPlaced;
        }

        private void HandleLearningAreaPlaced(LearningAreaAnchor anchor)
        {
            if (anchor == null)
            {
                return;
            }

            learningAreaAnchor = anchor;
            learningAreaAnchor.SetAreaSize(defaultLearningAreaSizeMeters);
            currentPlacementPosition = anchor.Pose.position;
            currentPlacementRotation = anchor.Pose.rotation;
            HidePlaneVisualizationAfterPlacement();

            if (!placementAvailable)
            {
                SetPlacementAvailable(true);
            }
            else
            {
                OnPlacementPositionAvailable?.Invoke(currentPlacementPosition);
            }

            OnLearningAreaPlaced?.Invoke();
        }

        private void EnsureLearningArea(Pose pose, ARPlane attachedPlane = null)
        {
            if (!autoCreateLearningAreaFromPlacement || HasLearningArea)
            {
                return;
            }

            if (learningAreaAnchor == null)
            {
                GameObject anchorObject = new GameObject("LearningAreaAnchor");
                anchorObject.transform.SetParent(null);
                learningAreaAnchor = anchorObject.AddComponent<LearningAreaAnchor>();
            }

            // Adjust the pose for iOS to ensure proper floor alignment
            Pose adjustedPose = AdjustPoseForPlatform(pose);

            learningAreaAnchor.SetPose(adjustedPose.position, adjustedPose.rotation);
            learningAreaAnchor.SetAreaSize(defaultLearningAreaSizeMeters);
            learningAreaAnchor.Initialize(attachedPlane);
            HidePlaneVisualizationAfterPlacement();

            if (!placementAvailable)
            {
                SetPlacementAvailable(true);
            }

            OnLearningAreaPlaced?.Invoke();
        }

        private Pose AdjustPoseForPlatform(Pose originalPose)
        {
            return originalPose;
        }

        private void HidePlaneVisualizationAfterPlacement()
        {
            if (!hidePlaneVisualizationAfterPlacement)
            {
                return;
            }

            ARPlaneDetectionController planeDetectionController = FindAnyObjectByType<ARPlaneDetectionController>();
            if (planeDetectionController != null)
            {
                planeDetectionController.SetPlaneVisualization(false);
                return;
            }

            if (planeManager == null)
            {
                return;
            }

            foreach (ARPlane plane in planeManager.trackables)
            {
                if (plane == null)
                {
                    continue;
                }

                if (plane.TryGetComponent<ARPlaneMeshVisualizer>(out var meshVisualizer))
                {
                    meshVisualizer.enabled = false;
                }

                if (plane.TryGetComponent<MeshRenderer>(out var meshRenderer))
                {
                    meshRenderer.enabled = false;
                }

                if (plane.TryGetComponent<LineRenderer>(out var lineRenderer))
                {
                    lineRenderer.enabled = false;
                }
            }
        }

        private bool TryUseBestDetectedPlane()
        {
            if (planeManager == null)
            {
                return false;
            }

            ARPlane bestPlane = null;
            float bestArea = 0f;

            foreach (ARPlane plane in planeManager.trackables)
            {
                if (plane == null || plane.trackingState != TrackingState.Tracking)
                {
                    continue;
                }

                if (plane.alignment != PlaneAlignment.HorizontalUp)
                {
                    continue;
                }

                float area = plane.size.x * plane.size.y;
                if (area < minimumPlaneArea || area <= bestArea)
                {
                    continue;
                }

                bestPlane = plane;
                bestArea = area;
            }

            if (bestPlane == null)
            {
                return false;
            }

            currentPlacementPosition = bestPlane.transform.position;
            currentPlacementRotation = bestPlane.transform.rotation;
            EnsureLearningArea(new Pose(currentPlacementPosition, currentPlacementRotation), bestPlane);

            if (!placementAvailable)
            {
                SetPlacementAvailable(true);
            }
            else
            {
                OnPlacementPositionAvailable?.Invoke(currentPlacementPosition);
            }

            return true;
        }

        private void SetPlacementAvailable(bool available)
        {
            if (placementAvailable == available)
            {
                return;
            }

            placementAvailable = available;

            if (available)
            {
                OnPlacementPositionAvailable?.Invoke(currentPlacementPosition);
            }
            else
            {
                OnPlacementPositionLost?.Invoke();
            }
        }

        private void TrackSpawned(GameObject instance)
        {
            if (instance == null)
            {
                return;
            }

            if (addCollidersToSpawned)
            {
                EnsureCollider(instance, defaultColliderRadius);
            }

            spawnedObjects.Add(instance);
        }

        private static void EnsureCollider(GameObject root, float colliderRadius)
        {
            Collider[] colliders = root.GetComponentsInChildren<Collider>();
            if (colliders.Length > 0)
            {
                return;
            }

            Renderer renderer = root.GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                SphereCollider sphere = renderer.gameObject.AddComponent<SphereCollider>();
                sphere.radius = colliderRadius;
                return;
            }

            SphereCollider fallback = root.AddComponent<SphereCollider>();
            fallback.radius = colliderRadius;
        }
    }
}
