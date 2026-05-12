using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace ARSpecialEducation.Core.AR
{
    [DisallowMultipleComponent]
    public sealed class ARDragInteractor : MonoBehaviour
    {
        static readonly List<ARRaycastHit> RaycastHits = new List<ARRaycastHit>();

        [SerializeField] Camera arCamera;
        [SerializeField] ARRaycastManager raycastManager;
        [SerializeField] LayerMask interactableLayers = ~0;
        [SerializeField] TrackableType dragTrackableTypes = TrackableType.PlaneWithinPolygon;
        [SerializeField] float maxObjectRaycastDistance = 20f;
        [SerializeField] float minimumDragDistancePixels = 2f;
        [SerializeField] bool ignorePointerOverUI = true;

        ARSelectableObject activeObject;
        Vector2 dragStartPosition;
        Vector3 dragOffset;
        float fallbackDragHeight;
        bool isDragging;

        public event Action<ARSelectableObject> OnObjectSelected;
        public event Action<ARSelectableObject, Vector3> OnObjectDragged;
        public event Action<ARSelectableObject> OnObjectReleased;

        void Awake()
        {
            ResolveReferences();
        }

        void Update()
        {
            if (activeObject == null)
            {
                TryBeginDrag();
                return;
            }

            if (TryGetPointerUp())
            {
                ReleaseActiveObject();
                return;
            }

            if (TryGetPointerPosition(out var screenPosition))
            {
                ContinueDrag(screenPosition);
            }
        }

        public void ResolveReferences()
        {
            if (arCamera == null)
            {
                arCamera = Camera.main != null ? Camera.main : FindFirstObjectByType<Camera>();
            }

            if (raycastManager == null)
            {
                raycastManager = FindFirstObjectByType<ARRaycastManager>();
            }
        }

        void TryBeginDrag()
        {
            if (!TryGetPointerDown(out var screenPosition))
            {
                return;
            }

            if (ignorePointerOverUI && IsPointerOverUI())
            {
                return;
            }

            if (!TryRaycastSelectable(screenPosition, out var selectableObject) || !selectableObject.CanDrag)
            {
                return;
            }

            activeObject = selectableObject;
            dragStartPosition = screenPosition;
            fallbackDragHeight = activeObject.transform.position.y;
            isDragging = false;

            if (TryGetDragPoint(screenPosition, out var dragPoint))
            {
                dragOffset = activeObject.transform.position - dragPoint;
            }
            else
            {
                dragOffset = Vector3.zero;
            }

            activeObject.Select();
            OnObjectSelected?.Invoke(activeObject);
        }

        void ContinueDrag(Vector2 screenPosition)
        {
            if (!isDragging &&
                Vector2.Distance(dragStartPosition, screenPosition) < minimumDragDistancePixels)
            {
                return;
            }

            if (!TryGetDragPoint(screenPosition, out var dragPoint))
            {
                return;
            }

            isDragging = true;
            var targetPosition = dragPoint + dragOffset;
            activeObject.DragTo(targetPosition);
            OnObjectDragged?.Invoke(activeObject, targetPosition);
        }

        void ReleaseActiveObject()
        {
            activeObject.Release();
            OnObjectReleased?.Invoke(activeObject);
            activeObject = null;
            isDragging = false;
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
            if (!Physics.Raycast(ray, out var hit, maxObjectRaycastDistance, interactableLayers, QueryTriggerInteraction.Collide))
            {
                return false;
            }

            selectableObject = hit.collider.GetComponentInParent<ARSelectableObject>();
            return selectableObject != null;
        }

        bool TryGetDragPoint(Vector2 screenPosition, out Vector3 point)
        {
            ResolveReferences();

            if (raycastManager != null &&
                raycastManager.Raycast(screenPosition, RaycastHits, dragTrackableTypes))
            {
                point = RaycastHits[0].pose.position;
                return true;
            }

            if (arCamera != null)
            {
                var ray = arCamera.ScreenPointToRay(screenPosition);
                var fallbackPlane = new Plane(Vector3.up, new Vector3(0f, fallbackDragHeight, 0f));
                if (fallbackPlane.Raycast(ray, out var enter))
                {
                    point = ray.GetPoint(enter);
                    return true;
                }
            }

            point = default;
            return false;
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

        static bool TryGetPointerPosition(out Vector2 position)
        {
            if (Touchscreen.current != null)
            {
                var touch = Touchscreen.current.primaryTouch;
                if (touch.press.isPressed)
                {
                    position = touch.position.ReadValue();
                    return true;
                }
            }

            if (Mouse.current != null && Mouse.current.leftButton.isPressed)
            {
                position = Mouse.current.position.ReadValue();
                return true;
            }

            position = default;
            return false;
        }

        static bool TryGetPointerUp()
        {
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasReleasedThisFrame)
            {
                return true;
            }

            return Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame;
        }

        static bool IsPointerOverUI()
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(-1);
        }
    }
}
