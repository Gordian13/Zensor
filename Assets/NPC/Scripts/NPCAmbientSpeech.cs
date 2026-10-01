using UnityEngine;

/// <summary>
/// Lets an NPC say a random short line while the player is nearby.
/// Speech is paused during direct interactions and limited by a cooldown.
/// </summary>
/// <remarks>
/// Inspector and prefab values override the initial values in this script.
/// Chance values use the range 0 to 1; they are probabilities, not percentages written from 0 to 100.
/// </remarks>
public class NPCAmbientSpeech : MonoBehaviour
{
    /// <summary>
    /// NPC that speaks the selected ambient line.
    /// When empty, the component searches for an <see cref="NPCController"/> on the same GameObject.
    /// </summary>
    [Header("References")]
    [Tooltip("NPC that should speak. The NPCController on this GameObject is used automatically when empty.")]
    [SerializeField] private NPCController npc;

    /// <summary>
    /// Transform used to measure the distance to the player.
    /// This can be the player root or camera, but it must follow the player and use the same world scale as the NPC.
    /// Ambient speech remains disabled when no transform is assigned.
    /// </summary>
    [Tooltip("Player or camera transform used for the distance check. Ambient speech cannot run while this is empty.")]
    [SerializeField] private Transform playerAnchor;

    /// <summary>
    /// Maximum world-space distance at which ambient lines may be spoken.
    /// The 2.5 unit default keeps comments local to a nearby player; larger values make NPCs audible from farther away.
    /// </summary>
    [Header("Detection")]
    [Tooltip("Maximum distance from the player for ambient speech. Measured in Unity world units.")]
    [SerializeField] private float triggerDistance = 2.5f;

    /// <summary>
    /// Minimum time in seconds between two ambient lines.
    /// The 12 second default prevents repeated lines from interrupting exploration too often.
    /// This cooldown starts only after a valid line was spoken.
    /// </summary>
    [Tooltip("Minimum seconds between spoken ambient lines. The timer starts after a valid line is spoken.")]
    [SerializeField] private float cooldownSeconds = 12f;

    /// <summary>
    /// Chance of speaking during one eligible check, written as a value from 0 to 1.
    /// For example, 0.35 means a 35 percent chance per check, not the number 35.
    /// Together with <see cref="checkInterval"/>, this controls how often speech starts after the cooldown.
    /// </summary>
    [Tooltip("Chance per check from 0 to 1. Use 0.35 for 35%; a value such as 35 will always pass the random check.")]
    [SerializeField] private float chancePerCheck = 0.35f;

    /// <summary>
    /// Time in seconds between distance and chance checks.
    /// The 1 second default keeps the check inexpensive and makes chancePerCheck easy to read as a chance per second.
    /// Shorter intervals check more often and therefore also increase the total chance of speaking over time.
    /// </summary>
    [Tooltip("Seconds between ambient-speech checks. Shorter intervals increase both check frequency and the total chance over time.")]
    [SerializeField] private float checkInterval = 1f;

    /// <summary>
    /// Lines from which one entry is selected at random.
    /// Empty or whitespace-only entries are ignored after selection, so remove them to avoid missed speech attempts.
    /// </summary>
    [Header("Lines")]
    [TextArea]
    [Tooltip("Possible ambient lines. One entry is selected randomly whenever all distance, cooldown and chance checks pass.")]
    [SerializeField] private string[] ambientLines;

    /// <summary>Absolute Time.time value after which the speech cooldown has finished.</summary>
    private float nextAllowedTime;

    /// <summary>Absolute Time.time value at which the next distance and chance check may run.</summary>
    private float nextCheckTime;

    /// <summary>Fills the NPC reference when the component is reset in Unity.</summary>
    private void Reset()
    {
        npc = GetComponent<NPCController>();
    }

    /// <summary>Finds the NPC on the same GameObject when none was assigned.</summary>
    private void Awake()
    {
        if (npc == null)
            npc = GetComponent<NPCController>();
    }
    /// <summary>Prevents ambient speech for the given time.</summary>
    /// <param name="seconds">Length of the pause in seconds.</param>
    public void PauseAmbient(float seconds)
    {
        nextAllowedTime = Time.time + seconds;
    }

    /// <summary>Checks at fixed intervals whether a random line should be spoken.</summary>
    private void Update()
    {
        if (npc == null || playerAnchor == null)
            return;

        if (npc.IsInteracting)
            return;

        if (Time.time < nextAllowedTime || Time.time < nextCheckTime)
            return;

        nextCheckTime = Time.time + checkInterval;

        float distance = Vector3.Distance(transform.position, playerAnchor.position);

        if (distance > triggerDistance)
            return;

        if (Random.value > chancePerCheck)
            return;

        SayRandomLine();
    }

    /// <summary>Selects a valid random line and asks the NPC to say it.</summary>
    private void SayRandomLine()
    {
        if (ambientLines == null || ambientLines.Length == 0)
            return;

        string line = ambientLines[Random.Range(0, ambientLines.Length)];

        if (string.IsNullOrWhiteSpace(line))
            return;

        npc.Say(line);
        nextAllowedTime = Time.time + cooldownSeconds;
    }
}
