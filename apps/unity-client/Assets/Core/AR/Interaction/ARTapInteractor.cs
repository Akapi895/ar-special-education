using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace ARSpecialEducation.Core.AR
{
    [DisallowMultipleComponent]
    public sealed class ARTapInteractor : MonoBehaviour
    {
        [SerializeField] Camera arCamera;
        [SerializeField] LayerMask interactableLayers = ~0;
        [SerializeField] float maxRaycastDistance = 20f;
        [SerializeField] bool ignorePointerOverUI = true;

        ARSelectableObject selectedObject;

        public event Action<ARSelectableObject> OnObjectTapped;
        public event Action<ARSelectableObject> OnObjectSelected;

        void Awake()
        {
            ResolveReferences();
        }

        void Update()
        {
            if (!TryGetPointerDown(out var screenPosition))
            {
                return;
            }

            if (ignorePointerOverUI && IsPointerOverUI())
            {
                return;
            }

            if (!TryRaycastSelectable(screenPosition, out var selectableObject))
            {
                return;
            }

            if (selectedObject != selectableObject)
            {
                selectedObject?.Release();
                selectedObject = selectableObject;
                selectedObject.Select();
                OnObjectSelected?.Invoke(selectedObject);
            }

            selectableObject.Tap();
            OnObjectTapped?.Invoke(selectableObject);
        }

        public void ResolveReferences()
        {
            if (arCamera == null)
            {
                arCamera = Camera.main != null ? Camera.main : FindFirstObjectByType<Camera>();
            }
        }

        bool TryRaycastSelectable(Vector2 screenPosition, out ARSelectableObject selectableObject)
        {
            ResolveReferences();
            selectableObject = null;

            if (arCamera == null)
            {
                return false;
            }

            var ray = arCamera.ScreenPointToRay(screenPosition);
            if (!Physics.Raycast(ray, out var hit, maxRaycastDistance, interactableLayers, QueryTriggerInteraction.Collide))
            {
                return false;
            }

            selectableObject = hit.collider.GetComponentInParent<ARSelectableObject>();
            return selectableObject != null;
        }

        static bool TryGetPointerDown(out Vector2 position)
        {
            if (Touchscreen.current != null)
            {
                var touch = Touchscreen.current.primaryTouch;
                if (touch.press.wasPressedThisFrame)
                {
                    position = touch.position.ReadValue();
                    return true;
                }
            }

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                position = Mouse.current.position.ReadValue();
                return true;
            }

            position = default;
            return false;
        }

        static bool IsPointerOverUI()
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(-1);
        }
    }
}
