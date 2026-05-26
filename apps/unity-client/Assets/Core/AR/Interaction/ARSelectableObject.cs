using System;
using UnityEngine;

namespace ARSpecialEducation.Core.AR
{
    [DisallowMultipleComponent]
    public sealed class ARSelectableObject : MonoBehaviour
    {
        [SerializeField] bool canDrag = true;
        [SerializeField] bool autoAddColliderIfMissing = true;

        public event Action<ARSelectableObject> OnObjectTapped;
        public event Action<ARSelectableObject> OnObjectSelected;
        public event Action<ARSelectableObject, Vector3> OnObjectDragged;
        public event Action<ARSelectableObject> OnObjectReleased;

        public bool CanDrag => canDrag;
        public bool IsSelected { get; private set; }

        void Awake()
        {
            if (autoAddColliderIfMissing && GetComponentInChildren<Collider>() == null)
            {
                gameObject.AddComponent<BoxCollider>();
            }
        }

        public void Tap()
        {
            OnObjectTapped?.Invoke(this);
        }

        public void Select()
        {
            if (IsSelected)
            {
                return;
            }

            IsSelected = true;
            OnObjectSelected?.Invoke(this);
        }

        public void DragTo(Vector3 worldPosition)
        {
            if (!canDrag)
            {
                return;
            }

            transform.position = worldPosition;
            OnObjectDragged?.Invoke(this, worldPosition);
        }

        public void Release()
        {
            if (!IsSelected)
            {
                return;
            }

            IsSelected = false;
            OnObjectReleased?.Invoke(this);
        }
    }
}
