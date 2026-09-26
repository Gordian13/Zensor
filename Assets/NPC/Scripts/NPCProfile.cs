using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores the reusable settings of an NPC in a Unity asset.
/// NPC prefabs can share a profile instead of keeping these values in their scripts.
/// </summary>
[CreateAssetMenu(fileName = "NPC_Profile_New", menuName = "NPC/Profile")]
public class NPCProfile : ScriptableObject
{
    /// <summary>
    /// Name used in dialogue labels and debug messages.
    /// The default makes an incomplete profile obvious instead of displaying an empty name.
    /// </summary>
    [Header("Identity")]
    [Tooltip("Readable NPC name used in dialogue labels and debug messages.")]
    public string npcName = "Unnamed NPC";

    /// <summary>
    /// Color reserved for hover or selection highlighting.
    /// Yellow is easy to notice, while the default 0.35 alpha keeps the original material visible.
    /// The current scripts in this NPC folder store this value but do not apply it themselves.
    /// </summary>
    [Header("Visual")]
    [Tooltip("Color intended for NPC highlighting. Current NPC scripts store it but do not apply it directly.")]
    public Color hoverColor = new Color(1f, 1f, 0f, 0.35f);

    /// <summary>
    /// Settings available for each NPC mood.
    /// NPCController searches this list for the current mood and uses the first matching entry.
    /// Each mood used by a reaction should normally have an entry here.
    /// </summary>
    [Header("Mood Settings")]
    [Tooltip("Movement, waiting and debug settings for each mood. Add an entry for every mood this NPC can use.")]
    public List<NPCMoodData> moods = new List<NPCMoodData>();

    /// <summary>
    /// Ordered rules for events sent through <see cref="NPCController.ReactTo"/>.
    /// Only the first matching rule runs, so more specific rules should be placed before broad fallback rules.
    /// </summary>
    [Header("Reaction Rules")]
    [Tooltip("Ordered NPC reaction rules. Only the first matching rule runs, so specific rules should come first.")]
    public List<NPCReactionRule> reactionRules = new List<NPCReactionRule>();

    /// <summary>
    /// Reusable actions shown by <see cref="NPCInteractionMenu"/>.
    /// Null entries are skipped. An empty list causes the menu's empty message to be shown.
    /// </summary>
    [Header("Interactions")]
    [Tooltip("Actions shown in the NPC interaction menu. Null entries are ignored.")]
    public List<NPCInteraction> interactions = new List<NPCInteraction>();

    /// <summary>
    /// Dialogue started by a normal player interaction through <see cref="NPCInteractionHandler"/>.
    /// This is separate from the optional automatic welcome dialogue stored on NPCController.
    /// </summary>
    [Header("Dialogue Script")]
    [Tooltip("Dialogue opened by a normal player interaction. The automatic welcome dialogue is configured on NPCController.")]
    public NPCDialogueScript defaultDialogueScript;
}
