using UnityEngine;

[System.Serializable]
public class NPCReactionRule
{
    [Header("Condition")]
    public NPCReactionEventType eventType;

    //requiredObjectId leer = reacts to all objects of this event type
    //requiredObjectId gesetzt = only reacts to specific object
    [Tooltip("Leave empty to match any object.")]
    public string requiredObjectId;

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

        return true;
    }
}