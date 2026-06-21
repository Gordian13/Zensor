using UnityEngine;

// Defines one rule for how an NPC reacts to a specific kind of event.
// Stored inside NPCProfile.
[System.Serializable]
public class NPCReactionRule
{
    [Header("Condition")]
    // Event type this rule listens for.
    // Example: RecordPlayed, FlyerRead, CabinetOpened.
    public NPCReactionEventType eventType;

    // Optional object ID filter.
    //
    // If empty:
    // this rule reacts to every object with the matching event type.
    //
    // If set:
    // this rule only reacts when context.objectId exactly matches this value.
    [Tooltip("Leave empty to match any object.")]
    public string requiredObjectId;

    [Header("Reaction")]
    // If true, this rule changes the NPC's mood when it matches.
    public bool changeMood;

    // Mood applied when changeMood is true.
    public NPCMood resultingMood;

    // Text the NPC says when this rule matches.
    [TextArea]
    public string reactionText;

    // If true, the caller should block the original action.
    // Example: player tries to play a forbidden record -> NPC reacts -> record should not play.
    public bool blockOriginalAction;

    // Checks whether this rule applies to the given reaction context.
    public bool Matches(NPCReactionContext context)
    {
        // Null context can never match.
        if (context == null)
            return false;

        // Event type must match exactly.
        if (eventType != context.eventType)
            return false;

        // If this rule has a required object ID, the context must match it exactly.
        if (!string.IsNullOrWhiteSpace(requiredObjectId))
        {
            if (requiredObjectId != context.objectId)
                return false;
        }

        // All checks passed: this rule applies.
        return true;
    }
}
