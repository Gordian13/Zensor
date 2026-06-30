using UnityEngine;

[System.Serializable]
public class NPCReactionRule
{
    [Header("Condition")]
    public NPCReactionEventType eventType;

    [Tooltip("Leave empty to match any object.")]
    public string requiredObjectId;

    [Header("Optional Numeric Condition")]
    public bool useMinimumIntValue;
    public int minimumIntValue;

    public bool useMinimumFloatValue;
    public float minimumFloatValue;

    [Header("Reaction")]
    public bool changeMood;
    public NPCMood resultingMood;

    [TextArea]
    public string reactionText;

    public bool blockOriginalAction;

    public bool Matches(NPCReactionContext context)
    {
        if (context == null)
            return false;

        if (eventType != context.eventType)
            return false;

        if (!string.IsNullOrWhiteSpace(requiredObjectId))
        {
            if (requiredObjectId != context.objectId)
                return false;
        }

        if (useMinimumIntValue && context.intValue < minimumIntValue)
            return false;

        if (useMinimumFloatValue && context.floatValue < minimumFloatValue)
            return false;

        return true;
    }
}