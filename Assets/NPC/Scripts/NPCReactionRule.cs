using UnityEngine;

/// <summary>
/// Describes when an NPC reacts and what happens after the rule matches.
/// The rule is stored inside an <see cref="NPCProfile"/> asset.
/// </summary>
[System.Serializable]
public class NPCReactionRule
{
    /// <summary>
    /// Event category that must equal <see cref="NPCReactionContext.eventType"/>.
    /// This is always checked before object IDs or numeric conditions.
    /// </summary>
    [Header("Condition")]
    [Tooltip("Event type required for this rule. It must match the reaction context exactly.")]
    public NPCReactionEventType eventType;

    /// <summary>
    /// Optional technical object ID that must exactly equal <see cref="NPCReactionContext.objectId"/>.
    /// Leave it empty to accept the chosen event from every object or numeric source.
    /// </summary>
    [Tooltip("Optional exact object or source ID. Leave empty to accept this event from any source.")]
    public string requiredObjectId;

    /// <summary>
    /// Changes the integer check from a minimum comparison to a divisibility check when the context value is not zero.
    /// With both integer options enabled, minimumIntValue is used as the divisor: a value of 6 matches 6, 12, 18 and so on.
    /// Despite the field name, this mode does not test a minimum. Keep minimumIntValue above zero to avoid division by zero.
    /// </summary>
    [Header("Optional Int Condition")]
    [Tooltip("Uses minimumIntValue as a divisor for non-zero values. Enable the minimum option too and keep its value above zero.")]
    public bool useIntModulus;

    /// <summary>
    /// Enables the integer condition.
    /// Alone it requires context.intValue to be at least minimumIntValue.
    /// Together with useIntModulus it requires non-zero values to be evenly divisible by minimumIntValue.
    /// </summary>
    [Tooltip("Enables the integer check. Alone: minimum comparison. With modulus: divisibility check for non-zero values.")]
    public bool useMinimumIntValue;

    /// <summary>
    /// Comparison value for the integer condition.
    /// It is a minimum when modulus mode is off and a divisor when modulus mode is on.
    /// </summary>
    [Tooltip("Integer threshold, or divisor when modulus mode is enabled. A divisor must be greater than zero.")]
    public int minimumIntValue;

    /// <summary>
    /// Changes the decimal check from a minimum comparison to a remainder check when the context value is not zero.
    /// Exact remainder checks with floating-point values can be sensitive to rounding, so integer conditions are safer for counters.
    /// Keep minimumFloatValue above zero when this mode is combined with the float condition.
    /// </summary>
    [Header("Optional Float Condition")]
    [Tooltip("Uses minimumFloatValue for a remainder check on non-zero values. Floating-point rounding can affect exact matches.")]
    public bool useFloatModulus;

    /// <summary>
    /// Enables the decimal condition.
    /// Alone it requires context.floatValue to be at least minimumFloatValue.
    /// Together with useFloatModulus it requires a zero remainder for non-zero context values.
    /// </summary>
    [Tooltip("Enables the decimal check. Alone: minimum comparison. With modulus: exact remainder check for non-zero values.")]
    public bool useMinimumFloatValue;

    /// <summary>
    /// Comparison value for the decimal condition.
    /// It is a minimum when modulus mode is off and the remainder divisor when modulus mode is on.
    /// </summary>
    [Tooltip("Decimal threshold, or divisor when float modulus mode is enabled. A divisor must be greater than zero.")]
    public float minimumFloatValue;

    /// <summary>
    /// Whether a matching rule changes the NPC's current mood before displaying its reaction text.
    /// The new mood immediately updates movement settings through NPCController.SetMood.
    /// </summary>
    [Header("Reaction")]
    [Tooltip("Changes the NPC mood when this rule matches. Movement settings are updated immediately.")]
    public bool changeMood;

    /// <summary>
    /// Mood selected when changeMood is enabled.
    /// The profile should contain matching NPCMoodData so the new mood has defined movement and waiting values.
    /// </summary>
    [Tooltip("Mood applied after a match. Add matching mood data to the NPC profile.")]
    public NPCMood resultingMood;

    /// <summary>
    /// Optional short line spoken when this rule matches.
    /// An empty or whitespace-only value still counts as a reaction but opens no reaction dialogue.
    /// </summary>
    [TextArea]
    [Tooltip("Optional line spoken when the rule matches. Leave empty for a silent reaction.")]
    public string reactionText;

    /// <summary>
    /// Instruction returned to the system that called NPCController.ReactTo.
    /// The NPC system does not cancel that external action itself; the caller must read the result and stop its own action.
    /// </summary>
    [Tooltip("Requests that the calling system stop its original action. The caller must read and handle the reaction result.")]
    public bool blockOriginalAction;

    /// <summary>
    /// Checks the event type, optional object ID and enabled numeric conditions in that order.
    /// Rules in NPCProfile are checked from top to bottom and only the first match is used.
    /// </summary>
    /// <param name="context">The event information sent to the NPC.</param>
    /// <returns>True when all enabled conditions match.</returns>
    public bool Matches(NPCReactionContext context)
    {
        if (context == null)
        {
            return false;
        }

        if (eventType != context.eventType)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(requiredObjectId) &&
            requiredObjectId != context.objectId)
        {
            return false;
        }

        if (useIntModulus && context.intValue != 0)
        {
            if (useMinimumIntValue &&
                (context.intValue % minimumIntValue != 0))
            {
                return false;
            }
        }
        else if (useMinimumIntValue &&
                 context.intValue < minimumIntValue)
        {
            return false;
        }

        if (useFloatModulus && context.floatValue != 0)
        {
            if (useMinimumFloatValue &&
                (context.floatValue % minimumFloatValue != 0))
            {
                return false;
            }
        }
        else if (useMinimumFloatValue &&
                 context.floatValue < minimumFloatValue)
        {
            return false;
        }

        return true;
    }
}
