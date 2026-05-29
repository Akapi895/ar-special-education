using UnityEngine;
using System;
using System.Collections;
using Core.Data;
using Core.Support.AudioManager;

namespace Core.Support.FeedbackSystem
{
    /// <summary>
    /// MonoBehaviour proxy for the FeedbackSystem.
    /// Attach to a GameObject in the scene to enable feedback functionality.
    /// Provides a singleton instance for easy access across activities.
    /// Incorporates SimpleAudioManager and visual particle prefabs.
    /// </summary>
    public class FeedbackServiceProxy : MonoBehaviour
    {
        private static FeedbackServiceProxy instance;
        private static FeedbackSystem feedbackSystem;

        [Header("Configuration")]
        [Tooltip("Feedback config asset to use")]
        [SerializeField]
        private FeedbackConfig feedbackConfig;

        [Header("Feedback Particles Prefabs (Optional)")]
        [SerializeField] private GameObject correctVfxPrefab;
        [SerializeField] private GameObject incorrectVfxPrefab;
        [SerializeField] private GameObject successVfxPrefab;
        [SerializeField] private float vfxDestroyDelay = 3f;

        private static Texture2D starParticleTexture;
        private static Material starParticleMaterial;

        /// <summary>
        /// Get the singleton FeedbackSystem instance.
        /// </summary>
        public static FeedbackSystem Instance
        {
            get
            {
                EnsureFeedbackSystemInitialized();
                return feedbackSystem;
            }
        }

        /// <summary>
        /// Initialize the feedback service without creating scene lifecycle objects.
        /// </summary>
        public static void Initialize()
        {
            EnsureFeedbackSystemInitialized();
        }

        private static void EnsureFeedbackSystemInitialized()
        {
            if (feedbackSystem == null)
            {
                feedbackSystem = new FeedbackSystem();
            }

            if (instance != null && instance.feedbackConfig != null)
            {
                feedbackSystem.Initialize(instance.feedbackConfig);
            }
        }

        private void Awake()
        {
            // Singleton pattern
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            // Initialize feedback system
            EnsureFeedbackSystemInitialized();

            // Subscribe to events
            feedbackSystem.OnSoundEffectRequested += HandleSoundRequested;
            feedbackSystem.OnVisualEffectRequested += HandleVisualEffectRequested;
        }

        /// <summary>
        /// Handle sound effect requests.
        /// Plays SFX via SimpleAudioManager.
        /// </summary>
        private void HandleSoundRequested(string soundName)
        {
            SimpleAudioManager.EnsureExists().PlaySound(soundName);
        }

        /// <summary>
        /// Handle visual effect requests.
        /// Spawns particle prefabs.
        /// </summary>
        private void HandleVisualEffectRequested(string effectName)
        {
            if (!UserPreferences.AnimationsEnabled)
            {
                return;
            }

            GameObject prefabToSpawn = null;
            string lowerName = effectName.ToLower();

            if (lowerName.Contains("correct") || lowerName.Contains("praise") || lowerName.Contains("win"))
            {
                prefabToSpawn = correctVfxPrefab;
            }
            else if (lowerName.Contains("incorrect") || lowerName.Contains("fail") || lowerName.Contains("encouragement"))
            {
                prefabToSpawn = incorrectVfxPrefab;
            }
            else if (lowerName.Contains("success") || lowerName.Contains("complete") || lowerName.Contains("congrats"))
            {
                prefabToSpawn = successVfxPrefab;
            }

            if (prefabToSpawn != null)
            {
                try
                {
                    // Spawn at center of camera or at (0, 0, 0)
                    Camera cam = Camera.main;
                    Vector3 spawnPos = Vector3.zero;
                    Quaternion spawnRot = Quaternion.identity;

                    if (cam != null)
                    {
                        // Spawn 2 meters in front of main camera so it's visible in AR/3D space
                        spawnPos = cam.transform.position + cam.transform.forward * 2f;
                        spawnRot = cam.transform.rotation;
                    }

                    GameObject vfxInstance = Instantiate(prefabToSpawn, spawnPos, spawnRot);
                    Destroy(vfxInstance, vfxDestroyDelay);

#if UNITY_EDITOR
                    Debug.Log($"[FeedbackServiceProxy] Spawned VFX: {effectName}");
#endif
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[FeedbackServiceProxy] Failed to spawn VFX '{effectName}': {ex.Message}");
                }
            }
            else
            {
                SpawnProceduralVfx(effectName);
            }
        }

        private void SpawnProceduralVfx(string effectName)
        {
            Camera cam = Camera.main;
            Vector3 spawnPos = cam != null ? cam.transform.position + cam.transform.forward * 1.8f : Vector3.zero;
            string lowerName = effectName.ToLowerInvariant();
            bool positive = lowerName.Contains("correct") || lowerName.Contains("success") || lowerName.Contains("complete");

            GameObject vfxGo = new GameObject($"Procedural_{effectName}");
            vfxGo.transform.position = spawnPos;

            ParticleSystem particles = vfxGo.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.playOnAwake = false;
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            main.duration = positive ? 0.95f : 0.45f;
            main.loop = false;
            main.startLifetime = positive ? new ParticleSystem.MinMaxCurve(0.65f, 1.15f) : new ParticleSystem.MinMaxCurve(0.28f, 0.55f);
            main.startSpeed = positive ? new ParticleSystem.MinMaxCurve(0.65f, 1.55f) : new ParticleSystem.MinMaxCurve(0.18f, 0.42f);
            main.startSize = positive ? new ParticleSystem.MinMaxCurve(0.1f, 0.18f) : new ParticleSystem.MinMaxCurve(0.05f, 0.09f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.maxParticles = positive ? 56 : 18;
            main.startColor = ResolveEffectColor(effectName);
            main.gravityModifier = positive ? -0.04f : 0.08f;

            var emission = particles.emission;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)(positive ? 44 : 12)) });

            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = positive ? 0.32f : 0.18f;

            var velocity = particles.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.Local;
            velocity.x = new ParticleSystem.MinMaxCurve(0f, 0f);
            velocity.y = positive ? new ParticleSystem.MinMaxCurve(0.25f, 0.65f) : new ParticleSystem.MinMaxCurve(0.05f, 0.18f);
            velocity.z = new ParticleSystem.MinMaxCurve(0f, 0f);

            var sizeOverLifetime = particles.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 1f, 1f, 0f));

            ParticleSystemRenderer particleRenderer = particles.GetComponent<ParticleSystemRenderer>();
            if (particleRenderer != null)
            {
                particleRenderer.renderMode = ParticleSystemRenderMode.Billboard;
                particleRenderer.material = positive ? GetStarParticleMaterial() : null;
            }

            particles.Play();
            Destroy(vfxGo, vfxDestroyDelay);
        }

        private static Color ResolveEffectColor(string effectName)
        {
            string lowerName = effectName.ToLower();
            if (lowerName.Contains("incorrect") || lowerName.Contains("wrong"))
            {
                return new Color(1f, 0.35f, 0.25f);
            }

            if (lowerName.Contains("boundary"))
            {
                return new Color(1f, 0.82f, 0.2f);
            }

            if (lowerName.Contains("success"))
            {
                return new Color(1f, 0.86f, 0.18f);
            }

            return new Color(1f, 0.9f, 0.22f);
        }

        private static Material GetStarParticleMaterial()
        {
            if (starParticleMaterial != null)
            {
                return starParticleMaterial;
            }

            Shader shader = Shader.Find("Sprites/Default")
                ?? Shader.Find("Universal Render Pipeline/Unlit")
                ?? Shader.Find("Unlit/Transparent");

            starParticleMaterial = new Material(shader)
            {
                name = "FeedbackStarParticle_Runtime",
                mainTexture = GetStarParticleTexture()
            };

            if (starParticleMaterial.HasProperty("_BaseMap"))
            {
                starParticleMaterial.SetTexture("_BaseMap", GetStarParticleTexture());
            }

            if (starParticleMaterial.HasProperty("_BaseColor"))
            {
                starParticleMaterial.SetColor("_BaseColor", Color.white);
            }

            if (starParticleMaterial.HasProperty("_Color"))
            {
                starParticleMaterial.SetColor("_Color", Color.white);
            }

            return starParticleMaterial;
        }

        private static Texture2D GetStarParticleTexture()
        {
            if (starParticleTexture != null)
            {
                return starParticleTexture;
            }

            const int size = 64;
            Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            Vector2[] vertices = CreateStarVertices(center, 28f, 12f);
            starParticleTexture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "FeedbackStarParticleTexture_Runtime",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 point = new Vector2(x, y);
                    bool inside = IsPointInPolygon(point, vertices);
                    starParticleTexture.SetPixel(x, y, inside ? Color.white : Color.clear);
                }
            }

            starParticleTexture.Apply();
            return starParticleTexture;
        }

        private static Vector2[] CreateStarVertices(Vector2 center, float outerRadius, float innerRadius)
        {
            Vector2[] vertices = new Vector2[10];
            for (int i = 0; i < vertices.Length; i++)
            {
                float radius = i % 2 == 0 ? outerRadius : innerRadius;
                float angle = (-90f + i * 36f) * Mathf.Deg2Rad;
                vertices[i] = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            }

            return vertices;
        }

        private static bool IsPointInPolygon(Vector2 point, Vector2[] vertices)
        {
            bool inside = false;
            for (int i = 0, j = vertices.Length - 1; i < vertices.Length; j = i++)
            {
                bool crosses = vertices[i].y > point.y != vertices[j].y > point.y
                    && point.x < (vertices[j].x - vertices[i].x) * (point.y - vertices[i].y) / (vertices[j].y - vertices[i].y) + vertices[i].x;
                if (crosses)
                {
                    inside = !inside;
                }
            }

            return inside;
        }

        /// <summary>
        /// Show feedback with coroutine-based timing.
        /// </summary>
        public void ShowFeedbackWithTiming(FeedbackData data)
        {
            StartCoroutine(ShowFeedbackCoroutine(data));
        }

        private IEnumerator ShowFeedbackCoroutine(FeedbackData data)
        {
            // Trigger the feedback
            Instance.NotifyFeedbackTriggered(data);

            // Play sound effect
            if (!string.IsNullOrEmpty(data.SoundEffect))
            {
                Instance.PlaySound(data.SoundEffect);
            }

            // Wait for duration
            yield return new WaitForSeconds(data.DisplayDuration > 0 ? data.DisplayDuration : 2f);

            // Complete
            Instance.NotifyFeedbackComplete();
        }

        private void OnDestroy()
        {
            if (feedbackSystem != null)
            {
                feedbackSystem.OnSoundEffectRequested -= HandleSoundRequested;
                feedbackSystem.OnVisualEffectRequested -= HandleVisualEffectRequested;
            }
        }

        /// <summary>
        /// Convenience method for showing correct feedback.
        /// </summary>
        public void ShowCorrect(string message = null)
        {
            Instance.ShowCorrect(message ?? Instance.GetRandomPraise());
        }

        /// <summary>
        /// Convenience method for showing incorrect feedback.
        /// </summary>
        public void ShowIncorrect(string message = null)
        {
            Instance.ShowIncorrect(message ?? Instance.GetRandomEncouragement());
        }

        /// <summary>
        /// Convenience method for showing hint feedback.
        /// </summary>
        public void ShowHint(string message)
        {
            Instance.ShowHint(message);
        }

        /// <summary>
        /// Convenience method for showing success feedback.
        /// </summary>
        public void ShowSuccess(string message = null)
        {
            Instance.ShowSuccess(message ?? Core.UI.Localization.SimpleLocalization.Get("feedback_success"));
        }
    }
}
