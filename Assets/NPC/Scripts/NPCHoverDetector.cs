using UnityEngine;
using UnityEngine.InputSystem;

// Central hover detector for NPCs.
// Instead of every NPC checking the mouse separately, this script raycasts once per frame
// and finds the NPC currently under the mouse cursor.
public class NPCHoverDetector : MonoBehaviour
{
    [Header("Raycast")]
    // Camera used to cast a ray from the mouse position into the 3D world.
    // Usually this should be the main camera.
    [SerializeField] private Camera raycastCamera;

    // Layer mask that decides which objects the hover raycast can hit.
    // ~0 means "Everything" by default.
    [SerializeField] private LayerMask npcLayerMask = ~0;

    // Maximum raycast distance from the camera.
    [SerializeField] private float maxDistance = 100f;

    // The NPC highlight component currently being hovered.
    // Used so we can turn off the previous highlight when the mouse moves away.
    private NPCHoverHighlight currentHovered;

    private void Awake()
    {
        // Auto-fill the camera if it was not assigned manually in the Inspector.
        if (raycastCamera == null)
            raycastCamera = Camera.main;
    }

    private void Update()
    {
        // Mouse.current belongs to Unity's new Input System.
        // If there is no mouse or no camera, we cannot raycast.
        if (Mouse.current == null || raycastCamera == null)
            return;

        // Read the current mouse position in screen coordinates.
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        // Convert the mouse position into a world-space ray.
        Ray ray = raycastCamera.ScreenPointToRay(mousePosition);

        // Cast the ray into the scene and check whether it hits an NPC collider.
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, npcLayerMask))
        {
            // The ray may hit a child object such as InteractionTrigger.
            // GetComponentInParent lets us find NPCHoverHighlight on the parent NPC object.
            NPCHoverHighlight hover = hit.collider.GetComponentInParent<NPCHoverHighlight>();

            // If the hovered NPC changed, remove the old highlight and apply the new one.
            if (hover != currentHovered)
            {
                ClearCurrentHover();

                currentHovered = hover;

                if (currentHovered != null)
                    currentHovered.SetHighlight(true);
            }
        }
        else
        {
            // If the ray hits nothing, make sure the previous NPC is no longer highlighted.
            ClearCurrentHover();
        }
    }

    // Turns off the currently active hover highlight, if there is one.
    private void ClearCurrentHover()
    {
        if (currentHovered != null)
        {
            currentHovered.SetHighlight(false);
            currentHovered = null;
        }
    }
}
