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
        private string moreButtonLabel = "More";

        [Tooltip("Text for the 'Fewer' button")]
        [SerializeField]
        private string fewerButtonLabel = "Fewer";

        [Tooltip("Text for the 'Equal' button")]
        [SerializeField]
        private string equalButtonLabel = "Equal";

        [Header("Feedback Strings - Outcome Specific")]
        [Tooltip("Feedback when user answers 'More' correctly")]
        [SerializeField]
        private string correctMoreFeedback = "Yes! The left group has MORE!";

        [Tooltip("Feedback when user answers 'Fewer' correctly")]
        [SerializeField]
        private string correctFewerFeedback = "Yes! The left group has FEWER!";

        [Tooltip("Feedback when user answers 'Equal' correctly")]
        [SerializeField]
        private string correctEqualFeedback = "Yes! Both groups are EQUAL!";

        [Tooltip("Feedback when user answers 'More' incorrectly")]
        [SerializeField]
        private string incorrectMoreFeedback = "Not more. Look again!";

        [Tooltip("Feedback when user answers 'Fewer' incorrectly")]
        [SerializeField]
        private string incorrectFewerFeedback = "Not fewer. Try counting again!";

        [Tooltip("Feedback when user answers 'Equal' incorrectly")]
        [SerializeField]
        private string incorrectEqualFeedback = "Not equal. Count each group carefully!";

        [Tooltip("Generic incorrect feedback when specific outcome feedback isn't available")]
        [SerializeField]
        private string genericIncorrectFeedback = "Not quite. Let's try again!";

        [Tooltip("Feedback when max attempts are reached")]
        [SerializeField]
        private string failedFeedback = "Good effort! Let's try another one.";

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
        private float groupSpacing = 2.3f;

        // Properties
        public List<CompareQuantityQuestion> Questions => questions;
        public string MoreButtonLabel => moreButtonLabel;
        public string FewerButtonLabel => fewerButtonLabel;
        public string EqualButtonLabel => equalButtonLabel;
        public string FailedFeedback => failedFeedback;
        public float GroupSpacing => groupSpacing;

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
                    equalityHintLevel1 ?? new ActivityHint("eq_hint1", "Count both groups — are they the same?", 1),
                    equalityHintLevel2 ?? new ActivityHint("eq_hint2", "The left group has X. The right group also has X.", 2),
                    equalityHintLevel3 ?? new ActivityHint("eq_hint3", "X and X are the same number — they're EQUAL!", 3)
                };
            }
            else
            {
                return new List<ActivityHint>
                {
                    hintLevel1 ?? new ActivityHint("hint1", "Count each group carefully.", 1),
                    hintLevel2 ?? new ActivityHint("hint2", "The left group has X objects. Now count the right group.", 2),
                    hintLevel3 ?? new ActivityHint("hint3", "Compare X and Y — which is bigger?", 3)
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
                    _ => "Great job!"
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
                return "Try counting each group carefully.";
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
                return "Count both groups — are they the same?";
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
