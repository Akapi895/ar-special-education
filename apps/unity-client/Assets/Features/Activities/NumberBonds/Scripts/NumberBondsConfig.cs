using Core.Learning.ActivityRunner;
using Core.Learning.Models;
using System.Collections.Generic;
using UnityEngine;

namespace Features.Activities.NumberBonds
{
    [CreateAssetMenu(fileName = "SO_NumberBondsConfig", menuName = "AR Learning/Number Bonds Config")]
    public class NumberBondsConfig : ActivityConfig
    {
        [Header("Number Bonds Settings")]
        [SerializeField]
        private List<NumberBondsQuestion> questions = new List<NumberBondsQuestion>();

        [SerializeField]
        private int maxObjectsPerRound = 8;

        [SerializeField]
        private float zoneRadiusMeters = 0.52f;

        [SerializeField]
        private float zoneHitRadiusMeters = 0.66f;

        [SerializeField]
        private float objectHeightMeters = 0.22f;

        [Header("Feedback")]
        [SerializeField]
        private string correctFeedback = "Tuy\u1ec7t v\u1eddi! Con \u0111\u00e3 t\u00e1ch s\u1ed1 r\u1ea5t \u0111\u00fang.";

        [SerializeField]
        private string notAllMovedFeedback = "Con h\u00e3y chuy\u1ec3n h\u1ebft con v\u1eadt xu\u1ed1ng hai nh\u00f3m nh\u00e9.";

        [SerializeField]
        private string wrongPartFeedback = "G\u1ea7n \u0111\u00fang r\u1ed3i. Con th\u1eed \u0111\u1ebfm l\u1ea1i t\u1eebng ph\u1ea7n nh\u00e9.";

        [SerializeField]
        private string lockedZoneFeedback = "Ph\u1ea7n n\u00e0y \u0111\u00e3 cho s\u1eb5n r\u1ed3i, m\u00ecnh gi\u1eef nguy\u00ean nh\u00e9.";

        [SerializeField]
        private string failedFeedback = "Con \u0111\u00e3 c\u1ed1 g\u1eafng r\u1ea5t t\u1ed1t. M\u00ecnh th\u1eed c\u00e2u kh\u00e1c nh\u00e9.";

        [Header("Hints")]
        [SerializeField]
        private ActivityHint hintLevel1 = new ActivityHint("nb_hint1", "Con h\u00e3y k\u00e9o t\u1eebng con v\u1eadt t\u1eeb T\u1ed5ng xu\u1ed1ng hai Ph\u1ea7n.", 1);

        [SerializeField]
        private ActivityHint hintLevel2 = new ActivityHint("nb_hint2", "Bi\u1ec3u th\u1ee9c \u0111ang l\u00e0 X = A + B. Con c\u00f2n C con trong T\u1ed5ng.", 2);

        [SerializeField]
        private ActivityHint hintLevel3 = new ActivityHint("nb_hint3", "Khi T\u1ed5ng c\u00f2n 0, hai ph\u1ea7n c\u1ed9ng l\u1ea1i ph\u1ea3i b\u1eb1ng X.", 3);

        public List<NumberBondsQuestion> Questions => questions;
        public int MaxObjectsPerRound => maxObjectsPerRound;
        public float ZoneRadiusMeters => zoneRadiusMeters;
        public float ZoneHitRadiusMeters => zoneHitRadiusMeters;
        public float ObjectHeightMeters => objectHeightMeters;
        public string FailedFeedback => failedFeedback;

        public void ConfigureRuntime(
            string activityId,
            string displayName,
            string description,
            List<NumberBondsQuestion> questions,
            ActivityHint hintLevel1,
            ActivityHint hintLevel2,
            ActivityHint hintLevel3,
            int maxObjectsPerRound = 8,
            float zoneRadiusMeters = 0.52f,
            float zoneHitRadiusMeters = 0.66f,
            float objectHeightMeters = 0.22f,
            int maxAttemptsPerQuestion = 3,
            int maxHintsPerQuestion = 3)
        {
            this.questions = questions ?? new List<NumberBondsQuestion>();
            this.hintLevel1 = hintLevel1;
            this.hintLevel2 = hintLevel2;
            this.hintLevel3 = hintLevel3;
            this.maxObjectsPerRound = maxObjectsPerRound;
            this.zoneRadiusMeters = zoneRadiusMeters;
            this.zoneHitRadiusMeters = zoneHitRadiusMeters;
            this.objectHeightMeters = objectHeightMeters;

            ConfigureBase(
                activityId,
                displayName,
                description,
                this.questions.Count,
                maxAttemptsPerQuestion,
                maxHintsPerQuestion);
        }

        public NumberBondsQuestion GetQuestion(int index)
        {
            if (questions == null || index < 0 || index >= questions.Count)
            {
                Debug.LogError($"[NumberBondsConfig] Invalid question index: {index}");
                return null;
            }

            return questions[index];
        }

        public override List<ActivityHint> GetHintsForLevel(int levelNumber)
        {
            NumberBondsQuestion question = GetQuestion(levelNumber - 1);
            if (question != null && question.CustomHints != null && question.CustomHints.Count > 0)
            {
                return question.CustomHints;
            }

            return new List<ActivityHint>
            {
                hintLevel1 ?? new ActivityHint("nb_hint1", "Con h\u00e3y k\u00e9o t\u1eebng con v\u1eadt xu\u1ed1ng hai ph\u1ea7n.", 1),
                hintLevel2 ?? new ActivityHint("nb_hint2", "Con h\u00e3y \u0111\u1ebfm l\u1ea1i s\u1ed1 con trong m\u1ed7i v\u00f2ng.", 2),
                hintLevel3 ?? new ActivityHint("nb_hint3", "Hai ph\u1ea7n c\u1ed9ng l\u1ea1i ph\u1ea3i b\u1eb1ng t\u1ed5ng.", 3)
            };
        }

        public string GetFormattedHintText(ActivityHint hint, NumberBondsQuestion question, NumberBondRoundState state)
        {
            if (hint == null || question == null || state == null)
            {
                return "Con h\u00e3y k\u00e9o t\u1eebng con v\u1eadt xu\u1ed1ng hai ph\u1ea7n nh\u00e9.";
            }

            return hint.HintText
                .Replace("X", question.WholeTarget.ToString())
                .Replace("A", state.PartACount.ToString())
                .Replace("B", state.PartBCount.ToString())
                .Replace("C", state.WholeCount.ToString());
        }

        public string GetFeedback(NumberBondValidationResult result, string expression)
        {
            string baseText = result switch
            {
                NumberBondValidationResult.Correct => correctFeedback,
                NumberBondValidationResult.NotAllObjectsMoved => notAllMovedFeedback,
                NumberBondValidationResult.WrongPartCount => wrongPartFeedback,
                NumberBondValidationResult.LockedZoneModified => lockedZoneFeedback,
                _ => wrongPartFeedback
            };

            return string.IsNullOrWhiteSpace(expression) ? baseText : $"{baseText}\n{expression}";
        }

        public override bool IsValid()
        {
            if (!base.IsValid())
            {
                return false;
            }

            if (questions == null || questions.Count == 0)
            {
                Debug.LogError("[NumberBondsConfig] No questions defined.");
                return false;
            }

            if (maxObjectsPerRound <= 0)
            {
                Debug.LogError("[NumberBondsConfig] Max objects per round must be positive.");
                return false;
            }

            for (int i = 0; i < questions.Count; i++)
            {
                if (questions[i] == null || !questions[i].IsValid())
                {
                    Debug.LogError($"[NumberBondsConfig] Question {i} is invalid.");
                    return false;
                }
            }

            return true;
        }
    }
}
