using UnityEngine;

public abstract class NPCInteraction : ScriptableObject
{
    [Header("Interaction")]
    public string interactionName = "Interaction";

    public abstract void Execute(NPCController npc);
}