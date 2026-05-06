using UnityEngine;
using UnityEngine.InputSystem;

public class RecordSelectionController : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private RecordInfoUI recordInfoUI;
    [SerializeField] private LayerMask interactableLayers = ~0;
    [SerializeField] private float rayDistance = 100f;
    [SerializeField] private bool hideUIWhenClickMisses = true;

    public void Configure(Camera cameraRef, RecordInfoUI infoUI)
    {
        targetCamera = cameraRef;
        recordInfoUI = infoUI;
    }

    private void Awake()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }

    private void Update()
    {
        var mouse = Mouse.current;
        if (mouse == null || !mouse.leftButton.wasPressedThisFrame)
        {
            return;
        }

        if (recordInfoUI == null)
        {
            Debug.LogWarning("RecordSelectionController: RecordInfoUI reference is missing.", this);
            return;
        }

        if (TryGetVinylUnderCursor(mouse.position.ReadValue(), out var vinyl))
        {
            recordInfoUI.ShowData(vinyl.GetData());  // vinyl.GetData() holt RecordData und ShowData(...) zeigt es im UI.
            return;
        }

        if (hideUIWhenClickMisses)
        {
            recordInfoUI.Hide();
        }
    }

    private bool TryGetVinylUnderCursor(Vector2 screenPosition, out IVinyl vinyl)
    {
        vinyl = null;

        if (targetCamera == null)
        {
            return false;
        }

        var ray = targetCamera.ScreenPointToRay(screenPosition);
        if (!Physics.Raycast(ray, out var hit, rayDistance, interactableLayers))
        {
            return false;
        }

        var components = hit.transform.GetComponentsInParent<MonoBehaviour>(true);
        foreach (var component in components)
        {
            if (component is IVinyl provider)
            {
                vinyl = provider;
                return true;
            }
        }

        return false;
    }
}
