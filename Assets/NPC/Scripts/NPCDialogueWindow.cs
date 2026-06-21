using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class NPCDialogueWindow : MonoBehaviour
{
    public static NPCDialogueWindow Instance;

    [Header("UI")]
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text dialogueText;
    private NPCController currentNPC;

    private void Awake()
    {
        Instance = this;

        if (root != null)
            root.SetActive(false);
    }

    private void Update()
    {
        if (root != null && root.activeSelf && Keyboard.current != null)
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
                Hide();
        }
    }

    public void Show(string text, NPCController npc = null)
    {
        if (root == null || dialogueText == null)
        {
            Debug.LogError("NPCDialogueWindow is missing UI references.");
            return;
        }

        currentNPC = npc;
        dialogueText.text = text;
        root.SetActive(true);
    }

    public void Hide()
    {
        if (root != null)
            root.SetActive(false);

        if (currentNPC != null)
        {
            currentNPC.EndInteraction();
            currentNPC = null;
        }
    }
}