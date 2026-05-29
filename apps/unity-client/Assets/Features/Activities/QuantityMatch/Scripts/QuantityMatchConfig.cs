using Core.Learning.ActivityRunner;
using Core.Learning.Models;
using Features.Activities.QuantityMatch;
using System.Collections.Generic;
using UnityEngine;

namespace Features.Activities.QuantityMatch
{
    /// <summary>
    /// Configuration for Quantity Match activity.
    /// Inherits from ActivityConfig and adds Quantity Match specific settings.
    /// </summary>
    [CreateAssetMenu(fileName = "SO_QuantityMatchConfig", menuName = "AR Learning/Quantity Match Config")]
    public class QuantityMatchConfig : ActivityConfig
    {
        [Header("Quantity Match Settings")]
        [Tooltip("List of questions/rounds for this activity")]
        [SerializeField]
        private List<QuantityMatchQuestion> questions;

        [Header("Feedback Strings")]
        [Tooltip("Message shown when answer is correct")]
        [SerializeField]
        private string correctFeedback = "Gi\u1ecfi l\u1eafm! Con \u0111\u00e3 ch\u1ecdn \u0111\u00fang nh\u00f3m!";

        [Tooltip("Message shown when answer is incorrect")]
        [SerializeField]
        private string incorrectFeedback = "Ch\u01b0a \u0111\u00fang r\u1ed3i. Con h\u00e3y th\u1eed \u0111\u1ebfm l\u1ea1i nh\u00e9!";

        [Tooltip("Message shown when max attempts are reached")]
        [SerializeField]
        private string failedFeedback = "Con \u0111\u00e3 c\u1ed1 g\u1eafng r\u1ea5t t\u1ed1t. M\u00ecnh th\u1eed c\u00e2u kh\u00e1c nh\u00e9.";

        [Header("Hints (3-level progression)")]
        [Tooltip("Level 1 hint: General nudge")]
        [SerializeField]
        private ActivityHint hintLevel1;

        [Tooltip("Level 2 hint: Point to the number")]
        [SerializeField]
        private ActivityHint hintLevel2;

        [Tooltip("Level 3 hint: Near-answer")]
        [SerializeField]
        private ActivityHint hintLevel3;

        [Header("Visual Settings")]
        [Tooltip("Default spacing between objects in a group (meters)")]
        [SerializeField]
        private float defaultObjectSpacing = 0.68f;

        [Tooltip("Spacing between groups (meters)")]
        [SerializeField]
        private float defaultGroupSpacing = 1.6f;

        [Tooltip("Arrangement pattern for groups")]
        [SerializeField]
        private GroupArrangementPattern groupArrangement = GroupArrangementPattern.Horizontal;

        [Header("Interaction Settings")]
        [Tooltip("When to switch from tap-to-select to number input mode. Set to -1 to disable number input mode.")]
        [SerializeField]
        private int switchToNumberInputAtRound = 6;

        // Properties
        public List<QuantityMatchQuestion> Questions => questions;
        public string CorrectFeedback => correctFeedback;
        public string IncorrectFeedback => incorrectFeedback;
        public string FailedFeedback => failedFeedback;
        public float DefaultObjectSpacing => defaultObjectSpacing;
        public float DefaultGroupSpacing => defaultGroupSpacing;
        public GroupArrangementPattern GroupArrangement => groupArrangement;
        public int SwitchToNumberInputAtRound => switchToNumberInputAtRound;

        /// <summary>
        /// Get a specific question by index.
        /// </summary>
        public QuantityMatchQuestion GetQuestion(int index)
        {
            if (questions == null || index < 0 || index >= questions.Count)
            {
                Debug.LogError($"[QuantityMatchConfig] Invalid question index: {index}");
                return null;
            }
            return questions[index];
        }

        /// <summary>
        /// Get hints for a specific question.
        /// Returns custom hints if available, otherwise uses default hints.
        /// </summary>
        public override List<ActivityHint> GetHintsForLevel(int levelNumber)
        {
            var question = GetQuestion(levelNumber - 1);
            if (question != null && question.CustomHints != null && question.CustomHints.Count > 0)
            {
                return question.CustomHints;
            }

            // Return default 3-level hint progression
            return new List<ActivityHint>
            {
                hintLevel1 ?? new ActivityHint("hint1", "Con nh\u00ecn k\u1ef9 t\u1eebng nh\u00f3m con v\u1eadt nh\u00e9.", 1),
                hintLevel2 ?? new ActivityHint("hint2", "S\u1ed1 c\u1ea7n t\u00ecm l\u00e0 X. Con h\u00e3y \u0111\u1ebfm t\u1eebng nh\u00f3m.", 2),
                hintLevel3 ?? new ActivityHint("hint3", "C\u00f3 m\u1ed9t nh\u00f3m c\u00f3 \u0111\u00fang X con v\u1eadt.", 3)
            };
        }

        /// <summary>
        /// Get the hint at a specific level (1-based).
        /// </summary>
        public ActivityHint GetHintAtLevel(int questionIndex, int hintLevel)
        {
            var hints = GetHintsForLevel(questionIndex + 1);
            if (hints == null || hintLevel < 1 || hintLevel > hints.Count)
            {
                return null;
            }
            return hints[hintLevel - 1];
        }

        /// <summary>
        /// Validate the configuration.
        /// </summary>
        public override bool IsValid()
        {
            if (!base.IsValid())
            {
                return false;
            }

            if (questions == null || questions.Count == 0)
            {
                Debug.LogError("[QuantityMatchConfig] No questions defined.");
                return false;
            }

            for (int i = 0; i < questions.Count; i++)
            {
                if (!questions[i].IsValid())
                {
                    Debug.LogError($"[QuantityMatchConfig] Question {i} is invalid.");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Get formatted hint text with the target number substituted.
        /// </summary>
        public string GetFormattedHintText(ActivityHint hint, int targetNumber)
        {
            if (hint == null)
            {
                return "Con h\u00e3y \u0111\u1ebfm t\u1eebng nh\u00f3m th\u1eadt ch\u1eadm nh\u00e9.";
            }

            return hint.HintText.Replace("X", targetNumber.ToString());
        }
    }

    /// <summary>
    /// How groups are arranged in the AR space.
    /// </summary>
    public enum GroupArrangementPattern
    {
        /// <summary>
        /// Groups arranged horizontally in a row.
        /// </summary>
        Horizontal,

        /// <summary>
        /// Groups arranged vertically in a column.
        /// </summary>
        Vertical,

        /// <summary>
        /// Groups arranged in a circle.
        /// </summary>
        Circular,

        /// <summary>
        /// Groups placed randomly (within bounds).
        /// </summary>
        Random
    }
}
