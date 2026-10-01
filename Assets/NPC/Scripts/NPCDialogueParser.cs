using System.Collections.Generic;

// doxygen uses &gt; for the > character, so -&gt; and =&gt; below mean -> and => in dialogue files
/// <summary>
/// Reads the project's plain-text dialogue format and turns it into dialogue nodes.
/// The parser supports NPC lines, player choices, links, endings and external dialogues.
/// </summary>
/// <remarks>
/// The supported format is intentionally small:
/// ::start begins the required first node, NPC: begins an NPC line, and &gt; begins a player choice.
/// -&gt; nodeId links to another node, -&gt; end closes the conversation, and =&gt; external opens the linked dialogue asset.
/// Lines beginning with # or // are ignored. Other text is appended either to the current NPC response or to the node's main NPC line.
/// If the same node ID appears more than once, the later node replaces the earlier entry in the parsed dictionary.
/// </remarks>
public static class NPCDialogueParser
{
    /// <summary>Parses a complete dialogue text.</summary>
    /// <param name="rawText">The text stored in an <see cref="NPCDialogueScript"/>.</param>
    /// <returns>All parsed nodes, addressed by their node ID.</returns>
    public static Dictionary<string, NPCParsedDialogueNode> Parse(string rawText)
    {
        Dictionary<string, NPCParsedDialogueNode> nodes =
            new Dictionary<string, NPCParsedDialogueNode>();

        if (string.IsNullOrWhiteSpace(rawText))
            return nodes;

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

            if (line.StartsWith("#") || line.StartsWith("//"))
                continue;

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

            if (line.StartsWith("->"))
            {
                string target = line.Substring(2).Trim();

                bool endsDialogue = target == "end";
                string nextNodeId = endsDialogue ? null : target;

                if (currentPlayerText != null)
                {
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
                    currentNode.nextNodeId = nextNodeId;
                    currentNode.endsDialogue = endsDialogue;
                }

                continue;
            }

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

    /// <summary>Adds a completed player choice to its node and clears the temporary values.</summary>
    /// <param name="node">The node that receives the choice.</param>
    /// <param name="playerText">The temporary player text.</param>
    /// <param name="npcResponse">The temporary NPC response.</param>
    /// <param name="nextNodeId">The optional ID of the next node.</param>
    /// <param name="endsDialogue">Whether the choice ends the dialogue.</param>
    /// <param name="opensExternalDialogue">Whether the choice opens the linked dialogue.</param>
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

    /// <summary>Adds a non-empty line to an existing text with a line break when needed.</summary>
    /// <param name="target">The text that receives the new line.</param>
    /// <param name="text">The line to add.</param>
    private static void AppendLine(ref string target, string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        if (!string.IsNullOrWhiteSpace(target))
            target += "\n";

        target += text;
    }
}
