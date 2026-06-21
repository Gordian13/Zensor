using UnityEngine;

// Abstract base class for all NPC interactions.
// Interactions are ScriptableObjects so designers/developers can create reusable interaction assets.
//
// Example subclasses:
// - NPCTalkInteraction
// - NPCQuestionInteraction
// - NPCTradeInteraction
// - NPCCustomActionInteraction
public abstract class NPCInteraction : ScriptableObject
{
    [Header("Interaction")]
    // Name shown in the interaction menu button.
    public string interactionName = "Interaction";

    // Every concrete interaction must define what happens when it is executed.
    // The NPC is passed in so the interaction can call npc.Say(), npc.SetMood(), etc.
    public abstract void Execute(NPCController npc);
}
