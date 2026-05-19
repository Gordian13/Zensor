// Data object describing something the NPC may react to.
// External systems create this and pass it to NPCController.ReactTo(context).
//
// Example:
// eventType = RecordPlayed
// objectId = "record_forbidden_01"
// objectDisplayName = "Forbidden Record"
public class NPCReactionContext
{
    // Type of event that happened.
    public NPCReactionEventType eventType;

    // Stable identifier for the object involved in the event.
    // This should be a technical ID, not a display name.
    // Example: "record_forbidden_01"
    public string objectId;

    // Human-readable name of the object.
    // Mainly useful for debugging or future dialogue text.
    public string objectDisplayName;

    // Constructor used to create a complete reaction context.
    public NPCReactionContext(
        NPCReactionEventType eventType,
        string objectId,
        string objectDisplayName = "")
    {
        this.eventType = eventType;
        this.objectId = objectId;
        this.objectDisplayName = objectDisplayName;
    }
}
