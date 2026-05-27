using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class DriveDolly : MonoBehaviour
{
    [SerializeField] private CinemachineSplineDolly dolly;
    [SerializeField] private float speed = 1f;
    [SerializeField] private float smoothTime = 0.2f;
    [SerializeField] private bool enableDrag = true;
    [SerializeField] private float dragPanSpeed = 0.01f;
    [SerializeField] private bool invertDragPan;
    [SerializeField] private bool enableRightClickLook = true;
    [SerializeField] private float lookSensitivity = 0.1f;
    [SerializeField] private Vector2 pitchLimits = new Vector2(-80f, 80f);
    [SerializeField] private Transform rotationTarget;

    private float currentInput;
    private float inputVelocity;
    private float pitch;
    private float yaw;
    private DollyRightClickLookExtension lookExtension;

    private void Awake()
    {
        if (dolly == null)
        {
            dolly = GetComponent<CinemachineSplineDolly>();
        }

        if (rotationTarget == null && dolly != null)
        {
            rotationTarget = dolly.transform;
        }

        if (rotationTarget != null)
        {
            Vector3 euler = rotationTarget.eulerAngles;
            pitch = NormalizeAngle(euler.x);
            yaw = euler.y;
        }

        if (dolly != null)
        {
            lookExtension = dolly.GetComponent<DollyRightClickLookExtension>();
            if (lookExtension == null)
            {
                lookExtension = dolly.gameObject.AddComponent<DollyRightClickLookExtension>();
            }

            lookExtension.SetRotation(pitch, yaw);
        }
    }

    private void Update()
    {
        if (dolly == null)
        {
            Debug.LogError($"{nameof(DriveDolly)} needs a {nameof(CinemachineSplineDolly)} reference.", this);
            enabled = false;
            return;
        }

        float targetInput = 0f;
        var kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) targetInput += 1f;
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed)  targetInput -= 1f;
        }

        currentInput = Mathf.SmoothDamp(currentInput, targetInput, ref inputVelocity, smoothTime);
        dolly.CameraPosition += currentInput * speed * Time.deltaTime;

        if (enableDrag)
        {
            ApplyMouseDrag();
        }

        if (enableRightClickLook)
        {
            ApplyRightClickLook();
        }
    }

    private void ApplyMouseDrag()
    {
        var mouse = Mouse.current;
        if (mouse == null || !mouse.leftButton.isPressed)
        {
            return;
        }

        Vector2 mouseDelta = mouse.delta.ReadValue();
        float dragDirection = invertDragPan ? -1f : 1f;
        Vector3 panOffset = new Vector3(-mouseDelta.x, -mouseDelta.y, 0f) * (dragPanSpeed * dragDirection);
        dolly.SplineOffset += panOffset;
    }

    private void ApplyRightClickLook()
    {
        var mouse = Mouse.current;
        if (mouse == null || rotationTarget == null || !mouse.rightButton.isPressed)
        {
            return;
        }

        Vector2 mouseDelta = mouse.delta.ReadValue();
        yaw += mouseDelta.x * lookSensitivity;
        pitch -= mouseDelta.y * lookSensitivity;
        pitch = Mathf.Clamp(pitch, pitchLimits.x, pitchLimits.y);

        if (lookExtension != null)
        {
            lookExtension.SetRotation(pitch, yaw);
        }

        rotationTarget.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    private static float NormalizeAngle(float angle)
    {
        return angle > 180f ? angle - 360f : angle;
    }
}

public class DollyRightClickLookExtension : CinemachineExtension
{
    private float pitch;
    private float yaw;

    public void SetRotation(float newPitch, float newYaw)
    {
        pitch = newPitch;
        yaw = newYaw;
    }

    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage,
        ref CameraState state,
        float deltaTime)
    {
        if (stage != CinemachineCore.Stage.Finalize)
        {
            return;
        }

        state.RawOrientation = Quaternion.Euler(pitch, yaw, 0f);
        state.OrientationCorrection = Quaternion.identity;
        state.ReferenceUp = state.RawOrientation * Vector3.up;
    }
}
