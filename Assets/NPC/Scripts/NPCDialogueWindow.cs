using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays conversations and short reaction messages for NPCs.
/// It parses dialogue assets, creates player choices and uses the shared interaction state while a conversation is open.
/// </summary>
/// <remarks>
/// UI references and display values are serialized on each prefab, so those saved values override the initial values in this script.
/// Conversation mode stays open for player input, while reaction mode has no choices and hides itself after a timer.
/// </remarks>
public class NPCDialogueWindow : MonoBehaviour
{
    /// <summary>The active NPC dialogue window in the scene.</summary>
    public static NPCDialogueWindow Instance;

    /// <summary>Main UI object that is enabled while any NPC dialogue is visible.</summary>
    [Header("UI")]
    [Tooltip("Top-level dialogue UI object that is shown and hidden by this component.")]
    [SerializeField] private GameObject root;

    /// <summary>Text element used for NPC lines and short reaction messages.</summary>
    [Tooltip("TextMeshPro element that displays the current NPC line or reaction.")]
    [SerializeField] private TMP_Text npcText;

    /// <summary>Parent transform under which generated player choices are placed.</summary>
    [Tooltip("Container for generated player-choice objects. Existing children are removed when choices are rebuilt.")]
    [SerializeField] private Transform choicesParent;

    /// <summary>
    /// Text prefab cloned for every player choice.
    /// The generated object receives a <see cref="DialogueChoiceClickHandler"/> at runtime, so it must support pointer events.
    /// </summary>
    [Tooltip("TMP text prefab cloned for each player choice. It must be visible under the choices parent and receive pointer events.")]
    [SerializeField] private TMP_Text choiceTextPrefab;

    /// <summary>Optional button that closes the full conversation through <see cref="CloseConversation"/>.</summary>
    [Tooltip("Optional button used to close the current conversation.")]
    [SerializeField] private Button closeButton;

    /// <summary>RectTransform whose height changes between conversation and reaction modes.</summary>
    [Tooltip("Dialogue window RectTransform whose height is changed for conversations and short reactions.")]
    [SerializeField] private RectTransform windowRect;

    /// <summary>UI group containing the player-choice controls, hidden for short reactions.</summary>
    [Tooltip("UI group containing player choices. It is hidden while a short reaction is displayed.")]
    [SerializeField] private GameObject choicesArea;

    /// <summary>
    /// Window height in Canvas units during a full conversation.
    /// The 400 unit default leaves space for both the NPC line and multiple player choices.
    /// </summary>
    [Tooltip("Window height in Canvas units during a full conversation. It should leave enough room for the available choices.")]
    [SerializeField] private float conversationHeight = 400f;

    /// <summary>
    /// Window height in Canvas units for a short reaction without choices.
    /// The smaller 180 unit default avoids leaving an empty choice area below a single line.
    /// </summary>
    [Tooltip("Window height in Canvas units for short reactions without player choices.")]
    [SerializeField] private float reactionHeight = 180f;

    /// <summary>
    /// Number of seconds a reaction remains visible when no custom duration is supplied.
    /// This does not close full conversations; those wait for a choice or close action.
    /// </summary>
    [Header("Reaction Dialogue")]
    [Tooltip("Seconds before a short reaction hides automatically. Full conversations are not affected.")]
    [SerializeField] private float defaultReactionDuration = 8f;

    /// <summary>
    /// Whether NPC text is revealed one character at a time.
    /// When disabled, the complete line is assigned immediately and charactersPerSecond is ignored.
    /// </summary>
    [Header("Typing")]
    [Tooltip("Reveals NPC text one character at a time. Disable to display each complete line immediately.")]
    [SerializeField] private bool useTypingAnimation = false;

    /// <summary>
    /// Number of characters revealed per second while typing animation is enabled.
    /// The 40 character default is readable but still faster than normal speech; larger values finish sooner.
    /// This value must stay above zero because it is used as a divisor for the delay between characters.
    /// </summary>
    [Tooltip("Characters shown per second by the typing effect. Higher is faster. Keep above zero.")]
    [SerializeField] private float charactersPerSecond = 40f;

    /// <summary>NPC currently speaking in a full conversation or named reaction.</summary>
    private NPCController currentNPC;

    /// <summary>Dialogue asset currently being displayed.</summary>
    private NPCDialogueScript currentScript;

    /// <summary>Parsed nodes of the current dialogue, addressed by node ID.</summary>
    private Dictionary<string, NPCParsedDialogueNode> currentNodes;

    /// <summary>Node currently shown in the dialogue window.</summary>
    private NPCParsedDialogueNode currentNode;

    /// <summary>Pending automatic hide used only by short reaction messages.</summary>
    private Coroutine autoHideRoutine;

    /// <summary>Active character-by-character text animation.</summary>
    private Coroutine typingRoutine;

    /// <summary>Complete text restored when a running typing animation is stopped early.</summary>
    private string fullCurrentText;

    /// <summary>Required ID of the first node in every full dialogue.</summary>
    private const string RootNodeId = "start";

    /// <summary>
    /// Time before ambient speech may resume after a full conversation.
    /// Five seconds keeps an ambient line from immediately following and visually competing with the closing dialogue.
    /// </summary>
    private const float AmbientSpeechResumeDelay = 5f;

    /// <summary>Normal player-choice color used when the pointer is not over the text.</summary>
    private static readonly Color ChoiceNormalColor = Color.white;

    /// <summary>Highlight color used to make the choice below the pointer easy to recognize.</summary>
    private static readonly Color ChoiceHoverColor = Color.yellow;

    /// <summary>Registers the window, hides it and connects the close button.</summary>
    private void Awake()
    {
        Instance = this;

        if (root != null)
            root.SetActive(false);

        if (closeButton != null)
            closeButton.onClick.AddListener(CloseConversation);
    }

    /// <summary>Starts a full conversation from a dialogue asset.</summary>
    /// <param name="dialogueScript">The dialogue asset to read.</param>
    /// <param name="npc">The NPC taking part in the conversation.</param>
    public void ShowDialogueScript(NPCDialogueScript dialogueScript, NPCController npc)
    {
        StopAutoHide();
        StopTyping();

        currentNPC = npc;

        if (dialogueScript == null)
        {
            Debug.LogWarning("Dialogue script is null.");
            CloseConversation();
            return;
        }

        currentScript = dialogueScript;
        currentNodes = NPCDialogueParser.Parse(dialogueScript.dialogueText);

        if (!currentNodes.TryGetValue(RootNodeId, out currentNode))
        {
            Debug.LogWarning("Dialogue script has no ::start node.");
            CloseConversation();
            return;
        }

        GlobalInteractionState.Instance.BlockInteractions();
        if (closeButton != null)
            closeButton.gameObject.SetActive(true);

        root.SetActive(true);
        SetWindowMode(true);
        RenderNode(currentNode);
    }

    /// <summary>Shows a short reaction using the default display time.</summary>
    /// <param name="text">The NPC's reaction text.</param>
    /// <param name="npc">The NPC that said the line.</param>
    public void ShowReactionDialogue(string text, NPCController npc)
    {
        currentNPC = npc;
        ShowReactionDialogue(text, defaultReactionDuration);
    }

    /// <summary>Shows a short reaction for a chosen amount of time.</summary>
    /// <param name="text">The reaction text.</param>
    /// <param name="duration">How long the text remains visible.</param>
    public void ShowReactionDialogue(string text, float duration)
    {
        StopAutoHide();
        StopTyping();
        string formattedText = FormatNpcText(text);
        currentNPC = null;
        currentScript = null;
        currentNodes = null;
        currentNode = null;

        ClearChoices();

        if (closeButton != null)
            closeButton.gameObject.SetActive(true);

        root.SetActive(true);
        SetWindowMode(false);
        PlayTypingAnimation(formattedText);

        autoHideRoutine = StartCoroutine(AutoHideAfterSeconds(duration));
    }

    /// <summary>Adjusts the window for either a conversation or a short reaction.</summary>
    /// <param name="conversationMode">True when player choices should be visible.</param>
    private void SetWindowMode(bool conversationMode)
    {
        if (choicesArea != null)
            choicesArea.SetActive(conversationMode);

        if (windowRect != null)
        {
            Vector2 size = windowRect.sizeDelta;
            size.y = conversationMode ? conversationHeight : reactionHeight;
            windowRect.sizeDelta = size;
        }
    }

    /// <summary>Opens the dialogue asset linked from the current script.</summary>
    private void OpenExternalDialogue()
    {
        if (currentScript == null ||
            currentScript.externalDialogue == null)
        {
            Debug.LogWarning("No external dialogue assigned.");
            return;
        }

        ShowDialogueScript(
            currentScript.externalDialogue,
            currentNPC
        );
    }

    /// <summary>Shows one dialogue node and creates its available choices.</summary>
    /// <param name="node">The node to display.</param>
    /// <param name="showNpcLine">Whether the node's first NPC line should be shown.</param>
    private void RenderNode(NPCParsedDialogueNode node, bool showNpcLine = true)
    {
        if (node == null)
            return;

        currentNode = node;

        if (showNpcLine)
            PlayTypingAnimation(FormatNpcText(node.npcLine));

        ClearChoices();

        foreach (NPCParsedDialogueChoice choice in node.choices)
            AddChoice(choice.playerText, () => SelectChoice(choice));

        if (node.choices.Count == 0)
        {
            if (node.endsDialogue)
            {
                AddSingleContinueChoice("Weiter", CloseConversation);
                return;
            }

            if (node.opensExternalDialogue)
            {
                AddSingleContinueChoice("Weiter", OpenExternalDialogue);
                return;
            }

            if (!string.IsNullOrWhiteSpace(node.nextNodeId))
            {
                if (currentNodes.TryGetValue(
                        node.nextNodeId,
                        out NPCParsedDialogueNode nextNode))
                {
                    AddSingleContinueChoice(
                        "Weiter",
                        () => RenderNode(nextNode)
                    );
                }
            }
        }
    }

    /// <summary>Applies a selected player choice and continues the dialogue.</summary>
    /// <param name="choice">The choice selected by the player.</param>
    private void SelectChoice(NPCParsedDialogueChoice choice)
    {
        if (choice == null)
            return;

        ClearChoices();

        if (!string.IsNullOrWhiteSpace(choice.npcResponse))
            PlayTypingAnimation(FormatNpcText(choice.npcResponse));

        if (choice.endsDialogue)
        {
            AddSingleContinueChoice("Weiter", CloseConversation);
            return;
        }

        if (choice.opensExternalDialogue)
        {
            if (currentScript != null && currentScript.externalDialogue != null)
                ShowDialogueScript(currentScript.externalDialogue, currentNPC);

            return;
        }

        if (!string.IsNullOrWhiteSpace(choice.nextNodeId))
        {
            if (currentNodes != null && currentNodes.TryGetValue(choice.nextNodeId, out NPCParsedDialogueNode nextNode))
            {
                if (!string.IsNullOrWhiteSpace(choice.npcResponse))
                    AddSingleContinueChoice("Zurück", () => RenderNode(nextNode, false));
                else
                    RenderNode(nextNode);
            }
            else
            {
                Debug.LogWarning($"Dialogue node not found: {choice.nextNodeId}");
            }
        }
    }

    /// <summary>Adds the current NPC's name in front of a line.</summary>
    /// <param name="text">The original NPC line.</param>
    /// <returns>The line with a name, or the unchanged line when no NPC is set.</returns>
    private string FormatNpcText(string text)
    {
        if (currentNPC == null)
            return text;

        return $"{currentNPC.NPCName}: {text}";
    }

    /// <summary>Closes the conversation, resumes ambient speech later and ends the NPC interaction.</summary>
    public void CloseConversation()
    {
        Hide();

        if (currentNPC != null)
        {
            NPCAmbientSpeech ambient = currentNPC.GetComponent<NPCAmbientSpeech>();

            if (ambient != null)
                ambient.PauseAmbient(AmbientSpeechResumeDelay);

            currentNPC.EndInteraction();
            currentNPC = null;
        }

        GlobalInteractionState.Instance.UnblockInteractions();
    }

    /// <summary>Hides the window and clears its temporary dialogue data.</summary>
    public void Hide()
    {
        StopAutoHide();
        StopTyping();

        if (root != null)
            root.SetActive(false);

        ClearChoices();

        currentScript = null;
        currentNodes = null;
        currentNode = null;
    }

    /// <summary>Shows a line immediately or starts the optional typing effect.</summary>
    /// <param name="text">The full text to display.</param>
    private void PlayTypingAnimation(string text)
    {
        StopTyping();

        fullCurrentText = text;

        if (!useTypingAnimation)
        {
            npcText.text = text;
            return;
        }

        typingRoutine = StartCoroutine(TypeText(text));
    }

    /// <summary>Reveals a line one character at a time.</summary>
    /// <param name="text">The full text to reveal.</param>
    /// <returns>An enumerator used by Unity as a coroutine.</returns>
    private IEnumerator TypeText(string text)
    {
        npcText.text = "";

        if (string.IsNullOrWhiteSpace(text))
        {
            typingRoutine = null;
            yield break;
        }

        float delay = 1f / charactersPerSecond;

        foreach (char c in text)
        {
            npcText.text += c;
            yield return new WaitForSeconds(delay);
        }

        typingRoutine = null;
    }

    /// <summary>Stops the typing effect and displays the complete current line.</summary>
    private void StopTyping()
    {
        if (typingRoutine != null)
        {
            StopCoroutine(typingRoutine);
            typingRoutine = null;
        }

        if (!string.IsNullOrEmpty(fullCurrentText) && npcText != null)
            npcText.text = fullCurrentText;
    }

    /// <summary>Creates one clickable player choice.</summary>
    /// <param name="text">The text shown for the choice.</param>
    /// <param name="action">The action run after the choice is clicked.</param>
    private void AddChoice(string text, Action action)
    {
        TMP_Text choiceText = Instantiate(choiceTextPrefab, choicesParent);
        choiceText.text = text;

        DialogueChoiceClickHandler clickHandler =
            choiceText.gameObject.AddComponent<DialogueChoiceClickHandler>();

        clickHandler.Setup(
            choiceText,
            ChoiceNormalColor,
            ChoiceHoverColor,
            action
        );
    }

    /// <summary>Creates the single button used to continue or close a dialogue.</summary>
    /// <param name="text">The text shown on the button.</param>
    /// <param name="action">The action run after the button is clicked.</param>
    private void AddSingleContinueChoice(string text, Action action)
    {
        AddChoice(text, action);
    }

    /// <summary>Removes every player choice currently shown.</summary>
    private void ClearChoices()
    {
        if (choicesParent == null)
            return;

        for (int i = choicesParent.childCount - 1; i >= 0; i--)
            Destroy(choicesParent.GetChild(i).gameObject);
    }

    /// <summary>Waits and then hides a short reaction.</summary>
    /// <param name="seconds">How long the reaction remains visible.</param>
    /// <returns>An enumerator used by Unity as a coroutine.</returns>
    private IEnumerator AutoHideAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        Hide();
    }

    /// <summary>Cancels a scheduled automatic hide.</summary>
    private void StopAutoHide()
    {
        if (autoHideRoutine != null)
        {
            StopCoroutine(autoHideRoutine);
            autoHideRoutine = null;
        }
    }
}
