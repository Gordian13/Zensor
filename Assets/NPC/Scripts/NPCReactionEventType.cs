// Event categories that an NPC can react to.
// These are NPC-facing events, not full museum object implementations.
// Other systems can use these values when creating an NPCReactionContext.
public enum NPCReactionEventType
{
    // No event / default value.
    None,

    // Generic object click.
    ObjectClicked,

    // Player read a flyer.
    FlyerRead,

    // Player selected a record but did not necessarily play it yet.
    RecordSelected,

    // Player attempted to play a record.
    RecordPlayed,

    // Player opened a cabinet.
    CabinetOpened,

    // Player entered the NPC's room/area.
    PlayerEnteredRoom,

    // Player left the NPC's room/area.
    PlayerLeftRoom
}
