using System;
using System.Collections.Generic;
using UnityEngine;

namespace Features.Activities.QuantityMatch
{
    /// <summary>
    /// Represents a single question in the Quantity Match activity.
    /// </summary>
    [Serializable]
    public class QuantityMatchQuestion
    {
        /// <summary>
        /// The target number the child needs to match.
        /// </summary>
        [Tooltip("The target number to match")]
        public int TargetNumber;

        /// <summary>
        /// Number of groups to display as choices.
        /// </summary>
        [Tooltip("Number of groups to show as choices")]
        public int NumberOfGroups;

        /// <summary>
        /// Object count for each group.
        /// Must match NumberOfGroups length.
        /// </summary>
        [Tooltip("How many objects in each group")]
        public int[] ObjectCountsPerGroup;

        /// <summary>
        /// Index of the correct group (0-based).
        /// Must match a group where ObjectCountsPerGroup == TargetNumber.
        /// </summary>
        [Tooltip("Which group is the correct answer (0-based index)")]
        public int CorrectGroupIndex;

        /// <summary>
        /// Optional: Prefab name to use for objects in this question.
        /// </summary>
        [Tooltip("Optional: specific prefab for this question")]
        public string ObjectPrefabName;

        /// <summary>
        /// Custom hints for this specific question (optional).
        /// If null, default hints from config will be used.
        /// </summary>
        [Tooltip("Custom hints for this question (optional)")]
        public List<ActivityHint> CustomHints;

        /// <summary>
        /// Validate the question data.
        /// </summary>
        public bool IsValid()
        {
            if (TargetNumber <= 0)
            {
                Debug.LogWarning($"[QuantityMatchQuestion] Invalid TargetNumber: {TargetNumber}");
                return false;
            }

            if (NumberOfGroups <= 0)
            {
                Debug.LogWarning($"[QuantityMatchQuestion] Invalid NumberOfGroups: {NumberOfGroups}");
                return false;
            }

            if (ObjectCountsPerGroup == null || ObjectCountsPerGroup.Length != NumberOfGroups)
            {
                Debug.LogWarning($"[QuantityMatchQuestion] ObjectCountsPerGroup length mismatch");
                return false;
            }

            if (CorrectGroupIndex < 0 || CorrectGroupIndex >= NumberOfGroups)
            {
                Debug.LogWarning($"[QuantityMatchQuestion] Invalid CorrectGroupIndex: {CorrectGroupIndex}");
                return false;
            }

            // Verify the correct group actually has the target number
            if (ObjectCountsPerGroup[CorrectGroupIndex] != TargetNumber)
            {
                Debug.LogWarning($"[QuantityMatchQuestion] CorrectGroupIndex {CorrectGroupIndex} has {ObjectCountsPerGroup[CorrectGroupIndex]} objects, not {TargetNumber}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Check if the given group index is correct.
        /// </summary>
        public bool IsCorrectAnswer(int groupIndex)
        {
            return groupIndex == CorrectGroupIndex;
        }
    }
}
