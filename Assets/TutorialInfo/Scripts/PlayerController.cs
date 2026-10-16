using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float mouseSensitivity = 2f;

    private float _rotationX = 0f;
    private Transform _playerBody;
    private void Start()
    {
        _playerBody = transform.parent;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    private void Update()
    {
        // Mouse look up/down (camera only)
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        _rotationX -= mouseY;
        _rotationX = Mathf.Clamp(_rotationX, -90f, 90f);
        transform.localRotation = Quaternion.Euler(_rotationX, 0f, 0f);

        // Mouse look left/right (whole player body)
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        _playerBody.Rotate(Vector3.up * mouseX);

        // WASD movement
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = _playerBody.right * moveX + _playerBody.forward * moveZ;
        _playerBody.position += move * moveSpeed * Time.deltaTime;
    }

    
}