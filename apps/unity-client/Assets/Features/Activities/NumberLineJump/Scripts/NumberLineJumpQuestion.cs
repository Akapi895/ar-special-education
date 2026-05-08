using System;
using System.Collections.Generic;
using UnityEngine;

namespace Features.Activities.NumberLineJump
{
    /// <summary>
    /// Represents a single question in the Number Line Jump activity.
    /// </summary>
    [Serializable]
    public class NumberLineJumpQuestion
    {
        [Header("Number Line Settings")]
        [Tooltip("Minimum number on the number line")]
        [SerializeField]
        private int numberLineMin = 0;

        [Tooltip("Maximum number on the number line")]
        [SerializeField]
        private int numberLineMax = 10;

        [Header("Question Settings")]
        [Tooltip("Starting position for the character")]
        [SerializeField]
        private int startNumber;

        [Tooltip("Target number to reach")]
        [SerializeField]
        private int targetNumber;

        [Tooltip("Allowed jump direction(s)")]
        [SerializeField]
        private JumpDirection jumpDirection = JumpDirection.RightOnly;

        [Tooltip("Maximum number of jumps allowed (0 = no limit)")]
        [SerializeField]
        private int maxJumpsAllowed = 10;

        [Tooltip("Whether to show the equation during jumps")]
        [SerializeField]
        private bool showEquationDuringJumps = true;

        [Tooltip("Custom hints for this specific question")]
        [SerializeField]
        private List<ActivityHint> customHints;

        [Header("Character Settings")]
        [Tooltip("Prefab name for the jump character")]
        [SerializeField]
        private string characterPrefabName = "JumpCharacter";

        [Tooltip("Prefab name for number tiles")]
        [SerializeField]
        private string tilePrefabName = "NumberTile";

        // Properties
        public int NumberLineMin => numberLineMin;
        public int NumberLineMax => numberLineMax;
        public int StartNumber => startNumber;
        public int TargetNumber => targetNumber;
        public JumpDirection JumpDirection => jumpDirection;
        public int MaxJumpsAllowed => maxJumpsAllowed;
        public bool ShowEquationDuringJumps => showEquationDuringJumps;
        public List<ActivityHint> CustomHints => customHints;
        public string CharacterPrefabName => characterPrefabName;
        public string TilePrefabName => tilePrefabName;

        /// <summary>
        /// Validate the question data.
        /// </summary>
        public bool IsValid()
        {
            if (numberLineMin < 0)
            {
                Debug.LogWarning($"[NumberLineJumpQuestion] Invalid NumberLineMin: {numberLineMin}");
                return false;
            }

            if (numberLineMax <= numberLineMin)
            {
                Debug.LogWarning($"[NumberLineJumpQuestion] NumberLineMax ({numberLineMax}) must be greater than NumberLineMin ({numberLineMin})");
                return false;
            }

            if (startNumber < numberLineMin || startNumber > numberLineMax)
            {
                Debug.LogWarning($"[NumberLineJumpQuestion] StartNumber ({startNumber}) is outside number line range [{numberLineMin}, {numberLineMax}]");
                return false;
            }

            if (targetNumber < numberLineMin || targetNumber > numberLineMax)
            {
                Debug.LogWarning($"[NumberLineJumpQuestion] TargetNumber ({targetNumber}) is outside number line range [{numberLineMin}, {numberLineMax}]");
                return false;
            }

            if (startNumber == targetNumber)
            {
                Debug.LogWarning($"[NumberLineJumpQuestion] StartNumber and TargetNumber cannot be the same");
                return false;
            }

            // Validate jump direction matches the problem
            if (jumpDirection == JumpDirection.RightOnly && targetNumber < startNumber)
            {
                Debug.LogWarning($"[NumberLineJumpQuestion] JumpDirection is RightOnly but TargetNumber ({targetNumber}) < StartNumber ({startNumber})");
                return false;
            }

            if (jumpDirection == JumpDirection.LeftOnly && targetNumber > startNumber)
            {
                Debug.LogWarning($"[NumberLineJumpQuestion] JumpDirection is LeftOnly but TargetNumber ({targetNumber}) > StartNumber ({startNumber})");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Calculate the expected number of jumps.
        /// </summary>
        public int GetExpectedJumpCount()
        {
            return Mathf.Abs(targetNumber - startNumber);
        }

        /// <summary>
        /// Check if the direction is allowed for this question.
        /// </summary>
        public bool IsDirectionAllowed(JumpStepDirection direction)
        {
            return jumpDirection switch
            {
                JumpDirection.RightOnly => direction == JumpStepDirection.Right,
                JumpDirection.LeftOnly => direction == JumpStepDirection.Left,
                JumpDirection.Both => true,
                _ => false
            };
        }

        /// <summary>
        /// Get the expected direction for this question.
        /// </summary>
        public JumpStepDirection GetExpectedDirection()
        {
            return targetNumber > startNumber ? JumpStepDirection.Right : JumpStepDirection.Left;
        }

        /// <summary>
        /// Check if a position is within the number line bounds.
        /// </summary>
        public bool IsWithinBounds(int position)
        {
            return position >= numberLineMin && position <= numberLineMax;
        }

        /// <summary>
        /// Check if a position has overshot the target.
        /// </summary>
        public bool HasOvershotTarget(int position, JumpStepDirection jumpDirection)
        {
            if (jumpDirection == JumpStepDirection.Right)
            {
                return position > targetNumber;
            }
            else // Left
            {
                return position < targetNumber;
            }
        }
    }
}
