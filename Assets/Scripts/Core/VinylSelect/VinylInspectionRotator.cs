using UnityEngine;
using UnityEngine.InputSystem;

namespace Core.VinylSelect
{
    /**
     * @brief Converts right-mouse dragging into cover or disc inspection rotation.
     *
     * Assign vinylSelectController and inspectionView in the Inspector. Awake attempts
     * to find either reference if missing. Horizontal mouse movement is multiplied by
     * degreesPerPixel; invertDirection reverses its sign. Only VinylSelected (cover)
     * and VinylDraggedOutFocused (disc) accept rotation. Other states ignore this input.
     * VinylInspectionView applies and resets the accumulated angles.
     * @see VinylInspectionView
     */
    public class VinylInspectionRotator : MonoBehaviour
    {
        [SerializeField] private VinylSelectController vinylSelectController;
        [SerializeField] private VinylInspectionView inspectionView;
        [SerializeField] private float degreesPerPixel = 1.2f;
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

        /**
         * @brief Routes the rotation delta to the object allowed by the current state.
         * @param rotationDegrees Signed angle increment in degrees.
         */
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
