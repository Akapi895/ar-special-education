using Core.AR;
using Core.Learning.ActivityRunner;
using Features.Activities;
using UnityEngine;

namespace Features.Activities.NumberLineJump
{
    /// <summary>
    /// Wires Number Line Jump with AR services and starts the activity when placement is ready.
    /// </summary>
    public class NumberLineJumpActivityBootstrap : MonoBehaviour
    {
        [SerializeField]
        private NumberLineJumpPresenter presenter;

        [SerializeField]
        private NumberLineJumpView view;

        [SerializeField]
        private NumberLineJumpConfig config;

        [SerializeField]
        private bool autoStartWhenReady = true;

        private bool started;
        private bool waitingForPlacement;

        public void Configure(NumberLineJumpPresenter presenter, NumberLineJumpView view, NumberLineJumpConfig config,
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
                presenter = GetComponent<NumberLineJumpPresenter>();
            }

            if (view == null)
            {
                view = GetComponent<NumberLineJumpView>();
            }

            if (GetComponent<ActivityPrefabSetup>() == null)
            {
                gameObject.AddComponent<ActivityPrefabSetup>();
            }

            if (GetComponent<NumberLineJumpRuntimeUI>() == null)
            {
                gameObject.AddComponent<NumberLineJumpRuntimeUI>();
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
                    Debug.LogError("[NumberLineJumpActivityBootstrap] NumberLineJumpConfig is not assigned.");
                }

                return;
            }

            var bootstrap = ARServiceBootstrap.Instance;
            if (bootstrap == null)
            {
                Debug.LogError("[NumberLineJumpActivityBootstrap] ARServiceBootstrap not found in scene.");
                return;
            }

            if (bootstrap.Placement == null || bootstrap.Interaction == null)
            {
                Debug.LogError("[NumberLineJumpActivityBootstrap] AR placement or interaction service missing.");
                return;
            }

            if (!bootstrap.Placement.IsPlacementAvailable || !bootstrap.Placement.HasLearningArea)
            {
                WaitForPlacement(bootstrap);
                Debug.Log("[NumberLineJumpActivityBootstrap] Waiting for learning area placement...");
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

            Debug.Log("[NumberLineJumpActivityBootstrap] Number Line Jump started.");
        }
    }
}
