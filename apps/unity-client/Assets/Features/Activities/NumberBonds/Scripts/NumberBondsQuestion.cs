using Core.Learning.Models;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Features.Activities.NumberBonds
{
    [Serializable]
    public class NumberBondsQuestion
    {
        [SerializeField]
        private NumberBondMode mode = NumberBondMode.FreeSplit;

        [SerializeField]
        private int wholeTarget = 5;

        [SerializeField]
        private int knownPartA = -1;

        [SerializeField]
        private int knownPartB = -1;

        [SerializeField]
        private List<ActivityHint> customHints = new List<ActivityHint>();

        [SerializeField]
        private string objectPrefabName = string.Empty;

        public NumberBondMode Mode => mode;
        public int WholeTarget => wholeTarget;
        public int KnownPartA => knownPartA;
        public int KnownPartB => knownPartB;
        public List<ActivityHint> CustomHints => customHints;
        public string ObjectPrefabName => objectPrefabName;

        public void Configure(
            NumberBondMode mode,
            int wholeTarget,
            int knownPartA = -1,
            int knownPartB = -1,
            List<ActivityHint> customHints = null,
            string objectPrefabName = "")
        {
            this.mode = mode;
            this.wholeTarget = wholeTarget;
            this.knownPartA = knownPartA;
            this.knownPartB = knownPartB;
            this.customHints = customHints ?? new List<ActivityHint>();
            this.objectPrefabName = objectPrefabName ?? string.Empty;
        }

        public bool IsValid()
        {
            if (wholeTarget <= 0 || wholeTarget > 10)
            {
                Debug.LogWarning($"[NumberBondsQuestion] Whole target must be 1..10. Value={wholeTarget}");
                return false;
            }

            if (knownPartA > wholeTarget || knownPartB > wholeTarget)
            {
                Debug.LogWarning("[NumberBondsQuestion] Known parts cannot exceed the whole target.");
                return false;
            }

            if (knownPartA >= 0 && knownPartB >= 0 && knownPartA + knownPartB != wholeTarget)
            {
                Debug.LogWarning("[NumberBondsQuestion] Known parts must add up to the whole target.");
                return false;
            }

            if (mode == NumberBondMode.TargetSplit && knownPartA < 0 && knownPartB < 0)
            {
                Debug.LogWarning("[NumberBondsQuestion] TargetSplit needs one known part.");
                return false;
            }

            return true;
        }
    }
}
