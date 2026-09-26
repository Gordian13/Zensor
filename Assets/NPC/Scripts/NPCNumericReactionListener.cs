using UnityEngine;

/// <summary>
/// Passes number changes from a <see cref="GameIntValueSource"/> to an NPC.
/// This is the connection between the shared number system and the NPC reaction rules.
/// </summary>
public class NPCNumericReactionListener : MonoBehaviour
{
    /// <summary>
    /// NPC that receives NumericValueChanged reaction events.
    /// When empty, the listener searches for an NPCController on the same GameObject during Awake.
    /// </summary>
    [Header("References")]
    [Tooltip("NPC that receives numeric reaction events. The NPCController on this GameObject is used automatically when empty.")]
    [SerializeField] private NPCController npc;

    /// <summary>
    /// Shared gameplay component that owns the number and raises its ValueChanged event.
    /// This type is defined outside the NPC folder so counters can notify an NPC without depending on NPC internals.
    /// </summary>
    [Tooltip("Gameplay number source whose ValueChanged event should be forwarded to the NPC.")]
    [SerializeField] private GameIntValueSource valueSource;

    /// <summary>
    /// Technical ID placed in <see cref="NPCReactionContext.objectId"/>.
    /// Reaction rules compare requiredObjectId against this value, so spelling and capitalization must match exactly.
    /// </summary>
    [Header("Reaction Context")]
    [Tooltip("Technical source ID used by reaction rules. It must exactly match a rule's required object ID.")]
    [SerializeField] private string sourceId = "numeric_source";

    /// <summary>
    /// Readable source name placed in <see cref="NPCReactionContext.objectDisplayName"/>.
    /// It helps with display or debugging but is not currently checked by NPCReactionRule.Matches.
    /// </summary>
    [Tooltip("Readable source name included in the reaction context. Current rule matching does not use this value.")]
    [SerializeField] private string sourceDisplayName = "Numeric Source";

    /// <summary>
    /// Whether the current number should be sent immediately when this listener becomes enabled.
    /// Keep it enabled when an NPC must notice a value that changed before the NPC became active.
    /// Disable it when reactions should happen only after future changes.
    /// </summary>
    [Header("Behaviour")]
    [Tooltip("Evaluates the source's existing value when enabled. Disable to react only to future changes.")]
    [SerializeField] private bool evaluateCurrentValueOnEnable = true;

    /// <summary>Finds the NPC on the same GameObject when none was assigned.</summary>
    private void Awake()
    {
        if (npc == null)
            npc = GetComponent<NPCController>();
    }

    /// <summary>Starts listening for number changes.</summary>
    private void OnEnable()
    {
        if (valueSource == null)
        {
            Debug.LogWarning(
                $"{name}: NPCNumericReactionListener has no value source assigned."
            );

            return;
        }

        valueSource.ValueChanged += OnValueChanged;

        if (evaluateCurrentValueOnEnable)
            OnValueChanged(valueSource.CurrentValue);
    }

    /// <summary>Stops listening when this component is disabled.</summary>
    private void OnDisable()
    {
        if (valueSource != null)
            valueSource.ValueChanged -= OnValueChanged;
    }

    /// <summary>Sends the changed number to the NPC as reaction context.</summary>
    /// <param name="newValue">The latest value from the shared number source.</param>
    private void OnValueChanged(int newValue)
    {
        if (npc == null)
            return;

        NPCReactionContext context = new NPCReactionContext(
            NPCReactionEventType.NumericValueChanged,
            sourceId,
            sourceDisplayName,
            newValue
        );

        npc.ReactTo(context);
    }
}
