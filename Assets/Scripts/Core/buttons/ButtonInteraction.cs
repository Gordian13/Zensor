using UnityEngine;
using UnityEngine.InputSystem;
using recordPlayer;

public class ButtonInteraction : MonoBehaviour
{
    private Camera _mainCamera;

    [SerializeField] private LayerMask buttonLayer;

    void Start()
    {
        _mainCamera = Camera.main;
    }

    void Update()
    {
        if (_mainCamera == null) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Debug.Log("Mouse clicked!");

            Ray ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, buttonLayer))
            {
                Debug.Log($"Hit: {hit.collider.name}");
                hit.collider.GetComponent<RecordButton>()?.OnPress();
            }
            else
            {
                Debug.Log("Raycast hit nothing!");
            }
        }
    }
}