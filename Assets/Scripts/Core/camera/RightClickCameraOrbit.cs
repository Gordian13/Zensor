using UnityEngine;
using UnityEngine.InputSystem;

/*
 * Adds right-click pitch/yaw offsets to a Cinemachine camera.
 */
public class RightClickCameraOrbit : MonoBehaviour
{
    [SerializeField] private float sensitivity = 0.15f;
    [SerializeField] private float minPitch = -40f;
    [SerializeField] private float maxPitch = 70f;

    private float pitch;
    private float yaw;
    private float roll;
    private float initialPitch;
    private float initialYaw;
    private float initialRoll;

    private void Awake()
    {
        Vector3 initialEuler = transform.eulerAngles;
        initialPitch = Mathf.Clamp(NormalizeAngle(initialEuler.x), minPitch, maxPitch);
        initialYaw = NormalizeAngle(initialEuler.y);
        initialRoll = NormalizeAngle(initialEuler.z);
        ResetLook();
    }

    private void Update()
    {
        Mouse mouse = Mouse.current;

        if (mouse == null || !mouse.rightButton.isPressed)
            return;

        Vector2 delta = mouse.delta.ReadValue();

        yaw += delta.x * sensitivity;
        pitch -= delta.y * sensitivity;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        transform.rotation = Quaternion.Euler(pitch, yaw, roll);
    }

    public void ResetLook()
    {
        pitch = initialPitch;
        yaw = initialYaw;
        roll = initialRoll;
        transform.rotation = Quaternion.Euler(pitch, yaw, roll);
    }

    private static float NormalizeAngle(float angle)
    {
        return Mathf.Repeat(angle + 180f, 360f) - 180f;
    }
}
