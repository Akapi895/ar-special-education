using Core.AR;
using UnityEngine;

namespace Features.Activities.NumberBonds
{
    public class NumberBondsActivityBootstrap : MonoBehaviour
    {
        [SerializeField]
        private NumberBondsPresenter presenter;

        [SerializeField]
        private NumberBondsView view;

        [SerializeField]
        private NumberBondsConfig config;

        [SerializeField]
        private bool autoStartWhenReady = true;

        [SerializeField]
        private float startDelaySeconds = 1f;

        private bool started;

        public void Configure(NumberBondsPresenter presenter, NumberBondsView view, NumberBondsConfig config,
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
            if (!value)
            {
                CancelInvoke(nameof(TryStartActivity));
            }
        }

        private void Awake()
        {
            if (presenter == null)
            {
                presenter = GetComponent<NumberBondsPresenter>();
            }

            if (view == null)
            {
                view = GetComponent<NumberBondsView>();
            }

            if (GetComponent<ActivityPrefabSetup>() == null)
            {
                gameObject.AddComponent<ActivityPrefabSetup>();
            }

            if (GetComponent<NumberBondsRuntimeUI>() == null)
            {
                gameObject.AddComponent<NumberBondsRuntimeUI>();
            }
        }

        private void Start()
        {
            if (autoStartWhenReady && Project.App.GameplayActivityRouter.Instance == null)
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
                    Debug.LogError("[NumberBondsActivityBootstrap] NumberBondsConfig is not assigned.");
                }

                return;
            }

            ARServiceBootstrap bootstrap = ARServiceBootstrap.Instance;
            if (bootstrap == null)
            {
                Debug.LogError("[NumberBondsActivityBootstrap] ARServiceBootstrap not found in scene.");
                return;
            }

            if (bootstrap.Placement == null || bootstrap.Interaction == null)
            {
                Debug.LogError("[NumberBondsActivityBootstrap] AR placement or interaction service missing.");
                return;
            }

            if (!bootstrap.Placement.IsPlacementAvailable || !bootstrap.Placement.HasLearningArea)
            {
                Debug.Log("[NumberBondsActivityBootstrap] Waiting for learning area placement...");
                Invoke(nameof(TryStartActivity), 0.5f);
                return;
            }

            presenter.Initialize(config, view, bootstrap.Placement, bootstrap.Interaction);
            presenter.StartActivity();
            view.Show();
            started = true;

            Debug.Log("[NumberBondsActivityBootstrap] Number Bonds started.");
        }
    }
}
