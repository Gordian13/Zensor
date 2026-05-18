using UnityEngine;

[CreateAssetMenu(
    fileName = "NPC_Interaction_Talk",
    menuName = "NPC/Interactions/Talk Interaction"
)]
public class NPCTalkInteraction : NPCInteraction
{
    [TextArea]
    public string dialogueText;

    public override void Execute(NPCController npc)
    {
        if (npc == null)
            return;

        npc.Say(dialogueText);
    }
}