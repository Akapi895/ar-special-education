using Core.Learning.Models;
using System;

namespace Features.Activities.NumberBonds
{
    [Serializable]
    public class NumberBondsAnswer : ActivityAnswer
    {
        public NumberBondMode Mode;
        public int WholeTarget;
        public int WholeCount;
        public int PartACount;
        public int PartBCount;
        public int KnownPartA;
        public int KnownPartB;
        public NumberBondValidationResult ValidationResult;
        public string Expression;

        public NumberBondsAnswer()
        {
        }

        public NumberBondsAnswer(
            NumberBondsQuestion question,
            NumberBondRoundState state,
            NumberBondValidationResult validationResult,
            string expression,
            float responseTimeSeconds,
            int attemptNumber,
            bool hintsUsed)
            : base(expression, responseTimeSeconds, attemptNumber, hintsUsed)
        {
            Mode = question != null ? question.Mode : NumberBondMode.FreeSplit;
            WholeTarget = question != null ? question.WholeTarget : 0;
            KnownPartA = question != null ? question.KnownPartA : -1;
            KnownPartB = question != null ? question.KnownPartB : -1;
            WholeCount = state != null ? state.WholeCount : 0;
            PartACount = state != null ? state.PartACount : 0;
            PartBCount = state != null ? state.PartBCount : 0;
            ValidationResult = validationResult;
            Expression = expression;
        }

        public bool IsCorrect()
        {
            return ValidationResult == NumberBondValidationResult.Correct;
        }
    }
}
