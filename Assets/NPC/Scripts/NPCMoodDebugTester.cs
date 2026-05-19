using UnityEngine;
using UnityEngine.InputSystem;

// Debug-only helper for manually testing NPC reactions with keyboard input.
// This should not be part of final gameplay logic.
// It simply simulates external systems calling npc.ReactTo(context).
public class NPCReactionDebugTester : MonoBehaviour
{
    // NPC that receives the test reaction contexts.
    [SerializeField] private NPCController npc;

    private void Awake()
    {
        // If no NPC was assigned manually, try to find one on the same GameObject.
        if (npc == null)
            npc = GetComponent<NPCController>();

        Debug.Log("NPCReactionDebugTester initialized.");
    }

    private void Update()
    {
        // Uses Unity's new Input System.
        if (Keyboard.current == null)
        {
            Debug.LogWarning("Keyboard.current is NULL.");
            return;
        }

        // If this is null, the tester has no NPC to test.
        if (npc == null)
        {
            Debug.LogWarning("NPC reference is NULL.");
            return;
        }

        // Press 1 to simulate playing a forbidden record.
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            Debug.Log("Pressed 1.");

            // Create the context that would normally come from another gameplay system.
            NPCReactionContext context = new NPCReactionContext(
                NPCReactionEventType.RecordPlayed,
                "record_forbidden_01",
                "Forbidden Record"
            );

            // Send the context to the NPC and receive the result.
            NPCReactionResult result = npc.ReactTo(context);

            // Print result data so rule matching can be debugged.
            Debug.Log($"Reaction happened: {result.hasReaction}");
            Debug.Log($"Blocked: {result.blockOriginalAction}");
            Debug.Log($"Reaction Text: {result.reactionText}");
        }

        // Press 2 to simulate reading a flyer.
        if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            Debug.Log("Pressed 2.");

            // Create another example context.
            NPCReactionContext context = new NPCReactionContext(
                NPCReactionEventType.FlyerRead,
                "flyer_old_event",
                "Old Flyer"
            );

            // Send the context to the NPC and inspect the result.
            NPCReactionResult result = npc.ReactTo(context);

            Debug.Log($"Reaction happened: {result.hasReaction}");
            Debug.Log($"Blocked: {result.blockOriginalAction}");
            Debug.Log($"Reaction Text: {result.reactionText}");
        }
    }
}
