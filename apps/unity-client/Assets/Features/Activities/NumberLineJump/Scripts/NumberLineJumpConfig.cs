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
        private string correctFeedback = "Ch\u00ednh x\u00e1c! Con \u0111\u00e3 \u0111\u1ebfn \u0111\u00fang s\u1ed1 r\u1ed3i!";

        [Tooltip("Feedback when correct with perfect jumps (no wasted steps)")]
        [SerializeField]
        private string perfectFeedback = "Tuy\u1ec7t v\u1eddi! Con nh\u1ea3y \u0111\u00fang s\u1ed1 b\u01b0\u1edbc c\u1ea7n thi\u1ebft!";

        [Header("Feedback Strings - Failure")]
        [Tooltip("Feedback when answer is incorrect (wrong final position)")]
        [SerializeField]
        private string incorrectFeedback = "Ch\u01b0a \u0111\u00fang r\u1ed3i. Con h\u00e3y \u0111\u1ebfm l\u1ea1i s\u1ed1 b\u01b0\u1edbc nh\u1ea3y nh\u00e9!";

        [Tooltip("Feedback when player overshot the target")]
        [SerializeField]
        private string overshootFeedback = "Con \u0111i qu\u00e1 xa r\u1ed3i. M\u00ecnh b\u1eaft \u0111\u1ea7u l\u1ea1i nh\u00e9!";

        [Tooltip("Feedback when player hit the boundary")]
        [SerializeField]
        private string boundaryFeedback = "Con \u0111\u00e3 t\u1edbi m\u00e9p r\u1ed3i, kh\u00f4ng \u0111i xa h\u01a1n \u0111\u01b0\u1ee3c n\u1eefa.";

        [Tooltip("Feedback when max jumps exceeded")]
        [SerializeField]
        private string maxJumpsFeedback = "Con nh\u1ea3y h\u01a1i nhi\u1ec1u r\u1ed3i. M\u00ecnh th\u1eed l\u1ea1i ch\u1eadm h\u01a1n nh\u00e9!";

        [Tooltip("Feedback when max attempts are reached")]
        [SerializeField]
        private string failedFeedback = "Con \u0111\u00e3 c\u1ed1 g\u1eafng r\u1ea5t t\u1ed1t. M\u00ecnh th\u1eed c\u00e2u kh\u00e1c nh\u00e9.";

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
        private string maxJumpsWarning = "Con s\u1eafp h\u1ebft l\u01b0\u1ee3t nh\u1ea3y r\u1ed3i. H\u00e3y ngh\u0129 th\u1eadt k\u1ef9 nh\u00e9!";

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
                hintLevel1 ?? new ActivityHint("hint1", "Con h\u00e3y di chuy\u1ec3n nh\u00e2n v\u1eadt v\u00e0 \u0111\u1ebfm t\u1eebng b\u01b0\u1edbc nh\u1ea3y.", 1),
                hintLevel2 ?? new ActivityHint("hint2", "Con b\u1eaft \u0111\u1ea7u \u1edf X v\u00e0 c\u1ea7n \u0111\u1ebfn Y. V\u1eady c\u1ea7n nh\u1ea3y m\u1ea5y b\u01b0\u1edbc?", 2),
                hintLevel3 ?? new ActivityHint("hint3", "H\u00e3y nh\u1ea3y [direction] [N] b\u01b0\u1edbc t\u1eeb v\u1ecb tr\u00ed hi\u1ec7n t\u1ea1i.", 3)
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
                return "Con h\u00e3y di chuy\u1ec3n nh\u00e2n v\u1eadt v\u00e0 \u0111\u1ebfm t\u1eebng b\u01b0\u1edbc nh\u1ea3y.";
            }

            string text = hint.HintText;
            text = text.Replace("X", startNumber.ToString());
            text = text.Replace("Y", targetNumber.ToString());

            // Replace [direction] and [N] for level 3 hint
            string directionHint = targetNumber >= startNumber ? "sang ph\u1ea3i" : "sang tr\u00e1i";
            text = text.Replace("[direction]", directionHint);
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
