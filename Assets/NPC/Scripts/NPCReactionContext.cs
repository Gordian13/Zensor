/// <summary>
/// Collects the details of an event that an NPC should react to.
/// Other gameplay systems can create it and pass it to <see cref="NPCController.ReactTo"/>.
/// </summary>
public class NPCReactionContext
{
    /// <summary>
    /// Type of event sent to the NPC.
    /// Every reaction rule compares this value before checking any other field.
    /// </summary>
    public NPCReactionEventType eventType;

    /// <summary>
    /// Technical ID of the object or source that caused the event.
    /// It is compared exactly with NPCReactionRule.requiredObjectId when that rule field is not empty.
    /// </summary>
    public string objectId;

    /// <summary>
    /// Readable name of the object or source.
    /// It can be used for UI or logging but the current reaction-rule matcher does not check it.
    /// </summary>
    public string objectDisplayName;

    /// <summary>
    /// Optional whole number belonging to the event, for example a counter value.
    /// The constructor default of zero means callers that do not need numeric matching can omit it.
    /// </summary>
    public int intValue;

    /// <summary>
    /// Optional decimal number belonging to the event.
    /// The constructor default of zero means callers that do not need decimal matching can omit it.
    /// </summary>
    public float floatValue;

    /// <summary>Creates the information for an NPC event.</summary>
    /// <param name="eventType">The type of event.</param>
    /// <param name="objectId">The technical object ID.</param>
    /// <param name="objectDisplayName">The readable object name.</param>
    /// <param name="intValue">An optional whole number.</param>
    /// <param name="floatValue">An optional decimal number.</param>
    public NPCReactionContext(
        NPCReactionEventType eventType,
        string objectId = "",
        string objectDisplayName = "",
        int intValue = 0,
        float floatValue = 0f)
    {
        this.eventType = eventType;
        this.objectId = objectId;
        this.objectDisplayName = objectDisplayName;
        this.intValue = intValue;
        this.floatValue = floatValue;
    }
}
