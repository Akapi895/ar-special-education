using System;
using System.Collections.Generic;
using Core.Learning.ActivityRunner;
using UnityEngine;

namespace Core.AR.Placement
{
    /// <summary>
    /// Editor/desktop mock for <see cref="IARPlacementService"/> when AR planes are unavailable.
    /// </summary>
    [DisallowMultipleComponent]
    public class ARPlacementServiceMock : MonoBehaviour, IARPlacementService
    {
        [SerializeField]
        private Vector3 mockPlacementPosition = new Vector3(0f, 0.28f, 2.35f);

        [SerializeField]
        private bool alwaysAvailable = true;

        [SerializeField]
        private bool addCollidersToSpawned = true;

        private readonly List<GameObject> spawnedObjects = new List<GameObject>();

        public event Action<Vector3> OnPlacementPositionAvailable;
        public event Action OnPlacementPositionLost;

        public bool IsPlacementAvailable => alwaysAvailable;
        public Vector3 CurrentPlacementPosition => mockPlacementPosition;

        public void Initialize()
        {
            if (alwaysAvailable)
            {
                OnPlacementPositionAvailable?.Invoke(mockPlacementPosition);
            }
            else
            {
                OnPlacementPositionLost?.Invoke();
            }

            Debug.Log("[ARPlacementServiceMock] Initialized (editor/mock mode).");
        }

        public GameObject SpawnAtPlacementPosition(GameObject prefab, Transform parent = null)
        {
            return SpawnAtPosition(prefab, mockPlacementPosition, Quaternion.identity, parent);
        }

        public GameObject SpawnAtPosition(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            if (prefab == null)
            {
                return null;
            }

            GameObject instance = Instantiate(prefab, position, rotation, parent);
            instance.SetActive(true);
            TrackSpawned(instance);
            return instance;
        }

        public GameObject[] SpawnGrid(GameObject prefab, Vector3 centerPosition, int count, float spacing)
        {
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
                    Vector3 pos = centerPosition + new Vector3(col * spacing - offsetX, 0f, row * spacing - offsetZ);
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
                if (spawnedObjects[i] != null)
                {
                    Destroy(spawnedObjects[i]);
                }
            }

            spawnedObjects.Clear();
        }

        private void TrackSpawned(GameObject instance)
        {
            if (instance == null)
            {
                return;
            }

            if (addCollidersToSpawned)
            {
                if (instance.GetComponentInChildren<Collider>() == null)
                {
                    SphereCollider sphere = instance.AddComponent<SphereCollider>();
                    sphere.radius = 0.08f;
                }
            }

            spawnedObjects.Add(instance);
        }
    }
}
