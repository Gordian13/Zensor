using UnityEngine;
using UnityEngine.InputSystem;

namespace camera
{
    public class CameraController : MonoBehaviour
    {
        public float moveSpeed = 2f;
        public float lookSpeed = 0.1f;

        private float _pitch;
        private float _yaw;

        void Start()
        {
            _pitch = transform.eulerAngles.x;
            _yaw = transform.eulerAngles.y;
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
            if (!mouse.rightButton.isPressed) return;

            Vector2 delta = mouse.delta.ReadValue();
            _yaw += delta.x * lookSpeed;
            _pitch -= delta.y * lookSpeed;
            _pitch = Mathf.Clamp(_pitch, -89f, 89f);

            transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
        }
    }
}
