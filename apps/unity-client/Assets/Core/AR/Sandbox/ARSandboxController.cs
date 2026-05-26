using Core.AR.Placement;
using Core.Learning.ActivityRunner;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Core.AR.Sandbox
{
    /// <summary>
    /// Manual test harness for AR services in SC_TestSandbox.
    /// Keyboard: G = spawn grid, C = spawn circle, X = clear, T = log tap test.
    /// </summary>
    public class ARSandboxController : MonoBehaviour
    {
        [SerializeField]
        private GameObject testPrefab;

        [SerializeField]
        private int gridCount = 6;

        [SerializeField]
        private float gridSpacing = 0.25f;

        [SerializeField]
        private int circleCount = 5;

        [SerializeField]
        private float circleRadius = 0.35f;

        private IARPlacementService placement;
        private IARInteractionService interaction;

        private void Start()
        {
            if (testPrefab == null)
            {
                testPrefab = CreateDefaultTestPrefab();
            }

            if (ARServiceBootstrap.Instance != null)
            {
                placement = ARServiceBootstrap.Instance.Placement;
                interaction = ARServiceBootstrap.Instance.Interaction;
            }

            if (interaction != null)
            {
                interaction.OnObjectTapped += OnObjectTapped;
            }

            LogStatus();
        }

        private void OnDestroy()
        {
            if (interaction != null)
            {
                interaction.OnObjectTapped -= OnObjectTapped;
            }
        }

        private void Update()
        {
            if (placement == null)
            {
                return;
            }

            if (Keyboard.current != null && Keyboard.current.gKey.wasPressedThisFrame)
            {
                SpawnGridTest();
            }

            if (Keyboard.current != null && Keyboard.current.cKey.wasPressedThisFrame)
            {
                SpawnCircleTest();
            }

            if (Keyboard.current != null && Keyboard.current.xKey.wasPressedThisFrame)
            {
                placement.ClearSpawnedObjects();
                Debug.Log("[ARSandbox] Cleared spawned objects.");
            }

            if (Keyboard.current != null && Keyboard.current.lKey.wasPressedThisFrame)
            {
                LogStatus();
            }
        }

        private void SpawnGridTest()
        {
            if (!placement.IsPlacementAvailable)
            {
                Debug.LogWarning("[ARSandbox] Placement not available yet.");
                return;
            }

            Vector3 center = placement.CurrentPlacementPosition;
            GameObject[] spawned = placement.SpawnGrid(testPrefab, center, gridCount, gridSpacing);
            RegisterForTap(spawned);
            Debug.Log($"[ARSandbox] Spawned grid: {spawned.Length} objects at {center}");
        }

        private void SpawnCircleTest()
        {
            if (!placement.IsPlacementAvailable)
            {
                Debug.LogWarning("[ARSandbox] Placement not available yet.");
                return;
            }

            Vector3 center = placement.CurrentPlacementPosition;
            GameObject[] spawned = placement.SpawnCircle(testPrefab, center, circleCount, circleRadius);
            RegisterForTap(spawned);
            Debug.Log($"[ARSandbox] Spawned circle: {spawned.Length} objects at {center}");
        }

        private void RegisterForTap(GameObject[] objects)
        {
            if (interaction == null || objects == null)
            {
                return;
            }

            foreach (GameObject obj in objects)
            {
                if (obj != null)
                {
                    interaction.RegisterInteractable(obj, obj.name);
                }
            }
        }

        private void OnObjectTapped(GameObject tapped)
        {
            Debug.Log($"[ARSandbox] OnObjectTapped: {tapped.name}");
            interaction?.SetHighlight(tapped, true);
        }

        private void LogStatus()
        {
            var bootstrap = ARServiceBootstrap.Instance;
            string placementName = placement != null ? placement.GetType().Name : "null";
            bool available = placement != null && placement.IsPlacementAvailable;
            Vector3 pos = placement != null ? placement.CurrentPlacementPosition : Vector3.zero;
            Debug.Log($"[ARSandbox] Placement={placementName}, Available={available}, Position={pos}");
        }

        private static GameObject CreateDefaultTestPrefab()
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = "PFB_SandboxTestSphere";
            sphere.transform.localScale = Vector3.one * 0.12f;
            sphere.SetActive(false);
            return sphere;
        }
    }
}
