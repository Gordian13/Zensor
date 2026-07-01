using UnityEngine;
using UnityEngine.InputSystem;

namespace Core.VinylSelect
{
    /**
     * Camera script that detects dragging on the selected vinyl disc.
     * Moves the disc between its selected and focused position and requests the matching state changes.
     */
    [RequireComponent(typeof(Camera))]
    public class VinylDragController : MonoBehaviour
    {
        [SerializeField] private VinylSelectController vinylSelectController;
        [SerializeField] private LayerMask vinylLayer = ~0;
        [SerializeField] private float rayDistance = 100f;

        [Header("Drag")]
        [SerializeField] private float localUnitsPerPixel = 0.001f;
        [SerializeField] private float focusedDistance = 0.6f;
        [SerializeField, Range(0f, 1f)] private float completionThreshold = 0.6f;

        private Camera targetCamera;
        private Transform draggedDisc;
        private Vector2 dragStartMousePosition;
        private Vector3 selectedDiscPosition;
        private Vector3 focusedDiscPosition;
        private bool isDraggingOut;

        /**
         * Caches the Camera component used for raycasts and validates the state controller reference.
         */
        private void Awake()
        {
            targetCamera = GetComponent<Camera>();

            if (vinylSelectController == null)
            {
                Debug.LogError(
                    $"{nameof(VinylDragController)} has no VinylSelectController assigned.",
                    this);
            }
        }

        /**
         * Starts, updates, or finishes a drag depending on the current mouse input.
         */
        private void Update()
        {
            Mouse mouse = Mouse.current;
            if (mouse == null || vinylSelectController?.SelectedVinyl == null)
                return;

            if (mouse.leftButton.wasPressedThisFrame)
                TryBeginDrag(mouse.position.ReadValue());

            if (draggedDisc != null && mouse.leftButton.isPressed)
                UpdateDrag(mouse.position.ReadValue());

            if (draggedDisc != null && mouse.leftButton.wasReleasedThisFrame)
                FinishDrag();
        }

        /**
         * Starts dragging only when the selected disc is under the cursor
         * and the current state allows dragging in or out.
         */
        private void TryBeginDrag(Vector2 mousePosition)
        {
            VinylState state = vinylSelectController.CurrentVinylState;
            if (state != VinylState.VinylSelected &&
                state != VinylState.VinylDraggedOutFocused)
            {
                return;
            }

            Transform disc = vinylSelectController.SelectedVinyl.GetVinylDiscTransform();
            if (disc == null || !IsDiscUnderCursor(disc, mousePosition))
                return;

            isDraggingOut = state == VinylState.VinylSelected;

            bool stateChanged = isDraggingOut
                ? vinylSelectController.BeginDragOut()
                : vinylSelectController.BeginDragIn();

            if (!stateChanged)
                return;

            draggedDisc = disc;
            dragStartMousePosition = mousePosition;

            if (isDraggingOut)
            {
                selectedDiscPosition = disc.localPosition;
                focusedDiscPosition =
                    selectedDiscPosition + Vector3.left * focusedDistance;
            }
        }

        /**
         * Converts the horizontal mouse movement into a clamped local disc position.
         */
        private void UpdateDrag(Vector2 mousePosition)
        {
            float horizontalPixels = dragStartMousePosition.x - mousePosition.x;
            float outwardDistance = isDraggingOut
                ? -horizontalPixels * localUnitsPerPixel
                : focusedDistance - horizontalPixels * localUnitsPerPixel;

            outwardDistance = Mathf.Clamp(outwardDistance, 0f, focusedDistance);
            draggedDisc.localPosition =
                selectedDiscPosition + Vector3.left * outwardDistance;
        }

        /**
         * Completes or cancels the drag depending on how far the disc was moved.
         */
        private void FinishDrag()
        {
            float draggedDistance = Vector3.Distance(
                draggedDisc.localPosition,
                selectedDiscPosition);

            float progress = focusedDistance > 0f
                ? draggedDistance / focusedDistance
                : 0f;

            if (isDraggingOut)
            {
                if (progress >= completionThreshold)
                {
                    draggedDisc.localPosition = focusedDiscPosition;
                    vinylSelectController.FinishDragOut();
                }
                else
                {
                    draggedDisc.localPosition = selectedDiscPosition;
                    vinylSelectController.CancelDragOut();
                }
            }
            else
            {
                if (progress <= 1f - completionThreshold)
                {
                    draggedDisc.localPosition = selectedDiscPosition;
                    vinylSelectController.FinishDragIn();
                }
                else
                {
                    draggedDisc.localPosition = focusedDiscPosition;
                    vinylSelectController.CancelDragIn();
                }
            }

            draggedDisc = null;
        }

        /**
         * Returns true if the raycast hits the selected disc or one of its child colliders.
         */
        private bool IsDiscUnderCursor(Transform disc, Vector2 mousePosition)
        {
            Ray ray = targetCamera.ScreenPointToRay(mousePosition);
            RaycastHit[] hits = Physics.RaycastAll(
                ray,
                rayDistance,
                vinylLayer,
                QueryTriggerInteraction.Ignore);

            System.Array.Sort(
                hits,
                (first, second) => first.distance.CompareTo(second.distance));

            foreach (RaycastHit hit in hits)
            {
                Transform hitTransform = hit.collider.transform;
                if (hitTransform == disc || hitTransform.IsChildOf(disc))
                    return true;
            }

            return false;
        }
    }
}
