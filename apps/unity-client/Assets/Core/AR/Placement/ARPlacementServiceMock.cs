using System;
using System.Collections.Generic;
using ARSpecialEducation.Core.AR;
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
        private const int XrSimulationEnvironmentLayer = 30;

        [SerializeField]
        private Vector3 mockPlacementPosition = new Vector3(0f, 0f, 3.1f);

        [SerializeField]
        private bool alwaysAvailable = true;

        [SerializeField]
        private bool addCollidersToSpawned = true;

        [SerializeField]
        private Vector2 mockLearningAreaSizeMeters = new Vector2(4.2f, 2.4f);

        [Header("Editor Simulation Preview")]
        [SerializeField]
        private bool alignMockPlacementToCamera = true;

        [SerializeField]
        private bool autoPoseLowEditorCamera = true;

        [SerializeField]
        private float previewCameraHeightMeters = 1.35f;

        [SerializeField]
        private float previewDistanceMeters = 3.1f;

        [SerializeField]
        private float previewLookAtHeightMeters = 0.2f;

        [SerializeField]
        private float minimumPreviewCameraHeight = 0.55f;

        [SerializeField]
        private float simulationSurfaceProbeDistance = 8f;

        [SerializeField]
        private float minimumSurfaceUpDot = 0.72f;

        [SerializeField]
        private bool hideDefaultSimulationEnvironment = true;

        [SerializeField]
        private bool createCleanPreviewSurface = true;

        [SerializeField]
        private float cleanPreviewSurfaceSizeMeters = 12f;

        private readonly List<GameObject> spawnedObjects = new List<GameObject>();
        private LearningAreaAnchor learningAreaAnchor;
        private Quaternion mockPlacementRotation = Quaternion.identity;
        private GameObject cleanPreviewSurface;

        public event Action<Vector3> OnPlacementPositionAvailable;
        public event Action OnPlacementPositionLost;
        public event Action OnLearningAreaPlaced;

        public bool IsPlacementAvailable => alwaysAvailable;
        public Vector3 CurrentPlacementPosition => mockPlacementPosition;
        public bool HasLearningArea => learningAreaAnchor != null && learningAreaAnchor.IsPlaced;
        public Transform LearningAreaContentRoot => HasLearningArea ? learningAreaAnchor.ContentRoot : transform;
        public Vector2 LearningAreaSizeMeters => HasLearningArea ? learningAreaAnchor.AreaSizeMeters : mockLearningAreaSizeMeters;

        public void Initialize()
        {
            if (alwaysAvailable)
            {
                ConfigureEditorSimulationPlacement();
                EnsureMockLearningArea();
                OnPlacementPositionAvailable?.Invoke(mockPlacementPosition);
                OnLearningAreaPlaced?.Invoke();
            }
            else
            {
                OnPlacementPositionLost?.Invoke();
            }

            Debug.Log("[ARPlacementServiceMock] Initialized (editor/mock mode).");
        }

        public GameObject SpawnAtPlacementPosition(GameObject prefab, Transform parent = null)
        {
            return SpawnAtPosition(prefab, mockPlacementPosition, mockPlacementRotation, parent);
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

        private void EnsureMockLearningArea()
        {
            if (HasLearningArea)
            {
                return;
            }

            GameObject anchorObject = new GameObject("MockLearningAreaAnchor");
            anchorObject.transform.SetParent(transform, true);
            learningAreaAnchor = anchorObject.AddComponent<LearningAreaAnchor>();
            learningAreaAnchor.SetPose(mockPlacementPosition, mockPlacementRotation);
            learningAreaAnchor.SetAreaSize(mockLearningAreaSizeMeters);
            learningAreaAnchor.Initialize(null);
        }

        private void ConfigureEditorSimulationPlacement()
        {
            if (!alignMockPlacementToCamera || !Application.isEditor || Application.isMobilePlatform)
            {
                return;
            }

            Camera camera = Camera.main != null ? Camera.main : FindFirstObjectByType<Camera>();
            if (camera == null)
            {
                return;
            }

            EnsureEditorSimulationRendering(camera);
            EnsureCleanEditorSimulationSurface();
            HideDefaultSimulationEnvironmentObjects();
            EnsureStableEditorCameraPose(camera);

            if (TryFindSimulationSurface(camera, out Vector3 surfacePosition, out Quaternion surfaceRotation))
            {
                mockPlacementPosition = surfacePosition;
                mockPlacementRotation = surfaceRotation;
                return;
            }

            mockPlacementPosition = EstimatePlacementOnFlatGround(camera);
            mockPlacementRotation = Quaternion.identity;
        }

        private void EnsureStableEditorCameraPose(Camera camera)
        {
            if (!autoPoseLowEditorCamera || camera == null || camera.transform.position.y >= minimumPreviewCameraHeight)
            {
                return;
            }

            Vector3 target = mockPlacementPosition;
            target.y = previewLookAtHeightMeters;
            Vector3 position = target - Vector3.forward * previewDistanceMeters + Vector3.up * previewCameraHeightMeters;
            Vector3 lookDirection = target - position;
            if (lookDirection.sqrMagnitude < 0.0001f)
            {
                return;
            }

            camera.transform.SetPositionAndRotation(position, Quaternion.LookRotation(lookDirection.normalized, Vector3.up));
        }

        private bool TryFindSimulationSurface(Camera camera, out Vector3 position, out Quaternion rotation)
        {
            Vector2[] probePoints =
            {
                new Vector2(0.5f, 0.42f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0.35f, 0.45f),
                new Vector2(0.65f, 0.45f),
                new Vector2(0.5f, 0.35f)
            };

            for (int i = 0; i < probePoints.Length; i++)
            {
                Ray ray = camera.ViewportPointToRay(new Vector3(probePoints[i].x, probePoints[i].y, 0f));
                int mask = hideDefaultSimulationEnvironment
                    ? Physics.DefaultRaycastLayers & ~(1 << XrSimulationEnvironmentLayer)
                    : Physics.DefaultRaycastLayers;
                if (!Physics.Raycast(ray, out RaycastHit hit, simulationSurfaceProbeDistance, mask, QueryTriggerInteraction.Ignore))
                {
                    continue;
                }

                if (hit.normal.y < minimumSurfaceUpDot || IsOwnLearningAreaHit(hit.collider))
                {
                    continue;
                }

                Vector3 forward = Vector3.ProjectOnPlane(camera.transform.forward, hit.normal);
                if (forward.sqrMagnitude < 0.0001f)
                {
                    forward = Vector3.ProjectOnPlane(Vector3.forward, hit.normal);
                }

                position = hit.point;
                rotation = Quaternion.LookRotation(forward.normalized, hit.normal);
                return true;
            }

            position = default;
            rotation = Quaternion.identity;
            return false;
        }

        private Vector3 EstimatePlacementOnFlatGround(Camera camera)
        {
            Vector3 forward = camera.transform.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.0001f)
            {
                forward = Vector3.forward;
            }
            else
            {
                forward.Normalize();
            }

            Vector3 position = camera.transform.position + forward * previewDistanceMeters;
            position.y = 0f;
            return position;
        }

        private bool IsOwnLearningAreaHit(Collider hitCollider)
        {
            if (hitCollider == null || learningAreaAnchor == null)
            {
                return false;
            }

            return hitCollider.transform == learningAreaAnchor.transform
                || hitCollider.transform.IsChildOf(learningAreaAnchor.transform);
        }

        private void EnsureEditorSimulationRendering(Camera camera)
        {
            if (camera == null)
            {
                return;
            }

            var arBackground = camera.GetComponent<UnityEngine.XR.ARFoundation.ARCameraBackground>();
            if (arBackground != null)
            {
                arBackground.enabled = false;
            }

            camera.cullingMask = ~0;
            if (hideDefaultSimulationEnvironment)
            {
                camera.cullingMask &= ~(1 << XrSimulationEnvironmentLayer);
            }
            camera.nearClipPlane = Mathf.Min(camera.nearClipPlane, 0.01f);
            camera.farClipPlane = Mathf.Max(camera.farClipPlane, 250f);
            camera.fieldOfView = Mathf.Clamp(camera.fieldOfView, 55f, 72f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.62f, 0.78f, 0.92f, 1f);
        }

        private void EnsureCleanEditorSimulationSurface()
        {
            if (!createCleanPreviewSurface || cleanPreviewSurface != null)
            {
                return;
            }

            cleanPreviewSurface = GameObject.CreatePrimitive(PrimitiveType.Plane);
            cleanPreviewSurface.name = "CleanSimulationLearningGround";
            cleanPreviewSurface.transform.SetParent(transform, true);
            cleanPreviewSurface.transform.position = Vector3.zero;
            cleanPreviewSurface.transform.rotation = Quaternion.identity;
            cleanPreviewSurface.transform.localScale = Vector3.one * Mathf.Max(1f, cleanPreviewSurfaceSizeMeters / 10f);

            Renderer renderer = cleanPreviewSurface.GetComponent<Renderer>();
            if (renderer != null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Unlit")
                    ?? Shader.Find("Unlit/Color")
                    ?? Shader.Find("Standard");
                var material = new Material(shader)
                {
                    name = "CleanSimulationGround_Runtime"
                };
                if (material.HasProperty("_BaseColor"))
                {
                    material.SetColor("_BaseColor", new Color(0.52f, 0.82f, 0.68f, 1f));
                }
                if (material.HasProperty("_Color"))
                {
                    material.SetColor("_Color", new Color(0.52f, 0.82f, 0.68f, 1f));
                }
                renderer.sharedMaterial = material;
            }
        }

        private void HideDefaultSimulationEnvironmentObjects()
        {
            if (!hideDefaultSimulationEnvironment)
            {
                return;
            }

            Renderer[] renderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer != null && IsDefaultSimulationEnvironmentObject(renderer.gameObject))
                {
                    renderer.enabled = false;
                }
            }

            Collider[] colliders = FindObjectsByType<Collider>(FindObjectsSortMode.None);
            for (int i = 0; i < colliders.Length; i++)
            {
                Collider collider = colliders[i];
                if (collider != null && IsDefaultSimulationEnvironmentObject(collider.gameObject))
                {
                    collider.enabled = false;
                }
            }
        }

        private static bool IsDefaultSimulationEnvironmentObject(GameObject obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj.layer == XrSimulationEnvironmentLayer)
            {
                return true;
            }

            string sceneName = obj.scene.name;
            return !string.IsNullOrEmpty(sceneName)
                && sceneName.ToLowerInvariant().Contains("simulated environment");
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
