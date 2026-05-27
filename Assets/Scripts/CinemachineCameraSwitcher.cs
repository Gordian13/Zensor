using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CinemachineCameraSwitcher : MonoBehaviour
{
    [SerializeField] private CinemachineCamera normalCamera;
    [SerializeField] private CinemachineCamera dollyCamera;
    [SerializeField] private int inactivePriority = 0;
    [SerializeField] private int activePriority = 10;
    [SerializeField] private bool startOnDollyCamera;

    private bool isDollyActive;

    private void Awake()
    {
        SetDollyActive(startOnDollyCamera);
    }

    private void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        if (keyboard.cKey.wasPressedThisFrame)
        {
            ToggleCamera();
        }
    }

    public void ToggleCamera()
    {
        SetDollyActive(!isDollyActive);
    }

    public void SwitchToNormalCamera()
    {
        SetDollyActive(false);
    }

    public void SwitchToDollyCamera()
    {
        SetDollyActive(true);
    }

    private void SetDollyActive(bool useDollyCamera)
    {
        isDollyActive = useDollyCamera;

        if (normalCamera != null)
        {
            normalCamera.Priority.Value = isDollyActive ? inactivePriority : activePriority;
        }

        if (dollyCamera != null)
        {
            dollyCamera.Priority.Value = isDollyActive ? activePriority : inactivePriority;
        }
    }
}
