using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NPC_Profile_New", menuName = "NPC/Profile")]
public class NPCProfile : ScriptableObject
{
    [Header("Identity")]
    public string npcName = "Unnamed NPC";

    [Header("Visual")]
    public Color hoverColor = new Color(1f, 1f, 0f, 0.35f);

    [Header("Mood Settings")]
    public List<NPCMoodData> moods = new List<NPCMoodData>();

    [Header("Reaction Rules")]
    public List<NPCReactionRule> reactionRules = new List<NPCReactionRule>();

    [Header("Interactions")]
    public List<NPCInteraction> interactions = new List<NPCInteraction>();

    [Header("Dialogue")]
    [TextArea]
    public string defaultSpeechBubbleText;
}