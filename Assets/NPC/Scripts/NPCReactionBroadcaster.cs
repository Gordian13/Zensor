using System.Collections.Generic;
using UnityEngine;

public class NPCReactionBroadcaster : MonoBehaviour
{
    public static NPCReactionBroadcaster Instance;

    private readonly List<NPCController> registeredNPCs = new List<NPCController>();

    private void Awake()
    {
        Instance = this;
    }

    public void Register(NPCController npc)
    {
        if (npc == null)
            return;

        if (!registeredNPCs.Contains(npc))
            registeredNPCs.Add(npc);
    }

    public void Unregister(NPCController npc)
    {
        if (npc == null)
            return;

        registeredNPCs.Remove(npc);
    }

    public NPCReactionResult Broadcast(NPCReactionContext context)
    {
        NPCReactionResult finalResult = NPCReactionResult.NoReaction();

        foreach (NPCController npc in registeredNPCs)
        {
            if (npc == null)
                continue;

            NPCReactionResult result = npc.ReactTo(context);

            if (result.hasReaction)
                finalResult = result;
        }

        return finalResult;
    }
}