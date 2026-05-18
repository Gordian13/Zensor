public class NPCReactionResult
{
    public bool hasReaction;
    public bool blockOriginalAction;
    public string reactionText;

    public NPCReactionResult(bool hasReaction, bool blockOriginalAction, string reactionText)
    {
        this.hasReaction = hasReaction;
        this.blockOriginalAction = blockOriginalAction;
        this.reactionText = reactionText;
    }

    public static NPCReactionResult NoReaction()
    {
        return new NPCReactionResult(false, false, "");
    }
}