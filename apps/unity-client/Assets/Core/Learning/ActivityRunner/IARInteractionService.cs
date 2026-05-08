using UnityEngine;
using System;

namespace Core.Learning.ActivityRunner
{
    /// <summary>
    /// Interface for AR interaction services.
    /// Learning activities use this to receive user input (tap, select, drag) on AR objects.
    ///
    /// TODO: KHÁNH'S TEAM - Implement this interface in the AR Core module.
    /// This interface is defined by Learning layer but implemented by AR layer.
    /// </summary>
    public interface IARInteractionService
    {
        /// <summary>
        /// Event fired when an AR object is tapped/clicked.
        /// </summary>
        event Action<GameObject> OnObjectTapped;

        /// <summary>
        /// Event fired when an AR object is selected.
        /// </summary>
        event Action<GameObject> OnObjectSelected;

        /// <summary>
        /// Event fired when an AR object is deselected.
        /// </summary>
        event Action<GameObject> OnObjectDeselected;

        /// <summary>
        /// Event fired when an AR object is dragged.
        /// </summary>
        event Action<GameObject, Vector3> OnObjectDragged;

        /// <summary>
        /// Event fired when a drag operation ends.
        /// </summary>
        event Action<GameObject, Vector3> OnObjectDragEnded;

        /// <summary>
        /// Register an object as interactable.
        /// </summary>
        /// <param name="obj">The GameObject to make interactable.</param>
        /// <param name="data">Optional data associated with this object.</param>
        void RegisterInteractable(GameObject obj, object data = null);

        /// <summary>
        /// Unregister an object from interaction.
        /// </summary>
        /// <param name="obj">The GameObject to remove from interaction.</param>
        void UnregisterInteractable(GameObject obj);

        /// <summary>
        /// Get the data associated with an interactable object.
        /// </summary>
        /// <param name="obj">The GameObject.</param>
        /// <returns>The associated data, or null if none.</returns>
        object GetInteractableData(GameObject obj);

        /// <summary>
        /// Highlight an object to show it can be interacted with.
        /// </summary>
        /// <param name="obj">The GameObject to highlight.</param>
        /// <param name="highlight">True to highlight, false to remove highlight.</param>
        void SetHighlight(GameObject obj, bool highlight);

        /// <summary>
        /// Enable or disable interaction on all registered objects.
        /// </summary>
        /// <param name="enabled">True to enable, false to disable.</param>
        void SetInteractionEnabled(bool enabled);

        /// <summary>
        /// Clear all registered interactables.
        /// </summary>
        void ClearInteractables();

        /// <summary>
        /// Initialize the interaction service.
        /// </summary>
        void Initialize();
    }
}
