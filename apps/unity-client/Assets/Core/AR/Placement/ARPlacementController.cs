using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace ARSpecialEducation.Core.AR
{
    [DisallowMultipleComponent]
    public sealed class ARPlacementController : MonoBehaviour
    {
        static readonly List<ARRaycastHit> RaycastHits = new List<ARRaycastHit>();

        [SerializeField] ARRaycastManager raycastManager;
        [SerializeField] ARPlaneManager planeManager;
        [SerializeField] GameObject learningAreaMarkerPrefab;
        [SerializeField] GameObject defaultSpawnPrefab;
        [SerializeField] TrackableType placementTrackableTypes = TrackableType.PlaneWithinPolygon;
        [SerializeField] bool allowTapToPlaceLearningArea = true;
        [SerializeField] bool allowRepositionLearningArea;
        [SerializeField] bool parentSpawnedObjectsUnderLearningArea = true;
        [SerializeField] bool spawnDefaultObjectAfterPlacement = true;
        [SerializeField] Vector3 defaultSpawnLocalOffset = new Vector3(0f, 0.08f, 0f);

        readonly List<GameObject> spawnedObjects = new List<GameObject>();

        public event Action<LearningAreaAnchor> OnLearningAreaPlaced;
        public event Action<GameObject> OnObjectSpawned;
        public event Action<string> OnPlacementFailed;

        public LearningAreaAnchor CurrentLearningArea { get; private set; }
        public IReadOnlyList<GameObject> SpawnedObjects => spawnedObjects;
        public bool HasLearningArea => CurrentLearningArea != null && CurrentLearningArea.IsPlaced;

        void Awake()
        {
            ResolveReferences();
        }

        void Update()
        {
            if (!allowTapToPlaceLearningArea ||
                (HasLearningArea && !allowRepositionLearningArea) ||
                IsPointerOverUI() ||
                !TryGetPointerDown(out var screenPosition))
            {
                return;
            }

            TryPlaceLearningAreaFromScreen(screenPosition);
        }

        public void ResolveReferences()
        {
            if (raycastManager == null)
            {
                raycastManager = FindFirstObjectByType<ARRaycastManager>();
            }

            if (planeManager == null)
            {
                planeManager = FindFirstObjectByType<ARPlaneManager>();
            }
        }

        public bool TryPlaceLearningAreaFromScreen(Vector2 screenPosition)
        {
            ResolveReferences();

            if (raycastManager == null)
            {
                NotifyPlacementFailed("Cannot place learning area because ARRaycastManager is missing.");
                return false;
            }

            if (!raycastManager.Raycast(screenPosition, RaycastHits, placementTrackableTypes))
            {
                NotifyPlacementFailed("No valid AR plane was hit for learning area placement.");
                return false;
            }

            var hit = RaycastHits[0];
            var attachedPlane = planeManager != null ? planeManager.GetPlane(hit.trackableId) : null;
            PlaceLearningArea(hit.pose, attachedPlane);
            return true;
        }

        public LearningAreaAnchor PlaceLearningArea(Vector3 position, Quaternion rotation)
        {
            return PlaceLearningArea(new Pose(position, rotation), null);
        }

        public LearningAreaAnchor PlaceLearningArea(Pose pose, ARPlane attachedPlane = null)
        {
            if (CurrentLearningArea != null)
            {
                DestroyGameObject(CurrentLearningArea.gameObject);
                CurrentLearningArea = null;
            }

            var marker = learningAreaMarkerPrefab != null
                ? Instantiate(learningAreaMarkerPrefab, pose.position, pose.rotation)
                : new GameObject("LearningAreaAnchor");

            if (!marker.TryGetComponent<LearningAreaAnchor>(out var anchor))
            {
                anchor = marker.AddComponent<LearningAreaAnchor>();
            }

            anchor.SetPose(pose.position, pose.rotation);
            anchor.Initialize(attachedPlane);
            CurrentLearningArea = anchor;

            OnLearningAreaPlaced?.Invoke(anchor);

            if (spawnDefaultObjectAfterPlacement && defaultSpawnPrefab != null)
            {
                SpawnDefaultObjectAtLearningArea();
            }

            return anchor;
        }

        public GameObject SpawnDefaultObjectAtLearningArea()
        {
            if (defaultSpawnPrefab == null)
            {
                NotifyPlacementFailed("Default AR spawn prefab is not assigned.");
                return null;
            }

            if (!HasLearningArea)
            {
                NotifyPlacementFailed("Cannot spawn default object before a learning area is placed.");
                return null;
            }

            var spawnTransform = CurrentLearningArea.ContentRoot;
            var spawnPosition = spawnTransform.TransformPoint(defaultSpawnLocalOffset);
            return SpawnObject(defaultSpawnPrefab, spawnPosition, spawnTransform.rotation);
        }

        public GameObject SpawnObject(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (prefab == null)
            {
                NotifyPlacementFailed("Cannot spawn a null AR prefab.");
                return null;
            }

            var parent = parentSpawnedObjectsUnderLearningArea && HasLearningArea
                ? CurrentLearningArea.ContentRoot
                : null;

            var instance = Instantiate(prefab, position, rotation, parent);
            spawnedObjects.Add(instance);
            OnObjectSpawned?.Invoke(instance);
            return instance;
        }

        public void ClearSpawnedObjects()
        {
            for (var i = spawnedObjects.Count - 1; i >= 0; i--)
            {
                var spawnedObject = spawnedObjects[i];
                if (spawnedObject != null)
                {
                    DestroyGameObject(spawnedObject);
                }
            }

            spawnedObjects.Clear();
        }

        public void ClearLearningArea(bool clearSpawnedObjects = true)
        {
            if (clearSpawnedObjects)
            {
                ClearSpawnedObjects();
            }

            if (CurrentLearningArea != null)
            {
                DestroyGameObject(CurrentLearningArea.gameObject);
                CurrentLearningArea = null;
            }
        }

        void NotifyPlacementFailed(string reason)
        {
            Debug.LogWarning(reason, this);
            OnPlacementFailed?.Invoke(reason);
        }

        static bool TryGetPointerDown(out Vector2 position)
        {
            if (Touchscreen.current != null)
            {
                var touch = Touchscreen.current.primaryTouch;
                if (touch.press.wasPressedThisFrame)
                {
                    position = touch.position.ReadValue();
                    return true;
                }
            }

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                position = Mouse.current.position.ReadValue();
                return true;
            }

            position = default;
            return false;
        }

        static bool IsPointerOverUI()
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(-1);
        }

        static void DestroyGameObject(GameObject target)
        {
            if (Application.isPlaying)
            {
                Destroy(target);
            }
            else
            {
                DestroyImmediate(target);
            }
        }
    }
}
