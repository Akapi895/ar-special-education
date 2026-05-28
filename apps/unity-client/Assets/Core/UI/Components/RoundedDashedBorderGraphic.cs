using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Core.UI.Components
{
    [AddComponentMenu("UI/Kid Friendly/Rounded Dashed Border Graphic")]
    public class RoundedDashedBorderGraphic : Graphic
    {
        [SerializeField] private float cornerRadius = 32f;
        [SerializeField] private float borderThickness = 4f;
        [SerializeField] private float dashLength = 22f;
        [SerializeField] private float gapLength = 12f;
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

        public float BorderThickness
        {
            get => borderThickness;
            set
            {
                borderThickness = Mathf.Max(1f, value);
                SetVerticesDirty();
            }
        }

        public float DashLength
        {
            get => dashLength;
            set
            {
                dashLength = Mathf.Max(2f, value);
                SetVerticesDirty();
            }
        }

        public float GapLength
        {
            get => gapLength;
            set
            {
                gapLength = Mathf.Max(1f, value);
                SetVerticesDirty();
            }
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Rect rect = GetPixelAdjustedRect();
            rect.xMin += borderThickness * 0.5f;
            rect.xMax -= borderThickness * 0.5f;
            rect.yMin += borderThickness * 0.5f;
            rect.yMax -= borderThickness * 0.5f;

            float radius = Mathf.Min(cornerRadius, rect.width * 0.5f, rect.height * 0.5f);
            List<Vector2> points = RoundedRectGraphic.BuildRoundedRectOutline(rect, radius, cornerSegments);
            Color32 vertexColor = color;

            for (int i = 0; i < points.Count; i++)
            {
                Vector2 start = points[i];
                Vector2 end = points[(i + 1) % points.Count];
                AddDashedSegment(vh, start, end, vertexColor);
            }
        }

        private void AddDashedSegment(VertexHelper vh, Vector2 start, Vector2 end, Color32 vertexColor)
        {
            Vector2 delta = end - start;
            float length = delta.magnitude;
            if (length <= 0.01f)
            {
                return;
            }

            Vector2 direction = delta / length;
            float cursor = 0f;
            while (cursor < length)
            {
                float dashEnd = Mathf.Min(cursor + dashLength, length);
                Vector2 a = start + direction * cursor;
                Vector2 b = start + direction * dashEnd;
                AddLineQuad(vh, a, b, borderThickness, vertexColor);
                cursor += dashLength + gapLength;
            }
        }

        private static void AddLineQuad(VertexHelper vh, Vector2 start, Vector2 end, float thickness, Color32 vertexColor)
        {
            Vector2 direction = (end - start).normalized;
            Vector2 normal = new Vector2(-direction.y, direction.x) * (thickness * 0.5f);
            int index = vh.currentVertCount;

            vh.AddVert(start - normal, vertexColor, Vector2.zero);
            vh.AddVert(start + normal, vertexColor, Vector2.zero);
            vh.AddVert(end + normal, vertexColor, Vector2.zero);
            vh.AddVert(end - normal, vertexColor, Vector2.zero);
            vh.AddTriangle(index, index + 1, index + 2);
            vh.AddTriangle(index, index + 2, index + 3);
        }
    }
}
