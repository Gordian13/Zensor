using System.Collections.Generic;

public class NPCParsedDialogueNode
{
    public string nodeId;
    public string npcLine;
    public List<NPCParsedDialogueChoice> choices = new List<NPCParsedDialogueChoice>();

    public NPCParsedDialogueNode(string nodeId)
    {
        this.nodeId = nodeId;
    }
}