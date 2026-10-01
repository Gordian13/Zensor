using UnityEngine;

/// <summary>
/// Stores the settings for one NPC mood.
/// These values are saved as part of an <see cref="NPCProfile"/> asset.
/// </summary>
[System.Serializable]
public class NPCMoodData
{
    /// <summary>
    /// Mood represented by this entry.
    /// NPCController selects the first profile entry whose value equals its current mood.
    /// </summary>
    [Tooltip("Mood represented by this entry. Only the first matching profile entry is used.")]
    public NPCMood mood;

    /// <summary>
    /// NavMesh movement speed in Unity units per second while this mood is active.
    /// The default 2 also matches NPCAnimationController's default reference speed, giving a base animation ratio of 1 before its manual multiplier.
    /// After changing this value, check the walk animation for foot sliding and retune the animation reference speed or multiplier when needed.
    /// </summary>
    [Header("Movement")]
    [Tooltip("NavMesh speed in units per second. Changing it may require animation retuning to prevent foot sliding.")]
    public float moveSpeed = 2f;

    /// <summary>
    /// Time in seconds spent at a reached patrol point before selecting the next one.
    /// The 1.5 second default creates a visible pause instead of making the NPC reverse direction immediately.
    /// Different moods can use different values to make the patrol feel calmer or more restless.
    /// </summary>
    [Tooltip("Seconds spent waiting at a patrol point. Larger values make the patrol feel calmer.")]
    public float waitAtPointSeconds = 1.5f;

    /// <summary>
    /// Optional default line associated with this mood.
    /// The current NPC runtime does not read this field; it is reserved for future mood-based speech or another component.
    /// </summary>
    [Header("Speech")]
    [TextArea]
    [Tooltip("Optional mood line reserved for mood-based speech. Current NPC scripts do not display it automatically.")]
    public string speechBubbleText;

    /// <summary>
    /// Color used for this mood's patrol helpers in Unity's Scene view.
    /// Yellow is the default because it remains visible against many scene backgrounds.
    /// This value does not change the NPC model in the built game.
    /// </summary>
    [Header("Debug")]
    [Tooltip("Scene-view color for patrol Gizmos in this mood. It does not change the NPC model.")]
    public Color gizmoColor = Color.yellow;
}
