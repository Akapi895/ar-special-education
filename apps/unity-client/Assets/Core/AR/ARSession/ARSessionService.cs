using System;
using System.Collections;
using Core.Learning.ActivityRunner;
using Core.UI.Components;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

namespace Core.AR.ARSession
{
    /// <summary>
    /// AR Foundation implementation of <see cref="IARSessionService"/>.
    /// Attach to the same hierarchy as <see cref="ARSession"/> (typically under XR Origin).
    /// </summary>
    [DisallowMultipleComponent]
    public class ARSessionService : MonoBehaviour, IARSessionService
    {
        [SerializeField]
        private UnityEngine.XR.ARFoundation.ARSession arSession;

        [SerializeField]
        private bool autoStartSession = true;

        [Header("Native Camera Background")]
        [SerializeField]
        private bool ensureNativeCameraBackground = true;

        [SerializeField]
        private bool allowWebCamTextureFallback = true;

        [SerializeField]
        private float fallbackDelaySeconds = 2.5f;

        [SerializeField]
        private int requestedCameraFps = 30;

        private bool sessionReady;
        private TrackingQuality trackingQuality = TrackingQuality.None;
        private bool initialized;
        private bool fallbackEvaluationStarted;
        private UnityEngine.Camera arCamera;
        private ARCameraBackground arCameraBackground;
        private WebCamTexture fallbackTexture;
        private GameObject fallbackQuad;
        private Material fallbackMaterial;
        private bool fallbackStarted;

        private static readonly int MainTexId = Shader.PropertyToID("_MainTex");

        public event Action OnSessionReady;
        public event Action OnSessionLost;

        public bool IsSessionReady => sessionReady;
        public bool IsTrackingStable =>
            sessionReady && trackingQuality >= TrackingQuality.Good;

        public TrackingQuality TrackingQuality => trackingQuality;

        private void Awake()
        {
            EnsureSessionReference();
        }

        private void OnEnable()
        {
            UnityEngine.XR.ARFoundation.ARSession.stateChanged += OnARSessionStateChanged;
            Debug.Log("[ARSessionService] ARSession state changed handler subscribed.");
        }

        private void OnDisable()
        {
            UnityEngine.XR.ARFoundation.ARSession.stateChanged -= OnARSessionStateChanged;
        }

        public void Initialize()
        {
            EnsureSessionReference();
            EnsureCameraBackground();

            if (arSession == null)
            {
                Debug.LogWarning("[ARSessionService] ARSession unavailable. Falling back to not-ready tracking state.");
                SetSessionReady(false);
                return;
            }

            if (initialized)
            {
                return;
            }

            initialized = true;
            UpdateFromSessionState(UnityEngine.XR.ARFoundation.ARSession.state);
            Debug.Log($"[ARSessionService] Initial ARSession state: {UnityEngine.XR.ARFoundation.ARSession.state}");
            Debug.Log($"[ARSessionService] Platform: {Application.platform}, Unity: {Application.unityVersion}");
            Debug.Log($"[ARSessionService] Device: {SystemInfo.deviceModel}, OS: {SystemInfo.operatingSystem}");
            CheckARSupport();
            StartFallbackEvaluationIfNeeded();
            Debug.Log("[ARSessionService] Initialized.");
        }

        public void StartSession()
        {
            EnsureSessionReference();

            if (arSession == null)
            {
                return;
            }

            if (arSession.subsystem != null && !arSession.subsystem.running)
            {
                arSession.subsystem.Start();
            }
        }

        public void StopSession()
        {
            if (arSession?.subsystem != null && arSession.subsystem.running)
            {
                arSession.subsystem.Stop();
            }

            SetSessionReady(false);
        }

        public void ResetSession()
        {
            EnsureSessionReference();

            if (arSession == null)
            {
                return;
            }

            arSession.Reset();
            SetSessionReady(false);
        }

        private void Start()
        {
            Initialize();

            if (autoStartSession)
            {
                StartSession();
            }
        }

        private void OnDestroy()
        {
            StopFallbackCamera();
        }

        private void EnsureSessionReference()
        {
            if (arSession != null)
            {
                return;
            }

            arSession = FindAnyObjectByType<UnityEngine.XR.ARFoundation.ARSession>();
            if (arSession != null)
            {
                return;
            }

            var sessionObject = new GameObject("AR Session");
            arSession = sessionObject.AddComponent<UnityEngine.XR.ARFoundation.ARSession>();
            Debug.Log("[ARSessionService] Created missing ARSession component.");
        }

        private void EnsureCameraBackground()
        {
            if (!ensureNativeCameraBackground || !ShouldRunNativeCameraPath())
            {
                return;
            }

            arCamera = ResolveCamera();
            if (arCamera == null)
            {
                Debug.LogWarning("[ARSessionService] Cannot attach ARCameraBackground because no camera was found.");
                return;
            }

            arCameraBackground = arCamera.GetComponent<ARCameraBackground>();
            if (arCameraBackground == null)
            {
                arCameraBackground = arCamera.gameObject.AddComponent<ARCameraBackground>();
                Debug.Log("[ARSessionService] Added ARCameraBackground to the AR camera.");
            }

            arCameraBackground.enabled = true;
        }

        private UnityEngine.Camera ResolveCamera()
        {
            XROrigin xrOrigin = FindAnyObjectByType<XROrigin>();
            if (xrOrigin != null && xrOrigin.Camera != null)
            {
                return xrOrigin.Camera;
            }

            return UnityEngine.Camera.main != null
                ? UnityEngine.Camera.main
                : FindFirstObjectByType<UnityEngine.Camera>();
        }

        private void StartFallbackEvaluationIfNeeded()
        {
            if (!allowWebCamTextureFallback || fallbackEvaluationStarted || !ShouldRunNativeCameraPath())
            {
                return;
            }

            fallbackEvaluationStarted = true;
            StartCoroutine(EvaluateFallbackCamera());
        }

        private IEnumerator EvaluateFallbackCamera()
        {
            yield return new WaitForSeconds(fallbackDelaySeconds);

            if (ShouldStartFallbackCamera())
            {
                yield return StartFallbackCamera();
            }
        }

        private bool ShouldStartFallbackCamera()
        {
            if (fallbackStarted)
            {
                return false;
            }

            if (arCameraBackground == null || !arCameraBackground.enabled)
            {
                return true;
            }

            ARSessionState state = UnityEngine.XR.ARFoundation.ARSession.state;
            return state == ARSessionState.None || state == ARSessionState.Unsupported;
        }

        private IEnumerator StartFallbackCamera()
        {
            arCamera = arCamera != null ? arCamera : ResolveCamera();
            if (arCamera == null)
            {
                yield break;
            }

            if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
            {
                Debug.Log("[ARSessionService] Requesting webcam permission for AR fallback...");
                yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
            }

            if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
            {
                Debug.LogError("[ARSessionService] Webcam permission DENIED. Fallback camera cannot start.");
                yield break;
            }
            Debug.Log("[ARSessionService] Webcam permission GRANTED. Starting fallback camera...");

            WebCamDevice? selectedDevice = SelectBackCamera();
            fallbackTexture = selectedDevice.HasValue
                ? new WebCamTexture(selectedDevice.Value.name, Screen.width, Screen.height, requestedCameraFps)
                : new WebCamTexture(Screen.width, Screen.height, requestedCameraFps);
            fallbackTexture.Play();

            EnsureFallbackQuad();
            fallbackMaterial.SetTexture(MainTexId, fallbackTexture);
            fallbackStarted = true;
            Debug.Log("[ARSessionService] Started WebCamTexture camera fallback.");
        }

        private static WebCamDevice? SelectBackCamera()
        {
            WebCamDevice[] devices = WebCamTexture.devices;
            if (devices == null || devices.Length == 0)
            {
                return null;
            }

            for (int i = 0; i < devices.Length; i++)
            {
                if (!devices[i].isFrontFacing)
                {
                    return devices[i];
                }
            }

            return devices[0];
        }

        private void EnsureFallbackQuad()
        {
            if (fallbackQuad != null || arCamera == null)
            {
                return;
            }

            fallbackQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            fallbackQuad.name = "NativeCameraFallbackBackground";
            fallbackQuad.transform.SetParent(arCamera.transform, false);

            Collider quadCollider = fallbackQuad.GetComponent<Collider>();
            if (quadCollider != null)
            {
                Destroy(quadCollider);
            }

            Shader shader = Shader.Find("Hidden/ARSpecialEducation/NativeCameraBackground")
                ?? Shader.Find("Unlit/Texture")
                ?? Shader.Find("Universal Render Pipeline/Unlit");
            fallbackMaterial = new Material(shader)
            {
                name = "NativeCameraFallbackBackground_Runtime"
            };

            Renderer renderer = fallbackQuad.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = fallbackMaterial;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = false;
            }

            UpdateFallbackQuad();
        }

        private void LateUpdate()
        {
            if (fallbackStarted)
            {
                if (Time.frameCount % 300 == 0)
                {
                    Debug.Log($"[ARSessionService] Fallback camera quad active. Position: {fallbackQuad?.transform.position}");
                }
                UpdateFallbackQuad();
            }
        }

        private void UpdateFallbackQuad()
        {
            if (fallbackQuad == null || arCamera == null)
            {
                return;
            }

            float distance = Mathf.Max(arCamera.nearClipPlane + 0.05f, 0.12f);
            float height = 2f * distance * Mathf.Tan(arCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            float width = height * arCamera.aspect;

            fallbackQuad.transform.localPosition = new Vector3(0f, 0f, distance);
            fallbackQuad.transform.localRotation = Quaternion.Euler(
                0f,
                0f,
                fallbackTexture != null ? -fallbackTexture.videoRotationAngle : 0f);
            fallbackQuad.transform.localScale = new Vector3(width, height, 1f);

            if (fallbackMaterial != null && fallbackTexture != null)
            {
                bool mirrored = fallbackTexture.videoVerticallyMirrored;
                fallbackMaterial.mainTextureScale = mirrored ? new Vector2(1f, -1f) : Vector2.one;
                fallbackMaterial.mainTextureOffset = mirrored ? new Vector2(0f, 1f) : Vector2.zero;
            }
        }

        private void StopFallbackCamera()
        {
            if (fallbackTexture != null)
            {
                if (fallbackTexture.isPlaying)
                {
                    fallbackTexture.Stop();
                }

                Destroy(fallbackTexture);
                fallbackTexture = null;
            }

            if (fallbackMaterial != null)
            {
                Destroy(fallbackMaterial);
                fallbackMaterial = null;
            }

            if (fallbackQuad != null)
            {
                Destroy(fallbackQuad);
                fallbackQuad = null;
            }

            fallbackStarted = false;
        }

        private static bool ShouldRunNativeCameraPath()
        {
            return Application.platform == RuntimePlatform.IPhonePlayer
                || Application.platform == RuntimePlatform.Android;
        }

        private ARSessionState previousSessionState = ARSessionState.None;

        private void OnARSessionStateChanged(ARSessionStateChangedEventArgs args)
        {
            Debug.Log($"[ARSessionService] ARSession state changed: {previousSessionState} -> {args.state}");
            previousSessionState = args.state;
            UpdateFromSessionState(args.state);
        }

        private void UpdateFromSessionState(ARSessionState state)
        {
            switch (state)
            {
                case ARSessionState.SessionInitializing:
                    trackingQuality = TrackingQuality.Fair;
                    SetSessionReady(false);
                    break;
                case ARSessionState.SessionTracking:
                    trackingQuality = TrackingQuality.Good;
                    SetSessionReady(true);
                    break;
                case ARSessionState.Ready:
                    trackingQuality = TrackingQuality.Fair;
                    SetSessionReady(true);
                    break;
                case ARSessionState.None:
                case ARSessionState.Unsupported:
                case ARSessionState.CheckingAvailability:
                case ARSessionState.NeedsInstall:
                case ARSessionState.Installing:
                    trackingQuality = TrackingQuality.None;
                    SetSessionReady(false);
                    break;
                default:
                    trackingQuality = TrackingQuality.Fair;
                    SetSessionReady(false);
                    break;
            }
        }

        private void ShowTrackingLostMessage()
        {
            Debug.Log("[ARSessionService] Tracking lost or not tracking. Showing recovery message.");
        }

        private void CheckARSupport()
        {
#if UNITY_IOS && !UNITY_EDITOR
            if (UnityEngine.XR.ARFoundation.ARSession.state == ARSessionState.Unsupported)
            {
                Debug.LogError("[ARSessionService] ARKit is not supported on this device.");
                ShowARUnsupportedMessage();
                return;
            }

            StartCoroutine(CheckARAvailability());
#endif
        }

        private IEnumerator CheckARAvailability()
        {
            yield return null;

            if (UnityEngine.XR.ARFoundation.ARSession.state == ARSessionState.Unsupported)
            {
                Debug.LogError("[ARSessionService] ARKit is not supported on this device.");
                ShowARUnsupportedMessage();
            }
        }

        private void ShowARUnsupportedMessage()
        {
            var canvasGo = new GameObject("ARUnsupportedCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;
            canvasGo.transform.SetParent(null);
            DontDestroyOnLoad(canvasGo);

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            var panelGo = new GameObject("MessagePanel", typeof(RectTransform), typeof(Image));
            panelGo.transform.SetParent(canvas.transform, false);
            var panelRect = panelGo.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(700f, 300f);
            panelRect.anchoredPosition = Vector2.zero;
            var panelImage = panelGo.GetComponent<Image>();
            panelImage.color = new Color(0.12f, 0.12f, 0.14f, 0.95f);

            var textGo = new GameObject("MessageText", typeof(RectTransform), typeof(Text));
            textGo.transform.SetParent(panelRect, false);
            var textRect = textGo.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(20f, 20f);
            textRect.offsetMax = new Vector2(-20f, -20f);
            var text = textGo.GetComponent<Text>();
            text.text = "Thiết bị này không hỗ trợ AR.\nVui lòng thử trên thiết bị iOS có ARKit.";
            text.fontSize = 30;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = new Color(1f, 0.86f, 0.36f, 1f);
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.font = UIKidFriendlyStyle.GetSharedFont();
        }

        private void SetSessionReady(bool ready)
        {
            if (sessionReady == ready)
            {
                return;
            }

            sessionReady = ready;

            if (ready)
            {
                OnSessionReady?.Invoke();
                Debug.Log($"[ARSessionService] Session state changed to ready. Ready: {sessionReady}, Tracking: {trackingQuality}");
            }
            else
            {
                OnSessionLost?.Invoke();
                ShowTrackingLostMessage();
            }
        }
    }
}
