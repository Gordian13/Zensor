public class NPCReactionContext
{
    public NPCReactionEventType eventType;
    public string objectId;
    public string objectDisplayName;

    public int intValue;
    public float floatValue;

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