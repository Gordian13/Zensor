using UnityEngine;
using UnityEngine.InputSystem;

// Detects right-clicks on NPCs and opens the NPC interaction menu.
// This script should usually live on a central scene object, for example "NPC_InputSystem".
public class NPCInteractionHandler : MonoBehaviour
{
    // Camera used to raycast from the mouse into the 3D world.
    [SerializeField] private Camera raycastCamera;

    // Defines which layers can be hit by the interaction raycast.
    // Default ~0 means Everything.
    [SerializeField] private LayerMask npcLayerMask = ~0;

    // Maximum distance the mouse raycast can reach.
    [SerializeField] private float maxDistance = 100f;

    [Header("Interaction Anchor")]
    [SerializeField] private Transform interactionAnchor;
    [SerializeField] private Transform lookAtTarget;

    private void Awake()
    {
        // Automatically use the main camera if none was assigned.
        if (raycastCamera == null)
            raycastCamera = Camera.main;
    }

    private void Update()
    {
        // This script uses Unity's new Input System.
        // If there is no mouse or no camera, interaction detection cannot run.
        if (Mouse.current == null || raycastCamera == null)
            return;

        // Only react on the exact frame the right mouse button is pressed.
        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        // Convert current mouse position into a ray from the camera.
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = raycastCamera.ScreenPointToRay(mousePosition);

        // If the ray does not hit anything interactable, stop here.
        if (!Physics.Raycast(ray, out RaycastHit hit, maxDistance, npcLayerMask))
            return;

        // The collider may be on a child object such as InteractionTrigger.
        // GetComponentInParent finds the NPCController on the parent NPC object.
        NPCController npc = hit.collider.GetComponentInParent<NPCController>();

        // If the clicked object is not part of an NPC, ignore it.
        if (npc == null)
            return;

        if(!npc.IsInteractable)
        {
            return;
        }

        // Open the interaction menu for the clicked NPC.
        // Requires NPCInteractionMenu to exist in the scene.
        if (NPCInteractionMenu.Instance == null)
        {
            Debug.LogError("NPCInteractionMenu.Instance is NULL.");
            return;
        }

        npc.MoveToInteractionAnchor(interactionAnchor, lookAtTarget, () =>
        {
            if (NPCDialogueWindow.Instance != null)
                NPCDialogueWindow.Instance.ShowDialogueScript(npc.Profile.defaultDialogueScript, npc);
        });
    }
}
