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
