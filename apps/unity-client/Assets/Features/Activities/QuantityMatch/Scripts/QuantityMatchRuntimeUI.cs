using Core.UI.Layout;
using UnityEngine;

namespace Features.Activities.QuantityMatch
{
    [RequireComponent(typeof(QuantityMatchView))]
    public class QuantityMatchRuntimeUI : MonoBehaviour
    {
        private void Awake()
        {
            var view = GetComponent<QuantityMatchView>();
            if (view == null || view.HasUiReferences)
            {
                return;
            }

            view.BuildRuntimeUi(ActivityRuntimeCanvas.Create(transform, "QuantityMatchCanvas"));
        }
    }
}
