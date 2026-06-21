using UnityEngine;
using UnityEngine.InputSystem;
using recordPlayer;

public class ButtonInteraction : MonoBehaviour
{
    private Camera _interactionCamera;

    [SerializeField] private LayerMask buttonLayer;

    private void Awake()
    {
        _interactionCamera = GetComponent<Camera>();

        if (_interactionCamera == null)
            _interactionCamera = Camera.main;

        if (_interactionCamera == null)
            Debug.LogError($"{nameof(ButtonInteraction)} on {name} could not find a camera.", this);
    }

    private void Update()
    {
        if (_interactionCamera == null || Mouse.current == null)
            return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = _interactionCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, buttonLayer))
            {
                hit.collider.GetComponentInParent<RecordButton>()?.OnPress();
            }
        }
    }
}
