using UnityEngine;

[System.Serializable]
public class NPCReactionRule
{
    [Header("Condition")]
    public NPCReactionEventType eventType;

    [Tooltip("Leave empty to match any object.")]
    public string requiredObjectId;

    [Header("Optional Int Condition")]
    public bool useIntModulus;
    public bool useMinimumIntValue;
    public int minimumIntValue;

    [Header("Optional Float Condition")]
    public bool useFloatModulus;
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

        if (!string.IsNullOrWhiteSpace(requiredObjectId) &&
            requiredObjectId != context.objectId)
        {
            return false;
        }

        if(useIntModulus)
        {
            if (useMinimumIntValue &&
                ((context.intValue % minimumIntValue != 0)))
            {
                return false;
            }
        }
        else
            if (useMinimumIntValue &&
                (context.intValue < minimumIntValue))
            {
                return false;
            }

        if(useFloatModulus)
        {
            if (useMinimumFloatValue &&
                ((context.floatValue % minimumFloatValue != 0)))
            {
                return false;
            }
        }
        else
            if (useMinimumFloatValue &&
                (context.floatValue < minimumFloatValue))
            {
                return false;
            }

        return true;
    }
}