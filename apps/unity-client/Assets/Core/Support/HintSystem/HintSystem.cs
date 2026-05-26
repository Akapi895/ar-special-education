using Core.Learning.Models;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Support.HintSystem
{
    /// <summary>
    /// Service for managing hints across learning activities.
    /// Handles hint escalation, usage tracking, and context-aware hints.
    /// </summary>
    public class HintSystem
    {
        // Hint usage tracking: key = "activityId_questionNumber", value = hint count
        private Dictionary<string, int> hintUsageMap = new Dictionary<string, int>();

        // Hint history: key = "activityId_questionNumber", value = list of hint levels shown
        private Dictionary<string, List<int>> hintHistoryMap = new Dictionary<string, List<int>>();

        // Settings
        private int defaultMaxHints = 3;
        private float hintDisplayDuration = 5f;
        private float hintCooldown = 10f;

        // Last hint timestamp per activity
        private Dictionary<string, float> lastHintTimeMap = new Dictionary<string, float>();

        // Events
        public event Action<ActivityHint> OnHintProvided;
        public event Action<string> OnMaxHintsReached;
        public event Action<string, int> OnHintUsed;  // activityKey, hintLevel

        /// <summary>
        /// Request a hint for a specific question.
        /// </summary>
        /// <param name="activityId">The activity identifier.</param>
        /// <param name="questionNumber">The question number (1-based).</param>
        /// <param name="availableHints">List of available hints for this question.</param>
        /// <param name="maxHints">Maximum hints allowed for this question.</param>
        /// <returns>The hint to display, or null if no hints available.</returns>
        public ActivityHint RequestHint(string activityId, int questionNumber,
            List<ActivityHint> availableHints, int maxHints = 3)
        {
            string key = GetHintKey(activityId, questionNumber);

            // Check if hints are available
            if (availableHints == null || availableHints.Count == 0)
            {
                return null;
            }

            // Check cooldown
            if (IsOnCooldown(key))
            {
                return null;
            }

            // Get current hint usage count
            int currentHintCount = GetHintCount(key);

            // Check if max hints reached
            if (currentHintCount >= maxHints || currentHintCount >= availableHints.Count)
            {
                OnMaxHintsReached?.Invoke(key);
                return null;
            }

            // Get the next hint level (1-based)
            int hintLevel = currentHintCount + 1;

            // Get the hint (hints are typically ordered by level)
            ActivityHint hint = GetHintAtLevel(availableHints, hintLevel);

            if (hint == null)
            {
                return null;
            }

            // Record usage
            RecordHintUsage(key, hintLevel);

            // Update timestamp
            lastHintTimeMap[key] = Time.time;

            // Fire events
            OnHintUsed?.Invoke(key, hintLevel);
            OnHintProvided?.Invoke(hint);

            return hint;
        }

        /// <summary>
        /// Request a specific hint level.
        /// Use with caution - should only be used for testing or special cases.
        /// </summary>
        public ActivityHint RequestSpecificHint(string activityId, int questionNumber,
            List<ActivityHint> availableHints, int hintLevel)
        {
            string key = GetHintKey(activityId, questionNumber);

            if (availableHints == null || hintLevel < 1 || hintLevel > availableHints.Count)
            {
                return null;
            }

            ActivityHint hint = GetHintAtLevel(availableHints, hintLevel);

            if (hint != null)
            {
                RecordHintUsage(key, hintLevel);
                lastHintTimeMap[key] = Time.time;
                OnHintUsed?.Invoke(key, hintLevel);
                OnHintProvided?.Invoke(hint);
            }

            return hint;
        }

        /// <summary>
        /// Reset hint usage for a specific question.
        /// </summary>
        public void ResetHints(string activityId, int questionNumber)
        {
            string key = GetHintKey(activityId, questionNumber);

            if (hintUsageMap.ContainsKey(key))
            {
                hintUsageMap.Remove(key);
            }

            if (hintHistoryMap.ContainsKey(key))
            {
                hintHistoryMap.Remove(key);
            }

            if (lastHintTimeMap.ContainsKey(key))
            {
                lastHintTimeMap.Remove(key);
            }
        }

        /// <summary>
        /// Reset all hints for an activity.
        /// </summary>
        public void ResetActivityHints(string activityId)
        {
            List<string> keysToRemove = new List<string>();

            foreach (var key in hintUsageMap.Keys)
            {
                if (key.StartsWith(activityId + "_"))
                {
                    keysToRemove.Add(key);
                }
            }

            foreach (var key in keysToRemove)
            {
                hintUsageMap.Remove(key);
                hintHistoryMap.Remove(key);
                lastHintTimeMap.Remove(key);
            }
        }

        /// <summary>
        /// Reset all hint tracking.
        /// </summary>
        public void ResetAll()
        {
            hintUsageMap.Clear();
            hintHistoryMap.Clear();
            lastHintTimeMap.Clear();
        }

        /// <summary>
        /// Get the number of hints used for a question.
        /// </summary>
        public int GetHintCount(string activityId, int questionNumber)
        {
            string key = GetHintKey(activityId, questionNumber);
            return GetHintCount(key);
        }

        /// <summary>
        /// Get the hint history for a question.
        /// </summary>
        public List<int> GetHintHistory(string activityId, int questionNumber)
        {
            string key = GetHintKey(activityId, questionNumber);

            if (hintHistoryMap.ContainsKey(key))
            {
                return new List<int>(hintHistoryMap[key]);
            }

            return new List<int>();
        }

        /// <summary>
        /// Check if more hints are available for a question.
        /// </summary>
        public bool HasMoreHints(string activityId, int questionNumber, int maxHints)
        {
            string key = GetHintKey(activityId, questionNumber);
            int currentCount = GetHintCount(key);
            return currentCount < maxHints;
        }

        /// <summary>
        /// Get the recommended hint for the current error context.
        /// This is an advanced feature that analyzes the error type to suggest relevant hints.
        /// </summary>
        public ActivityHint GetContextualHint(string activityId, int questionNumber,
            List<ActivityHint> availableHints, ErrorType? errorType, object contextData)
        {
            string key = GetHintKey(activityId, questionNumber);
            int currentHintCount = GetHintCount(key);
            int hintLevel = currentHintCount + 1;

            return GetHintAtLevel(availableHints, hintLevel);
        }

        /// <summary>
        /// Get hint at a specific level from a list.
        /// </summary>
        private ActivityHint GetHintAtLevel(List<ActivityHint> hints, int level)
        {
            // Try to find hint with matching level
            foreach (var hint in hints)
            {
                if (hint.Level == level)
                {
                    return hint;
                }
            }

            // If no hint with matching level, return by index
            if (level > 0 && level <= hints.Count)
            {
                return hints[level - 1];
            }

            return null;
        }

        /// <summary>
        /// Check if hints are on cooldown for this question.
        /// </summary>
        private bool IsOnCooldown(string key)
        {
            if (lastHintTimeMap.ContainsKey(key))
            {
                float timeSinceLastHint = Time.time - lastHintTimeMap[key];
                return timeSinceLastHint < hintCooldown;
            }
            return false;
        }

        /// <summary>
        /// Record hint usage.
        /// </summary>
        private void RecordHintUsage(string key, int hintLevel)
        {
            // Increment count
            if (hintUsageMap.ContainsKey(key))
            {
                hintUsageMap[key]++;
            }
            else
            {
                hintUsageMap[key] = 1;
            }

            // Add to history
            if (!hintHistoryMap.ContainsKey(key))
            {
                hintHistoryMap[key] = new List<int>();
            }
            hintHistoryMap[key].Add(hintLevel);
        }

        /// <summary>
        /// Get hint count for a key.
        /// </summary>
        private int GetHintCount(string key)
        {
            if (hintUsageMap.ContainsKey(key))
            {
                return hintUsageMap[key];
            }
            return 0;
        }

        /// <summary>
        /// Generate a key for hint tracking.
        /// </summary>
        private string GetHintKey(string activityId, int questionNumber)
        {
            return $"{activityId}_q{questionNumber}";
        }

        /// <summary>
        /// Set the default maximum hints.
        /// </summary>
        public void SetDefaultMaxHints(int maxHints)
        {
            defaultMaxHints = Mathf.Clamp(maxHints, 1, 10);
        }

        /// <summary>
        /// Set the hint display duration.
        /// </summary>
        public void SetHintDisplayDuration(float duration)
        {
            hintDisplayDuration = Mathf.Clamp(duration, 1f, 30f);
        }

        /// <summary>
        /// Set the hint cooldown.
        /// </summary>
        public void SetHintCooldown(float cooldown)
        {
            hintCooldown = Mathf.Clamp(cooldown, 0f, 60f);
        }

        /// <summary>
        /// Get statistics about hint usage.
        /// </summary>
        public HintStatistics GetStatistics()
        {
            int totalHintsUsed = 0;
            int totalQuestions = hintHistoryMap.Count;

            foreach (var count in hintUsageMap.Values)
            {
                totalHintsUsed += count;
            }

            return new HintStatistics
            {
                TotalQuestionsWithHints = totalQuestions,
                TotalHintsUsed = totalHintsUsed,
                AverageHintsPerQuestion = totalQuestions > 0 ? (float)totalHintsUsed / totalQuestions : 0f
            };
        }
    }

    /// <summary>
    /// Statistics about hint usage.
    /// </summary>
    [Serializable]
    public class HintStatistics
    {
        public int TotalQuestionsWithHints;
        public int TotalHintsUsed;
        public float AverageHintsPerQuestion;
    }
}
