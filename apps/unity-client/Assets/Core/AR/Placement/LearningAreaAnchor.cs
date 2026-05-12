using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace ARSpecialEducation.Core.AR
{
    [DisallowMultipleComponent]
    public sealed class LearningAreaAnchor : MonoBehaviour
    {
        [SerializeField] Transform contentRoot;
        [SerializeField] Vector2 areaSizeMeters = new Vector2(0.6f, 0.6f);

        public ARPlane AttachedPlane { get; private set; }
        public bool IsPlaced { get; private set; }
        public Transform ContentRoot => contentRoot != null ? contentRoot : transform;
        public Vector2 AreaSizeMeters => areaSizeMeters;
        public Pose Pose => new Pose(transform.position, transform.rotation);

        void Reset()
        {
            contentRoot = transform;
        }

        public void Initialize(ARPlane attachedPlane)
        {
            AttachedPlane = attachedPlane;
            IsPlaced = true;
        }

        public void SetPose(Vector3 position, Quaternion rotation)
        {
            transform.SetPositionAndRotation(position, rotation);
            IsPlaced = true;
        }

        public void SetAreaSize(Vector2 sizeMeters)
        {
            areaSizeMeters = new Vector2(Mathf.Max(0f, sizeMeters.x), Mathf.Max(0f, sizeMeters.y));
        }
    }
}
