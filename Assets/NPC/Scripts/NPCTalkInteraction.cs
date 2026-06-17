using UnityEngine;
/*
[CreateAssetMenu(
    fileName = "NPC_Interaction_Talk",
    menuName = "NPC/Interactions/Talk Interaction"
)]
public class NPCTalkInteraction : NPCInteraction
{
    [Header("Player")]
    [TextArea]
    public string playerText;

    [Header("NPC Response")]
    [TextArea]
    public string npcText;

    [Header("Mood Change")]
    public bool changesMood;
    public NPCMood resultingMood;

    [Header("Menu Behaviour")]
    public bool closeMenuAfterDelay;
    public float closeDelaySeconds = 3f;

    public override void Execute(NPCController npc)
    {
        if (npc == null)
            return;

        if (changesMood)
            npc.SetMood(resultingMood);

        if (NPCDialogueWindow.Instance != null)
        {
            string combinedText = "";

            if (!string.IsNullOrWhiteSpace(playerText))
                combinedText += $"You: {playerText}\n\n";

            if (!string.IsNullOrWhiteSpace(npcText))
                combinedText += $"{npc.NPCName}: {npcText}";

            NPCDialogueWindow.Instance.ShowDialogueScript(combinedText, npc);
        }

        if (closeMenuAfterDelay && NPCInteractionMenu.Instance != null)
            NPCInteractionMenu.Instance.CloseAfterDelay(closeDelaySeconds);
    }
}*/