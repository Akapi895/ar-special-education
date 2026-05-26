using System;
using Core.Learning.ActivityRunner;
using UnityEngine;

namespace Core.AR.ARSession
{
    /// <summary>
    /// Always-ready session stub for editor mock runs when AR Foundation session is absent.
    /// </summary>
    public class ARSessionFallback : MonoBehaviour, IARSessionService
    {
        public event Action OnSessionReady;
        public event Action OnSessionLost;

        public bool IsSessionReady { get; private set; } = true;
        public bool IsTrackingStable => true;
        public TrackingQuality TrackingQuality => TrackingQuality.Good;

        public void Initialize()
        {
            IsSessionReady = true;
            OnSessionReady?.Invoke();
        }

        public void StartSession()
        {
            Initialize();
        }

        public void StopSession()
        {
            if (IsSessionReady)
            {
                IsSessionReady = false;
                OnSessionLost?.Invoke();
            }
        }

        public void ResetSession()
        {
            StopSession();
            Initialize();
        }
    }
}
