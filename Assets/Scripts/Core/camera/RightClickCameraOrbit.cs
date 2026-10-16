using UnityEngine;
using UnityEngine.InputSystem;

/**
 * Adds right-click pitch/yaw offsets to a Cinemachine camera.
 */
public class RightClickCameraOrbit : MonoBehaviour
{
    /** How fast the camera turns when the mouse moves. */
    [SerializeField] private float sensitivity = 0.15f;
    /** How far the camera can look up (in degrees, negative is up). */
    [SerializeField] private float minPitch = -40f;
    /** How far the camera can look down (in degrees). */
    [SerializeField] private float maxPitch = 70f;

    /** Current rotation of the camera. */
    private float pitch;
    private float yaw;
    private float roll;
    /** Start rotation of the camera, used by ResetLook. */
    private float initialPitch;
    private float initialYaw;
    private float initialRoll;

    /** Saves the start rotation of the camera. */
    private void Awake()
    {
        Vector3 initialEuler = transform.eulerAngles;
        initialPitch = Mathf.Clamp(NormalizeAngle(initialEuler.x), minPitch, maxPitch);
        initialYaw = NormalizeAngle(initialEuler.y);
        initialRoll = NormalizeAngle(initialEuler.z);
        ResetLook();
    }

    /** Turns the camera while the right mouse button is pressed. */
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

    /** Turns the camera back to its start rotation. */
    public void ResetLook()
    {
        pitch = initialPitch;
        yaw = initialYaw;
        roll = initialRoll;
        transform.rotation = Quaternion.Euler(pitch, yaw, roll);
    }

    /**
     * Changes an angle to a value between -180 and 180.
     *
     * @param angle The angle in degrees.
     * @return The same angle between -180 and 180.
     */
    private static float NormalizeAngle(float angle)
    {
        return Mathf.Repeat(angle + 180f, 360f) - 180f;
    }
}
