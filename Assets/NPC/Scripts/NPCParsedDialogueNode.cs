using System.Collections.Generic;

/// <summary>
/// Represents one parsed section of a dialogue.
/// A node contains NPC text, player choices and the next step.
/// </summary>
public class NPCParsedDialogueNode
{
    /// <summary>The unique ID of this node.</summary>
    public string nodeId;
    /// <summary>The text the NPC says at the start of this node.</summary>
    public string npcLine;

    /// <summary>The answers the player can select.</summary>
    public List<NPCParsedDialogueChoice> choices =
        new List<NPCParsedDialogueChoice>();

    /// <summary>An optional next node that does not need a player choice.</summary>
    public string nextNodeId;
    /// <summary>Whether the dialogue ends at this node.</summary>
    public bool endsDialogue;
    /// <summary>Whether a linked dialogue is opened.</summary>
    public bool opensExternalDialogue;

    /// <summary>Creates a parsed dialogue node.</summary>
    /// <param name="nodeId">The unique ID of the node.</param>
    public NPCParsedDialogueNode(string nodeId)
    {
        this.nodeId = nodeId;
    }
}
