using Core.Learning.ActivityRunner;
using Core.Learning.Models;
using Features.Activities.CompareQuantity;
using System.Collections.Generic;
using UnityEngine;

namespace Features.Activities.CompareQuantity
{
    /// <summary>
    /// Configuration for Compare Quantity activity.
    /// Inherits from ActivityConfig and adds Compare Quantity specific settings.
    /// </summary>
    [CreateAssetMenu(fileName = "SO_CompareQuantityConfig", menuName = "AR Learning/Compare Quantity Config")]
    public class CompareQuantityConfig : ActivityConfig
    {
        [Header("Compare Quantity Settings")]
        [Tooltip("List of questions/rounds for this activity")]
        [SerializeField]
        private List<CompareQuantityQuestion> questions;

        [Header("Button Labels")]
        [Tooltip("Text for the 'More' button")]
        [SerializeField]
        private string moreButtonLabel = "B\u00ean tr\u00e1i nhi\u1ec1u h\u01a1n";

        [Tooltip("Text for the 'Fewer' button")]
        [SerializeField]
        private string fewerButtonLabel = "B\u00ean tr\u00e1i \u00edt h\u01a1n";

        [Tooltip("Text for the 'Equal' button")]
        [SerializeField]
        private string equalButtonLabel = "B\u1eb1ng nhau";

        [Header("Feedback Strings - Outcome Specific")]
        [Tooltip("Feedback when user answers 'More' correctly")]
        [SerializeField]
        private string correctMoreFeedback = "\u0110\u00fang r\u1ed3i! Nh\u00f3m b\u00ean tr\u00e1i nhi\u1ec1u h\u01a1n!";

        [Tooltip("Feedback when user answers 'Fewer' correctly")]
        [SerializeField]
        private string correctFewerFeedback = "\u0110\u00fang r\u1ed3i! Nh\u00f3m b\u00ean tr\u00e1i \u00edt h\u01a1n!";

        [Tooltip("Feedback when user answers 'Equal' correctly")]
        [SerializeField]
        private string correctEqualFeedback = "\u0110\u00fang r\u1ed3i! Hai nh\u00f3m b\u1eb1ng nhau!";

        [Tooltip("Feedback when user answers 'More' incorrectly")]
        [SerializeField]
        private string incorrectMoreFeedback = "Ch\u01b0a ph\u1ea3i nhi\u1ec1u h\u01a1n. Con h\u00e3y \u0111\u1ebfm l\u1ea1i nh\u00e9!";

        [Tooltip("Feedback when user answers 'Fewer' incorrectly")]
        [SerializeField]
        private string incorrectFewerFeedback = "Ch\u01b0a ph\u1ea3i \u00edt h\u01a1n. Con h\u00e3y \u0111\u1ebfm l\u1ea1i nh\u00e9!";

        [Tooltip("Feedback when user answers 'Equal' incorrectly")]
        [SerializeField]
        private string incorrectEqualFeedback = "Ch\u01b0a b\u1eb1ng nhau. Con h\u00e3y \u0111\u1ebfm t\u1eebng nh\u00f3m nh\u00e9!";

        [Tooltip("Generic incorrect feedback when specific outcome feedback isn't available")]
        [SerializeField]
        private string genericIncorrectFeedback = "Ch\u01b0a \u0111\u00fang r\u1ed3i. Con th\u1eed l\u1ea1i nh\u00e9!";

        [Tooltip("Feedback when max attempts are reached")]
        [SerializeField]
        private string failedFeedback = "Con \u0111\u00e3 c\u1ed1 g\u1eafng r\u1ea5t t\u1ed1t. M\u00ecnh th\u1eed c\u00e2u kh\u00e1c nh\u00e9.";

        [Header("Hints - Standard (3-level progression)")]
        [Tooltip("Level 1 hint: Count each group")]
        [SerializeField]
        private ActivityHint hintLevel1;

        [Tooltip("Level 2 hint: Show left group count")]
        [SerializeField]
        private ActivityHint hintLevel2;

        [Tooltip("Level 3 hint: Compare specific numbers")]
        [SerializeField]
        private ActivityHint hintLevel3;

        [Header("Hints - Equality Specific")]
        [Tooltip("Level 1 hint when both groups are equal")]
        [SerializeField]
        private ActivityHint equalityHintLevel1;

        [Tooltip("Level 2 hint when both groups are equal")]
        [SerializeField]
        private ActivityHint equalityHintLevel2;

        [Tooltip("Level 3 hint when both groups are equal")]
        [SerializeField]
        private ActivityHint equalityHintLevel3;

        [Header("Visual Settings")]
        [Tooltip("Spacing between the two groups (meters)")]
        [SerializeField]
        private float groupSpacing = 3.0f;

        // Properties
        public List<CompareQuantityQuestion> Questions => questions;
        public string MoreButtonLabel => moreButtonLabel;
        public string FewerButtonLabel => fewerButtonLabel;
        public string EqualButtonLabel => equalButtonLabel;
        public string FailedFeedback => failedFeedback;
        public float GroupSpacing => groupSpacing;

        public void ConfigureRuntime(
            string activityId,
            string displayName,
            string description,
            List<CompareQuantityQuestion> questions,
            string moreButtonLabel,
            string fewerButtonLabel,
            string equalButtonLabel,
            ActivityHint hintLevel1,
            ActivityHint hintLevel2,
            ActivityHint hintLevel3,
            ActivityHint equalityHintLevel1,
            ActivityHint equalityHintLevel2,
            ActivityHint equalityHintLevel3,
            float groupSpacing,
            int maxAttemptsPerQuestion = 3,
            int maxHintsPerQuestion = 3)
        {
            this.questions = questions ?? new List<CompareQuantityQuestion>();
            this.moreButtonLabel = moreButtonLabel;
            this.fewerButtonLabel = fewerButtonLabel;
            this.equalButtonLabel = equalButtonLabel;
            this.hintLevel1 = hintLevel1;
            this.hintLevel2 = hintLevel2;
            this.hintLevel3 = hintLevel3;
            this.equalityHintLevel1 = equalityHintLevel1;
            this.equalityHintLevel2 = equalityHintLevel2;
            this.equalityHintLevel3 = equalityHintLevel3;
            this.groupSpacing = groupSpacing;

            ConfigureBase(
                activityId,
                displayName,
                description,
                this.questions.Count,
                maxAttemptsPerQuestion,
                maxHintsPerQuestion);
        }

        /// <summary>
        /// Get a specific question by index.
        /// </summary>
        public CompareQuantityQuestion GetQuestion(int index)
        {
            if (questions == null || index < 0 || index >= questions.Count)
            {
                Debug.LogError($"[CompareQuantityConfig] Invalid question index: {index}");
                return null;
            }
            return questions[index];
        }

        /// <summary>
        /// Get hints for a specific question.
        /// Returns equality-specific hints if the question has equal groups.
        /// </summary>
        public override List<ActivityHint> GetHintsForLevel(int levelNumber)
        {
            var question = GetQuestion(levelNumber - 1);
            if (question == null)
            {
                return GetDefaultHints(false);
            }

            // Use custom hints if provided
            if (question.CustomHints != null && question.CustomHints.Count > 0)
            {
                return question.CustomHints;
            }

            // Use equality-specific hints if this is an equality question
            bool isEquality = question.IsEqualityQuestion();
            return GetDefaultHints(isEquality);
        }

        /// <summary>
        /// Get default hints based on whether it's an equality question.
        /// </summary>
        private List<ActivityHint> GetDefaultHints(bool isEquality)
        {
            if (isEquality)
            {
                return new List<ActivityHint>
                {
                    equalityHintLevel1 ?? new ActivityHint("eq_hint1", "Con \u0111\u1ebfm c\u1ea3 hai nh\u00f3m: hai nh\u00f3m c\u00f3 b\u1eb1ng nhau kh\u00f4ng?", 1),
                    equalityHintLevel2 ?? new ActivityHint("eq_hint2", "Nh\u00f3m b\u00ean tr\u00e1i c\u00f3 X con. Nh\u00f3m b\u00ean ph\u1ea3i c\u0169ng c\u00f3 X con.", 2),
                    equalityHintLevel3 ?? new ActivityHint("eq_hint3", "X v\u00e0 X l\u00e0 c\u00f9ng m\u1ed9t s\u1ed1, n\u00ean hai nh\u00f3m b\u1eb1ng nhau.", 3)
                };
            }
            else
            {
                return new List<ActivityHint>
                {
                    hintLevel1 ?? new ActivityHint("hint1", "Con h\u00e3y \u0111\u1ebfm t\u1eebng nh\u00f3m th\u1eadt ch\u1eadm nh\u00e9.", 1),
                    hintLevel2 ?? new ActivityHint("hint2", "Nh\u00f3m b\u00ean tr\u00e1i c\u00f3 X con. B\u00e2y gi\u1edd con \u0111\u1ebfm nh\u00f3m b\u00ean ph\u1ea3i.", 2),
                    hintLevel3 ?? new ActivityHint("hint3", "So s\u00e1nh X v\u00e0 Y: s\u1ed1 n\u00e0o l\u1edbn h\u01a1n?", 3)
                };
            }
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
        /// Get feedback string based on the answer and whether it was correct.
        /// </summary>
        public string GetFeedbackString(ComparisonAnswer answer, bool isCorrect)
        {
            if (isCorrect)
            {
                return answer switch
                {
                    ComparisonAnswer.More => correctMoreFeedback,
                    ComparisonAnswer.Fewer => correctFewerFeedback,
                    ComparisonAnswer.Equal => correctEqualFeedback,
                    _ => "\u0110\u00fang r\u1ed3i! Con l\u00e0m t\u1ed1t l\u1eafm!"
                };
            }
            else
            {
                return answer switch
                {
                    ComparisonAnswer.More => incorrectMoreFeedback,
                    ComparisonAnswer.Fewer => incorrectFewerFeedback,
                    ComparisonAnswer.Equal => incorrectEqualFeedback,
                    _ => genericIncorrectFeedback
                };
            }
        }

        /// <summary>
        /// Get formatted hint text with numbers substituted.
        /// </summary>
        public string GetFormattedHintText(ActivityHint hint, int leftCount, int rightCount)
        {
            if (hint == null)
            {
                return "Con h\u00e3y \u0111\u1ebfm t\u1eebng nh\u00f3m th\u1eadt ch\u1eadm nh\u00e9.";
            }

            string text = hint.HintText;
            text = text.Replace("X", leftCount.ToString());
            text = text.Replace("Y", rightCount.ToString());
            return text;
        }

        /// <summary>
        /// Get formatted hint text specifically for equality questions.
        /// </summary>
        public string GetFormattedEqualityHintText(ActivityHint hint, int count)
        {
            if (hint == null)
            {
                return "Con \u0111\u1ebfm c\u1ea3 hai nh\u00f3m: hai nh\u00f3m c\u00f3 b\u1eb1ng nhau kh\u00f4ng?";
            }

            return hint.HintText.Replace("X", count.ToString());
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
                Debug.LogError("[CompareQuantityConfig] No questions defined.");
                return false;
            }

            for (int i = 0; i < questions.Count; i++)
            {
                if (!questions[i].IsValid())
                {
                    Debug.LogError($"[CompareQuantityConfig] Question {i} is invalid.");
                    return false;
                }
            }

            return true;
        }
    }
}
