using System.Collections.Generic;

public static class NPCDialogueParser
{
    public static Dictionary<string, NPCParsedDialogueNode> Parse(string rawText)
    {
        Dictionary<string, NPCParsedDialogueNode> nodes =
            new Dictionary<string, NPCParsedDialogueNode>();

        if (string.IsNullOrWhiteSpace(rawText))
            return nodes;

        // Unterstützt Windows- und Unix-Zeilenumbrüche.
        string normalizedText = rawText
            .Replace("\r\n", "\n")
            .Replace('\r', '\n');

        string[] lines = normalizedText.Split('\n');

        NPCParsedDialogueNode currentNode = null;
        string currentPlayerText = null;
        string currentNpcResponse = "";

        foreach (string rawLine in lines)
        {
            string line = rawLine.Trim();

            if (string.IsNullOrWhiteSpace(line))
                continue;

            // Kommentare erlauben.
            if (line.StartsWith("#") || line.StartsWith("//"))
                continue;

            // Neuer Node.
            if (line.StartsWith("::"))
            {
                SavePendingChoice(
                    currentNode,
                    ref currentPlayerText,
                    ref currentNpcResponse
                );

                string nodeId = line.Substring(2).Trim();

                if (string.IsNullOrWhiteSpace(nodeId))
                    continue;

                currentNode = new NPCParsedDialogueNode(nodeId);
                nodes[nodeId] = currentNode;
                continue;
            }

            if (currentNode == null)
                continue;

            // NPC-Text.
            if (line.StartsWith("NPC:"))
            {
                string text = line.Substring(4).Trim();

                if (currentPlayerText == null)
                {
                    AppendLine(ref currentNode.npcLine, text);
                }
                else
                {
                    AppendLine(ref currentNpcResponse, text);
                }

                continue;
            }

            // Player-Option.
            if (line.StartsWith(">") && !line.StartsWith("=>"))
            {
                SavePendingChoice(
                    currentNode,
                    ref currentPlayerText,
                    ref currentNpcResponse
                );

                currentPlayerText = line.Substring(1).Trim();
                currentNpcResponse = "";
                continue;
            }

            // Externer Übergang.
            if (line.StartsWith("=>"))
            {
                string target = line.Substring(2).Trim();
                bool opensExternal = target == "external";

                if (currentPlayerText != null)
                {
                    SavePendingChoice(
                        currentNode,
                        ref currentPlayerText,
                        ref currentNpcResponse,
                        nextNodeId: null,
                        endsDialogue: false,
                        opensExternalDialogue: opensExternal
                    );
                }
                else
                {
                    currentNode.opensExternalDialogue = opensExternal;
                }

                continue;
            }

            // Interner Übergang oder Dialogende.
            if (line.StartsWith("->"))
            {
                string target = line.Substring(2).Trim();

                bool endsDialogue = target == "end";
                string nextNodeId = endsDialogue ? null : target;

                if (currentPlayerText != null)
                {
                    // Übergang gehört zur aktuellen Player-Option.
                    SavePendingChoice(
                        currentNode,
                        ref currentPlayerText,
                        ref currentNpcResponse,
                        nextNodeId,
                        endsDialogue,
                        false
                    );
                }
                else
                {
                    // Übergang gehört zum gesamten Node.
                    currentNode.nextNodeId = nextNodeId;
                    currentNode.endsDialogue = endsDialogue;
                }

                continue;
            }

            // Freier Text:
            // Innerhalb einer Player-Option wird er an die NPC-Antwort gehängt,
            // sonst an den allgemeinen NPC-Text des Nodes.
            if (currentPlayerText != null)
                AppendLine(ref currentNpcResponse, line);
            else
                AppendLine(ref currentNode.npcLine, line);
        }

        SavePendingChoice(
            currentNode,
            ref currentPlayerText,
            ref currentNpcResponse
        );

        return nodes;
    }

    private static void SavePendingChoice(
        NPCParsedDialogueNode node,
        ref string playerText,
        ref string npcResponse,
        string nextNodeId = null,
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

    private static void AppendLine(ref string target, string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        if (!string.IsNullOrWhiteSpace(target))
            target += "\n";

        target += text;
    }
}