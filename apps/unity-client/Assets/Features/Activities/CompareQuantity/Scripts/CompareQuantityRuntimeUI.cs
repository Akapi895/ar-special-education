using Core.AR;
using Core.Learning.ActivityRunner;
using UnityEngine;

namespace Features.Activities.CompareQuantity
{
    /// <summary>
    /// Runtime UI builder for Compare Quantity activity.
    /// Creates the necessary UI elements at runtime when prefabs are not available.
    /// </summary>
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

            CreateRuntimeUI();
        }

        private void CreateRuntimeUI()
        {
            // This would create UI elements programmatically
            // For now, it's a placeholder for the UI team to implement
            Debug.Log("[CompareQuantityRuntimeUI] Runtime UI creation not yet implemented. Please assign UI prefabs in the scene.");
        }
    }
}
