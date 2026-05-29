using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace Features.Activities.NumberBonds
{
    public class NumberBondDragAdapter : MonoBehaviour
    {
        private readonly List<NumberBondZoneView> zones = new List<NumberBondZoneView>();
        private readonly List<RaycastResult> uiRaycastResults = new List<RaycastResult>();

        private Camera interactionCamera;
        private NumberBondObjectView activeObject;
        private NumberBondZoneView hoveredZone;
        private NumberBondZoneView pressedZone;
        private Vector2 pointerDownPosition;
        private float dragPlaneY;
        private bool inputEnabled = true;

        private const float TapThresholdPixels = 16f;

        public event Action<NumberBondObjectView, NumberBondZoneView> OnObjectDropped;
        public event Action<NumberBondZoneView> OnZoneTapped;

        private void Awake()
        {
            interactionCamera = Camera.main;
        }

        private void OnEnable()
        {
            EnhancedTouchSupport.Enable();
        }

        private void OnDisable()
        {
            EnhancedTouchSupport.Disable();
        }

        private void Update()
        {
            if (!inputEnabled)
            {
                return;
            }

            if (activeObject == null && pressedZone == null && TryGetPointerDown(out Vector2 downPosition))
            {
                BeginPointer(downPosition);
            }

            if (activeObject != null && TryGetPointerPosition(out Vector2 position))
            {
                ContinueDrag(position);
            }

            if ((activeObject != null || pressedZone != null) && TryGetPointerUp(out Vector2 upPosition))
            {
                EndPointer(upPosition);
            }
        }

        public void Configure(Camera camera, IEnumerable<NumberBondZoneView> zoneViews)
        {
            interactionCamera = camera != null ? camera : Camera.main;
            zones.Clear();

            if (zoneViews == null)
            {
                return;
            }

            foreach (NumberBondZoneView zone in zoneViews)
            {
                if (zone != null)
                {
                    zones.Add(zone);
                }
            }
        }

        public void SetInputEnabled(bool enabled)
        {
            inputEnabled = enabled;
            if (!enabled && activeObject != null)
            {
                activeObject.ReturnToLastStablePosition();
                activeObject = null;
                SetHoveredZone(null);
            }

            if (!enabled)
            {
                pressedZone = null;
            }
        }

        private void BeginPointer(Vector2 screenPosition)
        {
            if (IsPointerOverUi(screenPosition) || interactionCamera == null)
            {
                return;
            }

            pointerDownPosition = screenPosition;
            Ray ray = interactionCamera.ScreenPointToRay(screenPosition);
            RaycastHit[] hits = Physics.RaycastAll(ray, 100f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide);
            if (hits.Length == 0)
            {
                return;
            }

            Array.Sort(hits, (left, right) => left.distance.CompareTo(right.distance));

            NumberBondObjectView objectView = null;
            for (int i = 0; i < hits.Length; i++)
            {
                objectView = hits[i].collider.GetComponentInParent<NumberBondObjectView>();
                if (objectView != null)
                {
                    break;
                }
            }

            if (objectView == null || !objectView.IsMovable)
            {
                pressedZone = FindZoneInHits(hits);
                return;
            }

            activeObject = objectView;
            dragPlaneY = objectView.transform.position.y;
        }

        private void ContinueDrag(Vector2 screenPosition)
        {
            if (activeObject == null || interactionCamera == null)
            {
                return;
            }

            Ray ray = interactionCamera.ScreenPointToRay(screenPosition);
            Plane plane = new Plane(Vector3.up, new Vector3(0f, dragPlaneY, 0f));
            if (!plane.Raycast(ray, out float distance))
            {
                return;
            }

            Vector3 point = ray.GetPoint(distance);
            activeObject.transform.position = point;
            SetHoveredZone(FindZoneAt(point));
        }

        private void EndDrag()
        {
            NumberBondObjectView droppedObject = activeObject;
            NumberBondZoneView targetZone = hoveredZone;
            activeObject = null;
            SetHoveredZone(null);
            OnObjectDropped?.Invoke(droppedObject, targetZone);
        }

        private void EndPointer(Vector2 screenPosition)
        {
            if (activeObject != null)
            {
                EndDrag();
                pressedZone = null;
                return;
            }

            NumberBondZoneView tappedZone = pressedZone;
            pressedZone = null;

            if (tappedZone == null || (screenPosition - pointerDownPosition).sqrMagnitude > TapThresholdPixels * TapThresholdPixels)
            {
                return;
            }

            NumberBondZoneView releaseZone = FindZoneAtScreenPosition(screenPosition);
            if (releaseZone == tappedZone)
            {
                OnZoneTapped?.Invoke(tappedZone);
            }
        }

        private NumberBondZoneView FindZoneAt(Vector3 worldPosition)
        {
            for (int i = 0; i < zones.Count; i++)
            {
                if (zones[i] != null && zones[i].ContainsWorldPoint(worldPosition))
                {
                    return zones[i];
                }
            }

            return null;
        }

        private NumberBondZoneView FindZoneAtScreenPosition(Vector2 screenPosition)
        {
            if (interactionCamera == null)
            {
                return null;
            }

            Ray ray = interactionCamera.ScreenPointToRay(screenPosition);
            RaycastHit[] hits = Physics.RaycastAll(ray, 100f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide);
            if (hits.Length == 0)
            {
                return null;
            }

            Array.Sort(hits, (left, right) => left.distance.CompareTo(right.distance));
            return FindZoneInHits(hits);
        }

        private static NumberBondZoneView FindZoneInHits(RaycastHit[] hits)
        {
            for (int i = 0; i < hits.Length; i++)
            {
                NumberBondZoneView zone = hits[i].collider.GetComponentInParent<NumberBondZoneView>();
                if (zone != null)
                {
                    return zone;
                }
            }

            return null;
        }

        private void SetHoveredZone(NumberBondZoneView zone)
        {
            if (hoveredZone == zone)
            {
                return;
            }

            if (hoveredZone != null)
            {
                hoveredZone.SetHover(false);
            }

            hoveredZone = zone;
            if (hoveredZone != null)
            {
                hoveredZone.SetHover(true);
            }
        }

        private bool IsPointerOverUi(Vector2 screenPosition)
        {
            if (EventSystem.current == null)
            {
                return false;
            }

            uiRaycastResults.Clear();
            var pointerData = new PointerEventData(EventSystem.current)
            {
                position = screenPosition
            };
            EventSystem.current.RaycastAll(pointerData, uiRaycastResults);
            return uiRaycastResults.Count > 0;
        }

        private static bool TryGetPointerDown(out Vector2 screenPosition)
        {
            if (Touch.activeTouches.Count > 0)
            {
                Touch touch = Touch.activeTouches[0];
                if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
                {
                    screenPosition = touch.screenPosition;
                    return true;
                }
            }

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                screenPosition = Mouse.current.position.ReadValue();
                return true;
            }

            screenPosition = default;
            return false;
        }

        private static bool TryGetPointerPosition(out Vector2 screenPosition)
        {
            if (Touch.activeTouches.Count > 0)
            {
                Touch touch = Touch.activeTouches[0];
                if (touch.phase == UnityEngine.InputSystem.TouchPhase.Moved
                    || touch.phase == UnityEngine.InputSystem.TouchPhase.Stationary)
                {
                    screenPosition = touch.screenPosition;
                    return true;
                }
            }

            if (Mouse.current != null && Mouse.current.leftButton.isPressed)
            {
                screenPosition = Mouse.current.position.ReadValue();
                return true;
            }

            screenPosition = default;
            return false;
        }

        private static bool TryGetPointerUp(out Vector2 screenPosition)
        {
            if (Touch.activeTouches.Count > 0)
            {
                Touch touch = Touch.activeTouches[0];
                UnityEngine.InputSystem.TouchPhase phase = touch.phase;
                screenPosition = touch.screenPosition;
                return phase == UnityEngine.InputSystem.TouchPhase.Ended
                    || phase == UnityEngine.InputSystem.TouchPhase.Canceled;
            }

            if (Mouse.current != null)
            {
                screenPosition = Mouse.current.position.ReadValue();
                return Mouse.current.leftButton.wasReleasedThisFrame;
            }

            screenPosition = default;
            return false;
        }
    }
}
