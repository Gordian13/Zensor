/// <summary>
/// Stores one answer the player can select and the NPC's following response.
/// The <see cref="NPCDialogueParser"/> reads these values from a dialogue text.
/// </summary>
public class NPCParsedDialogueChoice
{
    /// <summary>The answer shown to the player.</summary>
    public string playerText;
    /// <summary>The NPC's response.</summary>
    public string npcResponse;
    /// <summary>The ID of the next dialogue node.</summary>
    public string nextNodeId;
    /// <summary>Whether the dialogue ends after this choice.</summary>
    public bool endsDialogue;
    /// <summary>Whether a linked dialogue is opened.</summary>
    public bool opensExternalDialogue;

    /// <summary>Creates one parsed dialogue choice.</summary>
    /// <param name="playerText">The answer shown to the player.</param>
    /// <param name="npcResponse">The NPC's response.</param>
    /// <param name="nextNodeId">The ID of the next node.</param>
    /// <param name="endsDialogue">Whether the dialogue ends.</param>
    /// <param name="opensExternalDialogue">Whether a linked dialogue starts.</param>
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
