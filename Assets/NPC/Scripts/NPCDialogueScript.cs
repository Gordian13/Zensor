using UnityEngine;

/// <summary>
/// Stores dialogue text as a reusable Unity asset.
/// The <see cref="NPCDialogueParser"/> reads the text when a dialogue starts.
/// </summary>
[CreateAssetMenu(
    fileName = "NPC_DialogueScript",
    menuName = "NPC/Dialogue/Dialogue Script"
)]
public class NPCDialogueScript : ScriptableObject
{
    /// <summary>
    /// Dialogue source read by <see cref="NPCDialogueParser"/>.
    /// A complete conversation normally starts with a ::start node and can contain NPC lines, player choices, links, endings and external links.
    /// The large text area only makes editing easier and does not change parsing.
    /// </summary>
    [TextArea(20, 80)]
    [Tooltip("Dialogue source text. A normal conversation requires a ::start node.")]
    public string dialogueText;

    /// <summary>
    /// Optional second dialogue asset opened by an =&gt; external command.
    /// Leave it empty when the script has no external transition; using such a transition without an asset logs a warning.
    /// </summary>
    [Header("Optional External Link")]
    [Tooltip("Dialogue opened by an => external command. Leave empty when no external transition is used.")]
    public NPCDialogueScript externalDialogue;
}
