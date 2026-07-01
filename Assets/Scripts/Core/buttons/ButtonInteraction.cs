using UnityEngine;
using UnityEngine.InputSystem;
using recordPlayer;

public class ButtonInteraction : MonoBehaviour
{
    private Camera _interactionCamera;

    [SerializeField] private LayerMask buttonLayer;

    private void Update()
    {
        if (_interactionCamera == null)
            _interactionCamera = Camera.main;

        if (_interactionCamera == null || Mouse.current == null)
            return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = _interactionCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity, buttonLayer);
            foreach (RaycastHit hit in hits)
            {
                RecordButton button = hit.collider.GetComponentInParent<RecordButton>();
                if (button != null)
                {
                    button.OnPress();
                    break;
                }
            }
        }
    }
}
