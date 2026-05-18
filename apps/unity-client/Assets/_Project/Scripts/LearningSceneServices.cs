using Core.Data.LocalStorage;
using Core.Support.FeedbackSystem;
using UnityEngine;

namespace Project.Runtime
{
    /// <summary>
    /// Ensures learning support services exist in gameplay scenes.
    /// </summary>
    [DefaultExecutionOrder(-150)]
    public class LearningSceneServices : MonoBehaviour
    {
        private void Awake()
        {
            if (FindAnyObjectByType<ProgressStorageProxy>() == null)
            {
                var progressGo = new GameObject("ProgressStorageProxy");
                progressGo.AddComponent<ProgressStorageProxy>();
            }

            if (FindAnyObjectByType<FeedbackServiceProxy>() == null)
            {
                var feedbackGo = new GameObject("FeedbackServiceProxy");
                feedbackGo.AddComponent<FeedbackServiceProxy>();
            }

            ProgressStorageProxy.Instance.StartSession();
        }
    }
}
