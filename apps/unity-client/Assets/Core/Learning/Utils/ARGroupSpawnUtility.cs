using Core.Learning.ActivityRunner;
using Core.Support.Performance;
using UnityEngine;

namespace Core.Learning.Utils
{
    /// <summary>
    /// Shared utility for spawning object groups in AR activities.
    /// Extracted from QuantityMatchPresenter to be reused across activities.
    /// Depends only on the learning-facing placement interface.
    /// </summary>
    public static class ARGroupSpawnUtility
    {
        private const float DefaultObjectSpacing = 0.68f;

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

            int requestedCount = objectCount;
            objectCount = RuntimePerformanceSettings.ClampGroupObjectCount(objectCount);
            if (objectCount != requestedCount)
            {
                Debug.LogWarning($"[ARGroupSpawnUtility] Clamped group '{groupName}' from {requestedCount} to {objectCount} objects for device budget.");
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
                    objects = placementService.SpawnCircle(prefab, position, objectCount, CalculateCircleRadius(objectCount));
                    break;

                case ObjectArrangementPattern.Grid:
                    objects = placementService.SpawnGrid(prefab, position, objectCount, DefaultObjectSpacing);
                    break;

                default:
                    objects = placementService.SpawnCircle(prefab, position, objectCount, CalculateCircleRadius(objectCount));
                    break;
            }

            // Create parent object for the group
            GameObject group = new GameObject($"{groupName}_Count{objectCount}");
            group.transform.position = position;
            if (placementService.HasLearningArea && placementService.LearningAreaContentRoot != null)
            {
                group.transform.SetParent(placementService.LearningAreaContentRoot, true);
            }

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
                        float fallbackTotalWidth = (numberOfGroups - 1) * spacing;
                        float fallbackStartX = centerPosition.x - fallbackTotalWidth / 2f;

                        for (int i = 0; i < numberOfGroups; i++)
                        {
                            positions[i] = new Vector3(fallbackStartX + i * spacing, centerPosition.y, centerPosition.z);
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
                    (i % 3) * DefaultObjectSpacing,
                    0,
                    (i / 3) * DefaultObjectSpacing
                );
            }

            return group;
        }

        private static float CalculateCircleRadius(int objectCount)
        {
            if (objectCount <= 1)
            {
                return 0f;
            }

            float angleHalfStep = Mathf.PI / objectCount;
            float sin = Mathf.Sin(angleHalfStep);
            if (sin <= 0.0001f)
            {
                return DefaultObjectSpacing;
            }

            return DefaultObjectSpacing / (2f * sin);
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
    /// This remains local to avoid activity package dependencies in Core.
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
