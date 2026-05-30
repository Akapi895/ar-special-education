using System;
using UnityEngine;

namespace Features.Activities.NumberBonds
{
    [Serializable]
    public class NumberBondRoundState
    {
        public int WholeCount { get; private set; }
        public int PartACount { get; private set; }
        public int PartBCount { get; private set; }
        public bool WholeLocked { get; private set; }
        public bool PartALocked { get; private set; }
        public bool PartBLocked { get; private set; }

        public int GetCount(BondZone zone)
        {
            return zone switch
            {
                BondZone.Whole => WholeCount,
                BondZone.PartA => PartACount,
                BondZone.PartB => PartBCount,
                _ => 0
            };
        }

        public bool IsZoneLocked(BondZone zone)
        {
            return zone switch
            {
                BondZone.Whole => WholeLocked,
                BondZone.PartA => PartALocked,
                BondZone.PartB => PartBLocked,
                _ => true
            };
        }

        public bool TryMove(BondZone from, BondZone to)
        {
            if (from == to || IsZoneLocked(from) || IsZoneLocked(to) || GetCount(from) <= 0)
            {
                return false;
            }

            SetCount(from, GetCount(from) - 1);
            SetCount(to, GetCount(to) + 1);
            return true;
        }

        public static NumberBondRoundState FromQuestion(NumberBondsQuestion question)
        {
            if (question == null)
            {
                return new NumberBondRoundState();
            }

            var state = new NumberBondRoundState();
            switch (question.Mode)
            {
                case NumberBondMode.TargetSplit:
                    state.ConfigureTargetSplit(question);
                    break;

                case NumberBondMode.Compose:
                    state.PartACount = Mathf.Max(0, question.KnownPartA >= 0 ? question.KnownPartA : question.WholeTarget / 2);
                    state.PartBCount = Mathf.Max(0, question.KnownPartB >= 0 ? question.KnownPartB : question.WholeTarget - state.PartACount);
                    state.WholeCount = 0;
                    break;

                case NumberBondMode.MissingPart:
                    if (question.KnownPartA >= 0)
                    {
                        state.PartACount = question.KnownPartA;
                        state.PartALocked = true;
                        state.WholeCount = Mathf.Max(0, question.WholeTarget - question.KnownPartA);
                    }
                    else if (question.KnownPartB >= 0)
                    {
                        state.PartBCount = question.KnownPartB;
                        state.PartBLocked = true;
                        state.WholeCount = Mathf.Max(0, question.WholeTarget - question.KnownPartB);
                    }
                    else
                    {
                        state.WholeCount = question.WholeTarget;
                    }
                    break;

                default:
                    state.WholeCount = question.WholeTarget;
                    break;
            }

            return state;
        }

        private void ConfigureTargetSplit(NumberBondsQuestion question)
        {
            if (question.KnownPartA >= 0)
            {
                PartACount = question.KnownPartA;
                PartALocked = true;
                WholeCount = Mathf.Max(0, question.WholeTarget - PartACount);
                return;
            }

            PartBCount = Mathf.Max(0, question.KnownPartB);
            PartBLocked = true;
            WholeCount = Mathf.Max(0, question.WholeTarget - PartBCount);
        }

        /// <summary>
        /// Validate the current state against the question.
        /// </summary>
        public NumberBondValidationResult ValidateCurrentState(NumberBondsQuestion question)
        {
            if (question == null)
                return NumberBondValidationResult.TechnicalIssue;

            switch (question.Mode)
            {
                case NumberBondMode.FreeSplit:
                    if (WholeCount > 0)
                        return NumberBondValidationResult.NotAllObjectsMoved;
                    if (PartACount + PartBCount != question.WholeTarget)
                        return NumberBondValidationResult.WrongTotal;
                    return NumberBondValidationResult.Correct;

                case NumberBondMode.TargetSplit:
                    if (WholeCount > 0)
                        return NumberBondValidationResult.NotAllObjectsMoved;
                    if (PartALocked && PartACount != question.KnownPartA)
                        return NumberBondValidationResult.LockedZoneModified;
                    if (PartBLocked && PartBCount != question.KnownPartB)
                        return NumberBondValidationResult.LockedZoneModified;
                    if (PartACount + PartBCount != question.WholeTarget)
                        return NumberBondValidationResult.WrongPartCount;
                    return NumberBondValidationResult.Correct;

                case NumberBondMode.Compose:
                    if (PartACount > 0 || PartBCount > 0)
                        return NumberBondValidationResult.NotAllObjectsMoved;
                    if (WholeCount != question.WholeTarget)
                        return NumberBondValidationResult.WrongTotal;
                    return NumberBondValidationResult.Correct;

                case NumberBondMode.MissingPart:
                    if (WholeCount > 0)
                        return NumberBondValidationResult.NotAllObjectsMoved;
                    if (PartALocked && PartACount != question.KnownPartA)
                        return NumberBondValidationResult.LockedZoneModified;
                    if (PartBLocked && PartBCount != question.KnownPartB)
                        return NumberBondValidationResult.LockedZoneModified;
                    if (PartACount + PartBCount != question.WholeTarget)
                        return NumberBondValidationResult.WrongPartCount;
                    return NumberBondValidationResult.Correct;

                default:
                    return NumberBondValidationResult.TechnicalIssue;
            }
        }

        private void SetCount(BondZone zone, int value)
        {
            value = Mathf.Max(0, value);
            switch (zone)
            {
                case BondZone.Whole:
                    WholeCount = value;
                    break;
                case BondZone.PartA:
                    PartACount = value;
                    break;
                case BondZone.PartB:
                    PartBCount = value;
                    break;
            }
        }
    }
}
