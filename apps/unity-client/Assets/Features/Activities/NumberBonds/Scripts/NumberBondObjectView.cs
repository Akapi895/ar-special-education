using UnityEngine;

namespace Features.Activities.NumberBonds
{
    public class NumberBondObjectView : MonoBehaviour
    {
        public string ObjectId { get; private set; }
        public BondZone CurrentZone { get; private set; }
        public bool IsMovable { get; private set; }

        private Vector3 lastStablePosition;
        private const float MinimumHitboxWorldRadius = 0.18f;

        public void Initialize(string objectId, BondZone zone, bool isMovable)
        {
            ObjectId = objectId;
            CurrentZone = zone;
            IsMovable = isMovable;
            lastStablePosition = transform.position;
            EnsureCollider();
        }

        public void SetZone(BondZone zone)
        {
            CurrentZone = zone;
        }

        public void SnapTo(Vector3 position)
        {
            transform.position = position;
            lastStablePosition = position;
        }

        public void ReturnToLastStablePosition()
        {
            transform.position = lastStablePosition;
        }

        private void EnsureCollider()
        {
            SphereCollider collider = GetComponent<SphereCollider>();
            if (collider == null)
            {
                collider = gameObject.AddComponent<SphereCollider>();
            }

            Bounds bounds = CalculateRendererBounds();
            float worldRadius = bounds.size == Vector3.zero
                ? MinimumHitboxWorldRadius
                : Mathf.Max(MinimumHitboxWorldRadius, bounds.extents.x, bounds.extents.z, bounds.extents.y * 0.6f);
            float scale = Mathf.Max(
                Mathf.Abs(transform.lossyScale.x),
                Mathf.Abs(transform.lossyScale.y),
                Mathf.Abs(transform.lossyScale.z),
                0.0001f);

            collider.center = bounds.size == Vector3.zero
                ? Vector3.up * 0.08f
                : transform.InverseTransformPoint(bounds.center);
            collider.radius = worldRadius / scale;
            collider.isTrigger = true;
        }

        private Bounds CalculateRendererBounds()
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                return default;
            }

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            return bounds;
        }
    }
}
