using Core.Learning.ActivityRunner;
using UnityEngine;

namespace Core.Learning.Utils
{
    /// <summary>
    /// Shared utility for spawning object groups in AR activities.
    /// Extracted from QuantityMatchPresenter to be reused across activities.
    ///
    /// TODO: Review with team - this utility depends on IARPlacementService implementation.
    /// </summary>
    public static class ARGroupSpawnUtility
    {
        /// <summary>
        /// Spawn a single group with the given number of objects.
        /// </summary>
        /// <param name="placementService">The AR placement service.</param>
        /// <param name="prefab">The prefab to spawn.</param>
        /// <param name="position">Center position for the group.</param>
        /// <param name="objectCount">Number of objects in the group.</param>
        /// <param name="arrangementPattern">How to arrange objects within the group.</param>
        /// <param name="groupName">Name for the group GameObject.</param>
        /// <returns>The parent GameObject containing all spawned objects.</returns>
        public static GameObject SpawnGroup(IARPlacementService placementService, GameObject prefab,
            Vector3 position, int objectCount, ObjectArrangementPattern arrangementPattern,
            string groupName)
        {
            if (placementService == null)
            {
                Debug.LogError("[ARGroupSpawnUtility] Placement service is null.");
                return CreatePlaceholderGroup(position, objectCount, groupName);
            }

            if (prefab == null)
            {
                Debug.LogWarning($"[ARGroupSpawnUtility] Prefab is null for group '{groupName}'. Creating placeholder.");
                return CreatePlaceholderGroup(position, objectCount, groupName);
            }

            // Spawn objects based on arrangement pattern
            GameObject[] objects = null;

            switch (arrangementPattern)
            {
                case ObjectArrangementPattern.Circle:
                    objects = placementService.SpawnCircle(prefab, position, objectCount, 0.2f);
                    break;

                case ObjectArrangementPattern.Grid:
                    // For grid, we spawn a simple arrangement
                    // TODO: Add SpawnGrid method to IARPlacementService if needed
                    objects = placementService.SpawnCircle(prefab, position, objectCount, 0.2f);
                    break;

                default:
                    objects = placementService.SpawnCircle(prefab, position, objectCount, 0.2f);
                    break;
            }

            // Create parent object for the group
            GameObject group = new GameObject($"{groupName}_Count{objectCount}");
            group.transform.position = position;

            if (objects != null)
            {
                foreach (GameObject obj in objects)
                {
                    if (obj != null)
                    {
                        obj.transform.SetParent(group.transform);
                    }
                }
            }

            return group;
        }

        /// <summary>
        /// Calculate positions for multiple groups.
        /// </summary>
        /// <param name="numberOfGroups">Number of groups to position.</param>
        /// <param name="centerPosition">Center point for all groups.</param>
        /// <param name="spacing">Spacing between groups.</param>
        /// <param name="arrangementPattern">How to arrange the groups.</param>
        /// <returns>Array of positions for each group.</returns>
        public static Vector3[] CalculateGroupPositions(int numberOfGroups, Vector3 centerPosition,
            float spacing, GroupArrangementPattern arrangementPattern)
        {
            Vector3[] positions = new Vector3[numberOfGroups];

            switch (arrangementPattern)
            {
                case GroupArrangementPattern.Horizontal:
                    // Arrange in a horizontal row
                    float totalWidth = (numberOfGroups - 1) * spacing;
                    float startX = centerPosition.x - totalWidth / 2f;

                    for (int i = 0; i < numberOfGroups; i++)
                    {
                        positions[i] = new Vector3(startX + i * spacing, centerPosition.y, centerPosition.z);
                    }
                    break;

                case GroupArrangementPattern.Vertical:
                    // Arrange in a vertical column
                    float totalHeight = (numberOfGroups - 1) * spacing;
                    float startY = centerPosition.y - totalHeight / 2f;

                    for (int i = 0; i < numberOfGroups; i++)
                    {
                        positions[i] = new Vector3(centerPosition.x, startY + i * spacing, centerPosition.z);
                    }
                    break;

                case GroupArrangementPattern.Circular:
                    // Arrange in a circle
                    float radius = spacing * 0.8f;
                    for (int i = 0; i < numberOfGroups; i++)
                    {
                        float angle = (360f / numberOfGroups) * i * Mathf.Deg2Rad;
                        positions[i] = centerPosition + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
                    }
                    break;

                case GroupArrangementPattern.SideBySide:
                    // Special case for 2 groups - one left, one right
                    if (numberOfGroups == 2)
                    {
                        positions[0] = centerPosition + Vector3.left * (spacing / 2f);
                        positions[1] = centerPosition + Vector3.right * (spacing / 2f);
                    }
                    else
                    {
                        // Fall back to horizontal for other counts
                        float totalWidth = (numberOfGroups - 1) * spacing;
                        float startX = centerPosition.x - totalWidth / 2f;

                        for (int i = 0; i < numberOfGroups; i++)
                        {
                            positions[i] = new Vector3(startX + i * spacing, centerPosition.y, centerPosition.z);
                        }
                    }
                    break;

                case GroupArrangementPattern.Random:
                    // Random positions within bounds
                    for (int i = 0; i < numberOfGroups; i++)
                    {
                        positions[i] = centerPosition + new Vector3(
                            UnityEngine.Random.Range(-spacing, spacing),
                            0,
                            UnityEngine.Random.Range(-spacing, spacing)
                        );
                    }
                    break;
            }

            return positions;
        }

        /// <summary>
        /// Create a placeholder group when prefab is not available.
        /// </summary>
        private static GameObject CreatePlaceholderGroup(Vector3 position, int objectCount, string groupName)
        {
            GameObject group = new GameObject($"Placeholder_{groupName}_Count{objectCount}");
            group.transform.position = position;

            // Create placeholder objects
            for (int i = 0; i < objectCount; i++)
            {
                GameObject placeholder = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                placeholder.name = $"Placeholder_Object{i}";
                placeholder.transform.SetParent(group.transform);
                placeholder.transform.localScale = Vector3.one * 0.1f;
                placeholder.transform.localPosition = new Vector3(
                    (i % 3) * 0.15f,
                    0,
                    (i / 3) * 0.15f
                );
            }

            return group;
        }
    }

    /// <summary>
    /// How objects are arranged within a group.
    /// </summary>
    public enum ObjectArrangementPattern
    {
        /// <summary>
        /// Objects arranged in a circle.
        /// </summary>
        Circle,

        /// <summary>
        /// Objects arranged in a grid.
        /// </summary>
        Grid,

        /// <summary>
        /// Objects randomly placed.
        /// </summary>
        Random
    }

    /// <summary>
    /// How groups are arranged relative to each other.
    /// Reuse the enum from QuantityMatchConfig for consistency.
    /// This is duplicated here to avoid circular dependencies.
    /// TODO: Consider moving to a common location.
    /// </summary>
    public enum GroupArrangementPattern
    {
        Horizontal,
        Vertical,
        Circular,
        SideBySide,
        Random
    }
}
