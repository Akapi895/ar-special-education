using Core.UI.Layout;
using UnityEngine;

namespace Features.Activities.NumberLineJump
{
    public class NumberLineJumpRuntimeUI : MonoBehaviour
    {
        [SerializeField]
        private NumberLineJumpView view;

        private void Awake()
        {
            if (view == null)
            {
                view = GetComponent<NumberLineJumpView>();
            }

            if (view == null || view.HasUiReferences)
            {
                return;
            }

            view.BuildRuntimeUi(ActivityRuntimeCanvas.Create(transform, "NumberLineJumpRuntimeCanvas"));
        }
    }
}
