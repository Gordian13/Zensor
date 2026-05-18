using UnityEngine;

[System.Serializable]
public class NPCMoodData
{
    public NPCMood mood;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float waitAtPointSeconds = 1.5f;

    [Header("Speech")]
    [TextArea]
    public string speechBubbleText;

    [Header("Debug")]
    public Color gizmoColor = Color.yellow;
}