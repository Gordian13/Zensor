using UnityEngine;
using UnityEngine.InputSystem;

// Connects player input with the record metadata UI.
// It checks what the player clicked and tells RecordInfoUI which record data to show.
public class RecordSelectionController : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private RecordInfoUI recordInfoUI;
    [SerializeField] private LayerMask interactableLayers = ~0;
    [SerializeField] private float rayDistance = 100f;
    [SerializeField] private bool hideUIWhenClickMisses = true;

    // Used by the editor setup tool to connect the camera and UI automatically.
    public void Configure(Camera cameraRef, RecordInfoUI infoUI)
    {
        targetCamera = cameraRef;
        recordInfoUI = infoUI;
    }

    private void Awake()
    {
        // If no camera was assigned in the Inspector, use the scene's main camera.
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }

    private void Update()
    {
        var mouse = Mouse.current;
        // Only continue when the player clicks the left mouse button.
        if (mouse == null || !mouse.leftButton.wasPressedThisFrame)
        {
            return;
        }

        // Without a UI reference, there is nowhere to display the selected record data.
        if (recordInfoUI == null)
        {
            Debug.LogWarning("RecordSelectionController: RecordInfoUI reference is missing.", this);
            return;
        }

        // If the click hits a vinyl, get its RecordData and pass it to the UI.
        if (TryGetVinylUnderCursor(mouse.position.ReadValue(), out var vinyl))
        {
            recordInfoUI.ShowData(vinyl.GetData()); // vinyl.GetData() retrieves RecordData, and ShowData(...) displays it in the UI.
            return;
        }

        // Clicking empty space can close the metadata panel.
        if (hideUIWhenClickMisses)
        {
            recordInfoUI.Hide();
        }
    }

    // Looks under the mouse cursor for any object that can provide vinyl record data.
    private bool TryGetVinylUnderCursor(Vector2 screenPosition, out IVinyl vinyl)
    {
        // The out parameter starts empty and is filled only when a vinyl is found.
        vinyl = null;

        if (targetCamera == null)
        {
            return false;
        }

        // Convert the 2D mouse position into a 3D ray that can hit objects in the scene.
        var ray = targetCamera.ScreenPointToRay(screenPosition);
        if (!Physics.Raycast(ray, out var hit, rayDistance, interactableLayers))
        {
            return false;
        }

        // Check the clicked object and its parents for a script that implements IVinyl.
        var components = hit.transform.GetComponentsInParent<MonoBehaviour>(true);
        foreach (var component in components)
        {
            // IVinyl means "this object can provide record data".
            if (component is IVinyl provider)
            {
                vinyl = provider;
                return true;
            }
        }

        return false;
    }
}
