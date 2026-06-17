using UnityEngine;

public class NPCAmbientSpeech : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NPCController npc;
    [SerializeField] private Transform playerAnchor;

    [Header("Detection")]
    [SerializeField] private float triggerDistance = 2.5f;
    [SerializeField] private float cooldownSeconds = 12f;
    [SerializeField] private float chancePerCheck = 0.35f;
    [SerializeField] private float checkInterval = 1f;

    [Header("Lines")]
    [TextArea]
    [SerializeField] private string[] ambientLines;

    private float nextAllowedTime;
    private float nextCheckTime;

    private void Reset()
    {
        npc = GetComponent<NPCController>();
    }

    private void Awake()
    {
        if (npc == null)
            npc = GetComponent<NPCController>();
    }
    public void PauseAmbient(float seconds)
    {
        nextAllowedTime = Time.time + seconds;
    }

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