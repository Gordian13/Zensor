using UnityEngine;
using UnityEngine.InputSystem;

public class NPCReactionDebugTester : MonoBehaviour
{
    [SerializeField] private NPCController npc;

    private void Awake()
    {
        if (npc == null)
            npc = GetComponent<NPCController>();

        Debug.Log("NPCReactionDebugTester initialized.");
    }

    private void Update()
    {
        if (Keyboard.current == null)
        {
            Debug.LogWarning("Keyboard.current is NULL.");
            return;
        }

        if (npc == null)
        {
            Debug.LogWarning("NPC reference is NULL.");
            return;
        }

        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            Debug.Log("Pressed 1.");

            NPCReactionContext context = new NPCReactionContext(
                NPCReactionEventType.RecordPlayed,
                "record_forbidden_01",
                "Forbidden Record"
            );

            NPCReactionResult result = npc.ReactTo(context);

            Debug.Log($"Reaction happened: {result.hasReaction}");
            Debug.Log($"Blocked: {result.blockOriginalAction}");
            Debug.Log($"Reaction Text: {result.reactionText}");
        }

        if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            Debug.Log("Pressed 2.");

            NPCReactionContext context = new NPCReactionContext(
                NPCReactionEventType.FlyerRead,
                "flyer_old_event",
                "Old Flyer"
            );

            NPCReactionResult result = npc.ReactTo(context);

            Debug.Log($"Reaction happened: {result.hasReaction}");
            Debug.Log($"Blocked: {result.blockOriginalAction}");
            Debug.Log($"Reaction Text: {result.reactionText}");
        }
    }
}