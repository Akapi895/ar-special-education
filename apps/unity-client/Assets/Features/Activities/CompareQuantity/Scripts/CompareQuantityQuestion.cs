using Core.Learning.Models;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Features.Activities.CompareQuantity
{
    /// <summary>
    /// Represents a single question in the Compare Quantity activity.
    /// </summary>
    [Serializable]
    public class CompareQuantityQuestion
    {
        /// <summary>
        /// Number of objects in the left group.
        /// </summary>
        [Tooltip("Number of objects in the left group")]
        public int LeftGroupCount;

        /// <summary>
        /// Number of objects in the right group.
        /// </summary>
        [Tooltip("Number of objects in the right group")]
        public int RightGroupCount;

        /// <summary>
        /// The correct comparison answer.
        /// </summary>
        [Tooltip("Correct answer: More, Fewer, or Equal")]
        public ComparisonAnswer CorrectAnswer;

        /// <summary>
        /// Optional: Custom hints for this specific question.
        /// If null, default hints from config will be used.
        /// </summary>
        [Tooltip("Custom hints for this question (optional)")]
        public List<ActivityHint> CustomHints;

        /// <summary>
        /// Optional: Prefab name for objects in this question.
        /// </summary>
        [Tooltip("Optional: specific prefab for this question")]
        public string ObjectPrefabName;

        /// <summary>
        /// Validate the question data.
        /// </summary>
        public bool IsValid()
        {
            if (LeftGroupCount < 0 || RightGroupCount < 0)
            {
                Debug.LogWarning($"[CompareQuantityQuestion] Invalid group counts: {LeftGroupCount}, {RightGroupCount}");
                return false;
            }

            // Verify the correct answer matches the actual comparison
            ComparisonAnswer expectedAnswer = CalculateCorrectAnswer();
            if (CorrectAnswer != expectedAnswer)
            {
                Debug.LogWarning($"[CompareQuantityQuestion] CorrectAnswer ({CorrectAnswer}) doesn't match " +
                               $"actual comparison of {LeftGroupCount} vs {RightGroupCount} (should be {expectedAnswer})");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Calculate the correct answer based on group counts.
        /// </summary>
        public ComparisonAnswer CalculateCorrectAnswer()
        {
            if (LeftGroupCount == RightGroupCount)
            {
                return ComparisonAnswer.Equal;
            }
            else if (LeftGroupCount > RightGroupCount)
            {
                return ComparisonAnswer.More;
            }
            else
            {
                return ComparisonAnswer.Fewer;
            }
        }

        /// <summary>
        /// Check if this is an equality question (both groups same count).
        /// </summary>
        public bool IsEqualityQuestion()
        {
            return LeftGroupCount == RightGroupCount;
        }

        /// <summary>
        /// Check if the given answer is correct.
        /// </summary>
        public bool IsCorrectAnswer(ComparisonAnswer answer)
        {
            return answer == CorrectAnswer;
        }
    }

    /// <summary>
    /// Possible comparison answers.
    /// </summary>
    [Serializable]
    public enum ComparisonAnswer
    {
        /// <summary>
        /// Left group has more objects than right.
        /// </summary>
        More,

        /// <summary>
        /// Left group has fewer objects than right.
        /// </summary>
        Fewer,

        /// <summary>
        /// Both groups have equal objects.
        /// </summary>
        Equal
    }
}
