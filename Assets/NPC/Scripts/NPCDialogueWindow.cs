using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class NPCDialogueWindow : MonoBehaviour
{
    public static NPCDialogueWindow Instance;

    [Header("UI")]
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Button closeButton;

    [Header("Reaction Dialogue")]
    [SerializeField] private float defaultReactionDuration = 4f;

    private NPCController currentNPC;
    private Coroutine autoHideRoutine;

    private void Awake()
    {
        Instance = this;

        if (root != null)
            root.SetActive(false);

        if (closeButton != null)
            closeButton.onClick.AddListener(Hide);
    }

    public void ShowInteractionDialogue(string text, NPCController npc)
    {
        StopAutoHide();

        currentNPC = npc;

        if (closeButton != null)
            closeButton.gameObject.SetActive(false);

        ShowText(text);
    }

    public void ShowReactionDialogue(string text)
    {
        ShowReactionDialogue(text, defaultReactionDuration);
    }

    public void ShowReactionDialogue(string text, float duration)
    {
        StopAutoHide();

        currentNPC = null;

        if (closeButton != null)
            closeButton.gameObject.SetActive(true);

        ShowText(text);

        autoHideRoutine = StartCoroutine(AutoHideAfterSeconds(duration));
    }

    private void ShowText(string text)
    {
        if (root == null || dialogueText == null)
        {
            Debug.LogError("NPCDialogueWindow is missing UI references.");
            return;
        }

        dialogueText.text = text;
        root.SetActive(true);
    }

    public void Hide()
    {
        StopAutoHide();

        if (root != null)
            root.SetActive(false);

        currentNPC = null;
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