public class NPCParsedDialogueChoice
{
    public string playerText;
    public string npcResponse;
    public string nextNodeId;
    public bool endsDialogue;
    public bool opensExternalDialogue;

    public NPCParsedDialogueChoice(
        string playerText,
        string npcResponse,
        string nextNodeId,
        bool endsDialogue,
        bool opensExternalDialogue)
    {
        this.playerText = playerText;
        this.npcResponse = npcResponse;
        this.nextNodeId = nextNodeId;
        this.endsDialogue = endsDialogue;
        this.opensExternalDialogue = opensExternalDialogue;
    }
}