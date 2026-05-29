using Core.Learning.ActivityRunner;
using Core.Learning.Models;
using System.Collections.Generic;
using UnityEngine;

namespace Features.Activities.NumberBonds
{
    public class NumberBondsPresenter : ActivityPresenter
    {
        private IARPlacementService placementService;
        private IARInteractionService interactionService;
        private INumberBondsView view;
        private NumberBondsConfig numberBondsConfig;
        private NumberBondsQuestion currentQuestion;
        private NumberBondRoundState roundState;
        private string currentExpression;

        public void Initialize(NumberBondsConfig config, INumberBondsView activityView,
            IARPlacementService arPlacement, IARInteractionService arInteraction)
        {
            if (config == null || !config.IsValid())
            {
                Debug.LogError("[NumberBondsPresenter] Invalid config.");
                return;
            }

            numberBondsConfig = config;
            view = activityView;
            placementService = arPlacement;
            interactionService = arInteraction;

            base.Initialize(config);
            view?.Initialize(this);

            if (view != null)
            {
                view.OnObjectMoveRequested += HandleObjectMoveRequested;
                view.OnConfirmRequested += HandleConfirmRequested;
                view.OnHintRequested += HandleHintRequested;
                view.OnCancelRequested += HandleCancelRequested;
            }

            Debug.Log($"[NumberBondsPresenter] Initialized with {config.Questions.Count} questions.");
        }

        protected override void LoadRound(int roundNumber)
        {
            currentQuestion = numberBondsConfig.GetQuestion(roundNumber - 1);
            if (currentQuestion == null)
            {
                Debug.LogError($"[NumberBondsPresenter] Failed to load question {roundNumber}.");
                return;
            }

            roundState = NumberBondRoundState.FromQuestion(currentQuestion);
            currentExpression = NumberBondExpressionBinder.Format(currentQuestion, roundState);

            view?.SetupRound(currentQuestion, roundState, placementService, interactionService, numberBondsConfig);
            view?.UpdateProgress(roundNumber, numberBondsConfig.NumberOfRounds);
            view?.UpdateExpression(currentExpression);
            view?.SetConfirmEnabled(CanConfirm());

            Debug.Log($"[NumberBondsPresenter] Loading round {roundNumber}: Mode={currentQuestion.Mode}, Target={currentQuestion.WholeTarget}");
        }

        protected override bool CheckAnswer(ActivityAnswer answer)
        {
            if (answer is not NumberBondsAnswer numberBondsAnswer)
            {
                Debug.LogError("[NumberBondsPresenter] Answer is not a NumberBondsAnswer.");
                return false;
            }

            return numberBondsAnswer.IsCorrect();
        }

        protected override ErrorType? GetErrorType(ActivityAnswer answer)
        {
            return ErrorType.WrongQuantity;
        }

        private void HandleObjectMoveRequested(NumberBondMoveRequest request)
        {
            if (currentState != ActivityState.InProgress || roundState == null)
            {
                return;
            }

            NumberBondValidationResult moveResult = ValidateMove(request.FromZone, request.ToZone);
            if (moveResult != NumberBondValidationResult.Correct)
            {
                view?.RejectObjectMove(request.ObjectId, moveResult);
                return;
            }

            if (!roundState.TryMove(request.FromZone, request.ToZone))
            {
                view?.RejectObjectMove(request.ObjectId, NumberBondValidationResult.InvalidMove);
                return;
            }

            currentExpression = NumberBondExpressionBinder.Format(currentQuestion, roundState);
            view?.CommitObjectMove(request.ObjectId, request.FromZone, request.ToZone);
            view?.UpdateZoneCounts(roundState);
            view?.UpdateExpression(currentExpression);
            view?.SetConfirmEnabled(CanConfirm());
        }

        private NumberBondValidationResult ValidateMove(BondZone from, BondZone to)
        {
            if (from == to || roundState == null || roundState.GetCount(from) <= 0)
            {
                return NumberBondValidationResult.InvalidMove;
            }

            if (roundState.IsZoneLocked(from) || roundState.IsZoneLocked(to))
            {
                return NumberBondValidationResult.LockedZoneModified;
            }

            return NumberBondValidationResult.Correct;
        }

        private void HandleConfirmRequested()
        {
            if (currentState != ActivityState.InProgress)
            {
                return;
            }

            NumberBondValidationResult result = ValidateCurrentState();
            currentExpression = NumberBondExpressionBinder.Format(currentQuestion, roundState);
            var answer = new NumberBondsAnswer(
                currentQuestion,
                roundState,
                result,
                currentExpression,
                Time.time - roundStartTime,
                currentResult.TotalAttempts + 1,
                hintsUsedInCurrentRound > 0);

            SubmitAnswer(answer);
        }

        private NumberBondValidationResult ValidateCurrentState()
        {
            if (currentQuestion == null || roundState == null)
            {
                return NumberBondValidationResult.TechnicalIssue;
            }

            if (roundState.WholeCount != 0)
            {
                return NumberBondValidationResult.NotAllObjectsMoved;
            }

            int totalParts = roundState.PartACount + roundState.PartBCount;
            if (totalParts != currentQuestion.WholeTarget)
            {
                return NumberBondValidationResult.WrongPartCount;
            }

            if (currentQuestion.Mode == NumberBondMode.TargetSplit)
            {
                if (currentQuestion.KnownPartA >= 0 && roundState.PartACount != currentQuestion.KnownPartA)
                {
                    return NumberBondValidationResult.WrongPartCount;
                }

                if (currentQuestion.KnownPartB >= 0 && roundState.PartBCount != currentQuestion.KnownPartB)
                {
                    return NumberBondValidationResult.WrongPartCount;
                }
            }

            return NumberBondValidationResult.Correct;
        }

        private bool CanConfirm()
        {
            return roundState != null && roundState.WholeCount == 0;
        }

        private void HandleHintRequested()
        {
            RequestHint();
        }

        private void HandleCancelRequested()
        {
            Cancel();
        }

        protected override void OnHintProvided(ActivityHint hint)
        {
            base.OnHintProvided(hint);
            string formatted = numberBondsConfig.GetFormattedHintText(hint, currentQuestion, roundState);
            view?.ShowHint(new ActivityHint(hint.HintId, formatted, hint.Level, hint.ARHint));
        }

        protected override void HandleCorrectAnswer(ActivityAnswer answer)
        {
            string feedback = numberBondsConfig.GetFeedback(NumberBondValidationResult.Correct, currentExpression);
            view?.ShowCorrectFeedback(feedback);
            base.HandleCorrectAnswer(answer);
        }

        protected override void HandleIncorrectAnswer(ActivityAnswer answer)
        {
            NumberBondValidationResult result = answer is NumberBondsAnswer numberBondsAnswer
                ? numberBondsAnswer.ValidationResult
                : NumberBondValidationResult.WrongPartCount;

            string feedback = numberBondsConfig.GetFeedback(result, currentExpression);
            view?.ShowIncorrectFeedback(feedback);
            base.HandleIncorrectAnswer(answer);

            if (currentState == ActivityState.Failed)
            {
                view?.ShowActivityFailed(numberBondsConfig.FailedFeedback, currentResult);
            }
        }

        protected override void CompleteActivity(bool success)
        {
            base.CompleteActivity(success);

            if (success)
            {
                view?.ShowActivityComplete(currentResult);
            }
            else
            {
                view?.ShowActivityFailed(numberBondsConfig.FailedFeedback, currentResult);
            }
        }

        public override void Cleanup()
        {
            if (view != null)
            {
                view.OnObjectMoveRequested -= HandleObjectMoveRequested;
                view.OnConfirmRequested -= HandleConfirmRequested;
                view.OnHintRequested -= HandleHintRequested;
                view.OnCancelRequested -= HandleCancelRequested;
                view.ClearSpawnedObjects();
            }

            placementService = null;
            interactionService = null;
            numberBondsConfig = null;
            currentQuestion = null;
            roundState = null;
            base.Cleanup();
        }
    }
}
