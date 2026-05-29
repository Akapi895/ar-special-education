using Core.UI.Layout;
using UnityEngine;

namespace Features.Activities.CompareQuantity
{
    public class CompareQuantityRuntimeUI : MonoBehaviour
    {
        [SerializeField]
        private CompareQuantityView view;

        private void Awake()
        {
            if (view == null)
            {
                view = GetComponent<CompareQuantityView>();
            }

            if (view == null || view.HasUiReferences)
            {
                return;
            }

            view.BuildRuntimeUi(ActivityRuntimeCanvas.Create(transform, "CompareQuantityRuntimeCanvas"));
        }
    }
}
