using Core.AR;
using Core.Learning.ActivityRunner;
using Features.Activities;
using UnityEngine;

namespace Features.Activities.QuantityMatch
{
    /// <summary>
    /// Wires Quantity Match with AR services and starts the activity when placement is ready.
    /// </summary>
    public class QuantityMatchActivityBootstrap : MonoBehaviour
    {
        [SerializeField]
        private QuantityMatchPresenter presenter;

        [SerializeField]
        private QuantityMatchView view;

        [SerializeField]
        private QuantityMatchConfig config;

        [SerializeField]
        private bool autoStartWhenReady = true;

        private bool started;
        private bool waitingForPlacement;

        public void Configure(QuantityMatchPresenter presenter, QuantityMatchView view, QuantityMatchConfig config,
            bool autoStartWhenReady = false)
        {
            this.presenter = presenter;
            this.view = view;
            this.config = config;
            this.autoStartWhenReady = autoStartWhenReady;
        }

        public void SetAutoStartWhenReady(bool value)
        {
            autoStartWhenReady = value;
        }

        private void Awake()
        {
            if (presenter == null)
            {
                presenter = GetComponent<QuantityMatchPresenter>();
            }

            if (view == null)
            {
                view = GetComponent<QuantityMatchView>();
            }

            if (GetComponent<ActivityPrefabSetup>() == null)
            {
                gameObject.AddComponent<ActivityPrefabSetup>();
            }

            if (GetComponent<QuantityMatchRuntimeUI>() == null)
            {
                gameObject.AddComponent<QuantityMatchRuntimeUI>();
            }
        }

        private void Start()
        {
            if (autoStartWhenReady && Project.App.GameplayActivityRouter.Instance == null)
            {
                var bootstrap = ARServiceBootstrap.Instance;
                if (bootstrap != null && bootstrap.Placement != null)
                {
                    WaitForPlacement(bootstrap);
                }

                if (bootstrap != null && bootstrap.Placement != null
                    && bootstrap.Placement.IsPlacementAvailable && bootstrap.Placement.HasLearningArea)
                {
                    OnPlacementReady();
                }
            }
        }

        private void OnPlacementReady()
        {
            var bootstrap = ARServiceBootstrap.Instance;
            if (bootstrap != null && bootstrap.Placement != null)
            {
                bootstrap.Placement.OnLearningAreaPlaced -= OnPlacementReady;
            }
            waitingForPlacement = false;
            TryStartActivity();
        }

        private void OnDestroy()
        {
            var bootstrap = ARServiceBootstrap.Instance;
            if (bootstrap != null && bootstrap.Placement != null)
            {
                bootstrap.Placement.OnLearningAreaPlaced -= OnPlacementReady;
            }
        }

        private void WaitForPlacement(ARServiceBootstrap bootstrap)
        {
            if (waitingForPlacement || bootstrap == null || bootstrap.Placement == null)
            {
                return;
            }

            bootstrap.Placement.OnLearningAreaPlaced += OnPlacementReady;
            waitingForPlacement = true;
        }

        public void TryStartActivity()
        {
            if (started || presenter == null || view == null || config == null)
            {
                if (config == null)
                {
                    Debug.LogError("[QuantityMatchActivityBootstrap] QuantityMatchConfig is not assigned.");
                }

                return;
            }

            var bootstrap = ARServiceBootstrap.Instance;
            if (bootstrap == null)
            {
                Debug.LogError("[QuantityMatchActivityBootstrap] ARServiceBootstrap not found in scene.");
                return;
            }

            if (bootstrap.Placement == null || bootstrap.Interaction == null)
            {
                Debug.LogError("[QuantityMatchActivityBootstrap] AR placement or interaction service missing.");
                return;
            }

            if (!bootstrap.Placement.IsPlacementAvailable || !bootstrap.Placement.HasLearningArea)
            {
                WaitForPlacement(bootstrap);
                Debug.Log("[QuantityMatchActivityBootstrap] Waiting for learning area placement...");
                return;
            }

            presenter.Initialize(
                config,
                view,
                bootstrap.Placement,
                bootstrap.Interaction);

            presenter.StartActivity();
            view.Show();
            started = true;

            Debug.Log("[QuantityMatchActivityBootstrap] Quantity Match started.");
        }
    }
}
