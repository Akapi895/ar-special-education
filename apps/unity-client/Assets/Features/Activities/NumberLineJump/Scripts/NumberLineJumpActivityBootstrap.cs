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

        [SerializeField]
        private float startDelaySeconds = 1f;

        private bool started;

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

            if (!bootstrap.Placement.IsPlacementAvailable)
            {
                Debug.Log("[NumberLineJumpActivityBootstrap] Waiting for placement...");
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

            Debug.Log("[NumberLineJumpActivityBootstrap] Number Line Jump started.");
        }
    }
}
