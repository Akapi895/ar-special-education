using Core.AR;
using Core.Learning.ActivityRunner;
using UnityEngine;

namespace Features.Activities.NumberLineJump
{
    /// <summary>
    /// Runtime UI builder for Number Line Jump activity.
    /// Creates the necessary UI elements at runtime when prefabs are not available.
    /// </summary>
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

            CreateRuntimeUI();
        }

        private void CreateRuntimeUI()
        {
            // This would create UI elements programmatically
            // For now, it's a placeholder for the UI team to implement
            Debug.Log("[NumberLineJumpRuntimeUI] Runtime UI creation not yet implemented. Please assign UI prefabs in the scene.");
        }
    }
}
