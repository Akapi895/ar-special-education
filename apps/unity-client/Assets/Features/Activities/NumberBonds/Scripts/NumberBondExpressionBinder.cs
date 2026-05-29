namespace Features.Activities.NumberBonds
{
    public static class NumberBondExpressionBinder
    {
        public static string Format(NumberBondsQuestion question, NumberBondRoundState state)
        {
            if (question == null || state == null)
            {
                return string.Empty;
            }

            return question.Mode switch
            {
                NumberBondMode.TargetSplit => FormatTargetSplit(question, state),
                NumberBondMode.Compose => $"{state.PartACount} + {state.PartBCount} = {FormatWhole(state.WholeCount)}",
                _ => FormatSplit(question, state)
            };
        }

        private static string FormatSplit(NumberBondsQuestion question, NumberBondRoundState state)
        {
            bool revealZero = state.WholeCount == 0;
            string partA = FormatPart(state.PartACount, revealZero);
            string partB = FormatPart(state.PartBCount, revealZero);
            return $"{question.WholeTarget} = {partA} + {partB}";
        }

        private static string FormatTargetSplit(NumberBondsQuestion question, NumberBondRoundState state)
        {
            string partA = question.KnownPartA >= 0
                ? question.KnownPartA.ToString()
                : FormatPart(state.PartACount, state.WholeCount == 0);
            string partB = question.KnownPartB >= 0
                ? question.KnownPartB.ToString()
                : FormatPart(state.PartBCount, state.WholeCount == 0);
            return $"{question.WholeTarget} = {partA} + {partB}";
        }

        private static string FormatPart(int count, bool revealZero)
        {
            return count > 0 || revealZero ? count.ToString() : "__";
        }

        private static string FormatWhole(int count)
        {
            return count > 0 ? count.ToString() : "__";
        }
    }
}
