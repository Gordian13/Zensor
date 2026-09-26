using UnityEngine;

/// <summary>
/// Base class for reusable NPC interaction assets.
/// Classes that inherit from it decide what happens when the option is selected.
/// </summary>
public abstract class NPCInteraction : ScriptableObject
{
    /// <summary>
    /// Label written onto the generated interaction-menu button.
    /// The default keeps a newly created asset visible before a custom label is entered.
    /// </summary>
    [Header("Interaction")]
    [Tooltip("Label shown on the generated interaction-menu button.")]
    public string interactionName = "Interaction";

    /// <summary>Runs this interaction for the given NPC.</summary>
    /// <param name="npc">The NPC the player is interacting with.</param>
    public abstract void Execute(NPCController npc);
}
