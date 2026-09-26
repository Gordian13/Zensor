/// <summary>
/// Contains the result of an NPC reaction.
/// It also tells the calling system whether its own action may continue.
/// </summary>
public class NPCReactionResult
{
    /// <summary>
    /// Whether any reaction rule matched the supplied context.
    /// A matching silent rule still sets this to true.
    /// </summary>
    public bool hasReaction;

    /// <summary>
    /// Whether the calling system should stop its original action.
    /// This is only information; NPCController cannot cancel work owned by another system.
    /// </summary>
    public bool blockOriginalAction;

    /// <summary>
    /// Text configured by the matching rule.
    /// It is an empty string in the result returned by <see cref="NoReaction"/>.
    /// </summary>
    public string reactionText;

    /// <summary>Creates a result with the given values.</summary>
    /// <param name="hasReaction">Whether a reaction happened.</param>
    /// <param name="blockOriginalAction">Whether the original action should stop.</param>
    /// <param name="reactionText">The text spoken by the NPC.</param>
    public NPCReactionResult(bool hasReaction, bool blockOriginalAction, string reactionText)
    {
        this.hasReaction = hasReaction;
        this.blockOriginalAction = blockOriginalAction;
        this.reactionText = reactionText;
    }

    /// <summary>Creates a result for the case where nothing happened.</summary>
    /// <returns>A result without a reaction.</returns>
    public static NPCReactionResult NoReaction()
    {
        return new NPCReactionResult(false, false, "");
    }
}
