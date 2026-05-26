using System;
using System.Collections.Generic;
using ARSpecialEducation.Core.AR;
using Core.Learning.ActivityRunner;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace Core.AR.Placement
{
    /// <summary>
    /// AR Foundation placement and spawn service implementing <see cref="IARPlacementService"/>.
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

        private readonly List<GameObject> spawnedObjects = new List<GameObject>();
        private readonly List<ARRaycastHit> raycastHits = new List<ARRaycastHit>();

        private Vector3 currentPlacementPosition;
        private Quaternion currentPlacementRotation = Quaternion.identity;
        private bool placementAvailable;

        public event Action<Vector3> OnPlacementPositionAvailable;
        public event Action OnPlacementPositionLost;

        public bool IsPlacementAvailable => placementAvailable;
        public Vector3 CurrentPlacementPosition => currentPlacementPosition;
        public bool HasLearningArea => learningAreaAnchor != null && learningAreaAnchor.IsPlaced;
        public Transform LearningAreaContentRoot => HasLearningArea ? learningAreaAnchor.ContentRoot : transform;
        public Vector2 LearningAreaSizeMeters => HasLearningArea ? learningAreaAnchor.AreaSizeMeters : defaultLearningAreaSizeMeters;

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnDestroy()
        {
            if (placementController != null)
            {
                placementController.OnLearningAreaPlaced -= HandleLearningAreaPlaced;
            }
        }

        private void Update()
        {
            UpdatePlacementPosition();
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

            Debug.Log("[ARPlacementService] Initialized.");
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

            Transform resolvedParent = parent != null ? parent : LearningAreaContentRoot;
            GameObject instance = Instantiate(prefab, position, rotation, resolvedParent);
            instance.SetActive(true);
            TrackSpawned(instance);
            return instance;
        }

        public GameObject SpawnAtLearningAreaPosition(GameObject prefab, Vector3 localPosition, Quaternion localRotation, Transform parent = null)
        {
            Transform root = LearningAreaContentRoot;
            Vector3 worldPosition = root != null ? root.TransformPoint(localPosition) : localPosition;
            Quaternion worldRotation = root != null ? root.rotation * localRotation : localRotation;
            return SpawnAtPosition(prefab, worldPosition, worldRotation, parent != null ? parent : root);
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
                    Destroy(obj);
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
                if (planeManager == null)
                {
                    planeManager = xrOrigin != null
                        ? xrOrigin.GetComponent<ARPlaneManager>()
                        : null;

                    if (planeManager == null && xrOrigin != null)
                    {
                        planeManager = xrOrigin.gameObject.AddComponent<ARPlaneManager>();
                    }
                }
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

            if (!placementAvailable)
            {
                SetPlacementAvailable(true);
            }
            else
            {
                OnPlacementPositionAvailable?.Invoke(currentPlacementPosition);
            }
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
                anchorObject.transform.SetParent(transform, true);
                learningAreaAnchor = anchorObject.AddComponent<LearningAreaAnchor>();
            }

            learningAreaAnchor.SetPose(pose.position, pose.rotation);
            learningAreaAnchor.SetAreaSize(defaultLearningAreaSizeMeters);
            learningAreaAnchor.Initialize(attachedPlane);
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
