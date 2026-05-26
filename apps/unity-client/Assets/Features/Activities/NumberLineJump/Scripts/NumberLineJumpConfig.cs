using Core.Learning.ActivityRunner;
using Core.Learning.Models;
using Features.Activities.NumberLineJump;
using System.Collections.Generic;
using UnityEngine;

namespace Features.Activities.NumberLineJump
{
    /// <summary>
    /// Configuration for Number Line Jump activity.
    /// Inherits from ActivityConfig and adds Number Line Jump specific settings.
    /// </summary>
    [CreateAssetMenu(fileName = "SO_NumberLineJumpConfig", menuName = "AR Learning/Number Line Jump Config")]
    public class NumberLineJumpConfig : ActivityConfig
    {
        [Header("Number Line Jump Settings")]
        [Tooltip("List of questions/rounds for this activity")]
        [SerializeField]
        private List<NumberLineJumpQuestion> questions;

        [Header("Feedback Strings - Success")]
        [Tooltip("Feedback when answer is correct (reached target)")]
        [SerializeField]
        private string correctFeedback = "Excellent! You reached the target!";

        [Tooltip("Feedback when correct with perfect jumps (no wasted steps)")]
        [SerializeField]
        private string perfectFeedback = "Perfect! You took exactly the right number of jumps!";

        [Header("Feedback Strings - Failure")]
        [Tooltip("Feedback when answer is incorrect (wrong final position)")]
        [SerializeField]
        private string incorrectFeedback = "Not quite. Let's try counting the jumps again!";

        [Tooltip("Feedback when player overshot the target")]
        [SerializeField]
        private string overshootFeedback = "You went too far! Try starting over.";

        [Tooltip("Feedback when player hit the boundary")]
        [SerializeField]
        private string boundaryFeedback = "You reached the edge! Can't go further.";

        [Tooltip("Feedback when max jumps exceeded")]
        [SerializeField]
        private string maxJumpsFeedback = "That's a lot of jumps! Let's try again more carefully.";

        [Tooltip("Feedback when max attempts are reached")]
        [SerializeField]
        private string failedFeedback = "Good effort! Let's try another one.";

        [Header("Hints (3-level progression)")]
        [Tooltip("Level 1 hint: Move and count")]
        [SerializeField]
        private ActivityHint hintLevel1;

        [Tooltip("Level 2 hint: Show start and target")]
        [SerializeField]
        private ActivityHint hintLevel2;

        [Tooltip("Level 3 hint: Specific direction and count")]
        [SerializeField]
        private ActivityHint hintLevel3;

        [Header("Warnings")]
        [Tooltip("Warning shown when approaching max jumps")]
        [SerializeField]
        private string maxJumpsWarning = "You've used most of your jumps. Think carefully!";

        [Tooltip("Show warning when remaining jumps <= this value")]
        [SerializeField]
        private int maxJumpsWarningThreshold = 3;

        [Header("Visual Settings")]
        [Tooltip("Spacing between number tiles on the line (meters)")]
        [SerializeField]
        private float tileSpacing = 0.5f;

        [Tooltip("Height of the number line from the ground (meters)")]
        [SerializeField]
        private float numberLineHeight = 0.1f;

        [Tooltip("Duration of character jump animation (seconds)")]
        [SerializeField]
        private float jumpAnimationDuration = 0.5f;

        // Properties
        public List<NumberLineJumpQuestion> Questions => questions;
        public string CorrectFeedback => correctFeedback;
        public string PerfectFeedback => perfectFeedback;
        public string IncorrectFeedback => incorrectFeedback;
        public string OvershootFeedback => overshootFeedback;
        public string BoundaryFeedback => boundaryFeedback;
        public string MaxJumpsFeedback => maxJumpsFeedback;
        public string FailedFeedback => failedFeedback;
        public string MaxJumpsWarning => maxJumpsWarning;
        public int MaxJumpsWarningThreshold => maxJumpsWarningThreshold;
        public float TileSpacing => tileSpacing;
        public float NumberLineHeight => numberLineHeight;
        public float JumpAnimationDuration => jumpAnimationDuration;

        public void ConfigureRuntime(
            string activityId,
            string displayName,
            string description,
            List<NumberLineJumpQuestion> questions,
            ActivityHint hintLevel1,
            ActivityHint hintLevel2,
            ActivityHint hintLevel3,
            float tileSpacing,
            float numberLineHeight,
            float jumpAnimationDuration,
            int maxAttemptsPerQuestion = 3,
            int maxHintsPerQuestion = 3)
        {
            this.questions = questions ?? new List<NumberLineJumpQuestion>();
            this.hintLevel1 = hintLevel1;
            this.hintLevel2 = hintLevel2;
            this.hintLevel3 = hintLevel3;
            this.tileSpacing = tileSpacing;
            this.numberLineHeight = numberLineHeight;
            this.jumpAnimationDuration = jumpAnimationDuration;

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
        public NumberLineJumpQuestion GetQuestion(int index)
        {
            if (questions == null || index < 0 || index >= questions.Count)
            {
                Debug.LogError($"[NumberLineJumpConfig] Invalid question index: {index}");
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
                hintLevel1 ?? new ActivityHint("hint1", "Move the character and count your steps.", 1),
                hintLevel2 ?? new ActivityHint("hint2", "You started at X. You need to reach Y. How many steps is that?", 2),
                hintLevel3 ?? new ActivityHint("hint3", "Try jumping [direction] [N] times from where you are.", 3)
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
        /// Get formatted hint text with start and target numbers substituted.
        /// </summary>
        public string GetFormattedHintText(ActivityHint hint, int startNumber, int targetNumber)
        {
            if (hint == null)
            {
                return "Move the character and count your steps.";
            }

            string text = hint.HintText;
            text = text.Replace("X", startNumber.ToString());
            text = text.Replace("Y", targetNumber.ToString());

            // Replace [direction] and [N] for level 3 hint
            string directionHint = NumberLineJumpAnswer.GetDirectionHint(startNumber, targetNumber);
            text = text.Replace("[direction]", directionHint.Contains("right") ? "right" : "left");
            text = text.Replace("[N]", Mathf.Abs(targetNumber - startNumber).ToString());

            return text;
        }

        /// <summary>
        /// Get the appropriate feedback based on the answer state.
        /// </summary>
        public string GetFeedbackString(NumberLineJumpAnswer answer)
        {
            if (answer.IsCorrect())
            {
                // Check if it was a perfect run (exactly the right number of jumps)
                int expectedJumps = Mathf.Abs(answer.TargetNumber - answer.StartNumber);
                if (answer.GetTotalJumps() == expectedJumps)
                {
                    return PerfectFeedback;
                }
                return CorrectFeedback;
            }

            if (answer.HasOvershot)
            {
                return OvershootFeedback;
            }

            if (answer.HitBoundary)
            {
                return BoundaryFeedback;
            }

            if (answer.ExceededMaxJumps)
            {
                return MaxJumpsFeedback;
            }

            return IncorrectFeedback;
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
                Debug.LogError("[NumberLineJumpConfig] No questions defined.");
                return false;
            }

            for (int i = 0; i < questions.Count; i++)
            {
                if (!questions[i].IsValid())
                {
                    Debug.LogError($"[NumberLineJumpConfig] Question {i} is invalid.");
                    return false;
                }
            }

            return true;
        }
    }
}
