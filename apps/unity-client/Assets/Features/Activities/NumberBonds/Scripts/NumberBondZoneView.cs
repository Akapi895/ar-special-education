using System.Collections;
using UnityEngine;

namespace Features.Activities.NumberBonds
{
    public class NumberBondZoneView : MonoBehaviour
    {
        private const float VisualHeight = 0.018f;

        private TextMesh label;
        private TextMesh countLabel;
        private Renderer visualRenderer;
        private float zoneRadius;
        private float slotRadius;

        private static readonly Color WholeColor = new Color(0.2f, 0.55f, 1f, 0.72f);
        private static readonly Color PartAColor = new Color(1f, 0.78f, 0.22f, 0.72f);
        private static readonly Color PartBColor = new Color(0.34f, 0.86f, 0.48f, 0.72f);
        private static readonly Color LockedColor = new Color(0.7f, 0.72f, 0.78f, 0.72f);
        private static readonly Color CorrectColor = new Color(0.16f, 1f, 0.42f, 0.9f);
        private static readonly Color IncorrectColor = new Color(1f, 0.35f, 0.2f, 0.9f);

        public BondZone Zone { get; private set; }
        public bool IsLocked { get; private set; }

        public void Initialize(BondZone zone, string title, bool isLocked, float radius, float hitRadius)
        {
            Zone = zone;
            IsLocked = isLocked;
            zoneRadius = Mathf.Max(0.25f, radius);
            slotRadius = zoneRadius * 0.58f;

            CreateHitbox(hitRadius);
            CreateVisual();
            CreateLabels(title);
            SetNormal();
        }

        public void SetCount(int count)
        {
            if (countLabel != null)
            {
                countLabel.text = IsLocked ? $"{count} \U0001f512" : count.ToString();
            }
        }

        public bool ContainsWorldPoint(Vector3 worldPosition)
        {
            Vector3 local = transform.InverseTransformPoint(worldPosition);
            local.y = 0f;
            return local.sqrMagnitude <= zoneRadius * zoneRadius * 1.55f;
        }

        public Vector3 GetSlotPosition(int index, int total)
        {
            total = Mathf.Max(1, total);
            Vector3 local;
            if (index == 0)
            {
                local = Vector3.zero;
            }
            else
            {
                float angle = ((index - 1) / Mathf.Max(1f, total - 1f)) * Mathf.PI * 2f;
                local = new Vector3(Mathf.Cos(angle) * slotRadius, 0f, Mathf.Sin(angle) * slotRadius);
            }

            return transform.TransformPoint(local + Vector3.up * 0.04f);
        }

        public void SetNormal()
        {
            SetColor(IsLocked ? LockedColor : GetZoneColor(Zone));
        }

        public void SetHover(bool active)
        {
            if (!active)
            {
                SetNormal();
                return;
            }

            SetColor(Color.Lerp(GetZoneColor(Zone), Color.white, 0.35f));
        }

        public void SetValidationState(NumberBondValidationResult result)
        {
            SetColor(result == NumberBondValidationResult.Correct ? CorrectColor : IncorrectColor);
            StopAllCoroutines();
            StartCoroutine(PulseCoroutine(result == NumberBondValidationResult.Correct));
        }

        private IEnumerator PulseCoroutine(bool isCorrect)
        {
            Color targetColor = isCorrect ? CorrectColor : IncorrectColor;
            Color originalColor = GetZoneColor(Zone);
            float duration = 0.5f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float t = elapsed / duration;
                float pulse = Mathf.PingPong(t * 2f, 1f);
                Color c = Color.Lerp(originalColor, targetColor, pulse);
                if (isCorrect)
                {
                    c.a = Mathf.Lerp(0.72f, 0.9f, pulse);
                }
                SetColor(c);
                elapsed += Time.deltaTime;
                yield return null;
            }

            SetColor(targetColor);

            // Second pulse if correct
            if (isCorrect)
            {
                yield return new WaitForSeconds(0.3f);
                elapsed = 0f;
                while (elapsed < duration * 0.6f)
                {
                    float t = elapsed / (duration * 0.6f);
                    float pulse = Mathf.Sin(t * Mathf.PI);
                    Color c = Color.Lerp(targetColor, Color.white * 1.2f, pulse * 0.3f);
                    SetColor(c);
                    elapsed += Time.deltaTime;
                    yield return null;
                }
                SetColor(targetColor);
            }
        }

        private void CreateHitbox(float hitRadius)
        {
            BoxCollider collider = gameObject.GetComponent<BoxCollider>();
            if (collider == null)
            {
                collider = gameObject.AddComponent<BoxCollider>();
            }

            float diameter = Mathf.Max(hitRadius, zoneRadius) * 2f;
            collider.size = new Vector3(diameter, 0.16f, diameter);
            collider.center = Vector3.up * 0.04f;
            collider.isTrigger = true;
        }

        private void CreateVisual()
        {
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            visual.name = "ZoneDisc";
            visual.transform.SetParent(transform, false);
            visual.transform.localScale = new Vector3(zoneRadius * 2f, VisualHeight, zoneRadius * 2f);

            Collider collider = visual.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }

            visualRenderer = visual.GetComponent<Renderer>();
            if (visualRenderer != null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Unlit")
                    ?? Shader.Find("Unlit/Color")
                    ?? Shader.Find("Standard");
                visualRenderer.material = new Material(shader);
            }
        }

        private void CreateLabels(string title)
        {
            label = CreateText("ZoneLabel", title, new Vector3(0f, 0.1f, -zoneRadius - 0.12f), 0.018f);
            countLabel = CreateText("ZoneCount", "0", new Vector3(0f, 0.12f, 0f), 0.03f);
        }

        private TextMesh CreateText(string name, string content, Vector3 localPosition, float characterSize)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            go.transform.localPosition = localPosition;

            TextMesh text = go.AddComponent<TextMesh>();
            text.text = content;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.fontSize = 64;
            text.characterSize = characterSize;
            text.color = Color.white;
            go.AddComponent<NumberBondBillboard>();
            return text;
        }

        private void SetColor(Color color)
        {
            if (visualRenderer == null)
            {
                return;
            }

            if (visualRenderer.material.HasProperty("_BaseColor"))
            {
                visualRenderer.material.SetColor("_BaseColor", color);
            }
            else if (visualRenderer.material.HasProperty("_Color"))
            {
                visualRenderer.material.SetColor("_Color", color);
            }
        }

        private static Color GetZoneColor(BondZone zone)
        {
            return zone switch
            {
                BondZone.PartA => PartAColor,
                BondZone.PartB => PartBColor,
                _ => WholeColor
            };
        }
    }

    public class NumberBondBillboard : MonoBehaviour
    {
        private Camera mainCamera;

        private void LateUpdate()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
                if (mainCamera == null)
                {
                    return;
                }
            }

            Vector3 direction = transform.position - mainCamera.transform.position;
            if (direction.sqrMagnitude > 0.0001f)
            {
                transform.rotation = Quaternion.LookRotation(direction.normalized, mainCamera.transform.up);
            }
        }
    }
}
