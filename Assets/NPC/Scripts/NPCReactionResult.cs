// Result returned by NPCController.ReactTo(context).
// This tells the calling system whether the NPC reacted and whether the original action should continue.
public class NPCReactionResult
{
    // True if any NPCReactionRule matched the given context.
    public bool hasReaction;

    // If true, the external system should block its original action.
    // Example: do not play a forbidden record.
    public bool blockOriginalAction;

    // Text the NPC said as part of the reaction.
    // Empty if the reaction had no dialogue.
    public string reactionText;

    // Creates a reaction result with explicit values.
    public NPCReactionResult(bool hasReaction, bool blockOriginalAction, string reactionText)
    {
        this.hasReaction = hasReaction;
        this.blockOriginalAction = blockOriginalAction;
        this.reactionText = reactionText;
    }

    // Convenience factory for the common "nothing happened" case.
    public static NPCReactionResult NoReaction()
    {
        return new NPCReactionResult(false, false, "");
    }
}
