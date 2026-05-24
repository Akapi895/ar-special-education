using UnityEngine;

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

        [Header("Auto-Create Placeholders")]
        [SerializeField] private bool autoCreatePlaceholders = true;
        [SerializeField] private bool showInHierarchy = true;

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

        private void CreatePlaceholderPrefabs()
        {
            if (applePrefab == null) applePrefab = CreateApplePrefab();
            if (carrotPrefab == null) carrotPrefab = CreateCarrotPrefab();
            if (starPrefab == null) starPrefab = CreateStarPrefab();
            if (compareApplePrefab == null) compareApplePrefab = CreateApplePrefab();
            if (compareCarrotPrefab == null) compareCarrotPrefab = CreateCarrotPrefab();
            if (numberTilePrefab == null) numberTilePrefab = CreateNumberTilePrefab();
            if (jumpCharacterPrefab == null) jumpCharacterPrefab = CreateCharacterPrefab();

            Debug.Log("[ActivityPrefabSetup] Created placeholder prefabs for testing.");
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

        public void DestroyPlaceholders()
        {
            if (applePrefab != null) Destroy(applePrefab);
            if (carrotPrefab != null) Destroy(carrotPrefab);
            if (starPrefab != null) Destroy(starPrefab);
            if (numberTilePrefab != null) Destroy(numberTilePrefab);
            if (jumpCharacterPrefab != null) Destroy(jumpCharacterPrefab);
        }
    }
}
