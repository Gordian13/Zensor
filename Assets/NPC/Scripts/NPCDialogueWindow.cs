using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NPCDialogueWindow : MonoBehaviour
{
    public static NPCDialogueWindow Instance;

    [Header("UI")]
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text npcText;
    [SerializeField] private Transform choicesParent;
    [SerializeField] private TMP_Text choiceTextPrefab;
    [SerializeField] private Button closeButton;
    [SerializeField] private RectTransform windowRect;
    [SerializeField] private GameObject choicesArea;
    [SerializeField] private float conversationHeight = 400f;
    [SerializeField] private float reactionHeight = 180f;

    [Header("Reaction Dialogue")]
    [SerializeField] private float defaultReactionDuration = 4f;

    [Header("Typing")]
    [SerializeField] private bool useTypingAnimation = false;
    [SerializeField] private float charactersPerSecond = 40f;

    private NPCController currentNPC;
    private NPCDialogueScript currentScript;
    private Dictionary<string, NPCParsedDialogueNode> currentNodes;
    private NPCParsedDialogueNode currentNode;

    private Coroutine autoHideRoutine;
    private Coroutine typingRoutine;
    private string fullCurrentText;

    private const string RootNodeId = "start";

    private void Awake()
    {
        Instance = this;

        if (root != null)
            root.SetActive(false);

        if (closeButton != null)
            closeButton.onClick.AddListener(CloseConversation);
    }

    public void ShowDialogueScript(NPCDialogueScript dialogueScript, NPCController npc)
    {
        if (dialogueScript == null)
        {
            Debug.LogWarning("Dialogue script is null.");
            return;
        }

        StopAutoHide();
        StopTyping();

        currentNPC = npc;
        currentScript = dialogueScript;
        currentNodes = NPCDialogueParser.Parse(dialogueScript.dialogueText);

        if (!currentNodes.TryGetValue(RootNodeId, out currentNode))
        {
            Debug.LogWarning("Dialogue script has no ::start node.");
            return;
        }

        if (closeButton != null)
            closeButton.gameObject.SetActive(true);

        root.SetActive(true);
        SetWindowMode(true);
        RenderNode(currentNode);
    }

    public void ShowReactionDialogue(string text)
    {
        ShowReactionDialogue(text, defaultReactionDuration);
    }

    public void ShowReactionDialogue(string text, float duration)
    {
        StopAutoHide();
        StopTyping();

        currentNPC = null;
        currentScript = null;
        currentNodes = null;
        currentNode = null;

        ClearChoices();

        if (closeButton != null)
            closeButton.gameObject.SetActive(true);

        root.SetActive(true);
        SetWindowMode(false);
        PlayTypingAnimation(text);

        autoHideRoutine = StartCoroutine(AutoHideAfterSeconds(duration));
    }

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

    private void RenderNode(NPCParsedDialogueNode node, bool showNpcLine = true)
    {
        if (node == null)
            return;

        currentNode = node;

        if (showNpcLine)
            PlayTypingAnimation(node.npcLine);

        ClearChoices();

        foreach (NPCParsedDialogueChoice choice in node.choices)
            AddChoice(choice.playerText, () => SelectChoice(choice));
    }
    private void SelectChoice(NPCParsedDialogueChoice choice)
    {
        if (choice == null)
            return;

        ClearChoices();

        if (!string.IsNullOrWhiteSpace(choice.npcResponse))
            PlayTypingAnimation(choice.npcResponse);

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

    public void CloseConversation()
    {
        Hide();
        NPCAmbientSpeech ambient = currentNPC.GetComponent<NPCAmbientSpeech>();

        if (ambient != null)
            ambient.PauseAmbient(5f);

        if (currentNPC != null)
        {
            currentNPC.EndInteraction();
            currentNPC = null;
        }
    }

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

    private System.Collections.IEnumerator TypeText(string text)
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

    private void AddChoice(string text, System.Action action)
    {
        TMP_Text choiceText = Instantiate(choiceTextPrefab, choicesParent);
        choiceText.text = text;

        DialogueChoiceClickHandler clickHandler =
            choiceText.gameObject.AddComponent<DialogueChoiceClickHandler>();

        clickHandler.Setup(
            choiceText,
            Color.white,
            Color.yellow,
            action
        );
    }

    private void AddSingleContinueChoice(string text, System.Action action)
    {
        AddChoice(text, action);
    }

    private void ClearChoices()
    {
        if (choicesParent == null)
            return;

        for (int i = choicesParent.childCount - 1; i >= 0; i--)
            Destroy(choicesParent.GetChild(i).gameObject);
    }

    private System.Collections.IEnumerator AutoHideAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        Hide();
    }

    private void StopAutoHide()
    {
        if (autoHideRoutine != null)
        {
            StopCoroutine(autoHideRoutine);
            autoHideRoutine = null;
        }
    }
}