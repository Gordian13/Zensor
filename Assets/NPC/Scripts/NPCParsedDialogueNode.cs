using System.Collections.Generic;

public class NPCParsedDialogueNode
{
    public string nodeId;
    public string npcLine;

    public List<NPCParsedDialogueChoice> choices =
        new List<NPCParsedDialogueChoice>();

    // Optionaler automatischer Übergang des gesamten Nodes.
    public string nextNodeId;
    public bool endsDialogue;
    public bool opensExternalDialogue;

    public NPCParsedDialogueNode(string nodeId)
    {
        this.nodeId = nodeId;
    }
}