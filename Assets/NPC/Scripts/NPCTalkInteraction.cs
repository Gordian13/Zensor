using UnityEngine;

// Simple concrete NPC interaction.
// When selected from the interaction menu, the NPC says the configured dialogue text.
[CreateAssetMenu(
    fileName = "NPC_Interaction_Talk",
    menuName = "NPC/Interactions/Talk Interaction"
)]
public class NPCTalkInteraction : NPCInteraction
{
    // Text shown in the NPC dialogue window when this interaction is executed.
    [TextArea]
    public string dialogueText;

    // Executes the interaction on the given NPC.
    public override void Execute(NPCController npc)
    {
        // Defensive check: without an NPC, there is nothing to execute on.
        if (npc == null)
            return;

        // Reuse the NPC's central dialogue output method.
        npc.Say(dialogueText);
    }
}
