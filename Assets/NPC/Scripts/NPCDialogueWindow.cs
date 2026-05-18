using UnityEngine;
using TMPro;

public class NPCDialogueWindow : MonoBehaviour
{
    public static NPCDialogueWindow Instance;

    [Header("UI")]
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text dialogueText;

    private void Awake()
    {
        Instance = this;

        if (root != null)
            root.SetActive(false);
    }

    public void Show(string text)
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
        if (root != null)
            root.SetActive(false);
    }
}