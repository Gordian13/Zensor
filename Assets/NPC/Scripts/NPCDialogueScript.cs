using UnityEngine;

[CreateAssetMenu(
    fileName = "NPC_DialogueScript",
    menuName = "NPC/Dialogue/Dialogue Script"
)]
public class NPCDialogueScript : ScriptableObject
{
    [TextArea(20, 80)]
    public string dialogueText;

    [Header("Optional External Link")]
    public NPCDialogueScript externalDialogue;
}