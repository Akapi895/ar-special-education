using Core.Learning.Models;
using Features.Activities.NumberLineJump;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Features.Activities.NumberLineJump
{
    /// <summary>
    /// Represents a single jump in the Number Line Jump activity.
    /// </summary>
    [Serializable]
    public class JumpRecord
    {
        /// <summary>
        /// The direction of this jump.
        /// </summary>
        public JumpStepDirection Direction;

        /// <summary>
        /// Position before the jump.
        /// </summary>
        public int FromPosition;

        /// <summary>
        /// Position after the jump.
        /// </summary>
        public int ToPosition;

        /// <summary>
        /// Timestamp when this jump occurred.
        /// </summary>
        public float Timestamp;

        public JumpRecord(JumpStepDirection direction, int from, int to, float timestamp)
        {
            Direction = direction;
            FromPosition = from;
            ToPosition = to;
            Timestamp = timestamp;
        }
    }

    /// <summary>
    /// Represents an answer in the Number Line Jump activity.
    /// </summary>
    [Serializable]
    public class NumberLineJumpAnswer : ActivityAnswer
    {
        /// <summary>
        /// The starting number for this question.
        /// </summary>
        public int StartNumber;

        /// <summary>
        /// The final position where the character ended up.
        /// </summary>
        public int FinalPosition;

        /// <summary>
        /// The target number that was the goal.
        /// </summary>
        public int TargetNumber;

        /// <summary>
        /// All jumps made during this attempt.
        /// </summary>
        public List<JumpRecord> Jumps;

        /// <summary>
        /// Whether the player overshot the target.
        /// </summary>
        public bool HasOvershot;

        /// <summary>
        /// Whether the player hit a boundary.
        /// </summary>
        public bool HitBoundary;

        /// <summary>
        /// Whether max jumps were exceeded.
        /// </summary>
        public bool ExceededMaxJumps;

        /// <summary>
        /// The final equation string (e.g., "3 + 2 = 5").
        /// </summary>
        public string FinalEquation;

        public NumberLineJumpAnswer()
        {
            Jumps = new List<JumpRecord>();
        }

        public NumberLineJumpAnswer(int startNumber, int finalPosition, int targetNumber,
            List<JumpRecord> jumps, bool hasOvershot, bool hitBoundary, bool exceededMaxJumps,
            float responseTimeSeconds, int attemptNumber, bool hintsUsed)
            : base(finalPosition, responseTimeSeconds, attemptNumber, hintsUsed)
        {
            StartNumber = startNumber;
            FinalPosition = finalPosition;
            TargetNumber = targetNumber;
            Jumps = jumps ?? new List<JumpRecord>();
            HasOvershot = hasOvershot;
            HitBoundary = hitBoundary;
            ExceededMaxJumps = exceededMaxJumps;
            FinalEquation = BuildEquation();
        }

        /// <summary>
        /// Check if the final position is correct (reached the target).
        /// </summary>
        public bool IsCorrect()
        {
            return FinalPosition == TargetNumber;
        }

        /// <summary>
        /// Get the total number of jumps made.
        /// </summary>
        public int GetTotalJumps()
        {
            return Jumps?.Count ?? 0;
        }

        /// <summary>
        /// Check if the jumps were all in the same direction.
        /// </summary>
        public bool HasConsistentDirection()
        {
            if (Jumps == null || Jumps.Count == 0)
            {
                return true;
            }

            JumpStepDirection firstDirection = Jumps[0].Direction;
            foreach (var jump in Jumps)
            {
                if (jump.Direction != firstDirection)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Build the equation string from the jumps.
        /// </summary>
        private string BuildEquation()
        {
            if (Jumps == null || Jumps.Count == 0)
            {
                return $"{StartNumber} = {FinalPosition}";
            }

            int totalChange = FinalPosition - StartNumber;
            string operation = totalChange >= 0 ? "+" : "-";
            int absChange = Mathf.Abs(totalChange);

            return $"{StartNumber} {operation} {absChange} = {FinalPosition}";
        }

        /// <summary>
        /// Get the current equation string at a specific point during jumping.
        /// </summary>
        public static string GetCurrentEquation(int startNumber, int currentPosition)
        {
            int change = currentPosition - startNumber;

            if (change == 0)
            {
                return $"{startNumber}";
            }

            string operation = change >= 0 ? "+" : "-";
            int absChange = Mathf.Abs(change);

            return $"{startNumber} {operation} {absChange} = ?";
        }

        /// <summary>
        /// Get a hint direction string based on the target.
        /// </summary>
        public static string GetDirectionHint(int startNumber, int targetNumber)
        {
            if (targetNumber > startNumber)
            {
                int steps = targetNumber - startNumber;
                return $"sang ph\u1ea3i {steps} b\u01b0\u1edbc";
            }
            else
            {
                int steps = startNumber - targetNumber;
                return $"sang tr\u00e1i {steps} b\u01b0\u1edbc";
            }
        }
    }
}
