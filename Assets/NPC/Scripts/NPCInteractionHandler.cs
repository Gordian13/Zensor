using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
/// Detects clicks on NPCs and starts their dialogue interaction.
/// It also uses <see cref="GlobalInteractionState"/> so other input systems do not react at the same time.
/// </summary>
public class NPCInteractionHandler : MonoBehaviour
{
    /// <summary>The active interaction handler in the scene.</summary>
    public static NPCInteractionHandler Instance { get; private set; }

    /// <summary>
    /// Camera used to turn the pointer position into a ray through the 3D scene.
    /// When empty, <see cref="Camera.main"/> is used during Awake, which requires a camera tagged MainCamera.
    /// </summary>
    [Tooltip("Camera used for click raycasts. When empty, Unity's MainCamera is used.")]
    [SerializeField] private Camera raycastCamera;

    /// <summary>
    /// Physics layers that are allowed to receive the NPC click raycast.
    /// The default value includes every layer for easy setup, but limiting it to NPC layers avoids unrelated colliders being checked first.
    /// </summary>
    [Tooltip("Layers checked by the NPC click raycast. The default includes everything; use an NPC-only layer for clearer results.")]
    [SerializeField] private LayerMask npcLayerMask = ~0;

    /// <summary>
    /// Maximum length of the pointer ray in Unity world units.
    /// The 100 unit default supports large scenes while still rejecting objects far beyond the playable area.
    /// </summary>
    [Tooltip("Maximum click-ray distance in Unity world units.")]
    [SerializeField] private float maxDistance = 100f;

    /// <summary>
    /// Shared destination to which an NPC walks before its dialogue opens.
    /// Its position works together with NPCController.interactionArrivalDistance to keep the desired space from the player.
    /// </summary>
    [Header("Interaction Anchor")]
    [Tooltip("Scene point approached by an NPC before dialogue opens. Required for direct interactions.")]
    [SerializeField] private Transform interactionAnchor;

    /// <summary>
    /// Optional transform the NPC faces after reaching the interaction anchor.
    /// When empty, the NPC faces the anchor itself.
    /// </summary>
    [Tooltip("Optional object the NPC faces after arriving. When empty, the interaction anchor is used.")]
    [SerializeField] private Transform lookAtTarget;

    /// <summary>Registers this handler and finds the main camera if needed.</summary>
    private void Awake()
    {
        Instance = this;

        if (raycastCamera == null)
            raycastCamera = Camera.main;
    }
    /// <summary>Moves an NPC to the interaction point and opens the given dialogue.</summary>
    /// <param name="npc">The NPC that should start the interaction.</param>
    /// <param name="dialogueScript">The dialogue to open after the NPC arrives.</param>
    public void StartInteraction(
        NPCController npc,
        NPCDialogueScript dialogueScript)
    {
        if (npc == null || dialogueScript == null)
            return;

        if (!npc.IsInteractable)
            return;

        if (interactionAnchor == null)
        {
            Debug.LogWarning("Interaction anchor is null.");
            return;
        }

        if (GlobalInteractionState.Instance != null)
            GlobalInteractionState.Instance.BlockInteractions();

        npc.MoveToInteractionAnchor(
            interactionAnchor,
            lookAtTarget,
            () =>
            {
                if (NPCDialogueWindow.Instance != null)
                {
                    NPCDialogueWindow.Instance.ShowDialogueScript(
                        dialogueScript,
                        npc
                    );
                }
                else
                {
                    npc.EndInteraction();
                }
            }
        );
    }

    /// <summary>Starts the default dialogue stored in the NPC profile.</summary>
    /// <param name="npc">The NPC that should start the interaction.</param>
    public void StartInteraction(NPCController npc)
    {
        if (npc == null || npc.Profile == null)
            return;

        StartInteraction(
            npc,
            npc.Profile.defaultDialogueScript
        );
    }

    /// <summary>Checks for a valid click on an NPC each frame.</summary>
    private void Update()
    {
        if (Mouse.current == null || raycastCamera == null)
            return;
        
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (GlobalInteractionState.Instance != null &&
            GlobalInteractionState.Instance.IsInteractionBlocked)
            return;

        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = raycastCamera.ScreenPointToRay(mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit, maxDistance, npcLayerMask))
            return;

        NPCController npc = hit.collider.GetComponentInParent<NPCController>();

        if (npc == null || !npc.IsInteractable)
            return;

        StartInteraction(npc);
    }
}
