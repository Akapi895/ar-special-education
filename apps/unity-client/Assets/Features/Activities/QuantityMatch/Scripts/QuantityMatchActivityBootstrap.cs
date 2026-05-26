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

        [SerializeField]
        private float startDelaySeconds = 1f;

        private bool started;

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
            if (autoStartWhenReady)
            {
                Invoke(nameof(TryStartActivity), startDelaySeconds);
            }
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
                Debug.Log("[QuantityMatchActivityBootstrap] Waiting for learning area placement...");
                Invoke(nameof(TryStartActivity), 0.5f);
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
