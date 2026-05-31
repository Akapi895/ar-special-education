using UnityEngine;

namespace Features.Activities.QuantityMatch
{
    public class GroupAreaIndicator : MonoBehaviour
    {
        public float radius = 0.8f;
        public Color color = Color.white;
        public float pulseSpeed = 2f;
        
        private LineRenderer lineRenderer;
        private int segments = 36;
        private float currentAlpha = 0.5f;
        private float targetScale = 1f;
        private float currentScale = 1f;
        private float lastDrawnRadius = -1f;

        private Color originalColor;
        private bool isFlashingIncorrect;
        private float flashTimer;
        private int flashCount;

        private void Awake()
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.useWorldSpace = false;
            lineRenderer.loop = true;
            lineRenderer.startWidth = 0.12f;
            lineRenderer.endWidth = 0.02f;
            lineRenderer.positionCount = segments;
            lineRenderer.startColor = new Color(color.r, color.g, color.b, 0.85f);
            lineRenderer.endColor = new Color(color.r, color.g, color.b, 0.0f);

            // Set simple default material
            Shader defaultShader = Shader.Find("Sprites/Default");
            if (defaultShader != null)
            {
                lineRenderer.material = new Material(defaultShader);
            }

            DrawCircle();
        }

        private void Update()
        {
            if (!Mathf.Approximately(lastDrawnRadius, radius))
            {
                DrawCircle();
            }

            if (isFlashingIncorrect)
            {
                flashTimer += Time.deltaTime;
                if (flashTimer >= 0.15f)
                {
                    flashTimer = 0f;
                    flashCount++;
                    color = (color == originalColor) ? new Color(1f, 0.5f, 0f) : originalColor; // toggle orange
                    if (flashCount >= 6) // 3 full flashes
                    {
                        isFlashingIncorrect = false;
                        color = originalColor;
                    }
                }
            }

            // Pulse logic
            float baseAlpha = 0.7f + Mathf.PingPong(Time.time * pulseSpeed, 0.3f);
            currentAlpha = Mathf.Lerp(currentAlpha, baseAlpha, Time.deltaTime * 5f);
            
            // Update line colors
            Color c = color;
            c.a = currentAlpha;
            if (lineRenderer != null)
            {
                lineRenderer.startColor = new Color(c.r, c.g, c.b, 0.85f);
                lineRenderer.endColor = new Color(c.r, c.g, c.b, 0.85f);
            }

            // Animate scale (bounce effect when highlighted)
            currentScale = Mathf.Lerp(currentScale, targetScale, Time.deltaTime * 10f);
            transform.localScale = new Vector3(currentScale, 1f, currentScale);
        }

        private void DrawCircle()
        {
            if (lineRenderer == null)
            {
                return;
            }

            float angle = 0f;
            for (int i = 0; i < segments; i++)
            {
                float x = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
                float z = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;
                lineRenderer.SetPosition(i, new Vector3(x, 0.01f, z)); // slightly above ground
                angle += 360f / segments;
            }

            lastDrawnRadius = radius;
        }

        public void Highlight()
        {
            currentScale = 1.2f;
            targetScale = 1f;
            currentAlpha = 1f;
        }
        
        public void SetColor(Color newColor)
        {
            color = newColor;
            if (!isFlashingIncorrect)
            {
                originalColor = newColor;
            }
        }

        public void FlashIncorrect()
        {
            if (isFlashingIncorrect) return;
            originalColor = color;
            isFlashingIncorrect = true;
            flashCount = 0;
            flashTimer = 0f;
        }
    }
}

