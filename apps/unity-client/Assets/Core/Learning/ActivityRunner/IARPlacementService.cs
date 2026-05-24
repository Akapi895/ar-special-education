using UnityEngine;
using System;

namespace Core.Learning.ActivityRunner
{
    /// <summary>
    /// Interface for AR object placement services.
    /// Learning activities use this to spawn/place objects in the AR environment.
    ///
    /// TODO: KHÁNH'S TEAM - Implement this interface in the AR Core module.
    /// This interface is defined by Learning layer but implemented by AR layer.
    /// </summary>
    public interface IARPlacementService
    {
        /// <summary>
        /// Event fired when a valid placement position is detected.
        /// </summary>
        event Action<Vector3> OnPlacementPositionAvailable;

        /// <summary>
        /// Event fired when placement position becomes invalid.
        /// </summary>
        event Action OnPlacementPositionLost;

        /// <summary>
        /// Check if a valid placement position is currently available.
        /// </summary>
        bool IsPlacementAvailable { get; }

        /// <summary>
        /// Get the current valid placement position.
        /// </summary>
        Vector3 CurrentPlacementPosition { get; }

        /// <summary>
        /// Spawn a prefab at the current placement position.
        /// </summary>
        /// <param name="prefab">The prefab to spawn.</param>
        /// <param name="parent">Optional parent transform.</param>
        /// <returns>The spawned GameObject.</returns>
        GameObject SpawnAtPlacementPosition(GameObject prefab, Transform parent = null);

        /// <summary>
        /// Spawn a prefab at a specific world position.
        /// </summary>
        /// <param name="prefab">The prefab to spawn.</param>
        /// <param name="position">World position to spawn at.</param>
        /// <param name="rotation">Rotation for the spawned object.</param>
        /// <param name="parent">Optional parent transform.</param>
        /// <returns>The spawned GameObject.</returns>
        GameObject SpawnAtPosition(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null);

        /// <summary>
        /// Spawn multiple objects arranged in a grid.
        /// Useful for quantity matching activities.
        /// </summary>
        /// <param name="prefab">The prefab to spawn.</param>
        /// <param name="centerPosition">Center of the grid.</param>
        /// <param name="count">Number of objects to spawn.</param>
        /// <param name="spacing">Spacing between objects.</param>
        /// <returns>Array of spawned GameObjects.</returns>
        GameObject[] SpawnGrid(GameObject prefab, Vector3 centerPosition, int count, float spacing);

        /// <summary>
        /// Spawn multiple objects arranged in a circle.
        /// Useful for number line or grouping activities.
        /// </summary>
        /// <param name="prefab">The prefab to spawn.</param>
        /// <param name="centerPosition">Center of the circle.</param>
        /// <param name="count">Number of objects to spawn.</param>
        /// <param name="radius">Radius of the circle.</param>
        /// <returns>Array of spawned GameObjects.</returns>
        GameObject[] SpawnCircle(GameObject prefab, Vector3 centerPosition, int count, float radius);

        /// <summary>
        /// Clear all objects spawned by this service.
        /// </summary>
        void ClearSpawnedObjects();

        /// <summary>
        /// Initialize the placement service.
        /// </summary>
        void Initialize();
    }
}
