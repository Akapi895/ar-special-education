using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Features.Activities
{
    /// <summary>
    /// Helper component to set up activity prefabs at runtime.
    /// Creates placeholder objects if actual prefabs are not assigned.
    /// </summary>
    public class ActivityPrefabSetup : MonoBehaviour
    {
        private static ActivityPrefabSetup instance;

        public static ActivityPrefabSetup Instance => instance;

        [Header("Quantity Match Prefabs")]
        [SerializeField] private GameObject applePrefab;
        [SerializeField] private GameObject carrotPrefab;
        [SerializeField] private GameObject starPrefab;

        [Header("Compare Quantity Prefabs")]
        [SerializeField] private GameObject compareApplePrefab;
        [SerializeField] private GameObject compareCarrotPrefab;

        [Header("Number Line Jump Prefabs")]
        [SerializeField] private GameObject numberTilePrefab;
        [SerializeField] private GameObject jumpCharacterPrefab;

        [Header("Animal Learning Objects")]
        [SerializeField] private GameObject[] animalPrefabs;
        [SerializeField] private bool preferAnimalPrefabs = true;
        [SerializeField] private float learningObjectTargetHeight = 0.48f;
        [SerializeField] private string resourcesAnimalFolder = "ARAnimals";
        [SerializeField] private bool preferGroundedLearningAnimals = true;
        [SerializeField] private bool faceLearningObjectsToCamera = true;
        [SerializeField] private float animalFacingYawOffset;
        [SerializeField] private float randomFacingYaw = 12f;
        [SerializeField] private bool repairLearningObjectMaterials = true;
        [SerializeField] private string learningObjectShaderName = "Universal Render Pipeline/Unlit";
        [SerializeField] private bool boostLearningObjectContrast = true;
        [SerializeField] private float minimumLearningObjectBrightness = 0.58f;
        [SerializeField] private float learningObjectColorLift = 0.32f;

        [Header("Auto-Create Placeholders")]
        [SerializeField] private bool autoCreatePlaceholders = true;
        [SerializeField] private bool showInHierarchy = true;

        private bool usingGeneratedAnimalPrefabs;
        private readonly Dictionary<Material, Material> convertedMaterialCache = new Dictionary<Material, Material>();
        private static readonly string[] NonGroundAnimalKeywords =
        {
            "bird", "sparrow", "eagle", "owl", "parrot",
            "fish", "herring", "shark", "ray", "whale", "dolphin",
            "squid", "octopus"
        };

        private void Awake()
        {
            instance = this;

            if (autoCreatePlaceholders)
            {
                CreatePlaceholderPrefabs();
            }
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        public GameObject GetApplePrefab() => applePrefab;
        public GameObject GetLearningObjectPrefab() => GetRandomAnimalPrefab() ?? applePrefab;
        public GameObject GetJumpCharacterPrefab() => GetRandomAnimalPrefab() ?? jumpCharacterPrefab;

        private void CreatePlaceholderPrefabs()
        {
            LoadAnimalPrefabsIfNeeded();

            if (applePrefab == null) applePrefab = CreateApplePrefab();
            if (carrotPrefab == null) carrotPrefab = CreateCarrotPrefab();
            if (starPrefab == null) starPrefab = CreateStarPrefab();
            if (compareApplePrefab == null) compareApplePrefab = CreateApplePrefab();
            if (compareCarrotPrefab == null) compareCarrotPrefab = CreateCarrotPrefab();
            if (numberTilePrefab == null) numberTilePrefab = CreateNumberTilePrefab();
            if (jumpCharacterPrefab == null) jumpCharacterPrefab = CreateCharacterPrefab();
            if (!HasAnimalPrefabs()) animalPrefabs = CreateFallbackAnimalPrefabs();

            Debug.Log($"[ActivityPrefabSetup] Prepared activity prefabs. Animal source: {(usingGeneratedAnimalPrefabs ? "generated fallback" : "imported/resources")}.");
        }

        public GameObject GetAnimalPrefab(int index)
        {
            LoadAnimalPrefabsIfNeeded();
            List<GameObject> prefabs = GetPreferredAnimalPrefabs();
            if (prefabs.Count == 0)
            {
                return null;
            }
            return prefabs[Mathf.Abs(index) % prefabs.Count];
        }

        public GameObject GetRandomAnimalPrefab()
        {
            if (!preferAnimalPrefabs)
            {
                return null;
            }

            LoadAnimalPrefabsIfNeeded();

            List<GameObject> prefabs = GetPreferredAnimalPrefabs();
            if (prefabs.Count == 0)
            {
                return null;
            }

            int startIndex = Random.Range(0, prefabs.Count);
            for (int i = 0; i < prefabs.Count; i++)
            {
                int index = (startIndex + i) % prefabs.Count;
                if (prefabs[index] != null)
                {
                    return prefabs[index];
                }
            }

            return null;
        }

        public void PrepareLearningObject(GameObject obj, bool enableWander = false)
        {
            if (obj == null)
            {
                return;
            }

            NormalizeObjectHeight(obj, learningObjectTargetHeight);
            SnapObjectBottomToGroundPlane(obj);
            StabilizeSkinnedMeshRendering(obj);
            RepairLearningObjectMaterials(obj);
            FaceLearningObjectTowardCamera(obj.transform);

            var presentation = obj.GetComponent<ARAnimalPresentation>();
            if (presentation == null)
            {
                presentation = obj.AddComponent<ARAnimalPresentation>();
            }

            presentation.SetWandering(enableWander);
            presentation.ResetBasePose();
        }

        public void PrepareLearningObjectGroup(GameObject group)
        {
            if (group == null)
            {
                return;
            }

            foreach (Transform child in group.transform)
            {
                if (child != null)
                {
                    PrepareLearningObject(child.gameObject);
                }
            }
        }

        private void LoadAnimalPrefabsIfNeeded()
        {
            if (HasAnimalPrefabs() && !usingGeneratedAnimalPrefabs)
            {
                return;
            }

            List<GameObject> resourcePrefabs = LoadAnimalPrefabsFromResources(out string loadedResourcePath);
            if (resourcePrefabs.Count > 0)
            {
                animalPrefabs = resourcePrefabs.ToArray();
                usingGeneratedAnimalPrefabs = false;
                Debug.Log($"[ActivityPrefabSetup] Loaded {animalPrefabs.Length} animal prefab(s) from Resources/{loadedResourcePath}.");
                return;
            }

#if UNITY_EDITOR
            animalPrefabs = FindImportedAnimalPrefabsInEditor();
            usingGeneratedAnimalPrefabs = false;
            if (HasAnimalPrefabs())
            {
                Debug.Log($"[ActivityPrefabSetup] Found {animalPrefabs.Length} imported animal prefab/model asset(s) in Assets.");
                return;
            }
#endif
        }

        private List<GameObject> LoadAnimalPrefabsFromResources(out string loadedResourcePath)
        {
            string prefabFolder = $"{resourcesAnimalFolder}/Prefabs";
            loadedResourcePath = prefabFolder;

            List<GameObject> preferredPrefabs = FilterValidAnimalPrefabs(
                Resources.LoadAll<GameObject>(prefabFolder),
                excludeLodPrefabs: true);
            if (preferredPrefabs.Count > 0)
            {
                return preferredPrefabs;
            }

            loadedResourcePath = resourcesAnimalFolder;
            List<GameObject> rootPrefabs = FilterValidAnimalPrefabs(
                Resources.LoadAll<GameObject>(resourcesAnimalFolder),
                excludeLodPrefabs: true);
            if (rootPrefabs.Count > 0)
            {
                return rootPrefabs;
            }

            string lodFolder = $"{resourcesAnimalFolder}/Prefabs/Single LODs";
            loadedResourcePath = lodFolder;
            List<GameObject> lodPrefabs = FilterValidAnimalPrefabs(
                Resources.LoadAll<GameObject>(lodFolder),
                excludeLodPrefabs: false);
            if (lodPrefabs.Count > 0)
            {
                return lodPrefabs;
            }

            loadedResourcePath = resourcesAnimalFolder;
            return FilterValidAnimalPrefabs(
                Resources.LoadAll<GameObject>(resourcesAnimalFolder),
                excludeLodPrefabs: false);
        }

        private static List<GameObject> FilterValidAnimalPrefabs(GameObject[] candidates, bool excludeLodPrefabs)
        {
            var results = new List<GameObject>();
            if (candidates == null)
            {
                return results;
            }

            for (int i = 0; i < candidates.Length; i++)
            {
                GameObject candidate = candidates[i];
                if (candidate == null || !HasRenderer(candidate))
                {
                    continue;
                }

                if (excludeLodPrefabs && candidate.name.ToLowerInvariant().Contains("_lod"))
                {
                    continue;
                }

                results.Add(candidate);
            }

            return results;
        }

        private bool HasAnimalPrefabs()
        {
            if (animalPrefabs == null || animalPrefabs.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < animalPrefabs.Length; i++)
            {
                if (animalPrefabs[i] != null)
                {
                    return true;
                }
            }

            return false;
        }

        private List<GameObject> GetPreferredAnimalPrefabs()
        {
            var results = new List<GameObject>();
            if (!HasAnimalPrefabs())
            {
                return results;
            }

            if (preferGroundedLearningAnimals)
            {
                for (int i = 0; i < animalPrefabs.Length; i++)
                {
                    GameObject prefab = animalPrefabs[i];
                    if (prefab != null && IsGroundFriendlyAnimalPrefab(prefab))
                    {
                        results.Add(prefab);
                    }
                }
            }

            if (results.Count > 0)
            {
                return results;
            }

            for (int i = 0; i < animalPrefabs.Length; i++)
            {
                if (animalPrefabs[i] != null)
                {
                    results.Add(animalPrefabs[i]);
                }
            }

            return results;
        }

        private static bool IsGroundFriendlyAnimalPrefab(GameObject prefab)
        {
            if (prefab == null)
            {
                return false;
            }

            string name = prefab.name.ToLowerInvariant();
            for (int i = 0; i < NonGroundAnimalKeywords.Length; i++)
            {
                if (name.Contains(NonGroundAnimalKeywords[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private GameObject CreateApplePrefab()
        {
            GameObject obj = CreatePrimitivePrefab("PFB_Apple", Color.red, PrimitiveType.Sphere);
            obj.transform.localScale = Vector3.one * 0.1f;
            return obj;
        }

        private GameObject CreateCarrotPrefab()
        {
            GameObject obj = CreatePrimitivePrefab("PFB_Carrot", new Color(1f, 0.5f, 0f), PrimitiveType.Capsule);
            obj.transform.localScale = new Vector3(0.05f, 0.15f, 0.05f);
            return obj;
        }

        private GameObject CreateStarPrefab()
        {
            GameObject obj = CreatePrimitivePrefab("PFB_Star", Color.yellow, PrimitiveType.Sphere);
            obj.transform.localScale = Vector3.one * 0.15f;
            return obj;
        }

        private GameObject CreateNumberTilePrefab()
        {
            GameObject obj = CreatePrimitivePrefab("PFB_NumberTile", Color.white, PrimitiveType.Cube);
            obj.transform.localScale = new Vector3(0.2f, 0.05f, 0.2f);

            GameObject textObj = new GameObject("NumberText");
            textObj.transform.SetParent(obj.transform);
            textObj.transform.localPosition = Vector3.up * 0.03f;

            return obj;
        }

        private GameObject CreateCharacterPrefab()
        {
            GameObject obj = CreatePrimitivePrefab("PFB_JumpCharacter", new Color(0.3f, 0.6f, 1f), PrimitiveType.Capsule);
            obj.transform.localScale = new Vector3(0.1f, 0.15f, 0.1f);

            GameObject face = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            face.name = "Face";
            face.transform.SetParent(obj.transform);
            face.transform.localPosition = Vector3.up * 0.08f + Vector3.forward * 0.05f;
            face.transform.localScale = Vector3.one * 0.5f;
            face.GetComponent<Renderer>().material.color = Color.white;

            return obj;
        }

        private GameObject[] CreateFallbackAnimalPrefabs()
        {
            usingGeneratedAnimalPrefabs = true;

            return new[]
            {
                CreateAnimalPrefab("PFB_AR_Fox", new Color(1f, 0.42f, 0.12f), Color.white),
                CreateAnimalPrefab("PFB_AR_Frog", new Color(0.18f, 0.78f, 0.34f), new Color(0.8f, 1f, 0.75f)),
                CreateAnimalPrefab("PFB_AR_Penguin", new Color(0.08f, 0.12f, 0.18f), Color.white),
                CreateAnimalPrefab("PFB_AR_Bunny", new Color(0.86f, 0.86f, 0.92f), new Color(1f, 0.72f, 0.82f)),
                CreateAnimalPrefab("PFB_AR_Bear", new Color(0.48f, 0.28f, 0.13f), new Color(0.78f, 0.56f, 0.32f)),
                CreateAnimalPrefab("PFB_AR_Cat", new Color(0.95f, 0.7f, 0.18f), Color.white),
                CreateAnimalPrefab("PFB_AR_Panda", Color.white, new Color(0.05f, 0.05f, 0.06f)),
                CreateAnimalPrefab("PFB_AR_Turtle", new Color(0.1f, 0.55f, 0.28f), new Color(0.38f, 0.72f, 0.32f))
            };
        }

        private GameObject CreateAnimalPrefab(string name, Color bodyColor, Color accentColor)
        {
            var root = new GameObject(name);
            root.hideFlags = showInHierarchy ? HideFlags.HideInHierarchy : HideFlags.HideAndDontSave;

            GameObject body = AddAnimalPart(root.transform, "Body", PrimitiveType.Capsule, bodyColor);
            body.transform.localPosition = new Vector3(0f, 0.055f, 0f);
            body.transform.localScale = new Vector3(0.08f, 0.055f, 0.08f);

            GameObject head = AddAnimalPart(root.transform, "Head", PrimitiveType.Sphere, bodyColor);
            head.transform.localPosition = new Vector3(0f, 0.13f, 0.045f);
            head.transform.localScale = Vector3.one * 0.075f;

            GameObject belly = AddAnimalPart(root.transform, "Belly", PrimitiveType.Sphere, accentColor);
            belly.transform.localPosition = new Vector3(0f, 0.07f, 0.043f);
            belly.transform.localScale = new Vector3(0.052f, 0.046f, 0.018f);

            GameObject leftEar = AddAnimalPart(root.transform, "LeftEar", PrimitiveType.Sphere, accentColor);
            leftEar.transform.localPosition = new Vector3(-0.035f, 0.185f, 0.045f);
            leftEar.transform.localScale = new Vector3(0.026f, 0.042f, 0.026f);

            GameObject rightEar = AddAnimalPart(root.transform, "RightEar", PrimitiveType.Sphere, accentColor);
            rightEar.transform.localPosition = new Vector3(0.035f, 0.185f, 0.045f);
            rightEar.transform.localScale = new Vector3(0.026f, 0.042f, 0.026f);

            GameObject nose = AddAnimalPart(root.transform, "Nose", PrimitiveType.Sphere, Color.black);
            nose.transform.localPosition = new Vector3(0f, 0.13f, 0.087f);
            nose.transform.localScale = Vector3.one * 0.014f;

            GameObject tail = AddAnimalPart(root.transform, "Tail", PrimitiveType.Sphere, accentColor);
            tail.transform.localPosition = new Vector3(0f, 0.07f, -0.07f);
            tail.transform.localScale = Vector3.one * 0.032f;

            root.SetActive(false);
            return root;
        }

        private static GameObject AddAnimalPart(Transform parent, string name, PrimitiveType type, Color color)
        {
            GameObject part = GameObject.CreatePrimitive(type);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.GetComponent<Renderer>().material.color = color;
            return part;
        }

        private GameObject CreatePrimitivePrefab(string name, Color color, PrimitiveType type)
        {
            GameObject obj = GameObject.CreatePrimitive(type);
            obj.name = name;
            obj.GetComponent<Renderer>().material.color = color;
            obj.hideFlags = showInHierarchy ? HideFlags.HideInHierarchy : HideFlags.HideAndDontSave;
            obj.SetActive(false);
            return obj;
        }

        public GameObject GetPrefab(string prefabName)
        {
            return prefabName switch
            {
                "PFB_Apple" => applePrefab,
                "PFB_Carrot" => carrotPrefab,
                "PFB_Star" => starPrefab,
                "PFB_NumberTile" => numberTilePrefab,
                "PFB_JumpCharacter" => jumpCharacterPrefab,
                _ => null
            };
        }

        private static void NormalizeObjectHeight(GameObject obj, float targetHeight)
        {
            if (targetHeight <= 0f)
            {
                return;
            }

            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                return;
            }

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            if (bounds.size.y <= 0.0001f)
            {
                return;
            }

            float scaleFactor = targetHeight / bounds.size.y;
            obj.transform.localScale *= scaleFactor;
        }

        private static void SnapObjectBottomToGroundPlane(GameObject obj)
        {
            if (obj == null)
            {
                return;
            }

            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                return;
            }

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            float groundY = obj.transform.position.y;
            float deltaY = groundY - bounds.min.y;
            if (Mathf.Abs(deltaY) > 0.0005f)
            {
                obj.transform.position += Vector3.up * deltaY;
            }
        }

        private static void StabilizeSkinnedMeshRendering(GameObject obj)
        {
            if (obj == null)
            {
                return;
            }

            SkinnedMeshRenderer[] skinnedRenderers = obj.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            for (int i = 0; i < skinnedRenderers.Length; i++)
            {
                SkinnedMeshRenderer skinnedRenderer = skinnedRenderers[i];
                if (skinnedRenderer == null)
                {
                    continue;
                }

                skinnedRenderer.updateWhenOffscreen = true;
                Bounds localBounds = skinnedRenderer.localBounds;
                localBounds.Expand(0.12f);
                skinnedRenderer.localBounds = localBounds;
            }
        }

        private void FaceLearningObjectTowardCamera(Transform target)
        {
            if (!faceLearningObjectsToCamera || target == null)
            {
                return;
            }

            Camera camera = Camera.main;
            if (camera == null)
            {
                return;
            }

            Vector3 direction = camera.transform.position - target.position;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.0001f)
            {
                return;
            }

            float yaw = animalFacingYawOffset + Random.Range(-randomFacingYaw, randomFacingYaw);
            target.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up) * Quaternion.Euler(0f, yaw, 0f);
        }

        private void RepairLearningObjectMaterials(GameObject obj)
        {
            if (!repairLearningObjectMaterials || obj == null)
            {
                return;
            }

            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null)
                {
                    continue;
                }

                Material[] materials = renderer.sharedMaterials;
                bool changed = false;
                for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
                {
                    Material repaired = GetRenderableMaterial(materials[materialIndex]);
                    if (repaired != materials[materialIndex])
                    {
                        materials[materialIndex] = repaired;
                        changed = true;
                    }
                }

                if (changed)
                {
                    renderer.sharedMaterials = materials;
                }
            }
        }

        private Material GetRenderableMaterial(Material source)
        {
            bool needsRepair = NeedsMaterialRepair(source) || NeedsReadableShader(source);
            bool needsContrastBoost = boostLearningObjectContrast && NeedsContrastBoost(source);
            if (!needsRepair && !needsContrastBoost)
            {
                return source;
            }

            if (source != null && convertedMaterialCache.TryGetValue(source, out Material cachedMaterial))
            {
                return cachedMaterial;
            }

            Material material;
            if (needsRepair || source == null)
            {
                Shader shader = Shader.Find(learningObjectShaderName)
                    ?? Shader.Find("Universal Render Pipeline/Lit")
                    ?? Shader.Find("Unlit/Texture")
                    ?? Shader.Find("Standard");

                if (shader == null)
                {
                    return source;
                }

                material = new Material(shader)
                {
                    name = source != null ? $"{source.name}_Readable_Runtime" : "LearningObject_Readable_Runtime"
                };
            }
            else
            {
                material = new Material(source)
                {
                    name = $"{source.name}_Readable_Runtime"
                };
            }

            Texture mainTexture = GetMainTexture(source);
            if (mainTexture != null)
            {
                SetTextureIfPresent(material, "_BaseMap", mainTexture);
                SetTextureIfPresent(material, "_MainTex", mainTexture);
            }

            Color baseColor = boostLearningObjectContrast
                ? EnhanceLearningObjectColor(GetMaterialColor(source))
                : GetMaterialColor(source);
            SetColorIfPresent(material, "_BaseColor", baseColor);
            SetColorIfPresent(material, "_Color", baseColor);
            SetEmissionIfPresent(material, baseColor * 0.12f);

            if (source != null)
            {
                convertedMaterialCache[source] = material;
                Debug.Log($"[ActivityPrefabSetup] Prepared readable animal material '{source.name}' from shader '{source.shader?.name}'.");
            }

            return material;
        }

        private bool NeedsContrastBoost(Material material)
        {
            if (!boostLearningObjectContrast)
            {
                return false;
            }

            Color color = GetMaterialColor(material);
            float brightness = Mathf.Max(color.r, Mathf.Max(color.g, color.b));
            return brightness < minimumLearningObjectBrightness;
        }

        private bool NeedsReadableShader(Material material)
        {
            if (!boostLearningObjectContrast || material == null || material.shader == null)
            {
                return false;
            }

            string shaderName = material.shader.name;
            return !shaderName.Contains("Unlit");
        }

        private Color EnhanceLearningObjectColor(Color color)
        {
            if (color.a <= 0.001f)
            {
                color = Color.white;
            }

            color.r = Mathf.Clamp01(color.r);
            color.g = Mathf.Clamp01(color.g);
            color.b = Mathf.Clamp01(color.b);

            float brightness = Mathf.Max(color.r, Mathf.Max(color.g, color.b));
            if (brightness < minimumLearningObjectBrightness)
            {
                color = Color.Lerp(color, Color.white, learningObjectColorLift);
                brightness = Mathf.Max(color.r, Mathf.Max(color.g, color.b));
                if (brightness < minimumLearningObjectBrightness)
                {
                    float scale = minimumLearningObjectBrightness / Mathf.Max(0.001f, brightness);
                    color.r = Mathf.Clamp01(color.r * scale);
                    color.g = Mathf.Clamp01(color.g * scale);
                    color.b = Mathf.Clamp01(color.b * scale);
                }
            }

            color.a = Mathf.Max(color.a, 1f);
            return color;
        }

        private static bool NeedsMaterialRepair(Material material)
        {
            if (material == null || material.shader == null)
            {
                return true;
            }

            string shaderName = material.shader.name;
            return shaderName.Contains("InternalErrorShader")
                || shaderName == "Toon/SoftSurface"
                || shaderName.StartsWith("Toon/", System.StringComparison.Ordinal);
        }

        private static Texture GetMainTexture(Material material)
        {
            if (material == null)
            {
                return null;
            }

            if (material.HasProperty("_BaseMap"))
            {
                Texture baseMap = material.GetTexture("_BaseMap");
                if (baseMap != null)
                {
                    return baseMap;
                }
            }

            return material.HasProperty("_MainTex") ? material.GetTexture("_MainTex") : null;
        }

        private static Color GetMaterialColor(Material material)
        {
            if (material == null)
            {
                return Color.white;
            }

            if (material.shader != null && material.shader.name == "Toon/SoftSurface")
            {
                return Color.white;
            }

            if (material.HasProperty("_BaseColor"))
            {
                return material.GetColor("_BaseColor");
            }

            return material.HasProperty("_Color") ? material.GetColor("_Color") : Color.white;
        }

        private static void SetTextureIfPresent(Material material, string propertyName, Texture texture)
        {
            if (material != null && texture != null && material.HasProperty(propertyName))
            {
                material.SetTexture(propertyName, texture);
            }
        }

        private static void SetColorIfPresent(Material material, string propertyName, Color color)
        {
            if (material != null && material.HasProperty(propertyName))
            {
                material.SetColor(propertyName, color);
            }
        }

        private static void SetEmissionIfPresent(Material material, Color color)
        {
            if (material == null || !material.HasProperty("_EmissionColor"))
            {
                return;
            }

            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color);
        }

#if UNITY_EDITOR
        private static GameObject[] FindImportedAnimalPrefabsInEditor()
        {
            string[] guids = AssetDatabase.FindAssets("t:GameObject", new[] { "Assets" });
            var prefabs = new List<GameObject>();
            var seenPaths = new HashSet<string>();

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!LooksLikeAnimalAsset(path) || !IsSupportedAnimalAssetPath(path) || !seenPaths.Add(path))
                {
                    continue;
                }

                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null && HasRenderer(prefab))
                {
                    prefabs.Add(prefab);
                    LogAnimationClipsForAsset(path);
                }
            }

            return prefabs.ToArray();
        }

        private static void LogAnimationClipsForAsset(string path)
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
            var clipNames = new List<string>();
            for (int i = 0; i < assets.Length; i++)
            {
                if (assets[i] is AnimationClip clip && !clipNames.Contains(clip.name))
                {
                    clipNames.Add(clip.name);
                }
            }

            if (clipNames.Count == 0)
            {
                Debug.Log($"[ActivityPrefabSetup] Animal asset found: {path} (no embedded animation clips detected).");
                return;
            }

            Debug.Log($"[ActivityPrefabSetup] Animal asset found: {path}. Animations: {string.Join(", ", clipNames)}");
        }

        private static bool IsSupportedAnimalAssetPath(string path)
        {
            string extension = Path.GetExtension(path).ToLowerInvariant();
            return extension == ".prefab"
                || extension == ".fbx"
                || extension == ".dae"
                || extension == ".obj"
                || extension == ".gltf"
                || extension == ".glb";
        }

        private static bool LooksLikeAnimalAsset(string path)
        {
            string lower = path.Replace('\\', '/').ToLowerInvariant();
            if (lower.Contains("quirky")
                || lower.Contains("omabu")
                || lower.Contains("/animal")
                || lower.Contains("/animals/")
                || lower.Contains("shared/art/animals"))
            {
                return true;
            }

            string fileName = System.IO.Path.GetFileNameWithoutExtension(lower);
            string[] animalKeywords =
            {
                "bear", "bird", "bunny", "cat", "cow", "croc", "dog", "duck",
                "elephant", "fish", "fox", "frog", "giraffe", "lion", "monkey",
                "panda", "penguin", "pig", "rabbit", "snake", "tiger", "turtle", "zebra",
                "jellyfish", "octopus", "prawn", "salmon", "sardine", "seagull", "tuna", "whale"
            };

            for (int i = 0; i < animalKeywords.Length; i++)
            {
                if (fileName.Contains(animalKeywords[i]))
                {
                    return true;
                }
            }

            return false;
        }
#endif

        private static bool HasRenderer(GameObject obj)
        {
            return obj != null && obj.GetComponentInChildren<Renderer>(true) != null;
        }

        public void DestroyPlaceholders()
        {
            if (applePrefab != null) Destroy(applePrefab);
            if (carrotPrefab != null) Destroy(carrotPrefab);
            if (starPrefab != null) Destroy(starPrefab);
            if (numberTilePrefab != null) Destroy(numberTilePrefab);
            if (jumpCharacterPrefab != null) Destroy(jumpCharacterPrefab);
            if (!usingGeneratedAnimalPrefabs || animalPrefabs == null) return;

            for (int i = 0; i < animalPrefabs.Length; i++)
            {
                if (animalPrefabs[i] != null) Destroy(animalPrefabs[i]);
            }
        }
    }
}
