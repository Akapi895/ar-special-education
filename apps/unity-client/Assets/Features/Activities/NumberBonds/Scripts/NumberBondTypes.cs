using System;

namespace Features.Activities.NumberBonds
{
    [Serializable]
    public enum NumberBondMode
    {
        FreeSplit = 0,
        TargetSplit = 1,
        Compose = 2,
        MissingPart = 3
    }

    [Serializable]
    public enum BondZone
    {
        Whole = 0,
        PartA = 1,
        PartB = 2
    }

    public enum NumberBondValidationResult
    {
        Correct,
        NotAllObjectsMoved,
        WrongPartCount,
        WrongTotal,
        LockedZoneModified,
        InvalidMove,
        TechnicalIssue
    }

    public readonly struct NumberBondMoveRequest
    {
        public readonly string ObjectId;
        public readonly BondZone FromZone;
        public readonly BondZone ToZone;

        public NumberBondMoveRequest(string objectId, BondZone fromZone, BondZone toZone)
        {
            ObjectId = objectId;
            FromZone = fromZone;
            ToZone = toZone;
        }
    }
}
