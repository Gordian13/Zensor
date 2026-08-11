using System.Collections.Generic;
using UnityEngine;

// ScriptableObject containing reusable NPC configuration data.
// This asset lets multiple NPC prefabs share or customize the same setup without hardcoding values.
[CreateAssetMenu(fileName = "NPC_Profile_New", menuName = "NPC/Profile")]
public class NPCProfile : ScriptableObject
{
    [Header("Identity")]
    // Display/debug name of the NPC.
    public string npcName = "Unnamed NPC";

    [Header("Visual")]
    // Color used when the NPC is hovered.
    // Alpha controls highlight strength because NPCHoverHighlight uses it as the blend value.
    public Color hoverColor = new Color(1f, 1f, 0f, 0.35f);

    [Header("Mood Settings")]
    // List of mood configurations.
    // NPCController looks up the entry matching its current mood.
    public List<NPCMoodData> moods = new List<NPCMoodData>();

    [Header("Reaction Rules")]
    // Rules that define how this NPC reacts to external events.
    // Example: RecordPlayed + objectId "record_forbidden_01" -> Raged + block action.
    public List<NPCReactionRule> reactionRules = new List<NPCReactionRule>();

    [Header("Interactions")]
    // Interactions shown in the right-click NPC interaction menu.
    // Each entry is a ScriptableObject derived from NPCInteraction.
    public List<NPCInteraction> interactions = new List<NPCInteraction>();

    [Header("Dialogue Script")]
    public NPCDialogueScript defaultDialogueScript;
}
