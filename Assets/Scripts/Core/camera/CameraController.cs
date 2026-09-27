using UnityEngine;
using UnityEngine.InputSystem;
using Core.VinylSelect;

namespace camera
{
    /**
     * @brief Provides keyboard movement and right-mouse camera look when enabled.
     *
     * VinylSelected and VinylDraggedOutFocused reserve right-mouse movement for
     * VinylInspectionRotator, so HandleLook skips camera rotation in those states.
     * Keyboard movement is independent of this guard. Assign vinylSelectController
     * or let Start find it in the loaded scene.
     */
    public class CameraController : MonoBehaviour
    {
        /** How fast the camera moves with WASD. */
        public float moveSpeed = 2f;
        /** How fast the camera turns with the mouse. */
        public float lookSpeed = 0.1f;
        /** Used to block looking around while a vinyl is inspected. */
        [SerializeField] private VinylSelectController vinylSelectController;

        /** Current up and down rotation. */
        private float _pitch;
        /** Current left and right rotation. */
        private float _yaw;

        /** Saves the start rotation and searches the VinylSelectController if it is not set. */
        void Start()
        {
            _pitch = transform.eulerAngles.x;
            _yaw = transform.eulerAngles.y;

            if (vinylSelectController == null)
                vinylSelectController = FindFirstObjectByType<VinylSelectController>();
        }

        /** Moves and turns the camera every frame. */
        void Update()
        {
            HandleMovement();
            HandleLook();
        }

        /** Moves the camera with WASD. */
        private void HandleMovement()
        {
            var keyboard = Keyboard.current;
            Vector3 dir = Vector3.zero;

            if (keyboard.wKey.isPressed) dir += transform.forward;
            if (keyboard.sKey.isPressed) dir -= transform.forward;
            if (keyboard.aKey.isPressed) dir -= transform.right;
            if (keyboard.dKey.isPressed) dir += transform.right;

            transform.position += dir * (moveSpeed * Time.deltaTime);
        }

        /** Turns the camera while the right mouse button is pressed, but not while a vinyl is inspected. */
        private void HandleLook()
        {
            var mouse = Mouse.current;
            if (mouse == null || !mouse.rightButton.isPressed) return;
            if (IsVinylInspectionRotationState()) return;

            Vector2 delta = mouse.delta.ReadValue();
            _yaw += delta.x * lookSpeed;
            _pitch -= delta.y * lookSpeed;
            _pitch = Mathf.Clamp(_pitch, -89f, 89f);

            transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
        }

        /**
         * @brief Checks whether the inspection feature owns the right-mouse gesture.
         * @return True for cover or focused-disc inspection; false without a controller.
         */
        private bool IsVinylInspectionRotationState()
        {
            if (vinylSelectController == null)
                return false;

            VinylState state = vinylSelectController.CurrentVinylState;
            return state == VinylState.VinylSelected ||
                   state == VinylState.VinylDraggedOutFocused;
        }
    }
}
