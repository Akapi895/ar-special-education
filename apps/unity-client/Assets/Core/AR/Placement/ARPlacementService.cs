using System;
using System.Collections.Generic;
using Core.Learning.ActivityRunner;
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
        private Camera arCamera;

        [SerializeField]
        private bool addCollidersToSpawned = true;

        [SerializeField]
        private float defaultColliderRadius = 0.08f;

        private readonly List<GameObject> spawnedObjects = new List<GameObject>();
        private readonly List<ARRaycastHit> raycastHits = new List<ARRaycastHit>();

        private Vector3 currentPlacementPosition;
        private bool placementAvailable;

        public event Action<Vector3> OnPlacementPositionAvailable;
        public event Action OnPlacementPositionLost;

        public bool IsPlacementAvailable => placementAvailable;
        public Vector3 CurrentPlacementPosition => currentPlacementPosition;

        private void Awake()
        {
            if (raycastManager == null)
            {
                raycastManager = FindAnyObjectByType<ARRaycastManager>();
            }

            if (arCamera == null)
            {
                arCamera = Camera.main;
            }
        }

        private void Update()
        {
            UpdatePlacementPosition();
        }

        public void Initialize()
        {
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

            return SpawnAtPosition(prefab, currentPlacementPosition, Quaternion.identity, parent);
        }

        public GameObject SpawnAtPosition(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            if (prefab == null)
            {
                Debug.LogError("[ARPlacementService] Cannot spawn null prefab.");
                return null;
            }

            GameObject instance = Instantiate(prefab, position, rotation, parent);
            instance.SetActive(true);
            TrackSpawned(instance);
            return instance;
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
            if (raycastManager == null || arCamera == null)
            {
                SetPlacementAvailable(false);
                return;
            }

            Vector2 screenPoint = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            raycastHits.Clear();

            if (raycastManager.Raycast(screenPoint, raycastHits, TrackableType.PlaneWithinPolygon))
            {
                Pose pose = raycastHits[0].pose;
                currentPlacementPosition = pose.position;

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
                SetPlacementAvailable(false);
            }
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
