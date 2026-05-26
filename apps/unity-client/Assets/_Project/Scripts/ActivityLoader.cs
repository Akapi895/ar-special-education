using Core.Data.LocalStorage;
using Features.Activities.CompareQuantity;
using Features.Activities.NumberLineJump;
using Features.Activities.QuantityMatch;
using UnityEngine;

namespace Project.App
{
    /// <summary>
    /// Legacy serialized loader for handcrafted scenes. Production gameplay is routed by GameplayActivityRouter.
    /// </summary>
    public class ActivityLoader : MonoBehaviour
    {
        [Header("Quantity Match")]
        [SerializeField]
        private QuantityMatchPresenter quantityMatchPresenter;

        [SerializeField]
        private QuantityMatchView quantityMatchView;

        [SerializeField]
        private QuantityMatchConfig quantityMatchConfig;

        [Header("Number Line Jump")]
        [SerializeField]
        private NumberLineJumpPresenter numberLineJumpPresenter;

        [SerializeField]
        private NumberLineJumpView numberLineJumpView;

        [SerializeField]
        private NumberLineJumpConfig numberLineJumpConfig;

        [Header("Compare Quantity")]
        [SerializeField]
        private CompareQuantityPresenter compareQuantityPresenter;

        [SerializeField]
        private CompareQuantityView compareQuantityView;

        [SerializeField]
        private CompareQuantityConfig compareQuantityConfig;

        [Header("Default Activity")]
        [Tooltip("Activity to load if none selected (for testing)")]
        [SerializeField]
        private string defaultActivityId = "QuantityMatch";

        private Core.AR.ARServiceBootstrap arBootstrap;
        private const float PlacementRetrySeconds = 0.5f;

        private void Awake()
        {
            if (FindAnyObjectByType<GameplayActivityRouter>() != null)
            {
                Debug.Log("[ActivityLoader] GameplayActivityRouter is present; disabling legacy ActivityLoader.");
                enabled = false;
            }
        }

        private void Start()
        {
            if (!enabled)
            {
                return;
            }

            arBootstrap = Core.AR.ARServiceBootstrap.Instance;
            if (arBootstrap == null)
            {
                Debug.LogError("[ActivityLoader] ARServiceBootstrap not found in scene.");
                return;
            }

            LoadSelectedActivity();
        }

        private void LoadSelectedActivity()
        {
            string activityId = SelectedActivityData.ActivityId ?? defaultActivityId;

            Debug.Log($"[ActivityLoader] Loading activity: {activityId}");

            if (arBootstrap == null || arBootstrap.Placement == null || arBootstrap.Interaction == null)
            {
                Debug.LogError("[ActivityLoader] AR services not available.");
                return;
            }

            if (!arBootstrap.Placement.IsPlacementAvailable || !arBootstrap.Placement.HasLearningArea)
            {
                Debug.Log("[ActivityLoader] Waiting for learning area placement before starting activity.");
                Invoke(nameof(LoadSelectedActivity), PlacementRetrySeconds);
                return;
            }

            switch (activityId)
            {
                case "QuantityMatch":
                    LoadQuantityMatch();
                    break;

                case "NumberLineJump":
                    LoadNumberLineJump();
                    break;

                case "CompareQuantity":
                    LoadCompareQuantity();
                    break;

                default:
                    Debug.LogError($"[ActivityLoader] Unknown activity ID: {activityId}");
                    break;
            }

            // Clear the selection after loading
            SelectedActivityData.Clear();
        }

        private void LoadQuantityMatch()
        {
            if (quantityMatchPresenter == null || quantityMatchView == null || quantityMatchConfig == null)
            {
                Debug.LogError("[ActivityLoader] Quantity Match components not assigned.");
                return;
            }

            if (arBootstrap.Placement == null || arBootstrap.Interaction == null)
            {
                Debug.LogError("[ActivityLoader] AR services not available.");
                return;
            }

            quantityMatchPresenter.Initialize(
                quantityMatchConfig,
                quantityMatchView,
                arBootstrap.Placement,
                arBootstrap.Interaction);

            quantityMatchPresenter.StartActivity();
            quantityMatchView.Show();

            Debug.Log("[ActivityLoader] Quantity Match loaded.");
        }

        private void LoadNumberLineJump()
        {
            if (numberLineJumpPresenter == null || numberLineJumpView == null || numberLineJumpConfig == null)
            {
                Debug.LogError("[ActivityLoader] Number Line Jump components not assigned.");
                return;
            }

            if (arBootstrap.Placement == null || arBootstrap.Interaction == null)
            {
                Debug.LogError("[ActivityLoader] AR services not available.");
                return;
            }

            numberLineJumpPresenter.Initialize(
                numberLineJumpConfig,
                numberLineJumpView,
                arBootstrap.Placement,
                arBootstrap.Interaction);

            numberLineJumpPresenter.StartActivity();
            numberLineJumpView.Show();

            Debug.Log("[ActivityLoader] Number Line Jump loaded.");
        }

        private void LoadCompareQuantity()
        {
            if (compareQuantityPresenter == null || compareQuantityView == null || compareQuantityConfig == null)
            {
                Debug.LogError("[ActivityLoader] Compare Quantity components not assigned.");
                return;
            }

            if (arBootstrap.Placement == null || arBootstrap.Interaction == null)
            {
                Debug.LogError("[ActivityLoader] AR services not available.");
                return;
            }

            compareQuantityPresenter.Initialize(
                compareQuantityConfig,
                compareQuantityView,
                arBootstrap.Placement,
                arBootstrap.Interaction);

            compareQuantityPresenter.StartActivity();
            compareQuantityView.Show();

            Debug.Log("[ActivityLoader] Compare Quantity loaded.");
        }
    }
}
