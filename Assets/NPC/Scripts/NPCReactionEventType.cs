/// <summary>
/// Lists the events an NPC can react to.
/// Other systems pass one of these values through an <see cref="NPCReactionContext"/>.
/// </summary>
public enum NPCReactionEventType
{
    /// <summary>No event has happened.</summary>
    None,

    /// <summary>An object was clicked.</summary>
    ObjectClicked,

    /// <summary>A flyer was read.</summary>
    FlyerRead,

    /// <summary>A record was selected.</summary>
    RecordSelected,

    /// <summary>A record was played.</summary>
    RecordPlayed,

    /// <summary>A cabinet was opened.</summary>
    CabinetOpened,

    /// <summary>The player entered the NPC's area.</summary>
    PlayerEnteredRoom,

    /// <summary>The player left the NPC's area.</summary>
    PlayerLeftRoom,

    /// <summary>A numeric value from another system changed.</summary>
    NumericValueChanged
}
