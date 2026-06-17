using System.Collections.Generic;

public static class NPCDialogueParser
{
    public static Dictionary<string, NPCParsedDialogueNode> Parse(string rawText)
    {
        Dictionary<string, NPCParsedDialogueNode> nodes = new Dictionary<string, NPCParsedDialogueNode>();

        if (string.IsNullOrWhiteSpace(rawText))
            return nodes;

        string[] lines = rawText.Split('\n');

        NPCParsedDialogueNode currentNode = null;
        string currentPlayerText = null;
        string currentNpcResponse = "";

        foreach (string rawLine in lines)
        {
            string line = rawLine.Trim();

            if (string.IsNullOrWhiteSpace(line))
                continue;

            if (line.StartsWith("::"))
            {
                SavePendingChoice(currentNode, ref currentPlayerText, ref currentNpcResponse, null);

                string nodeId = line.Substring(2).Trim();
                currentNode = new NPCParsedDialogueNode(nodeId);
                nodes[nodeId] = currentNode;
                continue;
            }

            if (currentNode == null)
                continue;

            if (line.StartsWith("NPC:"))
            {
                string text = line.Substring(4).Trim();

                if (currentPlayerText == null)
                {
                    currentNode.npcLine = text;
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(currentNpcResponse))
                        currentNpcResponse += "\n";

                    currentNpcResponse += text;
                }

                continue;
            }

            if (line.StartsWith(">"))
            {
                SavePendingChoice(currentNode, ref currentPlayerText, ref currentNpcResponse, null);

                currentPlayerText = line.Substring(1).Trim();
                currentNpcResponse = "";
                continue;
            }

            if (line.StartsWith("->"))
            {
                string target = line.Substring(2).Trim();

                bool endsDialogue = target == "end";
                string nextNodeId = endsDialogue ? null : target;

                SavePendingChoice(
                    currentNode,
                    ref currentPlayerText,
                    ref currentNpcResponse,
                    nextNodeId,
                    endsDialogue,
                    false
                );

                continue;
            }

            if (line.StartsWith("=>"))
            {
                string target = line.Substring(2).Trim();

                bool opensExternal = target == "external";

                SavePendingChoice(
                    currentNode,
                    ref currentPlayerText,
                    ref currentNpcResponse,
                    null,
                    false,
                    opensExternal
                );
            }
        }

        SavePendingChoice(currentNode, ref currentPlayerText, ref currentNpcResponse, null);

        return nodes;
    }

    private static void SavePendingChoice(
        NPCParsedDialogueNode node,
        ref string playerText,
        ref string npcResponse,
        string nextNodeId,
        bool endsDialogue = false,
        bool opensExternalDialogue = false)
    {
        if (node == null || string.IsNullOrWhiteSpace(playerText))
            return;

        node.choices.Add(new NPCParsedDialogueChoice(
            playerText,
            npcResponse,
            nextNodeId,
            endsDialogue,
            opensExternalDialogue
        ));

        playerText = null;
        npcResponse = "";
    }
}