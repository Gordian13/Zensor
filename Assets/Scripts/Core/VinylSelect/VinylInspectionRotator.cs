using UnityEngine;
using UnityEngine.InputSystem;

namespace Core.VinylSelect
{
    /**
     * Converts right-mouse dragging into inspection rotation for the selected cover or focused disc.
     */
    public class VinylInspectionRotator : MonoBehaviour
    {
        [SerializeField] private VinylSelectController vinylSelectController;
        [SerializeField] private VinylInspectionView inspectionView;
        [SerializeField] private float degreesPerPixel = 0.35f;
        [SerializeField] private bool invertDirection;

        private void Awake()
        {
            if (vinylSelectController == null)
                vinylSelectController = FindFirstObjectByType<VinylSelectController>();

            if (inspectionView == null)
                inspectionView = FindFirstObjectByType<VinylInspectionView>();

            if (vinylSelectController == null)
            {
                Debug.LogError(
                    $"{nameof(VinylInspectionRotator)} has no VinylSelectController assigned.",
                    this);
            }

            if (inspectionView == null)
            {
                Debug.LogError(
                    $"{nameof(VinylInspectionRotator)} has no VinylInspectionView assigned.",
                    this);
            }
        }

        private void Update()
        {
            Mouse mouse = Mouse.current;
            if (mouse == null ||
                !mouse.rightButton.isPressed ||
                vinylSelectController?.SelectedVinyl == null ||
                inspectionView == null)
            {
                return;
            }

            float horizontalDelta = mouse.delta.ReadValue().x;
            if (Mathf.Approximately(horizontalDelta, 0f))
                return;

            float direction = invertDirection ? -1f : 1f;
            float rotationDegrees = horizontalDelta * degreesPerPixel * direction;

            ApplyRotationForCurrentState(rotationDegrees);
        }

        private void ApplyRotationForCurrentState(float rotationDegrees)
        {
            VinylState state = vinylSelectController.CurrentVinylState;

            if (state == VinylState.VinylSelected)
            {
                inspectionView.AddVinylInspectionRotation(rotationDegrees);
                return;
            }

            if (state == VinylState.VinylDraggedOutFocused)
            {
                inspectionView.AddDiscInspectionRotation(rotationDegrees);
            }
        }
    }
}
