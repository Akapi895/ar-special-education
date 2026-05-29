using Core.UI.Layout;
using UnityEngine;

namespace Features.Activities.NumberBonds
{
    public class NumberBondsRuntimeUI : MonoBehaviour
    {
        [SerializeField]
        private NumberBondsView view;

        private void Awake()
        {
            if (view == null)
            {
                view = GetComponent<NumberBondsView>();
            }

            if (view == null || view.HasUiReferences)
            {
                return;
            }

            view.BuildRuntimeUi(ActivityRuntimeCanvas.Create(transform, "NumberBondsRuntimeCanvas"));
        }
    }
}
