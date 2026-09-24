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
        public float moveSpeed = 2f;
        public float lookSpeed = 0.1f;
        [SerializeField] private VinylSelectController vinylSelectController;

        private float _pitch;
        private float _yaw;

        void Start()
        {
            _pitch = transform.eulerAngles.x;
            _yaw = transform.eulerAngles.y;

            if (vinylSelectController == null)
                vinylSelectController = FindFirstObjectByType<VinylSelectController>();
        }

        void Update()
        {
            HandleMovement();
            HandleLook();
        }

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
