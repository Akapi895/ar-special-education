using Core.AR;
using Core.Learning.ActivityRunner;
using Features.Activities;
using UnityEngine;

namespace Features.Activities.CompareQuantity
{
    /// <summary>
    /// Wires Compare Quantity with AR services and starts the activity when placement is ready.
    /// </summary>
    public class CompareQuantityActivityBootstrap : MonoBehaviour
    {
        [SerializeField]
        private CompareQuantityPresenter presenter;

        [SerializeField]
        private CompareQuantityView view;

        [SerializeField]
        private CompareQuantityConfig config;

        [SerializeField]
        private bool autoStartWhenReady = true;

        private bool started;

        public void Configure(CompareQuantityPresenter presenter, CompareQuantityView view, CompareQuantityConfig config,
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
                presenter = GetComponent<CompareQuantityPresenter>();
            }

            if (view == null)
            {
                view = GetComponent<CompareQuantityView>();
            }

            if (GetComponent<ActivityPrefabSetup>() == null)
            {
                gameObject.AddComponent<ActivityPrefabSetup>();
            }

            if (GetComponent<CompareQuantityRuntimeUI>() == null)
            {
                gameObject.AddComponent<CompareQuantityRuntimeUI>();
            }
        }

        private void Start()
        {
            if (autoStartWhenReady && Project.App.GameplayActivityRouter.Instance == null)
            {
                var bootstrap = ARServiceBootstrap.Instance;
                if (bootstrap != null && bootstrap.Placement != null)
                {
                    bootstrap.Placement.OnLearningAreaPlaced += OnPlacementReady;
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
            TryStartActivity();
        }

        public void TryStartActivity()
        {
            if (started || presenter == null || view == null || config == null)
            {
                if (config == null)
                {
                    Debug.LogError("[CompareQuantityActivityBootstrap] CompareQuantityConfig is not assigned.");
                }

                return;
            }

            var bootstrap = ARServiceBootstrap.Instance;
            if (bootstrap == null)
            {
                Debug.LogError("[CompareQuantityActivityBootstrap] ARServiceBootstrap not found in scene.");
                return;
            }

            if (bootstrap.Placement == null || bootstrap.Interaction == null)
            {
                Debug.LogError("[CompareQuantityActivityBootstrap] AR placement or interaction service missing.");
                return;
            }

            if (!bootstrap.Placement.IsPlacementAvailable || !bootstrap.Placement.HasLearningArea)
            {
                Debug.Log("[CompareQuantityActivityBootstrap] Waiting for learning area placement...");
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

            Debug.Log("[CompareQuantityActivityBootstrap] Compare Quantity started.");
        }
    }
}
