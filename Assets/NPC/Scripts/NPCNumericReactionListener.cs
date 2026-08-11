using UnityEngine;

public class NPCNumericReactionListener : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NPCController npc;
    [SerializeField] private GameIntValueSource valueSource;

    [Header("Reaction Context")]
    [SerializeField] private string sourceId = "numeric_source";
    [SerializeField] private string sourceDisplayName = "Numeric Source";

    [Header("Behaviour")]
    [SerializeField] private bool evaluateCurrentValueOnEnable = true;

    private void Awake()
    {
        if (npc == null)
            npc = GetComponent<NPCController>();
    }

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

    private void OnDisable()
    {
        if (valueSource != null)
            valueSource.ValueChanged -= OnValueChanged;
    }

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