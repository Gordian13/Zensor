using UnityEngine;

// Serializable data container for one mood configuration.
// This is not a MonoBehaviour and not a ScriptableObject by itself.
// It is stored inside NPCProfile so each profile can define its own mood values.
[System.Serializable]
public class NPCMoodData
{
    // Which mood this data belongs to.
    public NPCMood mood;

    [Header("Movement")]
    // Movement speed applied to the NavMeshAgent while this mood is active.
    public float moveSpeed = 2f;

    // How long the NPC waits after reaching a patrol point in this mood.
    public float waitAtPointSeconds = 1.5f;

    [Header("Speech")]
    // Optional default speech text for this mood.
    // Not heavily used yet, but useful for future speech bubbles/dialogue behavior.
    [TextArea]
    public string speechBubbleText;

    [Header("Debug")]
    // Color used for patrol gizmos while this mood is active.
    public Color gizmoColor = Color.yellow;
}
