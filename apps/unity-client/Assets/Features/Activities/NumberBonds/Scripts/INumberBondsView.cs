using Core.Learning.ActivityRunner;
using Core.Learning.Models;
using System;

namespace Features.Activities.NumberBonds
{
    public interface INumberBondsView : IActivityView
    {
        event Action<NumberBondMoveRequest> OnObjectMoveRequested;
        event Action OnConfirmRequested;

        void SetupRound(NumberBondsQuestion question, NumberBondRoundState state,
            IARPlacementService placementService, IARInteractionService interactionService, NumberBondsConfig config);

        void CommitObjectMove(string objectId, BondZone fromZone, BondZone toZone);
        void RejectObjectMove(string objectId, NumberBondValidationResult reason);
        void UpdateZoneCounts(NumberBondRoundState state);
        void UpdateExpression(string expression);
        void SetConfirmEnabled(bool enabled);
        void ShowCorrectFeedback(string message);
        void ShowIncorrectFeedback(string message);
        void ShowActivityComplete(ActivityResult result);
        void ShowActivityFailed(string message, ActivityResult result);
        void ClearSpawnedObjects();
    }
}
