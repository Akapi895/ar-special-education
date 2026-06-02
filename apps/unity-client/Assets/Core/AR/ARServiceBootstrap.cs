using ARSpecialEducation.Core.AR;
using Core.AR.ARSession;
using Core.AR.Interaction;
using Core.AR.Placement;
using Core.Learning.ActivityRunner;
using UnityEngine;

namespace Core.AR
{
    /// <summary>
    /// Locates and initializes AR Core services for learning activities and sandbox testing.
    /// Place once per AR scene (e.g. SC_TestSandbox, SC_ARGameplay).
    /// </summary>
    [DefaultExecutionOrder(-200)]
    public class ARServiceBootstrap : MonoBehaviour
    {
        private static ARServiceBootstrap instance;

        [Header("Mode")]
        [SerializeField]
        private bool usePlacementMockInEditor = true;

        [Header("Optional explicit references")]
        [SerializeField]
        private ARSessionService sessionService;

        [SerializeField]
        private ARPlacementService placementService;

        [SerializeField]
        private ARPlacementServiceMock placementMock;

        [SerializeField]
        private ARInteractionService interactionService;

        public static ARServiceBootstrap Instance => instance;

        public IARSessionService Session { get; private set; }
        public IARPlacementService Placement { get; private set; }
        public IARInteractionService Interaction { get; private set; }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            ResolveServices();
            InitializeServices();
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        private void ResolveServices()
        {
            if (sessionService == null)
            {
                sessionService = FindAnyObjectByType<ARSessionService>();
            }

            if (interactionService == null)
            {
                interactionService = FindAnyObjectByType<ARInteractionService>();
            }

            bool useMock = usePlacementMockInEditor
                && Application.isEditor
                && !Application.isMobilePlatform;

            if (useMock)
            {
                if (placementMock == null)
                {
                    placementMock = FindAnyObjectByType<ARPlacementServiceMock>();
                    if (placementMock == null)
                    {
                        placementMock = gameObject.AddComponent<ARPlacementServiceMock>();
                    }
                }

                Placement = placementMock;
                Session = CreateFallbackSession();
                DisableRealARSession();
            }
            else
            {
                if (placementService == null)
                {
                    placementService = FindAnyObjectByType<ARPlacementService>();
                }

                if (placementService == null && placementMock != null)
                {
                    Placement = placementMock;
                }
                else
                {
                    Placement = placementService;
                }

                Session = sessionService;
            }

            Interaction = interactionService;

            if (Session == null)
            {
                Session = CreateFallbackSession();
            }

            if (Placement == null)
            {
                Debug.LogError("[ARServiceBootstrap] No placement service found or created.");
            }

            if (Interaction == null)
            {
                Debug.LogError("[ARServiceBootstrap] No interaction service found.");
            }
        }

        private void InitializeServices()
        {
            Session?.Initialize();
            Placement?.Initialize();

            Interaction?.Initialize();

            Debug.Log($"[ARServiceBootstrap] Ready. Placement={(Placement != null ? Placement.GetType().Name : "null")}");
        }

        private IARSessionService CreateFallbackSession()
        {
            var fallback = gameObject.GetComponent<ARSessionFallback>();
            if (fallback == null)
            {
                fallback = gameObject.AddComponent<ARSessionFallback>();
            }

            return fallback;
        }

        private static void DisableRealARSession()
        {
            var arSession = FindFirstObjectByType<UnityEngine.XR.ARFoundation.ARSession>();
            if (arSession != null) arSession.enabled = false;

            var arCamBg = FindFirstObjectByType<UnityEngine.XR.ARFoundation.ARCameraBackground>();
            if (arCamBg != null) arCamBg.enabled = false;

            var arSvc = FindFirstObjectByType<ARSessionService>();
            if (arSvc != null) arSvc.enabled = false;
        }
    }
}
