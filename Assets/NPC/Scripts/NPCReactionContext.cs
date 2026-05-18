public class NPCReactionContext
{
    public NPCReactionEventType eventType;
    public string objectId;
    public string objectDisplayName;

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