using System;
using System.Collections.Generic;
using Core.Learning.ActivityRunner;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace Core.AR.Interaction
{
    /// <summary>
    /// Tap/select interaction for AR objects implementing <see cref="IARInteractionService"/>.
    /// Uses physics raycasts against colliders on registered objects.
    /// </summary>
    [DisallowMultipleComponent]
    public class ARInteractionService : MonoBehaviour, IARInteractionService
    {
        [SerializeField]
        private Camera interactionCamera;

        [SerializeField]
        private float highlightScaleMultiplier = 1.1f;

        [SerializeField]
        private Color highlightColor = new Color(1f, 0.92f, 0.3f, 1f);

        private readonly Dictionary<GameObject, InteractableEntry> interactables =
            new Dictionary<GameObject, InteractableEntry>();

        private bool interactionEnabled = true;
        private GameObject highlightedObject;
        private GameObject selectedObject;
        private GameObject draggingObject;

        public event Action<GameObject> OnObjectTapped;
        public event Action<GameObject> OnObjectSelected;
        public event Action<GameObject> OnObjectDeselected;
        public event Action<GameObject, Vector3> OnObjectDragged;
        public event Action<GameObject, Vector3> OnObjectDragEnded;

        private void Awake()
        {
            if (interactionCamera == null)
            {
                interactionCamera = Camera.main;
            }
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
            if (!interactionEnabled)
            {
                return;
            }

            if (TryGetPointerDown(out Vector2 downPosition))
            {
                ProcessPointerDown(downPosition);
            }

            if (draggingObject != null && TryGetPointerPosition(out Vector2 dragPosition))
            {
                ProcessDrag(dragPosition);
            }

            if (draggingObject != null && TryGetPointerUp())
            {
                EndDrag();
            }
        }

        public void Initialize()
        {
            Debug.Log("[ARInteractionService] Initialized.");
        }

        public void RegisterInteractable(GameObject obj, object data = null)
        {
            if (obj == null)
            {
                return;
            }

            interactables[obj] = new InteractableEntry
            {
                Data = data,
                OriginalScale = obj.transform.localScale
            };
        }

        public void UnregisterInteractable(GameObject obj)
        {
            if (obj == null)
            {
                return;
            }

            if (highlightedObject == obj)
            {
                SetHighlight(obj, false);
                highlightedObject = null;
            }

            if (selectedObject == obj)
            {
                DeselectCurrent();
            }

            if (draggingObject == obj)
            {
                EndDrag();
            }

            interactables.Remove(obj);
        }

        public object GetInteractableData(GameObject obj)
        {
            GameObject root = ResolveRegisteredRoot(obj);
            return root != null && interactables.TryGetValue(root, out InteractableEntry entry)
                ? entry.Data
                : null;
        }

        public void SetHighlight(GameObject obj, bool highlight)
        {
            GameObject root = ResolveRegisteredRoot(obj);
            if (root == null || !interactables.TryGetValue(root, out InteractableEntry entry))
            {
                return;
            }

            if (highlight)
            {
                if (highlightedObject != null && highlightedObject != root)
                {
                    SetHighlight(highlightedObject, false);
                }

                highlightedObject = root;
                entry.OriginalScale = root.transform.localScale;
                root.transform.localScale = entry.OriginalScale * highlightScaleMultiplier;
                ApplyHighlightColor(root, true);
            }
            else
            {
                root.transform.localScale = entry.OriginalScale;
                ApplyHighlightColor(root, false);

                if (highlightedObject == root)
                {
                    highlightedObject = null;
                }
            }
        }

        public void SetInteractionEnabled(bool enabled)
        {
            interactionEnabled = enabled;

            if (!enabled && highlightedObject != null)
            {
                SetHighlight(highlightedObject, false);
            }

            if (!enabled)
            {
                DeselectCurrent();
                EndDrag();
            }
        }

        public void ClearInteractables()
        {
            foreach (GameObject key in interactables.Keys)
            {
                if (key != null)
                {
                    SetHighlight(key, false);
                }
            }

            interactables.Clear();
            highlightedObject = null;
            selectedObject = null;
            draggingObject = null;
        }

        private void ProcessPointerDown(Vector2 screenPosition)
        {
            if (interactionCamera == null)
            {
                return;
            }

            Ray ray = interactionCamera.ScreenPointToRay(screenPosition);
            if (!Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                return;
            }

            GameObject root = ResolveRegisteredRoot(hit.collider.gameObject);
            if (root == null)
            {
                DeselectCurrent();
                return;
            }

            Select(root);
            OnObjectTapped?.Invoke(root);
            draggingObject = root;
        }

        private void ProcessDrag(Vector2 screenPosition)
        {
            if (interactionCamera == null)
            {
                return;
            }

            Ray ray = interactionCamera.ScreenPointToRay(screenPosition);
            Plane dragPlane = new Plane(Vector3.up, draggingObject.transform.position);
            if (dragPlane.Raycast(ray, out float distance))
            {
                OnObjectDragged?.Invoke(draggingObject, ray.GetPoint(distance));
            }
        }

        private void EndDrag()
        {
            if (draggingObject == null)
            {
                return;
            }

            OnObjectDragEnded?.Invoke(draggingObject, draggingObject.transform.position);
            draggingObject = null;
        }

        private void Select(GameObject root)
        {
            if (selectedObject == root)
            {
                return;
            }

            DeselectCurrent();
            selectedObject = root;
            OnObjectSelected?.Invoke(root);
        }

        private void DeselectCurrent()
        {
            if (selectedObject == null)
            {
                return;
            }

            GameObject previous = selectedObject;
            selectedObject = null;
            OnObjectDeselected?.Invoke(previous);
        }

        private GameObject ResolveRegisteredRoot(GameObject hitObject)
        {
            if (hitObject == null)
            {
                return null;
            }

            if (interactables.ContainsKey(hitObject))
            {
                return hitObject;
            }

            Transform current = hitObject.transform;
            while (current != null)
            {
                if (interactables.ContainsKey(current.gameObject))
                {
                    return current.gameObject;
                }

                current = current.parent;
            }

            return null;
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

        private static bool TryGetPointerUp()
        {
            if (Touch.activeTouches.Count > 0)
            {
                UnityEngine.InputSystem.TouchPhase phase = Touch.activeTouches[0].phase;
                return phase == UnityEngine.InputSystem.TouchPhase.Ended
                    || phase == UnityEngine.InputSystem.TouchPhase.Canceled;
            }

            return Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame;
        }

        private void ApplyHighlightColor(GameObject root, bool enabled)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                if (renderer.material != null && renderer.material.HasProperty("_Color"))
                {
                    if (enabled)
                    {
                        renderer.material.color = highlightColor;
                    }
                    else
                    {
                        renderer.material.color = Color.white;
                    }
                }
            }
        }

        private class InteractableEntry
        {
            public object Data;
            public Vector3 OriginalScale;
        }
    }
}
