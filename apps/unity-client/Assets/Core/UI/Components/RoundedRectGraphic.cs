using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Core.UI.Components
{
    [AddComponentMenu("UI/Kid Friendly/Rounded Rect Graphic")]
    public class RoundedRectGraphic : Graphic
    {
        [SerializeField] private float cornerRadius = 32f;
        [SerializeField, Range(2, 18)] private int cornerSegments = 10;

        public float CornerRadius
        {
            get => cornerRadius;
            set
            {
                cornerRadius = Mathf.Max(0f, value);
                SetVerticesDirty();
            }
        }

        public int CornerSegments
        {
            get => cornerSegments;
            set
            {
                cornerSegments = Mathf.Clamp(value, 2, 18);
                SetVerticesDirty();
            }
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Rect rect = GetPixelAdjustedRect();
            Color32 vertexColor = color;

            float radius = Mathf.Min(cornerRadius, rect.width * 0.5f, rect.height * 0.5f);
            if (radius <= 0.01f)
            {
                AddRect(vh, rect, vertexColor);
                return;
            }

            List<Vector2> outline = BuildRoundedRectOutline(rect, radius, cornerSegments);
            vh.AddVert(rect.center, vertexColor, Vector2.zero);

            for (int i = 0; i < outline.Count; i++)
            {
                vh.AddVert(outline[i], vertexColor, Vector2.zero);
            }

            for (int i = 1; i <= outline.Count; i++)
            {
                int next = i == outline.Count ? 1 : i + 1;
                vh.AddTriangle(0, i, next);
            }
        }

        internal static List<Vector2> BuildRoundedRectOutline(Rect rect, float radius, int segments)
        {
            var points = new List<Vector2>((segments + 1) * 4);
            AddCorner(points, new Vector2(rect.xMax - radius, rect.yMin + radius), radius, -90f, 0f, segments);
            AddCorner(points, new Vector2(rect.xMax - radius, rect.yMax - radius), radius, 0f, 90f, segments);
            AddCorner(points, new Vector2(rect.xMin + radius, rect.yMax - radius), radius, 90f, 180f, segments);
            AddCorner(points, new Vector2(rect.xMin + radius, rect.yMin + radius), radius, 180f, 270f, segments);
            return points;
        }

        private static void AddCorner(List<Vector2> points, Vector2 center, float radius, float startAngle, float endAngle, int segments)
        {
            for (int i = 0; i <= segments; i++)
            {
                float angle = Mathf.Lerp(startAngle, endAngle, i / (float)segments) * Mathf.Deg2Rad;
                points.Add(center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius);
            }
        }

        private static void AddRect(VertexHelper vh, Rect rect, Color32 vertexColor)
        {
            vh.AddVert(new Vector2(rect.xMin, rect.yMin), vertexColor, Vector2.zero);
            vh.AddVert(new Vector2(rect.xMin, rect.yMax), vertexColor, Vector2.zero);
            vh.AddVert(new Vector2(rect.xMax, rect.yMax), vertexColor, Vector2.zero);
            vh.AddVert(new Vector2(rect.xMax, rect.yMin), vertexColor, Vector2.zero);
            vh.AddTriangle(0, 1, 2);
            vh.AddTriangle(0, 2, 3);
        }
    }
}
